using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;

namespace ChurchPortal.Repositories.Implementattions
{
    public class FellowshipCenterRepository : BaseRepository<FellowshipCenter>, IFellowshipCenterRepository
    {
        public FellowshipCenterRepository(ChurchPortalDbContext context) : base(context)
        {
        }
    }
}
