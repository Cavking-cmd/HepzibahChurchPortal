using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchPortal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInventory2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Custodian",
                table: "InventoryItems",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "EquipmentSerialNumber",
                table: "InventoryItems",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 2, 45, 56, 661, DateTimeKind.Utc).AddTicks(2773));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 2, 45, 56, 661, DateTimeKind.Utc).AddTicks(2779));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 2, 45, 56, 661, DateTimeKind.Utc).AddTicks(2781));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 2, 45, 56, 661, DateTimeKind.Utc).AddTicks(2782));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 2, 45, 56, 661, DateTimeKind.Utc).AddTicks(2783));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 2, 45, 56, 761, DateTimeKind.Utc).AddTicks(6781));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 9, 7, 2, 45, 56, 761, DateTimeKind.Utc).AddTicks(6674), "$2a$11$r.HVSa.VOI718qutxTCYKu5HfqPjLI4IJlD/MboldjyKIdo1stUh6" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EquipmentSerialNumber",
                table: "InventoryItems");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "InventoryItems",
                newName: "Custodian");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 2, 16, 7, 414, DateTimeKind.Utc).AddTicks(8240));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 2, 16, 7, 414, DateTimeKind.Utc).AddTicks(8244));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 2, 16, 7, 414, DateTimeKind.Utc).AddTicks(8246));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 2, 16, 7, 414, DateTimeKind.Utc).AddTicks(8252));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 2, 16, 7, 414, DateTimeKind.Utc).AddTicks(8254));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 2, 16, 7, 516, DateTimeKind.Utc).AddTicks(358));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 9, 7, 2, 16, 7, 516, DateTimeKind.Utc).AddTicks(221), "$2a$11$L8uUZGs2nVt6S0FBTwvDgumFhIVJrihH7OQY05QG1N7RRQZzuOhyq" });
        }
    }
}
