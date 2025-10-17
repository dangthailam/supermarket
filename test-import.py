import requests
import os

# Configuration
api_url = "http://localhost:5000/api/products/import-excel"
excel_file_path = "DanhSachSanPham_KV17102025-132919-053.xlsx"

# Check if file exists
if not os.path.exists(excel_file_path):
    print(f"Error: Excel file not found at {excel_file_path}")
    exit(1)

print(f"Importing products from: {excel_file_path}")
print(f"API endpoint: {api_url}")
print("-" * 50)

# Open and send the file
with open(excel_file_path, 'rb') as f:
    files = {'file': (excel_file_path, f, 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet')}

    try:
        response = requests.post(api_url, files=files)

        print(f"Status Code: {response.status_code}")
        print("-" * 50)

        if response.status_code == 200:
            result = response.json()
            print("Import Result:")
            print(f"  Success: {result.get('success', False)}")
            print(f"  Imported: {result.get('imported', 0)}")
            print(f"  Updated: {result.get('updated', 0)}")
            print(f"  Skipped: {result.get('skipped', 0)}")
            print(f"  Summary: {result.get('summary', 'N/A')}")

            if result.get('errors'):
                print(f"\nErrors ({len(result['errors'])}):")
                for error in result['errors'][:10]:  # Show first 10 errors
                    print(f"  - {error}")
                if len(result['errors']) > 10:
                    print(f"  ... and {len(result['errors']) - 10} more errors")
        else:
            print(f"Error: {response.text}")

    except requests.exceptions.ConnectionError:
        print("Error: Could not connect to the API. Make sure the backend server is running.")
    except Exception as e:
        print(f"Error: {str(e)}")
