using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchPortal.Migrations
{
    /// <inheritdoc />
    public partial class RenameEquipmentSerialNumberToSerialNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EquipmentSerialNumber",
                table: "InventoryItems",
                newName: "SerialNumber");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 4, 6, 15, 108, DateTimeKind.Utc).AddTicks(2940));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 4, 6, 15, 108, DateTimeKind.Utc).AddTicks(2953));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 4, 6, 15, 108, DateTimeKind.Utc).AddTicks(2954));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 4, 6, 15, 108, DateTimeKind.Utc).AddTicks(2955));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 4, 6, 15, 108, DateTimeKind.Utc).AddTicks(2956));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 4, 6, 15, 209, DateTimeKind.Utc).AddTicks(1941));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 9, 7, 4, 6, 15, 209, DateTimeKind.Utc).AddTicks(1821), "$2a$11$XCFxgCqckpj/BW3wM8d44.XWSTR65xmIckpbvrSZvtZKyXaUGj0Zm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SerialNumber",
                table: "InventoryItems",
                newName: "EquipmentSerialNumber");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 3, 26, 50, 534, DateTimeKind.Utc).AddTicks(6990));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 3, 26, 50, 534, DateTimeKind.Utc).AddTicks(6994));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 3, 26, 50, 534, DateTimeKind.Utc).AddTicks(6995));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 3, 26, 50, 534, DateTimeKind.Utc).AddTicks(7001));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 3, 26, 50, 534, DateTimeKind.Utc).AddTicks(7003));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 7, 3, 26, 50, 688, DateTimeKind.Utc).AddTicks(7399));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 9, 7, 3, 26, 50, 688, DateTimeKind.Utc).AddTicks(7315), "$2a$11$i7L4J6tuZiNRcNCXqeHa3usgGLdFEqSd1W.FGpBvTyCGyUowM3.N6" });
        }
    }
}
