namespace LibraryCoreApi.Events;

/// <summary>
/// IEventStore interface to store events in the event store
/// </summary>
public interface IEventStore
{
    Task StoreEventAsync(string eventName, string routingKey, object eventData);
    Task<List<EventDocument>> GetBookEventsAsync(int lastIndex = 0, int pageSize = 100);
    Task<List<EventDocument>> GetUserEventsAsync(int lastIndex = 0, int pageSize = 100);
    Task DeleteEventsOlderByTimespanAsync(TimeSpan timespan);
}
