using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchPortal.Migrations
{
    /// <inheritdoc />
    public partial class MoveOnlineAttendanceToAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Attendances\" ADD COLUMN IF NOT EXISTS \"OnlineAttendance\" integer NOT NULL DEFAULT 0;");

            migrationBuilder.Sql(
                "UPDATE \"Attendances\" a " +
                "SET \"OnlineAttendance\" = s.\"OnlineAttendance\", " +
                "    \"Total\" = a.\"Men\" + a.\"Women\" + a.\"Children\" + s.\"OnlineAttendance\" " +
                "FROM \"Services\" s " +
                "WHERE a.\"ServiceId\" = s.\"Id\";");

            migrationBuilder.DropColumn(
                name: "OnlineAttendance",
                table: "Services");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OnlineAttendance",
                table: "Services",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE \"Services\" s " +
                "SET \"OnlineAttendance\" = COALESCE((" +
                "    SELECT MAX(a.\"OnlineAttendance\") FROM \"Attendances\" a " +
                "    WHERE a.\"ServiceId\" = s.\"Id\" AND NOT a.\"IsDeleted\"), 0);");

            migrationBuilder.Sql(
                "UPDATE \"Attendances\" " +
                "SET \"Total\" = \"Men\" + \"Women\" + \"Children\";");

            migrationBuilder.DropColumn(
                name: "OnlineAttendance",
                table: "Attendances");
        }
    }
}
