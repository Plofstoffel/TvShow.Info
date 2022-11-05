using Microsoft.EntityFrameworkCore;
using Polly.Extensions.Http;
using Polly;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using TvShows.Info.DAL;
using TvShows.Info.DAL.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace TvShows.Info.ScrapeWorkerService
{
    [ExcludeFromCodeCoverage(Justification = "Must be covered with integration tests")]
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

            Log.Information("Starting up");

            try
            {
                IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<TvShowDbContext>(
                    options => options.UseSqlServer("name=ConnectionStrings:TvShowDbContext")
                    );
                    services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
                    services.AddHttpClient("ScrapeHttpClient", client =>
                    {
                        client.BaseAddress = new Uri(hostContext.Configuration["TvShowsUrl"]);
                    })
                    .AddPolicyHandler(GetRetryPolicy(hostContext.Configuration));
                    services.AddHostedService<ScrapeWorker>();
                })
                .UseSerilog((ctx, lc) => lc
                    .WriteTo.Console()
                    .ReadFrom.Configuration(ctx.Configuration)
                )
                .Build();

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unhandled exception");
            }
            finally
            {
                Log.Information("Shut down complete");
                Log.CloseAndFlush();
            }
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IConfiguration configuration)
        {
            int retries;
            int.TryParse(configuration["TooManyRequestRetries"], out retries);
            var timeout = 10;
            int.TryParse(configuration["TooManyRequestTimeout"], out timeout);

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(retries, retryAttempt => TimeSpan.FromSeconds(timeout));
        }
    }
}