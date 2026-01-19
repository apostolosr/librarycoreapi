namespace LibraryCoreApi.Events;

/// <summary>
/// IEventStore interface to store events in the event store
/// </summary>
public interface IEventStore
{
    Task StoreEventAsync(string eventName, string routingKey, object eventData);
    Task<List<EventDocument>> GetEventsAsync(string? eventName = null, int limit = 100);
    Task DeleteEventsOlderByTimespanAsync(TimeSpan timespan);
}
