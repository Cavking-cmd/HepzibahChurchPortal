using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;

namespace ChurchPortal.Repositories.Implementattions
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ChurchPortalDbContext _context;
        public UnitOfWork(ChurchPortalDbContext context)
        {
            _context = context;
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
