using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dal.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    customer_id = table.Column<int>(type: "int", nullable: true),
                    first_name = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    last_name = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Kindergartens",
                columns: table => new
                {
                    KindergartenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Kinderga__2B06D7BC1EB3F444", x => x.KindergartenId);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "int", nullable: true),
                    order_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    amount = table.Column<int>(type: "int", nullable: true),
                    customer_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChildId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    PaymentDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Payments__9B556A389869C495", x => x.PaymentId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CC4C0814D012", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Children",
                columns: table => new
                {
                    ChildId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KindergartenId = table.Column<int>(type: "int", nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchoolYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Children__BEFA0716585D1C92", x => x.ChildId);
                    table.ForeignKey(
                        name: "FK_Children_Payments",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId");
                    table.ForeignKey(
                        name: "FK__Children__Kinder__52593CB8",
                        column: x => x.KindergartenId,
                        principalTable: "Kindergartens",
                        principalColumn: "KindergartenId");
                });

            migrationBuilder.CreateTable(
                name: "Forms",
                columns: table => new
                {
                    FormId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChildId = table.Column<int>(type: "int", nullable: false),
                    FormType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    AttachmentPaths = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Forms__FB05B7DD3B2B1448", x => x.FormId);
                    table.ForeignKey(
                        name: "FK__Forms__ChildId__59063A47",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "ChildId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Children_KindergartenId",
                table: "Children",
                column: "KindergartenId");

            migrationBuilder.CreateIndex(
                name: "IX_Children_PaymentId",
                table: "Children",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "UQ__Children__62DF80332B6EC341",
                table: "Children",
                column: "IdNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Forms_ChildId",
                table: "Forms",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "UQ__Kinderga__A25C5AA7B71BED89",
                table: "Kindergartens",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Users__536C85E4E7221715",
                table: "Users",
                column: "Username",
                unique: true);
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
