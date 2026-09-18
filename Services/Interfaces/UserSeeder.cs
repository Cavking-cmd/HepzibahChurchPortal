namespace ChurchPortal.Services.Interfaces;
using ChurchPortal.DataContext;
using global::ChurchPortal.Core.Entities;
using Microsoft.EntityFrameworkCore;

public static class UserSeeder
{
    private sealed record SeedUser(string Email, string Password, string Role, string DisplayName);

    private static readonly SeedUser[] RoleUsers =
    {
        new("attendance.officer@churchportal.local", "Attendance@123", "AttendanceOfficer", "Attendance Officer"),
        new("fellowship.leader@churchportal.local", "Fellowship@123", "FellowshipLeader", "Fellowship Leader"),
        new("inventory.officer@churchportal.local", "Inventory@123", "InventoryOfficer", "Inventory Officer"),
        new("viewer@churchportal.local", "Viewer@123", "Viewer", "Viewer")
    };

    public static async Task SeedAsync(ChurchPortalDbContext context)
    {
        var roles = await context.Roles.ToListAsync();

        foreach (var seed in RoleUsers)
        {
            var role = roles.FirstOrDefault(r => r.Name == seed.Role);
            if (role == null)
            {
                continue;
            }

            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == seed.Email && !u.IsDeleted);
            if (user == null)
            {
                user = new User
                {
                    Email = seed.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(seed.Password),
                    DisplayName = seed.DisplayName
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            var link = await context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id && !ur.IsDeleted);
            if (link == null)
            {
                context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            }
        }

        await context.SaveChangesAsync();
    }
}