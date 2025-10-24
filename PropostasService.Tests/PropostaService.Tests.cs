using Microsoft.AspNetCore.Mvc.Testing;
using PropostasService.Models;
using Shared.Domain;
using Shared.Mock;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace PropostasService.Tests;

public class ClientIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient clientesClient;
    private readonly HttpClient propostasClient;

    public ClientIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var clientesFactory = new WebApplicationFactory<ClientesService.Program>();
        clientesClient = clientesFactory.CreateClient();

        var propostasFactory = new WebApplicationFactory<PropostasService.Program>();
        propostasClient = propostasFactory.CreateClient();
    }

    [Fact]
    public async Task DeveCriarProposta()
    {
        var cliente = new Cliente
        {
            Nome = ClienteMock.GerarNome(),
            Cpf = ClienteMock.GerarCPF(),
            Renda = 1500m,
            Nascimento = ClienteMock.GerarNascimento(),
            Segmento = ClienteMock.EnumRandom<ClienteSegmento>(),
        };

        var response = await clientesClient.PostAsJsonAsync("/api/clientes", cliente);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var retorno = await response.Content.ReadFromJsonAsync<Cliente>();
        if (retorno == null)
            throw new Exception("Retorno nulo ao criar cliente");

        Assert.Equal(cliente.Nome, retorno?.Nome);
        Assert.NotEqual(Guid.Empty, retorno?.Id);

        await Task.Delay(1000);
        var clienteId = retorno?.Id;
        response = await propostasClient.GetAsync($"/api/propostas/cliente/{clienteId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var resultado = await response.Content.ReadFromJsonAsync<PagedResult<Proposta>>();
        Assert.NotNull(resultado);
        Assert.NotEmpty(resultado!.Items);
        Assert.Contains(resultado.Items, p => p.ClienteId == clienteId);
    }

    [Fact]
    public async Task DeveAprovarProposta()
    {
        var response = await propostasClient.GetAsync($"/api/propostas?page=1&pagesize=5");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var resultado = await response.Content.ReadFromJsonAsync<PagedResult<Proposta>>();
        Assert.NotNull(resultado);
        Assert.NotEmpty(resultado!.Items);
        
        foreach(var item in resultado.Items)
        {
            if (item.Status == PropostaStatus.EmAnalise)
            {
                var payload = new PropostaDecisaoRequest { Id = item.Id, Aprovado = (item.Score >= 200) };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var decidirResponse = await propostasClient.PostAsync("/api/propostas/decidir", content);
                Assert.Equal(HttpStatusCode.NoContent, decidirResponse.StatusCode);
            }
        }
    }

}
