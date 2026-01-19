namespace LibraryCoreApi.Events;

/// <summary>
/// IEventPublisher interface to publish events to the message broker
/// </summary>
public interface IEventPublisher
{
    Task PublishEvent<T>(string eventName, T eventData) where T : class;
}
