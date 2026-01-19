using LibraryCoreApi.Services.Events;
using LibraryCoreApi.Events;
using Xunit;
using Moq;
using MongoDB.Bson;

namespace LibraryCoreApi.Tests.Services;

public class EventsServiceTests
{
    [Fact]
    public async Task TestGetBookEvents_WithDefaultParameters()
    {
        // Arrange
        var mockEventStore = new Mock<IEventStore>();
        var eventDocuments = new List<EventDocument>
        {
            new EventDocument
            {
                EventName = "book.created",
                EventData = BsonDocument.Parse("{\"BookId\": 1, \"Title\": \"Test Book\"}"),
                Timestamp = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            },
            new EventDocument
            {
                EventName = "book.updated",
                EventData = BsonDocument.Parse("{\"BookId\": 2, \"Title\": \"Updated Book\"}"),
                Timestamp = DateTime.UtcNow.AddMinutes(-5),
                ProcessedAt = DateTime.UtcNow
            }
        };

        mockEventStore.Setup(s => s.GetBookEventsAsync(0, 100))
            .ReturnsAsync(eventDocuments);

        var eventsService = new EventsService(mockEventStore.Object);

        // Act
        var result = await eventsService.GetBookEvents();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.LastIndex);
        Assert.Equal(2, result.Events.Count());
        
        var firstEvent = result.Events.First();
        Assert.Equal("book.created", firstEvent.EventName);
        Assert.Contains("BookId", firstEvent.EventData);
        Assert.Contains("1", firstEvent.EventData);
        Assert.NotNull(firstEvent.ProcessedAt);

        var secondEvent = result.Events.Skip(1).First();
        Assert.Equal("book.updated", secondEvent.EventName);
        Assert.Contains("BookId", secondEvent.EventData);
        Assert.Contains("2", secondEvent.EventData);
        Assert.NotNull(secondEvent.ProcessedAt);

        mockEventStore.Verify(s => s.GetBookEventsAsync(0, 100), Times.Once);
    }

    [Fact]
    public async Task TestGetBookEvents_WithCustomParameters()
    {
        // Arrange
        var mockEventStore = new Mock<IEventStore>();
        var eventDocuments = new List<EventDocument>
        {
            new EventDocument
            {
                EventName = "book.deleted",
                EventData = BsonDocument.Parse("{\"BookId\": 3, \"Title\": \"Deleted Book\"}"),
                Timestamp = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            }
        };

        mockEventStore.Setup(s => s.GetBookEventsAsync(10, 50))
            .ReturnsAsync(eventDocuments);

        var eventsService = new EventsService(mockEventStore.Object);

        // Act
        var result = await eventsService.GetBookEvents(10, 50);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(11, result.LastIndex);
        Assert.Single(result.Events);
        Assert.Equal("book.deleted", result.Events.First().EventName);

        mockEventStore.Verify(s => s.GetBookEventsAsync(10, 50), Times.Once);
    }

    [Fact]
    public async Task TestGetBookEvents_WithEmptyResults()
    {
        // Arrange
        var mockEventStore = new Mock<IEventStore>();
        var emptyList = new List<EventDocument>();

        mockEventStore.Setup(s => s.GetBookEventsAsync(0, 100))
            .ReturnsAsync(emptyList);

        var eventsService = new EventsService(mockEventStore.Object);

        // Act
        var result = await eventsService.GetBookEvents();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.LastIndex);
        Assert.Empty(result.Events);

        mockEventStore.Verify(s => s.GetBookEventsAsync(0, 100), Times.Once);
    }

    [Fact]
    public async Task TestGetUserEvents_WithDefaultParameters()
    {
        // Arrange
        var mockEventStore = new Mock<IEventStore>();
        var eventDocuments = new List<EventDocument>
        {
            new EventDocument
            {
                EventName = "user.created",
                EventData = BsonDocument.Parse("{\"UserId\": 1, \"Name\": \"John Doe\"}"),
                Timestamp = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            },
            new EventDocument
            {
                EventName = "user.updated",
                EventData = BsonDocument.Parse("{\"UserId\": 2, \"Name\": \"Jane Doe\"}"),
                Timestamp = DateTime.UtcNow.AddMinutes(-10),
                ProcessedAt = DateTime.UtcNow
            }
        };

        mockEventStore.Setup(s => s.GetUserEventsAsync(0, 100))
            .ReturnsAsync(eventDocuments);

        var eventsService = new EventsService(mockEventStore.Object);

        // Act
        var result = await eventsService.GetUserEvents();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.LastIndex);
        Assert.Equal(2, result.Events.Count());
        
        var firstEvent = result.Events.First();
        Assert.Equal("user.created", firstEvent.EventName);
        Assert.Contains("UserId", firstEvent.EventData);
        Assert.Contains("1", firstEvent.EventData);
        Assert.NotNull(firstEvent.ProcessedAt);

        var secondEvent = result.Events.Skip(1).First();
        Assert.Equal("user.updated", secondEvent.EventName);
        Assert.Contains("UserId", secondEvent.EventData);
        Assert.Contains("2", secondEvent.EventData);
        Assert.NotNull(secondEvent.ProcessedAt);

        mockEventStore.Verify(s => s.GetUserEventsAsync(0, 100), Times.Once);
    }

    [Fact]
    public async Task TestGetUserEvents_WithCustomParameters()
    {
        // Arrange
        var mockEventStore = new Mock<IEventStore>();
        var eventDocuments = new List<EventDocument>
        {
            new EventDocument
            {
                EventName = "user.deleted",
                EventData = BsonDocument.Parse("{\"UserId\": 5, \"Name\": \"Deleted User\"}"),
                Timestamp = DateTime.UtcNow,
                ProcessedAt = null
            }
        };

        mockEventStore.Setup(s => s.GetUserEventsAsync(20, 25))
            .ReturnsAsync(eventDocuments);

        var eventsService = new EventsService(mockEventStore.Object);

        // Act
        var result = await eventsService.GetUserEvents(20, 25);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(21, result.LastIndex);
        Assert.Single(result.Events);
        Assert.Equal("user.deleted", result.Events.First().EventName);

        mockEventStore.Verify(s => s.GetUserEventsAsync(20, 25), Times.Once);
    }

    [Fact]
    public async Task TestGetUserEvents_WithEmptyResults()
    {
        // Arrange
        var mockEventStore = new Mock<IEventStore>();
        var emptyList = new List<EventDocument>();

        mockEventStore.Setup(s => s.GetUserEventsAsync(0, 100))
            .ReturnsAsync(emptyList);

        var eventsService = new EventsService(mockEventStore.Object);

        // Act
        var result = await eventsService.GetUserEvents();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.LastIndex);
        Assert.Empty(result.Events);

        mockEventStore.Verify(s => s.GetUserEventsAsync(0, 100), Times.Once);
    }
}
