using ClientesService;
using Microsoft.AspNetCore.Mvc.Testing;
using Shared.Domain;
using Shared.Mock;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Xunit;


namespace CartoesService.Tests;
public class ClientIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient clientesClient;
    private readonly HttpClient propostasClient;
    private readonly HttpClient cartoesClient;

    public ClientIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var clientesFactory = new WebApplicationFactory<ClientesService.Program>();
        clientesClient = clientesFactory.CreateClient();

        var propostasFactory = new WebApplicationFactory<PropostasService.Program>();
        propostasClient = propostasFactory.CreateClient();

        var cartoesFactory = new WebApplicationFactory<CartoesService.Program>();
        cartoesClient = cartoesFactory.CreateClient();
    }

    [Fact]
    public async Task DeveObterCartao()
    {
        
        var response = await cartoesClient.GetAsync($"/api/cartoes?page=1&pageSize=10");
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound, $"Status inesperado: {response.StatusCode}");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var resultado = await response.Content.ReadFromJsonAsync<PagedResult<Cartao>>();
            Assert.NotNull(resultado);
            Assert.NotEmpty(resultado!.Items);
        }
    }

    /*
    [Fact]
    public async Task DeveEmitirCartao()
    {
        var propostaId = "2BE3C8D0-FA2A-4BCB-A158-D7059C6A8F7C";
        Console.WriteLine($"Proposta {propostaId} sendo emitida sem mensageria para teste...");
        var response = await cartoesClient.PostAsJsonAsync("/api/cartoes/emitir",propostaId);
        Assert.True(response.StatusCode == HttpStatusCode.Created, $"Status inesperado: {response.StatusCode} {response}");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var resultado = await response.Content.ReadFromJsonAsync<PagedResult<Cartao>>();
            Assert.NotNull(resultado);
            Assert.NotEmpty(resultado!.Items);
        }
        Console.WriteLine($"Resposta do servidor: {response.StatusCode}");
    }
    */
}
