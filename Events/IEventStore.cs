namespace LibraryCoreApi.Events;

public interface IEventStore
{
    Task StoreEventAsync(string eventName, string routingKey, object eventData);
    Task<List<EventDocument>> GetEventsAsync(string? eventName = null, int limit = 100);
}
