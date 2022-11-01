using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TvShows.Info.DAL.Context.Models;

namespace TvShows.Info.DAL
{
    public class TvShowDbContext : DbContext
    {
        public readonly ILoggerFactory _loggerFactory;

        public TvShowDbContext(DbContextOptions<TvShowDbContext> options, ILoggerFactory loggerFactory)
            : base(options)
        {
            _loggerFactory = loggerFactory;
        }

        public DbSet<TvShow> TvShows => Set<TvShow>();

        public DbSet<CastMember> CastMembers => Set<CastMember>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseLoggerFactory(_loggerFactory);
    }
}
