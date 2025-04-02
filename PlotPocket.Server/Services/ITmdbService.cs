using System.Threading.Tasks;
using PlotPocket.Server.Models.Responses;

namespace PlotPocket.Server.Services;

public interface ITmdbService
{
    Task<TrendingResponse> GetTrendingShowsAsync(string timeWindow = "day");
    Task<TrendingResponse> GetTrendingMoviesAsync(string timeWindow = "day");
    Task<TrendingResponse> GetTrendingTvShowsAsync(string timeWindow = "day");
    Task<TrendingResponse> SearchShowsAsync(string query);
    Task<ApiMediaItem> GetShowDetailsAsync(int showId);
    
    
    Task<MovieResponse> GetNowPlayingMoviesAsync(int page = 1);
    Task<MovieResponse> GetTopRatedMoviesAsync(int page = 1);
    Task<MovieResponse> GetPopularMoviesAsync(int page = 1);
    
    
    Task<TvShowResponse> GetAiringTodayTvShowsAsync(int page = 1);
    Task<TvShowResponse> GetTopRatedTvShowsAsync(int page = 1);
    Task<TvShowResponse> GetPopularTvShowsAsync(int page = 1);
    Task<TvShowResponse> GetOnAirTvShowsAsync(int page = 1);
} 