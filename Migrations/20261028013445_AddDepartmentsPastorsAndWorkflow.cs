using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentsPastorsAndWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReimbursementHistories_ReimbursementRequestId",
                table: "ReimbursementHistories");

            migrationBuilder.AddColumn<bool>(
                name: "IsSelfRegistered",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PastorId",
                table: "Services",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "ReimbursementRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedAmount",
                table: "ReimbursementRequests",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Classification",
                table: "ReimbursementRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "ReimbursementRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OverBudgetAtApproval",
                table: "ReimbursementRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OverBudgetJustification",
                table: "ReimbursementRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentConfirmedAt",
                table: "ReimbursementRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedBy",
                table: "ReimbursementRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedDate",
                table: "ReimbursementRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "ReimbursementRequests",
                type: "text",
                nullable: true);

            // "xmin" is a built-in PostgreSQL system column used as the row version.
            // It already exists on every table, so it must not be added here.

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AnnualBudget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MonthlyBudget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.CheckConstraint("CK_Departments_AnnualBudget_NonNegative", "\"AnnualBudget\" >= 0");
                    table.CheckConstraint("CK_Departments_MonthlyBudget_NonNegative", "\"MonthlyBudget\" IS NULL OR \"MonthlyBudget\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Pastors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pastors", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 28, 1, 34, 44, 871, DateTimeKind.Utc).AddTicks(7889));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 28, 1, 34, 44, 871, DateTimeKind.Utc).AddTicks(7892));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 28, 1, 34, 44, 871, DateTimeKind.Utc).AddTicks(7894));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 28, 1, 34, 44, 871, DateTimeKind.Utc).AddTicks(7895));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 28, 1, 34, 44, 871, DateTimeKind.Utc).AddTicks(7896));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 28, 1, 34, 44, 871, DateTimeKind.Utc).AddTicks(7897));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedDate", "Description" },
                values: new object[] { new DateTime(2026, 10, 28, 1, 34, 44, 871, DateTimeKind.Utc).AddTicks(7898), "Reviews reimbursement requests and forwards them for final approval" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedDate", "Description" },
                values: new object[] { new DateTime(2026, 10, 28, 1, 34, 44, 871, DateTimeKind.Utc).AddTicks(7903), "Full access to all church records and Accounts Payable, including final approval and payment confirmation" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedDate", "Description" },
                values: new object[] { new DateTime(2026, 10, 28, 1, 34, 44, 871, DateTimeKind.Utc).AddTicks(7904), "Retired. Pastors are now name records used on services, not a login role." });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 28, 1, 34, 45, 61, DateTimeKind.Utc).AddTicks(4904));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                // Password deliberately left out so the admin's current password is not reset.
                columns: new[] { "CreatedDate", "IsSelfRegistered" },
                values: new object[] { new DateTime(2026, 10, 28, 1, 34, 45, 61, DateTimeKind.Utc).AddTicks(4826), false });

            migrationBuilder.CreateIndex(
                name: "IX_Services_PastorId",
                table: "Services",
                column: "PastorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementRequests_DepartmentId",
                table: "ReimbursementRequests",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementRequests_PaymentDate",
                table: "ReimbursementRequests",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementRequests_Status",
                table: "ReimbursementRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementRequests_SubmittedDate",
                table: "ReimbursementRequests",
                column: "SubmittedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementHistories_ReimbursementRequestId_Timestamp",
                table: "ReimbursementHistories",
                columns: new[] { "ReimbursementRequestId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_NormalizedName",
                table: "Departments",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pastors_NormalizedName",
                table: "Pastors",
                column: "NormalizedName");

            migrationBuilder.AddForeignKey(
                name: "FK_ReimbursementRequests_Departments_DepartmentId",
                table: "ReimbursementRequests",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Pastors_PastorId",
                table: "Services",
                column: "PastorId",
                principalTable: "Pastors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReimbursementRequests_Departments_DepartmentId",
                table: "ReimbursementRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Pastors_PastorId",
                table: "Services");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Pastors");

            migrationBuilder.DropIndex(
                name: "IX_Services_PastorId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_ReimbursementRequests_DepartmentId",
                table: "ReimbursementRequests");

            migrationBuilder.DropIndex(
                name: "IX_ReimbursementRequests_PaymentDate",
                table: "ReimbursementRequests");

            migrationBuilder.DropIndex(
                name: "IX_ReimbursementRequests_Status",
                table: "ReimbursementRequests");

            migrationBuilder.DropIndex(
                name: "IX_ReimbursementRequests_SubmittedDate",
                table: "ReimbursementRequests");

            migrationBuilder.DropIndex(
                name: "IX_ReimbursementHistories_ReimbursementRequestId_Timestamp",
                table: "ReimbursementHistories");

            migrationBuilder.DropColumn(
                name: "IsSelfRegistered",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PastorId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "ReimbursementRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedAmount",
                table: "ReimbursementRequests");

            migrationBuilder.DropColumn(
                name: "Classification",
                table: "ReimbursementRequests");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "ReimbursementRequests");

            migrationBuilder.DropColumn(
                name: "OverBudgetAtApproval",
                table: "ReimbursementRequests");

            migrationBuilder.DropColumn(
                name: "OverBudgetJustification",
                table: "ReimbursementRequests");

            migrationBuilder.DropColumn(
                name: "PaymentConfirmedAt",
                table: "ReimbursementRequests");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "ReimbursementRequests");

            migrationBuilder.DropColumn(
                name: "RejectedDate",
                table: "ReimbursementRequests");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "ReimbursementRequests");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 23, 5, 22, 9, 953, DateTimeKind.Utc).AddTicks(6584));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 23, 5, 22, 9, 953, DateTimeKind.Utc).AddTicks(6587));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 23, 5, 22, 9, 953, DateTimeKind.Utc).AddTicks(6588));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 23, 5, 22, 9, 953, DateTimeKind.Utc).AddTicks(6596));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 23, 5, 22, 9, 953, DateTimeKind.Utc).AddTicks(6597));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 23, 5, 22, 9, 953, DateTimeKind.Utc).AddTicks(6598));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedDate", "Description" },
                values: new object[] { new DateTime(2026, 10, 23, 5, 22, 9, 953, DateTimeKind.Utc).AddTicks(6599), "Reconciles reimbursement requests and processes payments" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedDate", "Description" },
                values: new object[] { new DateTime(2026, 10, 23, 5, 22, 9, 953, DateTimeKind.Utc).AddTicks(6600), "Full access to all church records and Accounts Payable, including approvals" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedDate", "Description" },
                values: new object[] { new DateTime(2026, 10, 23, 5, 22, 9, 953, DateTimeKind.Utc).AddTicks(6601), "Approves reimbursements as Minister in charge and oversees all records" });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 23, 5, 22, 10, 118, DateTimeKind.Utc).AddTicks(6654));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedDate",
                value: new DateTime(2026, 10, 23, 5, 22, 10, 118, DateTimeKind.Utc).AddTicks(6449));

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementHistories_ReimbursementRequestId",
                table: "ReimbursementHistories",
                column: "ReimbursementRequestId");
        }
    }
}
