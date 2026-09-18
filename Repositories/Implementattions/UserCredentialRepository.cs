using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;

namespace ChurchPortal.Repositories.Implementattions
{
    public class UserCredentialRepository : BaseRepository<UserCredential>, IUserCredentialRepository
    {
        public UserCredentialRepository(ChurchPortalDbContext context) : base(context)
        {
        }
    }
}
