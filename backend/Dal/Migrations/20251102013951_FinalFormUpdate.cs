using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dal.Migrations
{
    /// <inheritdoc />
    public partial class FinalFormUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🛑 מחק את כל מה שנוצר אוטומטית עד לשורה שמטפלת ב-Forms.

            // ⭐️ זהו הקוד היחיד שצריך להישאר (שינוי טבלת Forms): ⭐️
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Forms",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileContent",
                table: "Forms",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "FormLink",
                table: "Forms");

            // ⭐️ וודא שאין קוד נוסף מתחת לזה (למעט סגירת הבלוקים). ⭐️
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "Forms");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Children");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Kindergartens");
        }
    }
}
