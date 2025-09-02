using HackerNews.Api.Library.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;

namespace HackerNews.Api.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public async Task Get_Newest_Stories_Returns_ListOfStories()
        {
            // Arrange
            var factory = new WebApplicationFactory<Program>();
            var htmlClient = factory.CreateClient();

            // Act
            var response = await htmlClient.GetAsync("/api/v1/stories/newest");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var stories = await response.Content.ReadFromJsonAsync<List<HackerNewsItem>>();
            Assert.NotNull(stories);

            // This will pull live data, so we can't assert anything else know here.
        }

        [Fact]
        public async Task Get_Newest_Stories_From_Cache_Returns_ListOfStories()
        {
            // Arrange
            var factory = new WebApplicationFactory<Program>();
            var htmlClient = factory.CreateClient();

            // Act
            var response = await htmlClient.GetAsync("/api/v1/stories/newest");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var stories = await response.Content.ReadFromJsonAsync<List<HackerNewsItem>>();
            Assert.NotNull(stories);

            int oldLength = stories.Count;
            int firstId = stories[0].Id;

            // Act again
            var newresponse = await htmlClient.GetAsync("/api/v1/stories/newest");

            // Assert some more
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            stories = await newresponse.Content.ReadFromJsonAsync<List<HackerNewsItem>>();
            Assert.NotNull(stories);

            Assert.True(oldLength == stories.Count);
            Assert.Equal(firstId, stories[0].Id);
        }
    }
}
