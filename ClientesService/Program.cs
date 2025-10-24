using ClientesService.Domain;
using Shared.Domain;
using Shared.Messaging;
using Shared.Mock;
using Data.Factory;
using Data.Repository;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

var repo = new ClienteRepository(DbContextFactory.CreateConnection());
repo.Up();


app.MapGet("/api/clientes", (int? page, int? pageSize) =>
{
    int currentPage = page ?? 1;
    int size = pageSize ?? 10;

    var clientes = repo.GetAll(currentPage, size).ToList();

    return Results.Ok(new
    {
        pagina = currentPage,
        tamanho = size,
        total = repo.Count(),
        items = clientes
    });
});

app.MapPost("/api/clientes", async (Cliente cliente) =>
{
    try
    {
        ClienteValidator.Validate(cliente);
    }
    catch (Exception e)
    {
        return Results.BadRequest(new { erro = e.Message });
    }

    try
    {
        repo.Insert(cliente);
    }
    catch (Exception)
    {
        throw;
    }

    var publisher = new MessagePublisher("cliente.criado");
    await publisher.PublishAsync(cliente);
    return Results.Created($"/api/clientes/{cliente.Id}", cliente);
});

app.Run();

public partial class Program { }