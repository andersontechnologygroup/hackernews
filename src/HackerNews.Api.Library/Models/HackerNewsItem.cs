using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace HackerNews.Api.Library.Models
{
    public record HackerNewsItem
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; init; }

        [JsonPropertyName("type")]
        public string Type { get; init; } = String.Empty;

        [JsonPropertyName("by")]
        public string By { get; init; } = String.Empty;

        [JsonPropertyName("time")]
        public long Time { get; init; }

        [JsonPropertyName("text")]
        public string Text { get; init; } = String.Empty;

        [JsonPropertyName("dead")]
        public bool Dead { get; init; } = false;

        [JsonPropertyName("parent")]
        public int Parent { get; init; }

        [JsonPropertyName("poll")]
        public int Poll {get; init; }

        [JsonPropertyName("kids")]
        public List<int> Kids { get; init; } = [];

        [JsonPropertyName("url")]
        public string? Url { get; init; }

        [JsonPropertyName("score")]
        public int Score { get; init; }

        [JsonPropertyName("title")]
        public string Title { get; init; } = String.Empty;

        [JsonPropertyName("parts")]
        public List<int> Parts { get; init; } = [];

        [JsonPropertyName("descendants")]
        public int Descendants { get; init; }

        /// <summary>
        /// Convert Unix time to DateTimeOffset
        /// </summary>
        public DateTimeOffset PostedAt()
        {
            return DateTimeOffset.FromUnixTimeSeconds(Time);
        }
    }
}
