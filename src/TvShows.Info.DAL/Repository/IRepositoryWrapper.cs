namespace TvShows.Info.DAL.Repository
{
    public interface IRepositoryWrapper
    {
        ITvShowRepository TvShowRepository { get; }
        ICastMemberRepository CastMemberRepository { get; }
        Task<int> SaveAsync();
    }
}
