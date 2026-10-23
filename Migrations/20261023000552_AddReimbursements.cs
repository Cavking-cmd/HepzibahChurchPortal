using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChurchPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddReimbursements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: true),
                    ReimbursementRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReimbursementRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "text", nullable: false),
                    PayeeUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayeeName = table.Column<string>(type: "text", nullable: false),
                    Department = table.Column<string>(type: "text", nullable: false),
                    MinisterInCharge = table.Column<string>(type: "text", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Gst = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Pst = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Hst = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MinisterApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    MinisterApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidBy = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AmountPaid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PaymentMethod = table.Column<string>(type: "text", nullable: true),
                    PaymentReference = table.Column<string>(type: "text", nullable: true),
                    ChequeNumber = table.Column<string>(type: "text", nullable: true),
                    AuthorizedByName1 = table.Column<string>(type: "text", nullable: true),
                    AuthorizedByName2 = table.Column<string>(type: "text", nullable: true),
                    AuthorizationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinanceNotes = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    InfoRequestNotes = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReimbursementRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReimbursementHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReimbursementRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorName = table.Column<string>(type: "text", nullable: true),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReimbursementHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReimbursementHistories_ReimbursementRequests_ReimbursementR~",
                        column: x => x.ReimbursementRequestId,
                        principalTable: "ReimbursementRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReimbursementItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReimbursementRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNumber = table.Column<int>(type: "integer", nullable: false),
                    ItemName = table.Column<string>(type: "text", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Gst = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Pst = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Hst = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RequiresSerialNumber = table.Column<bool>(type: "boolean", nullable: false),
                    SerialNumber = table.Column<string>(type: "text", nullable: true),
                    ScreeningNote = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReimbursementItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReimbursementItems_ReimbursementRequests_ReimbursementReque~",
                        column: x => x.ReimbursementRequestId,
                        principalTable: "ReimbursementRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReimbursementReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReimbursementRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileData = table.Column<byte[]>(type: "bytea", nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReimbursementReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReimbursementReceipts_ReimbursementRequests_ReimbursementRe~",
                        column: x => x.ReimbursementRequestId,
                        principalTable: "ReimbursementRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedDate", "Description" },
                values: new object[] { new DateTime(2026, 10, 23, 0, 5, 52, 395, DateTimeKind.Utc).AddTicks(86), "Full access to church records, excluding Accounts Payable" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 23, 0, 5, 52, 395, DateTimeKind.Utc).AddTicks(90));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 23, 0, 5, 52, 395, DateTimeKind.Utc).AddTicks(91));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 23, 0, 5, 52, 395, DateTimeKind.Utc).AddTicks(92));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedDate", "Description" },
                values: new object[] { new DateTime(2026, 10, 23, 0, 5, 52, 395, DateTimeKind.Utc).AddTicks(97), "Read-only access (retired)" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedDate", "Description", "IsDeleted", "Name" },
                values: new object[,]
                {
                    { new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2026, 10, 23, 0, 5, 52, 395, DateTimeKind.Utc).AddTicks(98), "Submits and tracks personal reimbursement requests", false, "ChurchMember" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), new DateTime(2026, 10, 23, 0, 5, 52, 395, DateTimeKind.Utc).AddTicks(99), "Reconciles reimbursement requests and processes payments", false, "AccountsOfficer" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 10, 23, 0, 5, 52, 395, DateTimeKind.Utc).AddTicks(100), "Full access to all church records and Accounts Payable, including approvals", false, "AccountsAdmin" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new DateTime(2026, 10, 23, 0, 5, 52, 395, DateTimeKind.Utc).AddTicks(101), "Approves reimbursements as Minister in charge and oversees all records", false, "Pastor" }
                });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 23, 0, 5, 52, 495, DateTimeKind.Utc).AddTicks(3838));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 10, 23, 0, 5, 52, 495, DateTimeKind.Utc).AddTicks(3749), "$2a$11$IA/Kf/1UlwrH6FBqZpOjTej9h8yD4JhKhMLL99lEtDDqHvOvF0S36" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementHistories_ReimbursementRequestId",
                table: "ReimbursementHistories",
                column: "ReimbursementRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementItems_ReimbursementRequestId",
                table: "ReimbursementItems",
                column: "ReimbursementRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementReceipts_ReimbursementRequestId",
                table: "ReimbursementReceipts",
                column: "ReimbursementRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementRequests_PayeeUserId",
                table: "ReimbursementRequests",
                column: "PayeeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementRequests_RequestNumber",
                table: "ReimbursementRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.Sql("""
                UPDATE "Roles" SET "IsDeleted" = TRUE WHERE "Name" = 'Viewer';
                UPDATE "UserRoles" SET "IsDeleted" = TRUE WHERE "RoleId" = '55555555-5555-5555-5555-555555555555';
                UPDATE "Users" SET "IsDeleted" = TRUE WHERE "Email" = 'viewer@churchportal.local';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ReimbursementHistories");

            migrationBuilder.DropTable(
                name: "ReimbursementItems");

            migrationBuilder.DropTable(
                name: "ReimbursementReceipts");

            migrationBuilder.DropTable(
                name: "ReimbursementRequests");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedDate", "Description" },
                values: new object[] { new DateTime(2026, 10, 11, 12, 27, 34, 549, DateTimeKind.Utc).AddTicks(6586), "Full access, including approvals" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 12, 27, 34, 549, DateTimeKind.Utc).AddTicks(6590));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 12, 27, 34, 549, DateTimeKind.Utc).AddTicks(6615));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 12, 27, 34, 549, DateTimeKind.Utc).AddTicks(6617));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedDate", "Description" },
                values: new object[] { new DateTime(2026, 10, 11, 12, 27, 34, 549, DateTimeKind.Utc).AddTicks(6625), "Read-only access" });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 12, 27, 34, 698, DateTimeKind.Utc).AddTicks(4867));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 10, 11, 12, 27, 34, 698, DateTimeKind.Utc).AddTicks(4770), "$2a$11$n9VjXrFgYTV1auxxnx2nuOQv.AwnW/yCV5rT9FhOC68XIrH8Hhw3i" });
        }
    }
}
