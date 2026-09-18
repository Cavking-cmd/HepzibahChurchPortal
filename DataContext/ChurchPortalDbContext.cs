using ChurchPortal.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChurchPortal.DataContext
{
    public class ChurchPortalDbContext : DbContext
    {
        public ChurchPortalDbContext(DbContextOptions<ChurchPortalDbContext> options) : base(options)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<Service> Services { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<FellowshipCenter> FellowshipCenters { get; set; }
        public DbSet<FellowshipAttendance> FellowshipAttendances { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<UserCredential> UserCredentials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserCredential>().HasIndex(c => c.CredentialId).IsUnique();

            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var attendanceOfficerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var fellowshipLeaderRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var inventoryOfficerRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var viewerRoleId = Guid.Parse("55555555-5555-5555-5555-555555555555");

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = adminRoleId, Name = "Admin", Description = "Full access, including approvals" },
                new Role { Id = attendanceOfficerRoleId, Name = "AttendanceOfficer", Description = "Manages Services and Attendance records" },
                new Role { Id = fellowshipLeaderRoleId, Name = "FellowshipLeader", Description = "Manages FellowshipCenters and FellowshipAttendance records" },
                new Role { Id = inventoryOfficerRoleId, Name = "InventoryOfficer", Description = "Manages InventoryItem records" },
                new Role { Id = viewerRoleId, Name = "Viewer", Description = "Read-only access" }
            );

            var adminUserId = Guid.Parse("99999999-9999-9999-9999-999999999999");
            var adminUserRoleId = Guid.Parse("88888888-8888-8888-8888-888888888888");

            var adminPassword = BCrypt.Net.BCrypt.HashPassword("Admin@123");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = adminUserId,
                    Email = "admin@churchportal.local",
                    Password = adminPassword
                }
            );

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole
                {
                    Id = adminUserRoleId,
                    UserId = adminUserId,
                    RoleId = adminRoleId
                }
            );
        }
    }
}
