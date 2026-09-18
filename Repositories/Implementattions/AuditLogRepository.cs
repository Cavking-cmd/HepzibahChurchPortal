using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;

namespace ChurchPortal.Repositories.Implementattions
{
    public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ChurchPortalDbContext context) : base(context)
        {
        }
    }
}
