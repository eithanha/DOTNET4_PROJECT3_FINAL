using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using PlotPocket.Server.Services;
using PlotPocket.Server.Models.Dtos;
using System;
using PlotPocket.Server.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PlotPocket.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TvShowsController : ControllerBase
    {
        private readonly ITmdbService _tmdbService;
        private readonly ShowService _showService;

        public TvShowsController(ITmdbService tmdbService, ShowService showService)
        {
            _tmdbService = tmdbService;
            _showService = showService;
        }

        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<ShowDto>>> GetPopularTvShows()
        {
            try
            {
                var tvShowResponse = await _tmdbService.GetPopularTvShowsAsync();
                var tvShows = tvShowResponse.Results.Select(tvShow => new ShowDto
                {
                    Id = tvShow.Id,
                    Title = tvShow.Name,
                    Overview = tvShow.Overview,
                    ReleaseDate = tvShow.DisplayDate,
                    PosterPath = tvShow.PosterPath,
                    Type = ShowType.TvShow,
                    Rating = tvShow.VoteAverage
                }).ToList();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var userWatchlist = await _showService.GetUserWatchlistAsync(userId);
                    foreach (var tvShow in tvShows)
                    {
                        tvShow.IsWatchlisted = userWatchlist.Any(s => s.Id == tvShow.Id);
                    }
                }

                return Ok(tvShows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching popular TV shows: {ex.Message}");
            }
        }

        [HttpGet("top-rated")]
        public async Task<ActionResult<IEnumerable<ShowDto>>> GetTopRatedTvShows()
        {
            try
            {
                var tvShowResponse = await _tmdbService.GetTopRatedTvShowsAsync();
                var tvShows = tvShowResponse.Results.Select(tvShow => new ShowDto
                {
                    Id = tvShow.Id,
                    Title = tvShow.Name,
                    Overview = tvShow.Overview,
                    ReleaseDate = tvShow.DisplayDate,
                    PosterPath = tvShow.PosterPath,
                    Type = ShowType.TvShow,
                    Rating = tvShow.VoteAverage
                }).ToList();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var userWatchlist = await _showService.GetUserWatchlistAsync(userId);
                    foreach (var tvShow in tvShows)
                    {
                        tvShow.IsWatchlisted = userWatchlist.Any(s => s.Id == tvShow.Id);
                    }
                }

                return Ok(tvShows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching top-rated TV shows: {ex.Message}");
            }
        }

        [HttpGet("on-air")]
        public async Task<ActionResult<IEnumerable<ShowDto>>> GetOnAirTvShows()
        {
            try
            {
                var tvShowResponse = await _tmdbService.GetOnAirTvShowsAsync();
                var tvShows = tvShowResponse.Results.Select(tvShow => new ShowDto
                {
                    Id = tvShow.Id,
                    Title = tvShow.Name,
                    Overview = tvShow.Overview,
                    ReleaseDate = tvShow.DisplayDate,
                    PosterPath = tvShow.PosterPath,
                    Type = ShowType.TvShow,
                    Rating = tvShow.VoteAverage
                }).ToList();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var userWatchlist = await _showService.GetUserWatchlistAsync(userId);
                    foreach (var tvShow in tvShows)
                    {
                        tvShow.IsWatchlisted = userWatchlist.Any(s => s.Id == tvShow.Id);
                    }
                }

                return Ok(tvShows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching on-air TV shows: {ex.Message}");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ShowDto>>> SearchTvShows([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            try
            {
                var shows = await _showService.SearchShows(query);
                // Filter to only include TV shows
                var tvShows = shows.Where(s => s.Type == ShowType.TvShow).ToList();
                return Ok(tvShows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error searching TV shows: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("{id}/watchlist")]
        public async Task<ActionResult<ShowDto>> AddToWatchlist(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var show = await _showService.AddToWatchlistAsync(userId, id);
                return Ok(show);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error adding TV show to watchlist: {ex.Message}");
            }
        }

        [Authorize]
        [HttpDelete("{id}/watchlist")]
        public async Task<ActionResult<ShowDto>> RemoveFromWatchlist(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var show = await _showService.RemoveFromWatchlistAsync(userId, id);
                return Ok(show);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error removing TV show from watchlist: {ex.Message}");
            }
        }
    }
}
