using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using HackerNews.Api.Library.Models;
using HackerNews.Api.Library.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Microsoft.Extensions.Options;
using HackerNews.Api.Library.Configuration;

namespace HackerNews.Api.Library.Tests.UnitTests;

public class HackerNewsServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly HackerNewsService _service;
    private const string _mockSiteRoot = "https://mocksite.com/";
    HackerNewsServiceSettings _settings = new HackerNewsServiceSettings()
    {
        CacheKey = "HackerNewsStoryCacheKey",
        CacheTimeoutInMinutes = 5,
        ItemJSONPath = "item/{storyId}.json",
        NewStoriesJSONPath = "newstories.json",
        NumberOfStoriesToPull = 200
    };

    public HackerNewsServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockHttp = new MockHttpMessageHandler();
        var httpClient = _mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri(_mockSiteRoot);

        // Use NullLogger for tests where logging output is not important
        var logger = new NullLogger<HackerNewsService>();
        var options = Options.Create(_settings);

        _service = new HackerNewsService(httpClient, _memoryCache, logger, options);
    }

    private void Setup()
    {
        var _storyIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
        var _stories = new[]
        {
            new { id = 1, Title = "Story 1", url = "https://valid.com/story1", by = "user1", score = 10, time = 1672531200 },
            new { id = 2, Title = "Story 2", url = (string?)null, by = "user2", score = 20, time = 1672531201 }, // No URL
            new { id = 3, Title = "Story 3", url = "not-a-valid-url", by = "user3", score = 30, time = 1672531202 }, // Invalid URL
            new { id = 4, Title = "Story 4", url = "http://valid.com/story4", by = "user4", score = 40, time = 1672531203 },
            new { id = 5, Title = "Story 5", url = "http://valid.com/story5", by = "user1", score = 40, time = 1672531204 },
            new { id = 6, Title = "Story 6", url = "http://valid.com/story6", by = "user2", score = 40, time = 1672551205 },
            new { id = 7, Title = "Story 7", url = "http://valid.com/story7", by = "user7", score = 40, time = 1672551206 },
            new { id = 8, Title = "Story 8", url = "http://valid.com/story8", by = "user8", score = 40, time = 1672551207 },
            new { id = 9, Title = "Story 9", url = "http://valid.com/story9", by = "user9", score = 40, time = 1672551208 },
            new { id = 10, Title = "Story 10", url = "http://valid.com/story10", by = "user10", score = 40, time = 1672551209 },
            new { id = 11, Title = "Story 11", url = "http://valid.com/story11", by = "user11", score = 40, time = 1672551210 },
            new { id = 12, Title = "Story 12", url = "http://valid.com/story12", by = "user12", score = 40, time = 1672551211 },
            new { id = 13, Title = "Story 13", url = "http://valid.com/story13", by = "user13", score = 40, time = 1672551212 },
    };

        _mockHttp.When($"{_mockSiteRoot}{_settings.NewStoriesJSONPath!}")
                .Respond("application/json", JsonSerializer.Serialize(_storyIds));

        string itemJson6 = $"{_mockSiteRoot}{_settings.ItemJSONPath!.Replace("{storyId}", "6")}";
        _mockHttp.When(itemJson6)
            .Respond(HttpStatusCode.NotFound);

        foreach (var story in _stories)
        {
            string itemJson = $"{_mockSiteRoot}{_settings.ItemJSONPath!.Replace("{storyId}", story.id.ToString())}";
            _mockHttp.When(itemJson)
                .Respond("application/json", JsonSerializer.Serialize(story));
        }


    }

    private void Teardown()
    {
    }

    [Fact]
    public async Task GetStoryDetail_WhenCacheIsEmpty_FetchesFromApi_IfValid()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.GetStoryDetailsAsync(1);

        // Assert
        Assert.NotNull(result);
        
        Assert.Equal(1, result.Id);

        Teardown();
    }

    [Fact]
    public async Task GetStoryDetail_WhenCacheIsEmpty_ReturnsNull_IfInvalidId()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.GetStoryDetailsAsync(6);

        // Assert
        Assert.Null(result);

        Teardown();
    }

    [Fact]
    public async Task GetNewestStoriesAsync_WhenCacheIsEmpty_FetchesFromApiAndFiltersInvalidUrls()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.GetNewestStoriesAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();

        Assert.Equal(10, resultList.Count);
        Assert.Contains(resultList, s => s.Id == 1);
        Assert.Contains(resultList, s => s.Id == 4);
        Assert.Contains(resultList, s => s.Id == 5);
        Assert.Contains(resultList, s => s.Id == 7);
        Assert.Contains(resultList, s => s.Id == 8);
        Assert.Contains(resultList, s => s.Id == 9);
        Assert.Contains(resultList, s => s.Id == 10);
        Assert.Contains(resultList, s => s.Id == 11);
        Assert.Contains(resultList, s => s.Id == 12);
        Assert.Contains(resultList, s => s.Id == 13);

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 6);

        // Verify it was added to cache
        Assert.True(_memoryCache.TryGetValue(_settings.CacheKey!, out _));

        Teardown();
    }

    [Fact]
    public async Task GetNewestStoriesAsync_WhenApiFailsToGetIds_ReturnsEmptyList()
    {
        // Arrange
        _mockHttp.When($"{_mockSiteRoot}{_settings.NewStoriesJSONPath!}")
            .Respond(HttpStatusCode.InternalServerError);

        // Act
        var result = await _service.GetNewestStoriesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchStoriesAsync_WithKeyword_ReturnsMatchingStories_Multiple()
    {
        // Arrange
        Setup();
        
        // Act
        var result = await _service.SearchStoriesAsync(keyword: "Story", byUser: null);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(10, resultList.Count);
        Assert.Contains(resultList, s => s.Id == 1);
        Assert.Contains(resultList, s => s.Id == 4);
        Assert.Contains(resultList, s => s.Id == 5);
        Assert.Contains(resultList, s => s.Id == 7);
        Assert.Contains(resultList, s => s.Id == 8);
        Assert.Contains(resultList, s => s.Id == 9);
        Assert.Contains(resultList, s => s.Id == 10);
        Assert.Contains(resultList, s => s.Id == 11);
        Assert.Contains(resultList, s => s.Id == 12);
        Assert.Contains(resultList, s => s.Id == 13);

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 6);

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithKeyword_ReturnsSingleStory_1()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: "1", byUser: null);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(5, resultList.Count);
        Assert.Contains(resultList, s => s.Id == 1);
        Assert.Contains(resultList, s => s.Id == 10);
        Assert.Contains(resultList, s => s.Id == 11);
        Assert.Contains(resultList, s => s.Id == 12);
        Assert.Contains(resultList, s => s.Id == 13);

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 4);
        Assert.DoesNotContain(resultList, s => s.Id == 5);
        Assert.DoesNotContain(resultList, s => s.Id == 6);
        Assert.DoesNotContain(resultList, s => s.Id == 7);
        Assert.DoesNotContain(resultList, s => s.Id == 8);
        Assert.DoesNotContain(resultList, s => s.Id == 9);

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithKeyword_ReturnsSingleStory_4()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: "4", byUser: null);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Contains(resultList, s => s.Id == 4);

        Assert.DoesNotContain(resultList, s => s.Id == 1);
        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 5);
        Assert.DoesNotContain(resultList, s => s.Id == 6);
        Assert.DoesNotContain(resultList, s => s.Id == 7);
        Assert.DoesNotContain(resultList, s => s.Id == 8);
        Assert.DoesNotContain(resultList, s => s.Id == 9);
        Assert.DoesNotContain(resultList, s => s.Id == 10);
        Assert.DoesNotContain(resultList, s => s.Id == 11);
        Assert.DoesNotContain(resultList, s => s.Id == 12);
        Assert.DoesNotContain(resultList, s => s.Id == 13);


        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_ByUser_ReturnsMatchingStories()
    {
        // Arrange
        Setup();
       
        // Act
        var result = await _service.SearchStoriesAsync(keyword: null, byUser: "user1");

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.All(resultList, story => Assert.Equal("user1", story.By, StringComparer.OrdinalIgnoreCase));

        Assert.Contains(resultList, s => s.Id == 1);
        Assert.Contains(resultList, s => s.Id == 5);

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 4);
        Assert.DoesNotContain(resultList, s => s.Id == 6);
        Assert.DoesNotContain(resultList, s => s.Id == 7);
        Assert.DoesNotContain(resultList, s => s.Id == 8);
        Assert.DoesNotContain(resultList, s => s.Id == 9);
        Assert.DoesNotContain(resultList, s => s.Id == 10);
        Assert.DoesNotContain(resultList, s => s.Id == 11);
        Assert.DoesNotContain(resultList, s => s.Id == 12);
        Assert.DoesNotContain(resultList, s => s.Id == 13);

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithKeywordAndUser_ReturnsMatchingStories()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: "Story", byUser: "user1");

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal(1, resultList.First().Id);
        Assert.Equal(5, resultList.Last().Id);

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 4);
        Assert.DoesNotContain(resultList, s => s.Id == 6);
        Assert.DoesNotContain(resultList, s => s.Id == 7);
        Assert.DoesNotContain(resultList, s => s.Id == 8);
        Assert.DoesNotContain(resultList, s => s.Id == 9);
        Assert.DoesNotContain(resultList, s => s.Id == 10);
        Assert.DoesNotContain(resultList, s => s.Id == 11);
        Assert.DoesNotContain(resultList, s => s.Id == 12);
        Assert.DoesNotContain(resultList, s => s.Id == 13);


        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: "c#", byUser: null);

        // Assert
        Assert.Empty(result);

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithNoCriteria_ReturnsAllStories()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: null, byUser: null);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(10, resultList.Count);
        Assert.Contains(resultList, s => s.Id == 1);
        Assert.Contains(resultList, s => s.Id == 4);
        Assert.Contains(resultList, s => s.Id == 5);
        Assert.Contains(resultList, s => s.Id == 7);
        Assert.Contains(resultList, s => s.Id == 8);
        Assert.Contains(resultList, s => s.Id == 9);
        Assert.Contains(resultList, s => s.Id == 10);
        Assert.Contains(resultList, s => s.Id == 11);
        Assert.Contains(resultList, s => s.Id == 12);
        Assert.Contains(resultList, s => s.Id == 13);

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 6);

        Teardown();
    }

    [Fact]
    public async Task GetNewestStoriesAsync_WhenCacheIsEmpty_WithPagination_FetchesFromApiAndFiltersInvalidUrls()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.GetNewestStoriesAsync(2, 2);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();

        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, s => s.Id == 5);
        Assert.Contains(resultList, s => s.Id == 7);

        Assert.DoesNotContain(resultList, s => s.Id == 1);
        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 4);
        Assert.DoesNotContain(resultList, s => s.Id == 6);
        Assert.DoesNotContain(resultList, s => s.Id == 8);
        Assert.DoesNotContain(resultList, s => s.Id == 9);
        Assert.DoesNotContain(resultList, s => s.Id == 10);
        Assert.DoesNotContain(resultList, s => s.Id == 11);
        Assert.DoesNotContain(resultList, s => s.Id == 12);
        Assert.DoesNotContain(resultList, s => s.Id == 13);

        // Verify it was added to cache
        Assert.True(_memoryCache.TryGetValue(_settings.CacheKey!, out _));

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithKeyword_WithPagination_ReturnsMatchingStories_Multiple()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: "Story", byUser: null, 1, 2);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, s => s.Id == 1);
        Assert.Contains(resultList, s => s.Id == 4);

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 5);
        Assert.DoesNotContain(resultList, s => s.Id == 6);
        Assert.DoesNotContain(resultList, s => s.Id == 7);
        Assert.DoesNotContain(resultList, s => s.Id == 8);
        Assert.DoesNotContain(resultList, s => s.Id == 9);
        Assert.DoesNotContain(resultList, s => s.Id == 10);
        Assert.DoesNotContain(resultList, s => s.Id == 11);
        Assert.DoesNotContain(resultList, s => s.Id == 12);
        Assert.DoesNotContain(resultList, s => s.Id == 13);


        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithKeyword_WithPagination_ReturnsSingleStory_1()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: "1", byUser: null, 2, 2);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, s => s.Id == 11);
        Assert.Contains(resultList, s => s.Id == 12);

        Assert.DoesNotContain(resultList, s => s.Id == 1);
        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 4);
        Assert.DoesNotContain(resultList, s => s.Id == 5);
        Assert.DoesNotContain(resultList, s => s.Id == 6);
        Assert.DoesNotContain(resultList, s => s.Id == 7);
        Assert.DoesNotContain(resultList, s => s.Id == 8);
        Assert.DoesNotContain(resultList, s => s.Id == 9);
        Assert.DoesNotContain(resultList, s => s.Id == 10);
        Assert.DoesNotContain(resultList, s => s.Id == 13);



        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithKeyword_WithPagination_ReturnsSingleStory_4()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: "4", byUser: null, 1, 2);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Contains(resultList, s => s.Id == 4);

        Assert.DoesNotContain(resultList, s => s.Id == 1);
        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 5);
        Assert.DoesNotContain(resultList, s => s.Id == 6);
        Assert.DoesNotContain(resultList, s => s.Id == 7);
        Assert.DoesNotContain(resultList, s => s.Id == 8);
        Assert.DoesNotContain(resultList, s => s.Id == 9);
        Assert.DoesNotContain(resultList, s => s.Id == 10);
        Assert.DoesNotContain(resultList, s => s.Id == 11);
        Assert.DoesNotContain(resultList, s => s.Id == 12);
        Assert.DoesNotContain(resultList, s => s.Id == 13);


        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_ByUser_WithPagination_ReturnsMatchingStories()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: null, byUser: "user1", 1, 1);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.All(resultList, story => Assert.Equal("user1", story.By, StringComparer.OrdinalIgnoreCase));

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 4);
        Assert.DoesNotContain(resultList, s => s.Id == 5);
        Assert.DoesNotContain(resultList, s => s.Id == 6);
        Assert.DoesNotContain(resultList, s => s.Id == 7);
        Assert.DoesNotContain(resultList, s => s.Id == 8);
        Assert.DoesNotContain(resultList, s => s.Id == 9);
        Assert.DoesNotContain(resultList, s => s.Id == 10);
        Assert.DoesNotContain(resultList, s => s.Id == 11);
        Assert.DoesNotContain(resultList, s => s.Id == 12);
        Assert.DoesNotContain(resultList, s => s.Id == 13);

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithKeywordAndUser_WithPagination_ReturnsMatchingStories()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: "Story", byUser: "user1", 2, 1);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal(5, resultList.First().Id);

        Assert.DoesNotContain(resultList, s => s.Id == 1);
        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 4);
        Assert.DoesNotContain(resultList, s => s.Id == 6);
        Assert.DoesNotContain(resultList, s => s.Id == 7);
        Assert.DoesNotContain(resultList, s => s.Id == 8);
        Assert.DoesNotContain(resultList, s => s.Id == 9);
        Assert.DoesNotContain(resultList, s => s.Id == 10);
        Assert.DoesNotContain(resultList, s => s.Id == 11);
        Assert.DoesNotContain(resultList, s => s.Id == 12);
        Assert.DoesNotContain(resultList, s => s.Id == 13);

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithNoMatches_WithPagination_ReturnsEmptyList()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: "c#", byUser: null, 1, 2);

        // Assert
        Assert.Empty(result);

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithNoCriteria_WithPagination_ReturnsAllStories()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: null, byUser: null , 1, 2);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, result.Count());

        Assert.Contains(resultList, s => s.Id == 1);
        Assert.Contains(resultList, s => s.Id == 4);

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 5);
        Assert.DoesNotContain(resultList, s => s.Id == 6);
        Assert.DoesNotContain(resultList, s => s.Id == 7);
        Assert.DoesNotContain(resultList, s => s.Id == 8);
        Assert.DoesNotContain(resultList, s => s.Id == 9);
        Assert.DoesNotContain(resultList, s => s.Id == 10);
        Assert.DoesNotContain(resultList, s => s.Id == 11);
        Assert.DoesNotContain(resultList, s => s.Id == 12);
        Assert.DoesNotContain(resultList, s => s.Id == 13);


        Teardown();
    }

    [Fact]
    public async Task GetNewestStoriesAsync_WhenCacheIsEmpty_WithBadPagination_FetchesFromApiAndFiltersInvalidUrls()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.GetNewestStoriesAsync(1, null);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();

        Assert.Equal(10, resultList.Count);
        Assert.Contains(resultList, s => s.Id == 1);
        Assert.Contains(resultList, s => s.Id == 4);
        Assert.Contains(resultList, s => s.Id == 5);
        Assert.Contains(resultList, s => s.Id == 7);
        Assert.Contains(resultList, s => s.Id == 8);
        Assert.Contains(resultList, s => s.Id == 9);
        Assert.Contains(resultList, s => s.Id == 10);
        Assert.Contains(resultList, s => s.Id == 11);
        Assert.Contains(resultList, s => s.Id == 12);
        Assert.Contains(resultList, s => s.Id == 13);

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 6);

        // Verify it was added to cache
        Assert.True(_memoryCache.TryGetValue(_settings.CacheKey!, out _));

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithKeyword_WithBadPagination_ReturnsMatchingStories_Multiple()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: "Story", byUser: null, null, 2);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(10, resultList.Count);
        Assert.Contains(resultList, s => s.Id == 1);
        Assert.Contains(resultList, s => s.Id == 4);
        Assert.Contains(resultList, s => s.Id == 5);
        Assert.Contains(resultList, s => s.Id == 7);
        Assert.Contains(resultList, s => s.Id == 8);
        Assert.Contains(resultList, s => s.Id == 9);
        Assert.Contains(resultList, s => s.Id == 10);
        Assert.Contains(resultList, s => s.Id == 11);
        Assert.Contains(resultList, s => s.Id == 12);
        Assert.Contains(resultList, s => s.Id == 13);

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 6);

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithKeyword_WithBadPagination_ReturnsSingleStory_1()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: "1", byUser: null, 1, null);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(5, resultList.Count);
        Assert.Contains(resultList, s => s.Id == 1);
        Assert.Contains(resultList, s => s.Id == 10);
        Assert.Contains(resultList, s => s.Id == 11);
        Assert.Contains(resultList, s => s.Id == 12);
        Assert.Contains(resultList, s => s.Id == 13);

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 4);
        Assert.DoesNotContain(resultList, s => s.Id == 5);
        Assert.DoesNotContain(resultList, s => s.Id == 6);
        Assert.DoesNotContain(resultList, s => s.Id == 7);
        Assert.DoesNotContain(resultList, s => s.Id == 8);
        Assert.DoesNotContain(resultList, s => s.Id == 9);

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithKeyword_WithBadPagination_ReturnsSingleStory_4()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: "4", byUser: null, null, 2);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Contains(resultList, s => s.Id == 4);

        Assert.DoesNotContain(resultList, s => s.Id == 1);
        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 5);
        Assert.DoesNotContain(resultList, s => s.Id == 6);

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_ByUser_WithBadPagination_ReturnsMatchingStories()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: null, byUser: "user1", 1, null);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, s => s.Id == 1);
        Assert.Contains(resultList, s => s.Id == 5);

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 4);
        Assert.DoesNotContain(resultList, s => s.Id == 6);

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithKeywordAndUser_WithBadPagination_ReturnsMatchingStories()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: "Story", byUser: "user1", null, 1);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, s => s.Id == 1);
        Assert.Contains(resultList, s => s.Id == 5);

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 4);
        Assert.DoesNotContain(resultList, s => s.Id == 6);

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithNoMatches_WithBadPagination_ReturnsEmptyList()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: "c#", byUser: null, 1, null);

        // Assert
        Assert.Empty(result);

        Teardown();
    }

    [Fact]
    public async Task SearchStoriesAsync_WithNoCriteria_WithBadPagination_ReturnsAllStories()
    {
        // Arrange
        Setup();

        // Act
        var result = await _service.SearchStoriesAsync(keyword: null, byUser: null, 1, null);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(10, resultList.Count);
        Assert.Contains(resultList, s => s.Id == 1);
        Assert.Contains(resultList, s => s.Id == 4);
        Assert.Contains(resultList, s => s.Id == 5);
        Assert.Contains(resultList, s => s.Id == 7);
        Assert.Contains(resultList, s => s.Id == 8);
        Assert.Contains(resultList, s => s.Id == 9);
        Assert.Contains(resultList, s => s.Id == 10);
        Assert.Contains(resultList, s => s.Id == 11);
        Assert.Contains(resultList, s => s.Id == 12);
        Assert.Contains(resultList, s => s.Id == 13);

        Assert.DoesNotContain(resultList, s => s.Id == 2);
        Assert.DoesNotContain(resultList, s => s.Id == 3);
        Assert.DoesNotContain(resultList, s => s.Id == 6);

        Teardown();
    }
}