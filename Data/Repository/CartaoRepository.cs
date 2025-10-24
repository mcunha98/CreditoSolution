using Dapper;
using Shared.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository
{
    public class CartaoRepository
    {
        private readonly IDbConnection _connection;

        public CartaoRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public void Up()
        {
            _connection.Execute("""
            CREATE TABLE IF NOT EXISTS Cartoes (
                Id TEXT COLLATE NOCASE PRIMARY KEY,
                PropostaId TEXT COLLATE NOCASE,
                ClienteId TEXT COLLATE NOCASE,
                Numero TEXT,
                Bandeira TEXT,
                Validade TEXT,
                Limite REAL,
                Criado TEXT,
                Alterado TEXT
            );
        """);
        }

        public void Insert(Cartao cartao)
        {
            _connection.Execute("""
            INSERT INTO Cartoes (Id, PropostaId, ClienteId, Numero, Bandeira, Validade, Limite, Criado)
            VALUES (@Id, @PropostaId, @ClienteId, @Numero, @Bandeira, @Validade, @Limite, @Criado);
        """, cartao);
        }

        public IEnumerable<Cartao> GetAll(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            var offset = (page - 1) * pageSize;

            var sql = $"SELECT * FROM Cartoes ORDER BY Criado DESC LIMIT {pageSize} OFFSET {offset}";
            var result = _connection.Query(sql);

            foreach (var row in result)
            {
                yield return new Cartao
                {
                    Id = Guid.Parse((string)row.Id),
                    ClienteId = Guid.Parse((string)row.ClienteId),
                    PropostaId = Guid.Parse((string)row.ClienteId),
                    Numero = (string)row.Numero,
                    Bandeira = (string)row.Bandeira,
                    Limite = (decimal)row.Valor,
                    Validade = (string)row.Validade,
                    Criado = string.IsNullOrEmpty((string)row.Criado) ? DateTime.Now : DateTime.Parse((string)row.Criado),
                    Alterado = string.IsNullOrEmpty((string)row.Alterado) ? null : DateTime.Parse((string)row.Alterado)
                };
            }
        }

        public IEnumerable<Cartao> GetByProposta(Guid id)
        {
            var sql = "SELECT * FROM Cartoes WHERE PropostaId = @Id ORDER BY Criado desc";
            var result = _connection.Query(sql, new { Id = id.ToString() });

            foreach (var row in result)
            {
                yield return new Cartao
                {
                    Id = Guid.Parse((string)row.Id),
                    ClienteId = Guid.Parse((string)row.ClienteId),
                    PropostaId = Guid.Parse((string)row.ClienteId),
                    Numero = (string)row.Numero,
                    Bandeira = (string)row.Bandeira,
                    Limite = (decimal)row.Valor,
                    Validade = (string)row.Validade,
                    Criado = string.IsNullOrEmpty((string)row.Criado) ? DateTime.Now : DateTime.Parse((string)row.Criado),
                    Alterado = string.IsNullOrEmpty((string)row.Alterado) ? null : DateTime.Parse((string)row.Alterado)
                };
            }
        }

        public IEnumerable<Cartao> GetByCliente(Guid id)
        {
            var sql = "SELECT * FROM Cartoes WHERE ClienteId = @Id ORDER BY Criado desc";
            var result = _connection.Query(sql, new { Id = id.ToString() });

            foreach (var row in result)
            {
                yield return new Cartao
                {
                    Id = Guid.Parse((string)row.Id),
                    ClienteId = Guid.Parse((string)row.ClienteId),
                    PropostaId = Guid.Parse((string)row.ClienteId),
                    Numero = (string)row.Numero,
                    Bandeira = (string)row.Bandeira,
                    Limite = (decimal)row.Valor,
                    Validade = (string)row.Validade,
                    Criado = string.IsNullOrEmpty((string)row.Criado) ? DateTime.Now : DateTime.Parse((string)row.Criado),
                    Alterado = string.IsNullOrEmpty((string)row.Alterado) ? null : DateTime.Parse((string)row.Alterado)
                };
            }
        }

        public int Count()
        {
            return _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Cartoes");
        }

        public Cartao? Find(Guid id)
        {
            var sql = "SELECT * FROM Cartoes WHERE Id = @Id LIMIT 1";
            return _connection.QueryFirstOrDefault<Cartao>(sql, new { Id = id.ToString() });
        }

        public bool Update(Cartao cartao)
        {
            var sql = """
                UPDATE Cartoes
                SET
                    Limite = @Limite,
                    Alterado = datetime('now')
                WHERE Id = @Id
            """;

            int rows = _connection.Execute(sql, cartao);
            return rows > 0;
        }
    }
}
