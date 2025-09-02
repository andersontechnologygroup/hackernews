using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackerNews.Api.Library.Models;

namespace HackerNews.Api.Library.Tests.UnitTests
{
    public class ModelsTests
    {
        [Fact]
        public void Loads_Model_Correctly()
        {
            //Arange
            var story = new HackerNewsItem();
            var story2 = new HackerNewsItem()
            {
                Id = 1,
                Deleted = false,
                Type = "Testing",
                By = "jbanderson",
                Time = 100,
                Text = "Not Always available",
                Dead = false,
                Parent = 0,
                Poll = 0,
                Kids = [],
                Url = "http://good.com",
                Score = 10,
                Title = "The Title",
                Parts = [],
                Descendants = 0
            };
            var story3 = new HackerNewsItem()
            {
                Id = 2,
                Deleted = true,
                Type = "ThisIsDeleted",
                By = "jjanderson",
                Time = 500,
                Text = "",
                Dead = true,
                Parent = 1,
                Poll = 2,
                Kids = [10,20],
                Url = null,
                Score = 20,
                Title = "Number2",
                Parts = [30,40],
                Descendants = 2
            };

            // Act

            // Assert
            Assert.NotNull(story);
            Assert.NotNull(story2);
            Assert.NotNull(story3);

            {
                Assert.Equal(0, story.Id);
                Assert.False(story.Deleted);
                Assert.Equal(string.Empty, story.Type);
                Assert.Equal(string.Empty, story.By);
                Assert.Equal(0, story.Time);
                Assert.Equal(string.Empty, story.Text);
                Assert.False(story.Dead);
                Assert.Equal(0, story.Parent);
                Assert.Equal(0, story.Poll);
                Assert.Equal([], story.Kids);
                Assert.Null(story.Url);
                Assert.Equal(0, story.Score);
                Assert.Equal(string.Empty, story.Title);
                Assert.Equal([], story.Parts);
                Assert.Equal(0, story.Descendants);
                Assert.Equal("1/1/1970 12:00:00 AM +00:00", story.PostedAt().ToString());
            }

            {
                Assert.Equal(1, story2.Id);
                Assert.False(story2.Deleted);
                Assert.Equal("Testing", story2.Type);
                Assert.Equal("jbanderson", story2.By);
                Assert.Equal(100, story2.Time);
                Assert.Equal("Not Always available", story2.Text);
                Assert.False(story2.Dead);
                Assert.Equal(0, story2.Parent);
                Assert.Equal(0, story2.Poll);
                Assert.Equal([], story2.Kids);
                Assert.Equal("http://good.com", story2.Url);
                Assert.Equal(10, story2.Score);
                Assert.Equal("The Title", story2.Title);
                Assert.Equal([], story2.Parts);
                Assert.Equal(0, story2.Descendants);
                Assert.Equal("1/1/1970 12:01:40 AM +00:00", story2.PostedAt().ToString());
            }

            {
                Assert.Equal(2, story3.Id);
                Assert.True(story3.Deleted);
                Assert.Equal("ThisIsDeleted", story3.Type);
                Assert.Equal("jjanderson", story3.By);
                Assert.Equal(500, story3.Time);
                Assert.Equal(string.Empty, story3.Text);
                Assert.True(story3.Dead);
                Assert.Equal(1, story3.Parent);
                Assert.Equal(2, story3.Poll);
                Assert.Equal([10,20], story3.Kids);
                Assert.Null(story3.Url);
                Assert.Equal(20, story3.Score);
                Assert.Equal("Number2", story3.Title);
                Assert.Equal([30,40], story3.Parts);
                Assert.Equal(2, story3.Descendants);
                Assert.Equal("1/1/1970 12:08:20 AM +00:00", story3.PostedAt().ToString());
            }

            /*
        public int Id { get; init; }
        public bool Deleted { get; init; }
        public string Type { get; init; } = String.Empty;
        public string By { get; init; } = String.Empty;
        public long Time { get; init; }
        public string Text { get; init; } = String.Empty;
        public bool Dead { get; init; } = false;
        public int Parent { get; init; }
        public int Poll {get; init; }
        public List<int> Kids { get; init; } = [];
        public string? Url { get; init; }
        public int Score { get; init; }
        public string Title { get; init; } = String.Empty;
        public List<int> Parts { get; init; } = [];
        public int Descendants { get; init; }
        public DateTimeOffset PostedAt()
                   
             */

        }

        [Fact]
        public void Converts_Date_Correctly()
        {
            // Arrange
            var story = new HackerNewsItem()
            {
                Id = 1,
                Title = "Story 1",
                Url = "https://valid.com/story1",
                By = "user1",
                Score = 10,
                Time = 1756618690
            };

            // Act

            // Assert
            Assert.Equal("8/31/2025 5:38:10 AM +00:00", story.PostedAt().ToString());
        }
    }
}
