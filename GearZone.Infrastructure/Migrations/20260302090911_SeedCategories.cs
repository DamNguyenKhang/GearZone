using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "IsActive", "Name", "ParentId", "Slug" },
                values: new object[,]
                {
                    { 1, true, "Keyboards", null, "keyboards" },
                    { 2, true, "Mice", null, "mice" },
                    { 3, true, "Headsets", null, "headsets" },
                    { 4, true, "Monitors", null, "monitors" },
                    { 5, true, "PC Components", null, "pc-components" },
                    { 6, true, "Gaming Furniture", null, "gaming-furniture" },
                    { 7, true, "Setup Accessories", null, "setup-accessories" },
                    { 8, true, "Console & Controllers", null, "console-controllers" },
                    { 11, true, "Mechanical Keyboards", 1, "mechanical-keyboards" },
                    { 12, true, "Membrane Keyboards", 1, "membrane-keyboards" },
                    { 13, true, "Keycaps", 1, "keycaps" },
                    { 14, true, "Keyboard Switches", 1, "keyboard-switches" },
                    { 21, true, "Gaming Mice", 2, "gaming-mice" },
                    { 22, true, "Office Mice", 2, "office-mice" },
                    { 23, true, "Mouse Pads", 2, "mouse-pads" },
                    { 31, true, "Gaming Headsets", 3, "gaming-headsets" },
                    { 32, true, "Wireless Headphones", 3, "wireless-headphones" },
                    { 33, true, "Microphones", 3, "microphones" },
                    { 41, true, "Gaming Monitors", 4, "gaming-monitors" },
                    { 42, true, "Office Monitors", 4, "office-monitors" },
                    { 43, true, "Curved Monitors", 4, "curved-monitors" },
                    { 51, true, "CPUs", 5, "cpus" },
                    { 52, true, "GPUs", 5, "gpus" },
                    { 53, true, "RAM", 5, "ram" },
                    { 54, true, "Motherboards", 5, "motherboards" },
                    { 55, true, "Storage (SSD/HDD)", 5, "storage" },
                    { 56, true, "Power Supplies", 5, "power-supplies" },
                    { 57, true, "PC Cases", 5, "pc-cases" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
