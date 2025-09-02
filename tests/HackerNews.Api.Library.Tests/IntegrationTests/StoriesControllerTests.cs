using Castle.Components.DictionaryAdapter.Xml;
using HackerNews.Api.Controllers;
using HackerNews.Api.Library.Interfaces;
using HackerNews.Api.Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HackerNews.Api.Library.Tests.IntegrationTests
{
    public class StoriesControllerTests
    {
        private readonly Mock<IHackerNewsService> _mockHackerNewsService;
        private readonly Mock<ILogger<StoriesController>> _mockLogger;
        private readonly StoriesController _controller;

        public StoriesControllerTests()
        {
            // Initialize mocks for the dependencies
            _mockHackerNewsService = new Mock<IHackerNewsService>();
            _mockLogger = new Mock<ILogger<StoriesController>>();

            // Create an instance of the controller with the mocked dependencies
            _controller = new StoriesController(_mockHackerNewsService.Object, _mockLogger.Object);
        }

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

        [Fact]
        public async Task GetNewestStories_ShouldReturnOkResult_WithStories()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var fakeStories = new List<HackerNewsItem>
            {
            new HackerNewsItem { Id = 1, Title = "Test Story 1", Url = "http://test.com/1" },
            new HackerNewsItem { Id = 2, Title = "Test Story 2", Url = "http://test.com/2" }
            };

            // Setup the mock service to return the fake stories
            _mockHackerNewsService
                .Setup(s => s.GetNewestStoriesAsync(null, null, cancellationToken))
                .ReturnsAsync(fakeStories);

            // Act
            var result = await _controller.GetNewestStories(cancellationToken);

            // Assert
            // Verify that the result is an OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Verify that the value of the result is the list of stories we faked
            var returnedStories = Assert.IsAssignableFrom<IEnumerable<HackerNewsItem>>(okResult.Value);
            Assert.Equal(2, ((List<HackerNewsItem>)returnedStories).Count);

            // Verify that the service method was called exactly once
            _mockHackerNewsService.Verify(s => s.GetNewestStoriesAsync(null, null, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetNewestStories_ShouldReturn500InternalServerError_OnException()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var exception = new Exception("Something went wrong");

            // Setup the mock service to throw an exception when called
            _mockHackerNewsService
                .Setup(s => s.GetNewestStoriesAsync(null, null, cancellationToken))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.GetNewestStories(cancellationToken);

            // Assert
            // Verify that the result is a generic StatusCodeResult
            var statusCodeResult = Assert.IsType<ObjectResult>(result);

            // Verify that the status code is 500
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An internal server error occurred.", statusCodeResult.Value);

            // Verify that the logger's LogError method was called
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An Unexpected Exception Occured")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        public async Task SearchStories_ShouldReturnBadRequest_WhenNoParametersProvided(string? keyword, string? byUser)
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _controller.SearchStories(keyword, byUser, cancellationToken);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("At least one search parameter (keyword or byUser) must be provided.", badRequestResult.Value);
        }

        [Theory]
        [InlineData("test", null)]
        [InlineData("test", "")]
        [InlineData("test", " ")]
        [InlineData(null, "user1")]
        [InlineData("", "user1")]
        [InlineData(" ", "user1")]
        public async Task SearchStories_ShouldReturnOkResult_WithResults(string? keyword, string? byUser)
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var fakeItems = new List<HackerNewsItem>
        {
            new HackerNewsItem { Id = 1, Title = "A test story", By = "user1" },
            new HackerNewsItem { Id = 2, Title = "Another test", By = "user1" }
        };

            _mockHackerNewsService
                .Setup(s => s.SearchStoriesAsync(keyword, byUser, null, null, cancellationToken))
                .ReturnsAsync(fakeItems);

            // Act
            var result = await _controller.SearchStories(keyword, byUser, cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedItems = Assert.IsAssignableFrom<IEnumerable<HackerNewsItem>>(okResult.Value);
            Assert.Equal(2, ((List<HackerNewsItem>)returnedItems).Count);
            _mockHackerNewsService.Verify(s => s.SearchStoriesAsync(keyword, byUser, null, null, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task SearchStories_ShouldReturn500InternalServerError_OnException()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var keyword = "test";
            var exception = new Exception("Search failed");

            _mockHackerNewsService
                .Setup(s => s.SearchStoriesAsync(keyword, null, null, null, cancellationToken))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.SearchStories(keyword, null, cancellationToken);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An internal server error occurred.", statusCodeResult.Value);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An Unexpected Exception occured while searching for stories.")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void VerifyAuth_ShouldReturnOkResult()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            var result = _controller.VerifyAuth(cancellationToken);

            // Assert
            // This test simply confirms that the method returns an OK result.
            // The [Authorize] attribute itself is tested at the integration level, not the unit level.
            Assert.IsType<OkResult>(result);
        }

    }
}
