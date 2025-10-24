using ClientesService;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Shared.Domain;
using Shared.Mock;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ClientesService.Tests;

public class ClientIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ClientIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeveCriarCliente()
    {
        var cliente = new Cliente
        {
            Nome = ClienteMock.GerarNome(),
            Cpf = ClienteMock.GerarCPF(),
            Renda = 1500m,
            Nascimento = ClienteMock.GerarNascimento(),
            Segmento = ClienteMock.EnumRandom<ClienteSegmento>(),
        };

        var response = await _client.PostAsJsonAsync("/api/clientes", cliente);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var retorno = await response.Content.ReadFromJsonAsync<Cliente>();
        Assert.Equal(cliente.Nome, retorno?.Nome);
        Assert.NotEqual(Guid.Empty, retorno?.Id);
    }
}
