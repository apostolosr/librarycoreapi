using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace LibraryCoreApi.Events;

/// <summary>
/// RabbitMQEventPublisher class to publish events to RabbitMQ
/// </summary>
public class RabbitMQEventPubliser : IEventPublisher, IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly string _exchangeName;
    private readonly ILogger<RabbitMQEventPubliser> _logger;
    private readonly ConnectionFactory _factory;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public RabbitMQEventPubliser(IConfiguration configuration, ILogger<RabbitMQEventPubliser> logger)
    {
        _logger = logger;
        var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        var port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
        var userName = configuration["RabbitMQ:UserName"] ?? "guest";
        var password = configuration["RabbitMQ:Password"] ?? "guest";
        _exchangeName = configuration["RabbitMQ:ExchangeName"] ?? "library_events";

        _factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password
        };
    }

    /// <summary>
    /// Ensures connection and channel exist; creates them asynchronously on first use.
    /// </summary>
    private async Task EnsureConnectionAsync(CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_channel != null && _channel.IsOpen)
                return;
        
            _connection?.Dispose();
            _channel?.Dispose();

            _connection = await _factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(null, cancellationToken);
            await _channel.ExchangeDeclareAsync(
                exchange: _exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot ensure connection: {ErrorMessage}", ex.Message);
            return;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Publish event to RabbitMQ
    /// </summary>
    /// <typeparam name="T">The type of the event data</typeparam>
    /// <param name="eventName">The name of the event</param>
    /// <param name="eventData">The event data</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task PublishEvent<T>(string eventName, T eventData) where T : class
    {
        try
        {
            await EnsureConnectionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot publish event {EventName}: RabbitMQ connection failed", eventName);
            return;
        }

        if (_channel == null)
        {
            _logger.LogError("Cannot publish event: RabbitMQ channel is not initialized");
            return;
        }

        try
        {
            var message = JsonSerializer.Serialize(eventData);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = new BasicProperties
            {
                Persistent = true,
                Type = eventName,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await _channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: eventName,
                mandatory: true,
                basicProperties: properties,
                body: new ReadOnlyMemory<byte>(body)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error publishing event {eventName}: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }
    }
}
