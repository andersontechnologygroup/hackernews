using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using HackerNews.Api.Library.Interfaces;
using HackerNews.Api.Library.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using HackerNews.Api.Library.Configuration;
using Microsoft.Extensions.Options;

namespace HackerNews.Api.Library.Services
{
    public class HackerNewsService : IHackerNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<HackerNewsService> _logger;
        private readonly HackerNewsServiceSettings? _settings;

        public HackerNewsService(HttpClient httpClient, IMemoryCache memoryCache, ILogger<HackerNewsService> logger, IOptions<HackerNewsServiceSettings> hnsSetting)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _logger = logger;
            _settings = hnsSetting.Value;
        }

        public async Task<IEnumerable<HackerNewsItem>> GetNewestStoriesAsync(int? page = null, int? pageSize = null, CancellationToken cancellationToken = default)
        {
            // Try to get stories from the cache first
            if (_memoryCache.TryGetValue(_settings.CacheKey!, out IEnumerable<HackerNewsItem>? cachedStories))
            {
                _logger.LogInformation($"Cache hit!!  Is Null? {cachedStories == null}...  cacheSize: {cachedStories?.Count()}");
                return cachedStories ?? Enumerable.Empty<HackerNewsItem>();
            }

            _logger.LogInformation("Cache miss. Fetching newest stories from Hacker News API.");

            // Fetch the list of newest story IDs.  If this returns an error, just set it to null
            int[]? storyIds = null;
            try
            {
                storyIds = await _httpClient.GetFromJsonAsync<int[]>(_settings.NewStoriesJSONPath!, cancellationToken);
            }
            catch (Exception ex)
            {
                // Nothing to do here.   The API returned an error of some kind.  
                // So we just want an empty list.
            }

            if (storyIds == null || storyIds.Length == 0)
            {
                return Enumerable.Empty<HackerNewsItem>();
            }

            // Create parallel tasks to fetch details for each story (we'll limit to 200 for performance)
            var tasks = storyIds.Take(_settings.NumberOfStoriesToPull!.Value).Select(id => GetStoryDetailsAsync(id, cancellationToken));
            var stories = await Task.WhenAll(tasks);

            // If the fetched data is null, it was a failed fetch.   Filter that out.
            // Also filter out any stories with no URL
            var validStories = stories
                .Where(story => story is not null &&
                                !string.IsNullOrEmpty(story.Url) &&
                                Uri.TryCreate(story.Url, UriKind.Absolute, out var uriResult) &&
                                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                .ToList();

            // Cache the result for 5 minutes
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.CacheTimeoutInMinutes!.Value));

            // Caching should never be effected by paging
            _memoryCache.Set(_settings.CacheKey!, validStories, cacheEntryOptions);

            _logger.LogInformation($"Fetched and cached {validStories.Count} newest stories.");

            if (page.HasValue && pageSize.HasValue)
            {
                return validStories.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            }

            return validStories;
        }

        public async Task<HackerNewsItem?> GetStoryDetailsAsync(int storyId, CancellationToken cancellationToken = default)
        {
            try
            {
                string itemJson = _settings.ItemJSONPath!.Replace("{storyId}", storyId.ToString().Trim());
                var story = await _httpClient.GetFromJsonAsync<HackerNewsItem>(itemJson, cancellationToken);
                return story;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected Exception Occurred.  Failed to fetch details for story ID {storyId}");
            }

            return null;
        }

        public async Task<IEnumerable<HackerNewsItem>> SearchStoriesAsync(string? keyword, string? byUser, int? page = null, int? pageSize = null, CancellationToken cancellationToken = default)
        {
            // First, ensure we have the stories by calling the GetNewStories method.
            // This will either return from cache or fetch from the API.
            // We will get all of them here, and not mess with pagination yet.
            var allStories = await GetNewestStoriesAsync(null, null, cancellationToken);

            // Start with the full list of stories.
            IEnumerable<HackerNewsItem> filteredStories = allStories;

            // Apply keyword filter if provided.
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                // Search is case-insensitive and checks the title.
                filteredStories = filteredStories.Where(s =>
                    s.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            // Apply user filter if provided.
            if (!string.IsNullOrWhiteSpace(byUser))
            {
                // Search is a case-insensitive exact match on the username.
                filteredStories = filteredStories.Where(s =>
                    s.By.Equals(byUser, StringComparison.OrdinalIgnoreCase));
            }

            _logger.LogInformation("Search completed with keyword '{Keyword}' and user '{User}'. Found {Count} results.", keyword, byUser, filteredStories.Count());

            if(page.HasValue && pageSize.HasValue)
            {
                filteredStories = filteredStories.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return filteredStories;
        }
    }
}
