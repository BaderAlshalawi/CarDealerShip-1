using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CarDealerShip.Migrations
{
    /// <inheritdoc />
    public partial class intialmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Make = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CarId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purchases_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Purchases_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "IsAvailable", "Make", "Model", "Price", "Year" },
                values: new object[,]
                {
                    { new Guid("30dac83d-022f-42a1-adec-04df1db61848"), true, "BMW", "3 Series", 35000m, 2022 },
                    { new Guid("5c2d89fd-0e76-4e30-91ee-6c41988c0c9d"), true, "Chevrolet", "Malibu", 16000m, 2019 },
                    { new Guid("7cae4529-4bed-47fa-80f3-b49e8970991e"), true, "Ford", "Mustang", 30000m, 2021 },
                    { new Guid("a23bd5b3-2981-4ce5-a97d-616c91dc0c02"), true, "Nissan", "Altima", 18000m, 2020 },
                    { new Guid("bba7ccb4-871f-4b79-8f87-cfd1b6e4fc4a"), true, "Honda", "Civic", 14000m, 2019 },
                    { new Guid("bd27a697-f06e-4cda-a0ec-8bc675cbf07c"), true, "Toyota", "Camry", 20000m, 2021 },
                    { new Guid("d151cfdd-51b7-4715-a45a-4070837e6142"), true, "Ford", "Focus", 12000m, 2018 },
                    { new Guid("d884b695-b973-405b-9299-b0b3450405ae"), true, "Honda", "Accord", 25000m, 2022 },
                    { new Guid("d914343b-5c88-417e-8112-0dc9e2beea31"), true, "Toyota", "Corolla", 15000m, 2020 },
                    { new Guid("f52880fb-a538-4a2b-816f-eebd59b0ac12"), true, "Mercedes", "C-Class", 40000m, 2022 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "PasswordHash", "Role" },
                values: new object[] { "4574785d-904d-48af-9740-5f23585faaf5", "admin@local", "Admin#12345", "Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_CarId",
                table: "Purchases",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_CustomerId",
                table: "Purchases",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "Cars");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
