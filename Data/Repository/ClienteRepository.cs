using Dapper;
using Shared.Domain;
using Shared.Mock;
using System.Data;

namespace Data.Repository;

public class ClienteRepository
{
    private readonly IDbConnection _connection;

    public ClienteRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public void Up()
    {
        _connection.Execute("""
            CREATE TABLE IF NOT EXISTS Clientes (
                Id TEXT PRIMARY KEY,
                Nome TEXT,
                Cpf TEXT,
                Renda REAL,
                Nascimento TEXT,
                Segmento INTEGER,
                Criado TEXT,
                Alterado TEXT
            );
        """);


        var clientes = Enumerable.Range(1, 10)
        .Select(_ => new Cliente
        {
            Nome = ClienteMock.GerarNome(),
            Cpf = ClienteMock.GerarCPF(),
            Renda = ClienteMock.GerarRenda(),
            Nascimento = ClienteMock.GerarNascimento(),
            Criado = DateTime.Now,
        }).ToList();

        foreach (var cliente in clientes)
        {
            Insert(cliente);
        }
    }

    public void Insert(Cliente cliente)
    {
        _connection.Execute("""
            INSERT INTO Clientes (Id, Nome, Cpf, Renda, Nascimento, Segmento, Criado)
            VALUES (@Id, @Nome, @Cpf, @Renda, @Nascimento, @Segmento, @Criado);
        """, cliente);
    }

    public IEnumerable<Cliente> GetAll(int page = 1, int pageSize = 10)
    {
        if (page < 1) page = 1;
        var offset = (page - 1) * pageSize;

        var sql = $"SELECT * FROM Clientes LIMIT {pageSize} OFFSET {offset}";
        var result = _connection.Query(sql);

        foreach (var row in result)
        {
            yield return new Cliente
            {
                Id = Guid.Parse((string)row.Id),
                Nome = (string)row.Nome,
                Cpf = (string)row.Cpf,
                Renda = (decimal)row.Renda,
                Nascimento = DateTime.Parse((string)row.Nascimento),
                Segmento = (ClienteSegmento)(int)row.Segmento,
                Criado = string.IsNullOrEmpty((string)row.Criado) ? DateTime.Now : DateTime.Parse((string)row.Criado),
                Alterado = string.IsNullOrEmpty((string)row.Alterado) ? null : DateTime.Parse((string)row.Alterado),
            };
        }
    }

    public int Count()
    {
        return _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Clientes");
    }
}
