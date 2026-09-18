using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettingsAndPasskeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "AvatarFile",
                table: "Users",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarMimeType",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CredentialId = table.Column<byte[]>(type: "bytea", nullable: false),
                    PublicKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    SignatureCounter = table.Column<long>(type: "bigint", nullable: false),
                    Nickname = table.Column<string>(type: "text", nullable: true),
                    AaGuid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCredentials_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 20, 9, 57, 36, 179, DateTimeKind.Utc).AddTicks(4797));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 20, 9, 57, 36, 179, DateTimeKind.Utc).AddTicks(4809));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 20, 9, 57, 36, 179, DateTimeKind.Utc).AddTicks(4812));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 20, 9, 57, 36, 179, DateTimeKind.Utc).AddTicks(4815));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 20, 9, 57, 36, 179, DateTimeKind.Utc).AddTicks(4817));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 20, 9, 57, 36, 561, DateTimeKind.Utc).AddTicks(625));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "AvatarFile", "AvatarMimeType", "CreatedDate", "DisplayName", "Password" },
                values: new object[] { null, null, new DateTime(2026, 8, 20, 9, 57, 36, 561, DateTimeKind.Utc).AddTicks(381), null, "$2a$11$9x.gI2B8X3vxXL7pj0SJH.VqubHF4L6rLnP3fLUdNgUPsAfvQ1Ywa" });

            migrationBuilder.CreateIndex(
                name: "IX_UserCredentials_CredentialId",
                table: "UserCredentials",
                column: "CredentialId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCredentials_UserId",
                table: "UserCredentials",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCredentials");

            migrationBuilder.DropColumn(
                name: "AvatarFile",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarMimeType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 16, 22, 16, 47, 745, DateTimeKind.Utc).AddTicks(3208));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 16, 22, 16, 47, 745, DateTimeKind.Utc).AddTicks(3214));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 16, 22, 16, 47, 745, DateTimeKind.Utc).AddTicks(3217));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 16, 22, 16, 47, 745, DateTimeKind.Utc).AddTicks(3218));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 16, 22, 16, 47, 745, DateTimeKind.Utc).AddTicks(3232));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 16, 22, 16, 48, 130, DateTimeKind.Utc).AddTicks(4303));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 8, 16, 22, 16, 48, 130, DateTimeKind.Utc).AddTicks(4184), "$2a$11$Hh8ip3sUEl16WV0sTpl4A.3l1PXQXMhx53LC3BMwr/eE6Wg.FHtd." });
        }
    }
}
