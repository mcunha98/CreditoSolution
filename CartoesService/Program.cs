using CartoesService.Domain;
using Data.Factory;
using Data.Repository;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Domain;
using Shared.Messaging;
using Shared.Mock;
using System.Text;
using System.Text.Json;

namespace CartoesService;

public class Startup
{
    public static void Main(string[] args)
    {
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
                Console.WriteLine($"[CartoesService] Recebida mensagem para ser consumida : {message}");

                var proposta = JsonSerializer.Deserialize<Proposta>(message);
                if (proposta is null)
                {
                    Console.WriteLine($"[CartoesService] Proposta não encontrada");
                    return;
                }

                try
                {
                     new CartaoEmissor(repoCartao).Emitir(proposta);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[CartoesService] Erro ao emitir cartão: {e.Message}");
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

            return Results.Ok(new PagedResult<Cartao>
            {
                Pagina = currentPage,
                Tamanho = size,
                Total = repoCartao.Count(),
                Items = registros
            });
        });

        app.MapGet("/api/cartoes/proposta/{id:guid}", (Guid id) =>
        {
            var registros = repoCartao.GetByProposta(id).ToList();
            return Results.Ok(new PagedResult<Cartao>
            {
                Pagina = 1,
                Tamanho = registros.Count,
                Total = registros.Count,
                Items = registros
            });
        });

        app.MapGet("/api/cartoes/cliente/{id:guid}", (Guid id) =>
        {
            var registros = repoCartao.GetByCliente(id).ToList();
            return Results.Ok(new PagedResult<Cartao>
            {
                Pagina = 1,
                Tamanho = registros.Count,
                Total = registros.Count,
                Items = registros
            });
        });

        app.MapPost("/api/cartoes/emitir", ([FromBody] Guid propostaId) =>
        {
            var proposta = repoProposta.Find(propostaId);
            if (proposta is null)
            {
                Console.WriteLine($"[CartoesService] Proposta não encontrada");
                return Results.NotFound();
            }

            try
            {
                new CartaoEmissor(repoCartao).Emitir(proposta);
                return Results.Created();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[CartoesService] Erro ao emitir cartão: {e.Message}");
                return Results.Problem(e.Message + " -> " + e.StackTrace);
            }
        });

        app.MapControllers();
        app.Run();
    }
}

public partial class Program { }
