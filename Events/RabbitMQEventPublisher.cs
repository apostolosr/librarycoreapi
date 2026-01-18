using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace LibraryCoreApi.Events;

public class RabbitMQEventPubliser : IEventPublisher, IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly string _exchangeName;

    private readonly ILogger<RabbitMQEventPubliser> _logger;

    private readonly ConnectionFactory _factory;

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
        
        // Initialize connection synchronously in constructor
        CreateConnectionAsync().GetAwaiter().GetResult();
    }

    private async Task CreateConnectionAsync()
    {
        _connection = await _factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
        await _channel.ExchangeDeclareAsync(
            exchange: _exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );
    }

    public async Task PublishEvent<T>(string eventName, T eventData) where T : class
    {
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
