using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;

namespace ChurchPortal.Repositories.Implementattions
{
    public class InventoryItemRepository : BaseRepository<InventoryItem>, IInventoryItemRepository
    {
        public InventoryItemRepository(ChurchPortalDbContext context) : base(context)
        {
        }
    }
}
