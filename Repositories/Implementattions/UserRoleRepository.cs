using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;

namespace ChurchPortal.Repositories.Implementattions
{
    public class UserRoleRepository : BaseRepository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(ChurchPortalDbContext context) : base(context)
        {
        }
    }
}
