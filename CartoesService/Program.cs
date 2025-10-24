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

        if (proposta.Status != PropostaStatus.Aprovada)
        {
            Console.WriteLine($"[CartaoService] Proposta {propostaId} nao esta aprovada");
            return;
        }

        if (proposta.Score <= 100)
        {
            Console.WriteLine($"[CartaoService] Proposta {propostaId} com score insuficiente");
            return;
        }

        int quantidade = 0;
        decimal limite = 0m;

        Console.WriteLine($"[CartaoService] Proposta {proposta.Id} aprovada, gerando cartoes");
        //De 101 a 500 – Permitido liberação de cartão de crédito(limite R$ 1.000,00);
        //De 501 a 1000 – Permitido liberação de até 2 cartão de crédito (limite R$ 5.000,00) cada.
        if (proposta.Score >= 101 && proposta.Score <= 500)
        {
            quantidade = 1;
            limite = 1000m;
        }
        else
        {
            quantidade = 2;
            limite = 5000m;
        }

        if (quantidade <= 0 || limite <= 0m)
        {
            Console.WriteLine($"[CartaoService] Proposta {propostaId} tem score invalido para determinar produtos selecionados");
            return;
        }

        for (int i = 0; i < quantidade; i++)
        {
            var plastico = CartaoMock.GerarCartao();

            var cartao = new Cartao
            {
                ClienteId  = proposta.ClienteId,
                PropostaId = proposta.Id,
                Numero = plastico.Numero,
                Bandeira = plastico.Bandeira,
                Validade = plastico.Validade,
                Limite = limite,
            };
            repoCartao.Insert(cartao);
            Console.WriteLine($"[CartaoService] Cartao {cartao.Numero} gerado para proposta {proposta.Id}");
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


app.MapControllers();
app.Run();

public partial class Program { }