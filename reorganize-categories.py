import pyodbc
from datetime import datetime
from collections import defaultdict
import sys
import io

# Set UTF-8 encoding for console output
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# Database connection string
# Update this with your actual connection string
CONNECTION_STRING = (
    "DRIVER={ODBC Driver 17 for SQL Server};"
    "SERVER=localhost;"
    "DATABASE=SuperMarketDb;"
    "Trusted_Connection=yes;"
    "TrustServerCertificate=yes;"
)

def get_connection():
    """Create database connection"""
    return pyodbc.connect(CONNECTION_STRING)

def parse_category_hierarchy(category_name):
    """
    Parse category name with >> delimiter into hierarchy
    Example: "Gia vị>>Nước chấm>>Mắm ruốc, mắm nêm"
    Returns: ["Gia vị", "Nước chấm", "Mắm ruốc, mắm nêm"]
    """
    if '>>' not in category_name:
        return [category_name]
    return [part.strip() for part in category_name.split('>>')]

def reorganize_categories():
    """Main function to reorganize categories"""
    conn = get_connection()
    cursor = conn.cursor()

    try:
        # Step 1: Apply migration to add ParentCategoryId column
        print("Step 1: Applying migration...")
        cursor.execute("UPDATE __EFMigrationsHistory SET ProductVersion = '9.0.9' WHERE MigrationId LIKE '%AddCategoryHierarchy%'")

        # Step 2: Fetch all existing categories
        print("\nStep 2: Fetching existing categories...")
        cursor.execute("SELECT Id, Name FROM Categories ORDER BY Id")
        existing_categories = cursor.fetchall()

        print(f"Found {len(existing_categories)} categories")

        # Step 3: Parse hierarchy and build category tree
        print("\nStep 3: Parsing category hierarchies...")
        category_map = {}  # Maps category name to (id, parent_name)
        hierarchy_data = []  # List of (original_id, hierarchy_parts)

        for cat_id, cat_name in existing_categories:
            parts = parse_category_hierarchy(cat_name)
            hierarchy_data.append((cat_id, cat_name, parts))
            print(f"  {cat_id}: {cat_name} -> {parts}")

        # Step 4: Build unique categories and relationships
        print("\nStep 4: Building unique categories and relationships...")
        unique_categories = {}  # Maps category name -> id
        category_parent_map = {}  # Maps category name -> parent name
        new_category_id = max([c[0] for c in existing_categories]) + 1

        for original_id, original_name, parts in hierarchy_data:
            for i, part in enumerate(parts):
                if part not in unique_categories:
                    # This is a new unique category
                    if i == 0:
                        # Root category, no parent
                        unique_categories[part] = new_category_id if part not in [c[1] for c in existing_categories] else next(c[0] for c in existing_categories if c[1] == part)
                        category_parent_map[part] = None
                        if part not in [c[1] for c in existing_categories]:
                            new_category_id += 1
                    else:
                        # Sub-category, parent is previous part
                        parent_name = parts[i-1]
                        unique_categories[part] = new_category_id
                        category_parent_map[part] = parent_name
                        new_category_id += 1

        print(f"  Found {len(unique_categories)} unique categories")

        # Step 5: Clear existing categories and insert new hierarchy
        print("\nStep 5: Reorganizing database...")

        # Define timestamp
        now = datetime.now()

        # First, get products that reference categories
        cursor.execute("SELECT Id, CategoryId FROM Products")
        products = cursor.fetchall()
        product_category_map = {}
        for prod_id, cat_id in products:
            # Find the original category name
            original_cat = next((c for c in existing_categories if c[0] == cat_id), None)
            if original_cat:
                # Parse its hierarchy and use the LAST part (most specific category)
                parts = parse_category_hierarchy(original_cat[1])
                product_category_map[prod_id] = parts[-1]  # Use most specific category

        # Create a temporary category for products with a very high ID to avoid conflicts
        print("  Creating temporary category...")
        temp_cat_id = 999999
        cursor.execute("SET IDENTITY_INSERT Categories ON")
        cursor.execute("INSERT INTO Categories (Id, Name, Description, IsActive, CreatedAt) VALUES (?, 'TEMP', 'Temporary', 1, ?)", temp_cat_id, now)
        cursor.execute("SET IDENTITY_INSERT Categories OFF")

        #Update products to use temporary category
        print(f"  Setting all products to temporary category {temp_cat_id}...")
        cursor.execute(f"UPDATE Products SET CategoryId = {temp_cat_id}")

        # Delete all existing categories except TEMP
        print("  Deleting existing categories...")
        cursor.execute("DELETE FROM Categories WHERE Id != ?",(temp_cat_id))

        # Insert new categories with hierarchy
        print("  Inserting new category hierarchy...")

        # Enable IDENTITY_INSERT to allow explicit ID values
        cursor.execute("SET IDENTITY_INSERT Categories ON")

        # First pass: insert all categories
        for cat_name, cat_id in sorted(unique_categories.items(), key=lambda x: x[1]):
            cursor.execute(
                """INSERT INTO Categories (Id, Name, Description, IsActive, CreatedAt, ParentCategoryId)
                   VALUES (?, ?, ?, ?, ?, NULL)""",
                cat_id, cat_name, f"Auto-generated from hierarchy", True, now
            )
            print(f"    Inserted: {cat_id} - {cat_name}")

        # Disable IDENTITY_INSERT
        cursor.execute("SET IDENTITY_INSERT Categories OFF")

        # Second pass: update parent relationships
        for cat_name, parent_name in category_parent_map.items():
            if parent_name is not None:
                cat_id = unique_categories[cat_name]
                parent_id = unique_categories[parent_name]
                cursor.execute(
                    """UPDATE Categories SET ParentCategoryId = ? WHERE Id = ?""",
                    parent_id, cat_id
                )
                print(f"    Set parent: {cat_name} ({cat_id}) -> {parent_name} ({parent_id})")

        # Update products to use new category IDs
        print("\n  Updating product categories...")
        for prod_id, cat_name in product_category_map.items():
            new_cat_id = unique_categories.get(cat_name)
            if new_cat_id:
                cursor.execute("UPDATE Products SET CategoryId = ? WHERE Id = ?", new_cat_id, prod_id)
                print(f"    Product {prod_id} -> Category '{cat_name}' ({new_cat_id})")

        # Delete the TEMP category
        print("\n  Deleting temporary category...")
        cursor.execute("DELETE FROM Categories WHERE Id = ?", temp_cat_id)

        # Commit all changes
        conn.commit()
        print("\n✓ Successfully reorganized categories!")

        # Display summary
        print("\n=== Summary ===")
        cursor.execute("SELECT COUNT(*) FROM Categories WHERE ParentCategoryId IS NULL")
        root_count = cursor.fetchone()[0]
        cursor.execute("SELECT COUNT(*) FROM Categories WHERE ParentCategoryId IS NOT NULL")
        sub_count = cursor.fetchone()[0]
        print(f"Root categories: {root_count}")
        print(f"Sub-categories: {sub_count}")
        print(f"Total categories: {root_count + sub_count}")

    except Exception as e:
        conn.rollback()
        print(f"\n✗ Error: {e}")
        raise
    finally:
        cursor.close()
        conn.close()

if __name__ == "__main__":
    print("=" * 60)
    print("Category Hierarchy Reorganization Script")
    print("=" * 60)
    print("\nThis script will:")
    print("1. Parse existing category names with >> delimiter")
    print("2. Create a hierarchical category structure")
    print("3. Update the database with new relationships")
    print("\nWARNING: This will modify your database!")
    print("=" * 60)

    response = input("\nDo you want to proceed? (yes/no): ")
    if response.lower() == 'yes':
        reorganize_categories()
    else:
        print("Operation cancelled.")
