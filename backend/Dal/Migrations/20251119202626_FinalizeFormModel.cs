using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dal.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeFormModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "DiscountRequestLink",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "HealthDeclarationLink",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "UploadedDocumentPaths",
                table: "Children");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPaths",
                table: "Forms",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Forms",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormType",
                table: "Forms",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentPaths",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "FormType",
                table: "Forms");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileContent",
                table: "Forms",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountRequestLink",
                table: "Children",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HealthDeclarationLink",
                table: "Children",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UploadedDocumentPaths",
                table: "Children",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
