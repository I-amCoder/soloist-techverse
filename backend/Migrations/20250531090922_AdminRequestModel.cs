using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class AdminRequestModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    RequesterId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimantId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolvedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminRequests_AspNetUsers_ClaimantId",
                        column: x => x.ClaimantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdminRequests_AspNetUsers_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdminRequests_AspNetUsers_ResolvedById",
                        column: x => x.ResolvedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdminRequests_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 31, 9, 9, 21, 841, DateTimeKind.Utc).AddTicks(8441));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 31, 9, 9, 21, 841, DateTimeKind.Utc).AddTicks(8499));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 31, 9, 9, 21, 841, DateTimeKind.Utc).AddTicks(8511));

            migrationBuilder.CreateIndex(
                name: "IX_AdminRequests_ClaimantId",
                table: "AdminRequests",
                column: "ClaimantId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminRequests_ItemId",
                table: "AdminRequests",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminRequests_RequesterId",
                table: "AdminRequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminRequests_ResolvedById",
                table: "AdminRequests",
                column: "ResolvedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminRequests");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 31, 7, 25, 55, 530, DateTimeKind.Utc).AddTicks(2692));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 31, 7, 25, 55, 530, DateTimeKind.Utc).AddTicks(2762));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 31, 7, 25, 55, 530, DateTimeKind.Utc).AddTicks(2765));
        }
    }
}
