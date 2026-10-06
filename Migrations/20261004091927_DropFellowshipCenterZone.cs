using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchPortal.Migrations
{
    /// <inheritdoc />
    public partial class DropFellowshipCenterZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Zone",
                table: "FellowshipCenters");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 4, 9, 19, 27, 207, DateTimeKind.Utc).AddTicks(2804));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 4, 9, 19, 27, 207, DateTimeKind.Utc).AddTicks(2810));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 4, 9, 19, 27, 207, DateTimeKind.Utc).AddTicks(2819));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 4, 9, 19, 27, 207, DateTimeKind.Utc).AddTicks(2820));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 4, 9, 19, 27, 207, DateTimeKind.Utc).AddTicks(2821));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 4, 9, 19, 27, 373, DateTimeKind.Utc).AddTicks(476));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 10, 4, 9, 19, 27, 373, DateTimeKind.Utc).AddTicks(384), "$2a$11$BSWyfkHuKQsTohEtyVHHnO9YmbGfR2tablrE0Wsm1OBs0nfaZtYuq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Zone",
                table: "FellowshipCenters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 19, 22, 16, 33, 383, DateTimeKind.Utc).AddTicks(1207));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 19, 22, 16, 33, 383, DateTimeKind.Utc).AddTicks(1214));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 19, 22, 16, 33, 383, DateTimeKind.Utc).AddTicks(1215));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 19, 22, 16, 33, 383, DateTimeKind.Utc).AddTicks(1217));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 19, 22, 16, 33, 383, DateTimeKind.Utc).AddTicks(1218));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedDate",
                value: new DateTime(2026, 9, 19, 22, 16, 33, 544, DateTimeKind.Utc).AddTicks(43));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 9, 19, 22, 16, 33, 543, DateTimeKind.Utc).AddTicks(9711), "$2a$11$OXQkVyw.1hvzpuZMOe4QW.0gCzJYTAk0Z.g91LQsLDYgcyBYQbcfS" });
        }
    }
}
