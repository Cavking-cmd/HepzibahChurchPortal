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
        public DbSet<ReimbursementRequest> ReimbursementRequests { get; set; }
        public DbSet<ReimbursementItem> ReimbursementItems { get; set; }
        public DbSet<ReimbursementReceipt> ReimbursementReceipts { get; set; }
        public DbSet<ReimbursementHistory> ReimbursementHistories { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserCredential>().HasIndex(c => c.CredentialId).IsUnique();

            modelBuilder.Entity<ReimbursementRequest>(entity =>
            {
                entity.HasIndex(r => r.RequestNumber).IsUnique();
                entity.HasIndex(r => r.PayeeUserId);
                entity.Property(r => r.Subtotal).HasPrecision(18, 2);
                entity.Property(r => r.Gst).HasPrecision(18, 2);
                entity.Property(r => r.Pst).HasPrecision(18, 2);
                entity.Property(r => r.Hst).HasPrecision(18, 2);
                entity.Property(r => r.TotalAmount).HasPrecision(18, 2);
                entity.Property(r => r.AmountPaid).HasPrecision(18, 2);
                entity.HasMany(r => r.Items).WithOne(i => i.ReimbursementRequest).HasForeignKey(i => i.ReimbursementRequestId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(r => r.Receipts).WithOne(x => x.ReimbursementRequest).HasForeignKey(x => x.ReimbursementRequestId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(r => r.History).WithOne(h => h.ReimbursementRequest).HasForeignKey(h => h.ReimbursementRequestId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ReimbursementItem>(entity =>
            {
                entity.Property(i => i.Subtotal).HasPrecision(18, 2);
                entity.Property(i => i.Gst).HasPrecision(18, 2);
                entity.Property(i => i.Pst).HasPrecision(18, 2);
                entity.Property(i => i.Hst).HasPrecision(18, 2);
                entity.Property(i => i.Total).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasIndex(n => n.UserId);
            });

            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var attendanceOfficerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var fellowshipLeaderRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var inventoryOfficerRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var viewerRoleId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var churchMemberRoleId = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var accountsOfficerRoleId = Guid.Parse("77777777-7777-7777-7777-777777777777");
            var accountsAdminRoleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var pastorRoleId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = adminRoleId, Name = "Admin", Description = "Full access to church records, excluding Accounts Payable" },
                new Role { Id = attendanceOfficerRoleId, Name = "AttendanceOfficer", Description = "Manages Services and Attendance records" },
                new Role { Id = fellowshipLeaderRoleId, Name = "FellowshipLeader", Description = "Manages FellowshipCenters and FellowshipAttendance records" },
                new Role { Id = inventoryOfficerRoleId, Name = "InventoryOfficer", Description = "Manages InventoryItem records" },
                new Role { Id = viewerRoleId, Name = "Viewer", Description = "Read-only access (retired)" },
                new Role { Id = churchMemberRoleId, Name = "ChurchMember", Description = "Submits and tracks personal reimbursement requests" },
                new Role { Id = accountsOfficerRoleId, Name = "AccountsOfficer", Description = "Reconciles reimbursement requests and processes payments" },
                new Role { Id = accountsAdminRoleId, Name = "AccountsAdmin", Description = "Full access to all church records and Accounts Payable, including approvals" },
                new Role { Id = pastorRoleId, Name = "Pastor", Description = "Approves reimbursements as Minister in charge and oversees all records" }
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
