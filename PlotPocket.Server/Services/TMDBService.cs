using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PlotPocket.Server.Models.Responses;
using RestSharp;

namespace PlotPocket.Server.Services;

public class TMDBService : ITmdbService
{
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly RestClient _client;
    private const string ImageBaseUrl = "https://image.tmdb.org/t/p/w500";

    public TMDBService(IConfiguration configuration)
    {
        _apiKey = configuration["TMDB:ApiKey"];
        _baseUrl = "https://api.themoviedb.org/3";
        _client = new RestClient(_baseUrl);
    }

    private string GetFullImageUrl(string posterPath)
    {
        if (string.IsNullOrEmpty(posterPath)) return null;
        if (posterPath.StartsWith("http")) return posterPath;
        return $"{ImageBaseUrl}{posterPath}";
    }

    public async Task<TrendingResponse> GetTrendingShowsAsync(string timeWindow = "day")
    {
        var request = new RestRequest($"/trending/all/{timeWindow}");
        request.AddParameter("api_key", _apiKey);
        request.AddHeader("accept", "application/json");

        var response = await _client.ExecuteGetAsync(request);
        var result = JsonSerializer.Deserialize<TrendingResponse>(response.Content);
        
        // Update poster paths to use consistent size
        if (result?.Results != null)
        {
            foreach (var item in result.Results)
            {
                if (item.MediaType == "movie")
                {
                    item.PosterPath = GetFullImageUrl(item.PosterPath);
                }
                else if (item.MediaType == "tv")
                {
                    item.PosterPath = GetFullImageUrl(item.PosterPath);
                }
            }
        }
        
        return result;
    }

    public async Task<TrendingResponse> GetTrendingMoviesAsync(string timeWindow = "day")
    {
        var request = new RestRequest($"/trending/movie/{timeWindow}");
        request.AddParameter("api_key", _apiKey);
        request.AddHeader("accept", "application/json");

        var response = await _client.ExecuteGetAsync(request);
        return JsonSerializer.Deserialize<TrendingResponse>(response.Content);
    }

    public async Task<TrendingResponse> GetTrendingTvShowsAsync(string timeWindow = "day")
    {
        var request = new RestRequest($"/trending/tv/{timeWindow}");
        request.AddParameter("api_key", _apiKey);
        request.AddHeader("accept", "application/json");

        var response = await _client.ExecuteGetAsync(request);
        return JsonSerializer.Deserialize<TrendingResponse>(response.Content);
    }

    public async Task<TrendingResponse> SearchShowsAsync(string query)
    {
        var request = new RestRequest("/search/multi");
        request.AddParameter("api_key", _apiKey);
        request.AddParameter("query", query);
        request.AddHeader("accept", "application/json");

        var response = await _client.ExecuteGetAsync(request);
        return JsonSerializer.Deserialize<TrendingResponse>(response.Content);
    }

    public async Task<ApiMediaItem> GetShowDetailsAsync(int showId)
    {
        // Try to get movie details first
        var movieRequest = new RestRequest($"/movie/{showId}");
        movieRequest.AddParameter("api_key", _apiKey);
        movieRequest.AddHeader("accept", "application/json");

        var movieResponse = await _client.ExecuteGetAsync(movieRequest);
        if (movieResponse.IsSuccessful)
        {
            var movie = JsonSerializer.Deserialize<Movie>(movieResponse.Content);
            if (movie != null)
            {
                movie.PosterPath = GetFullImageUrl(movie.PosterPath);
            }
            return movie;
        }

        // If movie not found, try TV show
        var tvRequest = new RestRequest($"/tv/{showId}");
        tvRequest.AddParameter("api_key", _apiKey);
        tvRequest.AddHeader("accept", "application/json");

        var tvResponse = await _client.ExecuteGetAsync(tvRequest);
        var tvShow = JsonSerializer.Deserialize<TvShow>(tvResponse.Content);
        if (tvShow != null)
        {
            tvShow.PosterPath = GetFullImageUrl(tvShow.PosterPath);
        }
        return tvShow;
    }

    // Movie Endpoints
    public async Task<MovieResponse> GetNowPlayingMoviesAsync(int page = 1)
    {
        var request = new RestRequest($"/movie/now_playing?api_key={_apiKey}&page={page}")
                      .AddHeader("accept", "application/json");

        var response = await _client.GetAsync(request);
        MovieResponse? movieResponse = JsonSerializer.Deserialize<MovieResponse>(response.Content);

        return movieResponse ?? new MovieResponse { Results = new List<Movie>() };
    }

