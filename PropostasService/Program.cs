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

namespace PropostasService;

public class Startup
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        var app = builder.Build();

        var repo = new PropostaRepository(DbContextFactory.CreateConnection());
        repo.Up();

        // Configura o consumidor RabbitMQ ao iniciar o serviço
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
                    Console.WriteLine("[PropostasService] Mensagem inválida recebida");
                    return;
                }

                int score = PropostaScoreService.Calculate(cliente);

                var proposta = new Proposta
                {
                    ClienteId = cliente.Id,
                    Valor = cliente.Renda * 0.8m,
                    Score = score,
                    Status = PropostaStatus.EmAnalise
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

        // Endpoint para listar propostas
        app.MapGet("/api/propostas", (int? page, int? pageSize) =>
        {
            int currentPage = page ?? 1;
            int size = pageSize ?? 10;

            var registros = repo.GetAll(currentPage, size).ToList();

            return Results.Ok(new PagedResult<Proposta>(){Pagina = currentPage, Tamanho = size, Total = repo.Count(), Items = registros});
        });

        app.MapGet("/api/propostas/cliente/{id:guid}", (Guid id) =>
        {
            var registros = repo.GetByCliente(id).ToList();
            return Results.Ok(new { pagina = 1, tamanho = registros.Count, total = registros.Count, items = registros });
        });

        // Endpoint para decisão de proposta
        app.MapPost("/api/propostas/decidir", async (PropostaDecisaoRequest request) =>
        {
            if (request == null || request.Id == Guid.Empty)
                return Results.BadRequest(new { erro = "Requisição inválida" });

            var proposta = repo.Find(request.Id);
            if (proposta is null)
                return Results.NotFound(new { erro = $"Proposta {request.Id} não encontrada" });

            if (proposta.Status != PropostaStatus.EmAnalise)
                return Results.BadRequest(new { erro = "Apenas propostas em análise podem ser aprovadas" });

            proposta.Status = request.Aprovado ? PropostaStatus.Aprovada : PropostaStatus.Rejeitada;

            if (repo.Update(proposta))
            {
                var publisher = new MessagePublisher(request.Aprovado ? "proposta.aprovada" : "proposta.rejeitada");
                await publisher.PublishAsync(proposta);
                return Results.NoContent();
            }

            return Results.Json(new { erro = "Erro interno de processamento" }, statusCode: 422);
        });

        app.Run();
    }
}
public partial class Program { }

