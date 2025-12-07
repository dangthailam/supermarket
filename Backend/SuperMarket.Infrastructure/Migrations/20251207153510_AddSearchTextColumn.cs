using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchTextColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable unaccent extension for PostgreSQL
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");

            // Create an immutable wrapper function for unaccent
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION immutable_unaccent(text)
                RETURNS text
                LANGUAGE sql
                IMMUTABLE PARALLEL SAFE STRICT
                AS $$
                    SELECT unaccent($1);
                $$;
            ");

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                table: "Products",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(immutable_unaccent(\"Name\" || ' ' || \"SKU\"))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SearchText",
                table: "Products",
                column: "SearchText");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_SearchText",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SearchText",
                table: "Products");

            // Drop the immutable wrapper function
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS immutable_unaccent(text);");
        }
    }
}
