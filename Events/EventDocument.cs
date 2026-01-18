using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LibraryCoreApi.Events;

public class EventDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("eventName")]
    public string EventName { get; set; } = string.Empty;

    [BsonElement("eventData")]
    public BsonDocument EventData { get; set; } = new BsonDocument();

    [BsonElement("routingKey")]
    public string RoutingKey { get; set; } = string.Empty;

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("processedAt")]
    public DateTime? ProcessedAt { get; set; }
}
