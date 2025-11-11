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
            // ⭐ שינויים לטבלת Forms (כפי שהיו בקוד המקורי שלך) ⭐
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

            // ⭐ שינויים לטבלת Children (הוספת שם פרטי ושם משפחה) ⭐
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Children",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: ""); // יש צורך ב-defaultValue כי השדה הוא IsRequired

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Children",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: ""); // יש צורך ב-defaultValue כי השדה הוא IsRequired

            // ⭐ שינויים לטבלת Kindergartens (הוספת כתובת הגן) ⭐
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Kindergartens",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: ""); // יש צורך ב-defaultValue כי השדה הוא IsRequired
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 🛑 שינויים לטבלת Forms (פעולות הפוכות) 🛑
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "Forms");

            migrationBuilder.AddColumn<string>(
                name: "FormLink",
                table: "Forms",
                type: "nvarchar(max)",
                nullable: true);

            // 🛑 שינויים לטבלת Children (מחיקת שם פרטי ושם משפחה) 🛑
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Children");

            // 🛑 שינויים לטבלת Kindergartens (מחיקת כתובת הגן) 🛑
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Kindergartens");

            // הערה: נשמרו פעולות מחיקת הטבלאות המקוריות שלך מכיוון שאיני יודע אם הן נדרשות אוטומטית.
            // אם הטבלאות הללו כבר קיימות בבסיס הנתונים, יש למחוק את הפקודות הבאות:

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