using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using TvShows.Info.DAL.Context.Models;
using TvShows.Info.DAL.Repository;

namespace TvShows.Info.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", Type = typeof(string))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", Type = typeof(string))]
    public class TvShowController : ControllerBase
    {
        private readonly ILogger<TvShowController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IConfiguration _configuration;

        public TvShowController(ILogger<TvShowController> logger, IRepositoryWrapper repositoryWrapper, IConfiguration configuration)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("[action]")]
        [SwaggerOperation(
            Summary = "Determine if a reboot is allowed",
            Description = "Returns true if a reboot is allowed, false otherwise",
            OperationId = nameof(GetTvShows))]
        [SwaggerResponse(StatusCodes.Status200OK, "Success", Type = typeof(List<TvShow>))]
        public IActionResult GetTvShows(int pageSize, int pageNumber)
        {
            int maxEntriesPerPage;
            if (!int.TryParse(_configuration["MaxEntriesPerPage"], out maxEntriesPerPage))
            {
                maxEntriesPerPage = 25;
            }
            if (pageSize > maxEntriesPerPage)
            {
                return BadRequest($"PageSize is grater than the allowed PageSize of {maxEntriesPerPage}.");
            }
            _logger.LogInformation($"Retrieving {pageSize} TvShows from page {pageNumber}.");
            return Ok(_repositoryWrapper.TvShowRepository.GetTvShows(pageSize, pageNumber));
        }
    }
}
