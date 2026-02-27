using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSystemSettingsConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "SystemSettings",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "Description", "Value" },
                values: new object[] { "Commission Rate (Decimal: 0.05 = 5%)", "0.05" });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222223"),
                column: "Value",
                value: "Pending");

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "DataType", "Description", "GroupName", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111115"), 0, "Webhook signature secret", "Payment", "Payment_WebhookSecret", null, "" },
                    { new Guid("44444444-4444-4444-4444-444444444443"), 1, "Hold funds before payout (days)", "Finance", "Finance_PayoutDelayDays", null, "7" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Key",
                table: "SystemSettings",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemSettings_Key",
                table: "SystemSettings");

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111115"));

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444443"));

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "Description", "Value" },
                values: new object[] { "Commission Rate (%)", "5" });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222223"),
                column: "Value",
                value: "Active");
        }
    }
}
