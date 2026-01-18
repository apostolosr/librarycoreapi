using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.Json;

namespace LibraryCoreApi.Events;

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

    // store event
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

    // get events by name
    public async Task<List<EventDocument>> GetEventsAsync(string? eventName = null, int limit = 100)
    {
        var filter = eventName != null
            ? Builders<EventDocument>.Filter.Eq(e => e.EventName, eventName)
            : Builders<EventDocument>.Filter.Empty;

        return await _collection
            .Find(filter)
            .SortByDescending(e => e.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }

    // delete all events older than given timespan
    public async Task DeleteEventsByTimespan(TimeSpan timespan)
    {
        var filter = Builders<EventDocument>.Filter.Lte(e => e.Timestamp, DateTime.UtcNow.Subtract(timespan));
        await _collection.DeleteManyAsync(filter);
    }

}
