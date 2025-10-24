using CartoesService.Domain;
using Data.Factory;
using Data.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Domain;
using Shared.Messaging;
using Shared.Mock;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

var connection = DbContextFactory.CreateConnection();
var repoCartao = new CartaoRepository(connection);
var repoProposta = new PropostaRepository(connection);
repoCartao.Up();

app.Lifetime.ApplicationStarted.Register(async () =>
{

    var factory = new ConnectionFactory()
    {
        HostName = "localhost",
        UserName = "guest",
        Password = "guest",
        Port = 5672
    };

    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();

    await channel.QueueDeclareAsync(
        queue: "proposta.aprovada",
        durable: false,
        exclusive: false,
        autoDelete: false,
        arguments: null
    );

    var consumer = new AsyncEventingBasicConsumer(channel);
    consumer.ReceivedAsync += async (sender, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var propostaId = JsonSerializer.Deserialize<Guid>(message);

        var proposta = repoProposta.Find(propostaId);
        if (proposta is null)
        {
            Console.WriteLine($"[CartaoService] Proposta {propostaId} nao encontrada");
            return;
        }

        try
        {
            var emissor = new CartaoEmissor(repoCartao);
            emissor.Emitir(proposta);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[CartaoService] {e.Message}");
        }

        await Task.Yield();
    };

    await channel.BasicConsumeAsync(
        queue: "proposta.aprovada",
        autoAck: true,
        consumer: consumer
    );

    Console.WriteLine("[CartoesService] Consumidor iniciado e aguardando mensagens...");
});


app.MapGet("/api/cartoes", (int? page, int? pageSize) =>
{
    int currentPage = page ?? 1;
    int size = pageSize ?? 10;
    var registros = repoCartao.GetAll(currentPage, size).ToList();

    return Results.Ok(new
    {
        pagina = currentPage,
        tamanho = size,
        total = repoCartao.Count(),
        items = registros
    });
});

app.MapGet("/api/cartoes/proposta/{id:guid}", (Guid id) =>
{
    var registros = repoCartao.GetByProposta(id).ToList();
    return Results.Ok(new { pagina = 1, tamanho = registros.Count, total = registros.Count, items = registros });
});

app.MapGet("/api/cartoes/cliente/{id:guid}", (Guid id) =>
{
    var registros = repoCartao.GetByCliente(id).ToList();
    return Results.Ok(new { pagina = 1, tamanho = registros.Count, total = registros.Count, items = registros });
});


app.MapControllers();
app.Run();

public partial class Program { }