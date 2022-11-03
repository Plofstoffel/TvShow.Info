namespace TvShows.Info.DAL.Repository
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly TvShowDbContext _repoContext;
        private ITvShowRepository? _tvShows;
        private ICastMemberRepository? _castMembers;
        public ITvShowRepository TvShowRepository
        {
            get
            {
                if (_tvShows == null)
                {
                    _tvShows = new TvShowRepository(_repoContext);
                }
                return _tvShows;
            }
        }
        public ICastMemberRepository CastMemberRepository
        {
            get
            {
                if (_castMembers == null)
                {
                    _castMembers = new CastMemberRepository(_repoContext);
                }
                return _castMembers;
            }
        }
        public RepositoryWrapper(TvShowDbContext repositoryContext) => _repoContext = repositoryContext;

        public async Task<int> SaveAsync()
        {
            return await _repoContext.SaveChangesAsync();
        }
    }
}
