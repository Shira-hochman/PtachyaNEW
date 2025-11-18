using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dal.Migrations
{
    /// <inheritdoc />
    public partial class RenameFormLinkAndAddDiscountLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FormLink",
                table: "Children",
                newName: "HealthDeclarationLink");

            migrationBuilder.AddColumn<string>(
                name: "DiscountRequestLink",
                table: "Children",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountRequestLink",
                table: "Children");

            migrationBuilder.RenameColumn(
                name: "HealthDeclarationLink",
                table: "Children",
                newName: "FormLink");
        }
    }
}
