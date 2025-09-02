using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackerNews.Api.Library.Models;

namespace HackerNews.Api.Library.Interfaces
{
    public interface IHackerNewsService
    {
        /// <summary>
        /// Gets the newest stories from the Hacker News API.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A collection of the newest stories with valid URLs.</returns>
        Task<IEnumerable<HackerNewsItem>> GetNewestStoriesAsync(int? page = null, int? pageSize = null, CancellationToken cancellationToken = default);
        Task<HackerNewsItem?> GetStoryDetailsAsync(int storyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches the cached newest stories by keyword in the title and/or by username.
        /// If the cache has expired, it will do a standard pull of newest stories
        /// </summary>
        /// <param name="keyword">A keyword to search for in the story titles. Case-insensitive.</param>
        /// <param name="byUser">The username (author) to filter by. Case-insensitive.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A collection of stories matching the search criteria.</returns>
        Task<IEnumerable<HackerNewsItem>> SearchStoriesAsync(string? keyword, string? byUser, int? page = null, int? pageSize = null, CancellationToken cancellationToken = default);
    }
}
