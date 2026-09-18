using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddCustodianColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Custodian",
                table: "InventoryItems",
                type: "text",
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Custodian",
                table: "InventoryItems");

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
    }
}