    public async Task<MovieResponse> GetTopRatedMoviesAsync(int page = 1)
    {
        var request = new RestRequest($"/movie/top_rated?api_key={_apiKey}&page={page}")
                      .AddHeader("accept", "application/json");

        var response = await _client.GetAsync(request);
        MovieResponse? movieResponse = JsonSerializer.Deserialize<MovieResponse>(response.Content);

        return movieResponse ?? new MovieResponse { Results = new List<Movie>() };
    }

    public async Task<MovieResponse> GetPopularMoviesAsync(int page = 1)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("TMDB API key is not configured");
            }

            var request = new RestRequest($"/movie/popular?api_key={_apiKey}&page={page}")
                          .AddHeader("accept", "application/json");
            
            var response = await _client.GetAsync(request);
            if (!response.IsSuccessful)
            {
                throw new HttpRequestException($"TMDB API request failed with status code: {response.StatusCode}");
            }

            if (string.IsNullOrEmpty(response.Content))
            {
                throw new InvalidOperationException("Empty response received from TMDB API");
            }

            var movieResponse = JsonSerializer.Deserialize<MovieResponse>(response.Content);
            if (movieResponse == null)
            {
                throw new InvalidOperationException("Failed to deserialize TMDB API response");
            }

            return movieResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPopularMoviesAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    // TV Show Endpoints
    public async Task<TvShowResponse> GetAiringTodayTvShowsAsync(int page = 1)
    {
        var request = new RestRequest($"/tv/airing_today?api_key={_apiKey}&page={page}")
                      .AddHeader("accept", "application/json");

        var response = await _client.GetAsync(request);
        TvShowResponse? tvShowResponse = JsonSerializer.Deserialize<TvShowResponse>(response.Content);

        return tvShowResponse ?? new TvShowResponse { Results = new List<TvShow>() };
    }

    public async Task<TvShowResponse> GetTopRatedTvShowsAsync(int page = 1)
    {
        var request = new RestRequest($"/tv/top_rated?api_key={_apiKey}&page={page}")
                      .AddHeader("accept", "application/json");

        var response = await _client.GetAsync(request);
        TvShowResponse? tvShowResponse = JsonSerializer.Deserialize<TvShowResponse>(response.Content);

        return tvShowResponse ?? new TvShowResponse { Results = new List<TvShow>() };
    }

    public async Task<TvShowResponse> GetPopularTvShowsAsync(int page = 1)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("TMDB API key is not configured");
            }

            var request = new RestRequest($"/tv/popular?api_key={_apiKey}&page={page}")
                          .AddHeader("accept", "application/json");
            
            var response = await _client.GetAsync(request);
            if (!response.IsSuccessful)
            {
                throw new HttpRequestException($"TMDB API request failed with status code: {response.StatusCode}");
            }

            if (string.IsNullOrEmpty(response.Content))
            {
                throw new InvalidOperationException("Empty response received from TMDB API");
            }

            var tvShowResponse = JsonSerializer.Deserialize<TvShowResponse>(response.Content);
            if (tvShowResponse == null)
            {
                throw new InvalidOperationException("Failed to deserialize TMDB API response");
            }

            return tvShowResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPopularTvShowsAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<TvShowResponse> GetOnAirTvShowsAsync(int page = 1)
    {
        var request = new RestRequest($"/tv/on_the_air?api_key={_apiKey}&page={page}")
                      .AddHeader("accept", "application/json");

        var response = await _client.GetAsync(request);
        TvShowResponse? tvShowResponse = JsonSerializer.Deserialize<TvShowResponse>(response.Content);

        return tvShowResponse ?? new TvShowResponse { Results = new List<TvShow>() };
    }

    /**
	 * TODO:
	 * Implement methods to hit the following endpoints. Write any helper methods that you see fit
	 * following good design practices.
	 * 
	 * Trending
	 * ----------
	 * - All Trending (DONE above)
	 * - Trending Movies
	 * - Trending TvShows
	 * 
	 * Movies
	 * ----------
	 * - Now Playing Movies
	 * - Top Rated Movies
	 * - Popular Movies
	 * 
	 * TvShows
	 * ----------
	 * - Airing Today TvShows
	 * - Top Rated TvShows
	 * - Popular TvShows
	 * 
	 * */

}