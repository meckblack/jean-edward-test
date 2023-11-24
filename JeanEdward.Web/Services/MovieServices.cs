using System.Net;
using jean_edwards.Database;
using jean_edwards.Database.Entities;
using jean_edwards.Interfaces;
using jean_edwards.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace jean_edwards.Services
{
    public class MovieServices : IMovieServices
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        public MovieServices(HttpClient httpClient, IConfiguration config, AppDbContext context)
        {
            _httpClient = httpClient;
            _config = config;
            _context = context;
        }
        public async Task<MovieModel?> SearchMovies(string title, MovieQuery query)
        {
            try
            {
                var apiKey = _config.GetSection("ApiKey").Value;
                var year = query.Year == 0 ? string.Empty : query.Year.ToString();
                var plot = string.IsNullOrEmpty(query.Plot) ? "short" : query.Plot;
                var responseString = await _httpClient.GetStringAsync($"?apiKey={apiKey}&t={title}&plot={plot}&y={year}");
                var responseModel = JsonConvert.DeserializeObject<MovieModel>(responseString);
                if (responseModel != null &&  responseModel.Response)
                {
                    var movieResult = await _context.MovieResults.FirstOrDefaultAsync(x => x.ImdbId == responseModel.imdbID);
                    if (movieResult == null)
                    {
                        await _context.MovieResults.AddAsync(new MovieResult
                        {
                            Keyword = title,
                            SearchResult = JsonConvert.SerializeObject(responseModel),
                            ImdbId = responseModel.imdbID
                        });
                    }
                    else
                    {
                        movieResult.DateCreated = DateTime.Now;
                        movieResult.Keyword = title;
                    }

                    await _context.SaveChangesAsync();
                }
                return responseModel;
            }
            catch (WebException webEx)
            {
                throw new WebException($"Error connecting to remote server. {webEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw new Exception($"An error occurred. {ex.Message}");
            }
        }

        public async Task<List<SearchResultResponse>> GetMovieSearchResults()
        {
            var searchResults = await _context.MovieResults.OrderByDescending(x => x.DateCreated).Take(5).ToListAsync();
            List<SearchResultResponse> response = new();
            searchResults.ForEach(x =>
            {
                response.Add(new SearchResultResponse
                {
                    Title = x.Keyword,
                    SearchResult = JsonConvert.DeserializeObject<MovieModel>(x.SearchResult)
                });
            });

            return response;
        }
    }
}