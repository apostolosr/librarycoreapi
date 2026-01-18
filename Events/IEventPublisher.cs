namespace LibraryCoreApi.Events;

public interface IEventPublisher
{
    Task PublishEvent<T>(string eventName, T eventData) where T : class;
}
