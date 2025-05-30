using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class AddEscalationFieldsToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminDecisionUserId",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EscalationDate",
                table: "Items",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EscalationReason",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEscalatedToAdmin",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 30, 14, 51, 43, 581, DateTimeKind.Utc).AddTicks(4115));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 30, 14, 51, 43, 581, DateTimeKind.Utc).AddTicks(4206));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 30, 14, 51, 43, 581, DateTimeKind.Utc).AddTicks(4308));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminDecisionUserId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "EscalationDate",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "EscalationReason",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "IsEscalatedToAdmin",
                table: "Items");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 30, 14, 12, 47, 39, DateTimeKind.Utc).AddTicks(399));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 30, 14, 12, 47, 39, DateTimeKind.Utc).AddTicks(469));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 30, 14, 12, 47, 39, DateTimeKind.Utc).AddTicks(472));
        }
    }
}
