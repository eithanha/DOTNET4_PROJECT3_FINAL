using Microsoft.AspNetCore.Mvc;
using PlotPocket.Server.Services;
using PlotPocket.Server.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PlotPocket.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowsController : ControllerBase
    {
        private readonly ShowService _showService;

        public ShowsController(ShowService showService)
        {
            _showService = showService;
        }

        [HttpGet("trending/all")]
        public async Task<ActionResult<IEnumerable<ShowDto>>> GetTrendingShows()
        {
            try
            {
                var shows = await _showService.GetTrendingShowsAsync();
                return Ok(shows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("trending/movies")]
        public async Task<ActionResult<IEnumerable<ShowDto>>> GetTrendingMovies()
        {
            try
            {
                var movies = await _showService.GetTrendingMoviesAsync();
                return Ok(movies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("trending/tv-shows")]
        public async Task<ActionResult<IEnumerable<ShowDto>>> GetTrendingTvShows()
        {
            try
            {
                var tvShows = await _showService.GetTrendingTvShowsAsync();
                return Ok(tvShows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ShowDto>>> SearchShows([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            try
            {
                var results = await _showService.SearchShowsAsync(query);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("{showId}/bookmark")]
        public async Task<ActionResult<ShowDto>> AddBookmark(int showId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var show = await _showService.AddBookmarkAsync(showId, userId);
                return Ok(show);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpDelete("{showId}/bookmark")]
        public async Task<ActionResult<ShowDto>> RemoveBookmark(int showId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var show = await _showService.RemoveBookmarkAsync(showId, userId);
                return Ok(show);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("bookmarks")]
        public async Task<ActionResult<IEnumerable<ShowDto>>> GetBookmarks()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var bookmarks = await _showService.GetBookmarksAsync(userId);
                return Ok(bookmarks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
