using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.Json;

namespace LibraryCoreApi.Events;

/// <summary>
/// MongoEventStore class to store events in MongoDB
/// </summary>
public class MongoEventStore : IEventStore
{
    private readonly IMongoCollection<EventDocument> _collection;
    private readonly ILogger<MongoEventStore> _logger;

    public MongoEventStore(IConfiguration configuration, ILogger<MongoEventStore> logger)
    {
        _logger = logger;
        
        var connectionString = configuration["MongoDB:ConnectionString"] 
            ?? "mongodb://admin:admin@mongodb:27017";
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "library_events";
        
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _collection = database.GetCollection<EventDocument>("events");

        // Create index on eventName and timestamp for better query performance
        var indexKeys = Builders<EventDocument>.IndexKeys
            .Ascending(e => e.EventName)
            .Descending(e => e.Timestamp);
        var indexOptions = new CreateIndexOptions { Name = "eventName_timestamp_idx" };
        _collection.Indexes.CreateOne(new CreateIndexModel<EventDocument>(indexKeys, indexOptions));
    }

    /// <summary>
    /// Store event in the event store
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="routingKey"></param>
    /// <param name="eventData"></param>
    /// <returns></returns>
    public async Task StoreEventAsync(string eventName, string routingKey, object eventData)
    {
        try
        {
            // Serialize event data to JSON, then convert to BSON document
            var jsonString = JsonSerializer.Serialize(eventData);
            var bsonDoc = BsonDocument.Parse(jsonString);

            var eventDoc = new EventDocument
            {
                EventName = eventName,
                RoutingKey = routingKey,
                EventData = bsonDoc,
                Timestamp = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            };

            await _collection.InsertOneAsync(eventDoc);
            _logger.LogInformation("Event stored: {EventName} with routing key {RoutingKey}", eventName, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing event {EventName} to MongoDB", eventName);
            throw;
        }
    }


    /// <summary>
    /// Get book-related (book or category) events from the event store
    /// </summary>
    /// <param name="limit">The maximum number of events to return</param>
    /// <returns>A list of EventDocument objects</returns>
    public async Task<List<EventDocument>> GetBookEventsAsync(int limit = 100)
    {
        var filter = Builders<EventDocument>.Filter.Regex(e => e.EventName, new BsonRegularExpression("^(book|category)\\..*$"));
        return await _collection
            .Find(filter)
            .SortByDescending(e => e.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Get user-related events (reservation, party, role) from the event store
    /// </summary>
    /// <param name="limit">The maximum number of events to return</param>
    /// <returns>A list of EventDocument objects</returns>
    public async Task<List<EventDocument>> GetUserEventsAsync(int limit = 100)
    {   
        var regex = new BsonRegularExpression("^(reservation|party|role)\\..*$");
        var filter = Builders<EventDocument>.Filter.Regex(e => e.EventName, regex);
        return await _collection
            .Find(filter)
            .SortByDescending(e => e.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }
    

    /// <summary>
    /// Delete events from the event store older than given timespan
    /// </summary>
    /// <param name="timespan">The timespan to delete events older than</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task DeleteEventsOlderByTimespanAsync(TimeSpan timespan)
    {
        var filter = Builders<EventDocument>.Filter.Lte(e => e.Timestamp, DateTime.UtcNow.Subtract(timespan));
        await _collection.DeleteManyAsync(filter);
    }

}
