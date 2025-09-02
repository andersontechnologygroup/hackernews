namespace HackerNews.Api.Library.Configuration
{
    public class HackerNewsServiceSettings
    {
        public string? CacheKey { get; set; } = null;
        public int? CacheTimeoutInMinutes { get; set; } = null;

        public string? NewStoriesJSONPath { get; set; } = null;
        public string? ItemJSONPath { get; set; } = null;
        public int? NumberOfStoriesToPull { get; set; } = null;
    }
}
