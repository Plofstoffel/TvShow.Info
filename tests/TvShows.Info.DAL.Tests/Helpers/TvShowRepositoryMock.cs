using Moq;
using TvShows.Info.DAL.Context.Models;
using TvShows.Info.DAL.Repository;

namespace TvShows.Info.DAL.Tests.Helpers
{
    public static class TvShowRepositoryMock
    {
        public static Mock<ITvShowRepository> GetMock()
        {
            var mock = new Mock<ITvShowRepository>();

            mock.Setup(m => m.AddOrUpdate(It.IsAny<TvShow>()))
                .Returns<TvShow>(x => x);

            mock.Setup(m => m.GetTvShows(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int pageSize, int pageNumber) => GetFilledListOfShows().Skip(pageSize*(pageNumber-1)).Take(pageSize).AsQueryable());

            return mock;
        }

        private static List<TvShow> GetFilledListOfShows()
        {
            var shows = new List<TvShow>();
            for (int i = 0; i <= 10; i++)
            {
                shows.Add(
                new TvShow()
                {
                    Id = 1,
                    Name = $"Invader Zim {i}",
                    Cast = new List<CastMember>()
                    {
                        new CastMember()
                        {
                            Id = 1,
                            Name = "Zim",
                            Birthday = DateTime.Now.AddYears(-25)
                        }
                    }
                });
            }
            return shows;
        }
    }

}
