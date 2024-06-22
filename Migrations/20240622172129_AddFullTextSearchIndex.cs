using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeProfileManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearchIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
            CREATE EXTENSION IF NOT EXISTS unaccent;
            CREATE INDEX employee_name_full_text_search_idx ON ""Employees"" USING GIN (to_tsvector('simple', ""Name""));
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
            DROP INDEX IF EXISTS employee_name_full_text_search_idx;
            DROP EXTENSION IF EXISTS unaccent;
            ");
        }
    }
}
