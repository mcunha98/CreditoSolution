using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Shared.Messaging;

public class MessagePublisher
{
    private readonly string _queueName;

    public MessagePublisher(string queueName)
    {
        _queueName = queueName;
    }

    public async Task PublishAsync<T>(T message)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            Port = 5672
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Transient // não persistente
        };

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: _queueName,
            mandatory: false,
            //basicProperties: props,
            body: body
        );
        Console.WriteLine($"[MessagePublisher] Mensagem publicada na fila '{_queueName}'");
    }
}
