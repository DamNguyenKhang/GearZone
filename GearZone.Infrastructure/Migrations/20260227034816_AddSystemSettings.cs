using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataType = table.Column<int>(type: "int", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "DataType", "Description", "GroupName", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 1, "Commission Rate (%)", "Payment", "Payment_CommissionRate", null, "5" },
                    { new Guid("11111111-1111-1111-1111-111111111112"), 1, "Minimum Payout (VND)", "Payment", "Payment_MinimumPayout", null, "500000" },
                    { new Guid("11111111-1111-1111-1111-111111111113"), 2, "Allow credit cards & e-wallets", "Payment", "Payment_OnlinePayments", null, "true" },
                    { new Guid("11111111-1111-1111-1111-111111111114"), 2, "Allow payment upon receipt", "Payment", "Payment_CashOnDelivery", null, "true" },
                    { new Guid("22222222-2222-2222-2222-222222222221"), 2, "Manually approve new vendors", "Store", "Store_NewStoreApproval", null, "false" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 2, "Allow non-business entities to sell", "Store", "Store_IndividualSellers", null, "true" },
                    { new Guid("22222222-2222-2222-2222-222222222223"), 0, "Default Store Status (Active, Pending Review, Inactive)", "Store", "Store_DefaultStatus", null, "Active" },
                    { new Guid("33333333-3333-3333-3333-333333333331"), 1, "Auto Complete (Days)", "Order", "Order_AutoCompleteDays", null, "7" },
                    { new Guid("33333333-3333-3333-3333-333333333332"), 1, "Auto Cancel (Minutes)", "Order", "Order_AutoCancelMinutes", null, "30" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 2, "Allow buyers to cancel pending orders", "Order", "Order_BuyerCancellation", null, "true" },
                    { new Guid("44444444-4444-4444-4444-444444444441"), 2, "Vendors must request payouts manually", "Finance", "Finance_ManualWithdraw", null, "true" },
                    { new Guid("44444444-4444-4444-4444-444444444442"), 2, "Release funds only after order completion", "Finance", "Finance_HoldFunds", null, "true" },
                    { new Guid("55555555-5555-5555-5555-555555555551"), 2, "Require sellers to upload ID documents", "Security", "Security_KYCRequired", null, "false" },
                    { new Guid("55555555-5555-5555-5555-555555555552"), 2, "Mandatory tax code input", "Security", "Security_TaxCodeVerification", null, "true" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");
        }
    }
}
