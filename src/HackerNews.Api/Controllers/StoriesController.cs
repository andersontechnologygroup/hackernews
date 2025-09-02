using HackerNews.Api.Library.Interfaces;
using HackerNews.Api.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HackerNews.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly IHackerNewsService _hackerNewsService;
    private readonly ILogger<StoriesController> _logger;

    public StoriesController(IHackerNewsService hackerNewsService, ILogger<StoriesController> logger)
    {
        _hackerNewsService = hackerNewsService;
        _logger = logger;
    }

    [HttpGet("newest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetNewestStories(CancellationToken cancellationToken)
    {
        try
        {
            var stories = await _hackerNewsService.GetNewestStoriesAsync(null, null, cancellationToken);
            return Ok(stories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An Unexpected Exception Occured. An error occurred while fetching newest stories.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<HackerNewsItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchStories([FromQuery] string? keyword, [FromQuery] string? byUser, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(keyword) && string.IsNullOrWhiteSpace(byUser))
        {
            return BadRequest("At least one search parameter (keyword or byUser) must be provided.");
        }

        try
        {
            var stories = await _hackerNewsService.SearchStoriesAsync(keyword, byUser, null, null, cancellationToken);
            return Ok(stories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An Unexpected Exception occured while searching for stories.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }



    // For this limited functionality, there isn't a need for authorization
    // But for a bigger app, there would almost definately by some sort of
    // administrative functionality.  In that case, you'd need auth in some/all
    // of the controllers.

    // Dropping this in here to demo that functionality.
    [Authorize]
    [HttpGet("admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult VerifyAuth(CancellationToken cancellationToken)
    {
        return Ok();
    }
}