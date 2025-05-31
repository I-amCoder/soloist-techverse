using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClaimDescriptionToHandoverNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "AdminRequests",
                newName: "HandoverNotes");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 31, 9, 30, 12, 400, DateTimeKind.Utc).AddTicks(5888));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 31, 9, 30, 12, 400, DateTimeKind.Utc).AddTicks(5950));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 31, 9, 30, 12, 400, DateTimeKind.Utc).AddTicks(5953));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HandoverNotes",
                table: "AdminRequests",
                newName: "Description");

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
        }
    }
}
