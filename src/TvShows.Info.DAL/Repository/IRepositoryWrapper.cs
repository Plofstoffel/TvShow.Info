namespace TvShows.Info.DAL.Repository
{
    public interface IRepositoryWrapper
    {
        ITvShowRepository TvShowRepository { get; }
        ICastMemberRepository CastMemberRepository { get; }
        IScrapesRepository ScrapesRepository { get; }
        Task<int> SaveAsync();
    }
}
