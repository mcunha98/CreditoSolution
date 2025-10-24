using Data.Factory;
using Data.Repository;
using PropostasService.Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Domain;
using Shared.Messaging;
using System.Text;
using System.Text.Json;
using PropostasService.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

var repo = new PropostaRepository(DbContextFactory.CreateConnection());
repo.Up();

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
        queue: "cliente.criado",
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
        var cliente = JsonSerializer.Deserialize<Cliente>(message);

        Console.WriteLine($"[PropostasService] Cliente {cliente?.Nome} recebido, criando proposta");
        if (cliente is null)
        {
            Console.WriteLine($"[PropostasService] Mensagem invalida recebida");
            return;
        }

        int score = PropostaScoreService.Calculate(cliente);
        var proposta = new Proposta
        {
            ClienteId = cliente.Id,
            Valor = cliente.Renda * 0.8m,
            Score = score,
            Status = PropostaStatus.EmAnalise,
        };
        repo.Insert(proposta);
        await Task.Yield();
    };

    await channel.BasicConsumeAsync(
        queue: "cliente.criado",
        autoAck: true,
        consumer: consumer
    );

    Console.WriteLine("[PropostasService] Consumidor iniciado e aguardando mensagens...");
});


app.MapGet("/api/propostas", (int? page, int? pageSize) =>
{
    int currentPage = page ?? 1;
    int size = pageSize ?? 10;

    var registros = repo.GetAll(currentPage, size).ToList();

    return Results.Ok(new
    {
        pagina = currentPage,
        tamanho = size,
        total = repo.Count(),
        items = registros
    });
});

app.MapPost("/api/propostas/decidir", async (PropostaDecisaoRequest request) =>
{
    if (request == null || request.Id == Guid.Empty)
        return Results.BadRequest(new { erro = "Requisição inválida" });

    var proposta = repo.Find(request.Id);
    if (proposta is null)
        return Results.NotFound(new { erro = $"Proposta {request.Id} não encontrada" });

    if (proposta.Status != PropostaStatus.EmAnalise)
        return Results.BadRequest(new { erro = "Apenas propostas em analise podem ser aprovadas" });

    proposta.Status = request.Aprovado ? PropostaStatus.Aprovada : PropostaStatus.Rejeitada;
    if (repo.Update(proposta))
    {
        var publisher = new MessagePublisher(request.Aprovado ? "proposta.aprovada" : "proposta.rejeitada");
        await publisher.PublishAsync(proposta);
        return Results.NoContent();
    }

    return Results.Json(new { erro = "Erro interno de processamento" }, statusCode: 422);
});

app.MapControllers();
app.Run();

public partial class Program { }