using TvShows.Info.DAL.Context.Models;

namespace TvShows.Info.DAL.Repository
{
    public class CastMemberRepository : RepositoryBase<CastMember>, ICastMemberRepository
    {
        public CastMemberRepository(TvShowDbContext tvShowDbContext) : base(tvShowDbContext)
        {
        }
    }
}
