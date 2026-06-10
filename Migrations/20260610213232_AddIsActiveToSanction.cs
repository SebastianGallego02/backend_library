using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_library.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToSanction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Sanctions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Sanctions");
        }
    }
}
