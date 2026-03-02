using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStoreUserAndAddStaffs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoreUsers");

            migrationBuilder.CreateTable(
                name: "StoreStaffs",
                columns: table => new
                {
                    StaffStoresId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffsId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreStaffs", x => new { x.StaffStoresId, x.StaffsId });
                    table.ForeignKey(
                        name: "FK_StoreStaffs_AspNetUsers_StaffsId",
                        column: x => x.StaffsId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoreStaffs_Stores_StaffStoresId",
                        column: x => x.StaffStoresId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111112"),
                column: "Value",
                value: "50000");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222221"),
                column: "Value",
                value: "true");

            migrationBuilder.CreateIndex(
                name: "IX_StoreStaffs_StaffsId",
                table: "StoreStaffs",
                column: "StaffsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoreStaffs");

            migrationBuilder.CreateTable(
                name: "StoreUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoreUsers_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111112"),
                column: "Value",
                value: "500000");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222221"),
                column: "Value",
                value: "false");

            migrationBuilder.CreateIndex(
                name: "IX_StoreUsers_StoreId",
                table: "StoreUsers",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreUsers_StoreId_UserId",
                table: "StoreUsers",
                columns: new[] { "StoreId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreUsers_UserId",
                table: "StoreUsers",
                column: "UserId");
        }
    }
}
