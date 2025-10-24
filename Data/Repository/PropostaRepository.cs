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
    public class PropostaRepository
    {
        private readonly IDbConnection _connection;

        public PropostaRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public void Up()
        {
            _connection.Execute("""
            CREATE TABLE IF NOT EXISTS Propostas (
                Id TEXT COLLATE NOCASE PRIMARY KEY,
                ClienteId TEXT COLLATE NOCASE,
                Status INTEGER,
                Score INTEGER,
                Valor REAL,
                Criado TEXT,
                Alterado TEXT
            );
        """);
        }

        public void Insert(Proposta proposta)
        {
            _connection.Execute("""
            INSERT INTO Propostas (Id, ClienteId, Status, Score, Valor, Criado)
            VALUES (@Id, @ClienteId, @Status, @Score, @Valor, @Criado);
        """, proposta);
        }

        public IEnumerable<Proposta> GetAll(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            var offset = (page - 1) * pageSize;

            var sql = $"SELECT * FROM Propostas ORDER by Criado desc LIMIT {pageSize} OFFSET {offset}";
            var result = _connection.Query(sql);

            foreach (var row in result)
            {
                yield return Fetch(row);
            }
        }

        public IEnumerable<Proposta> GetByCliente(Guid id)
        {
            var sql = "SELECT * FROM Propostas WHERE ClienteId = @Id ORDER BY Criado desc";
            var result = _connection.Query(sql, new { Id = id.ToString() });

            foreach (var row in result)
            {
                yield return Fetch(row);
            }
        }

        public int Count()
        {
            return _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Propostas");
        }

        public Proposta? Find(Guid id)
        {
            var sql = "SELECT * FROM Propostas WHERE Id = @Id LIMIT 1";
            var row = _connection.QueryFirstOrDefault(sql, new { Id = id.ToString() });

            if (row == null) 
                return null;

            return Fetch(row);
        }

        public bool Update(Proposta proposta)
        {
            var sql = """
                UPDATE 
                    Propostas
                SET
                    Valor = @Valor,
                    Score = @Score,
                    Status = @Status,
                    Alterado=datetime('now')
                WHERE Id = @Id
            """;

            int rows = _connection.Execute(sql, proposta);
            return rows > 0;
        }

        private Proposta? Fetch(dynamic? row)
        {
            if (row == null) 
                return null;

            return new Proposta
            {
                Id = Guid.Parse((string)row.Id),
                ClienteId = Guid.Parse((string)row.ClienteId),
                Valor = (decimal)row.Valor,
                Score = (int)row.Score,
                Status = (PropostaStatus)(int)row.Status,
                Criado = DateTime.Parse((string)row.Criado),
                Alterado = string.IsNullOrEmpty((string)row.Alterado) ? null : DateTime.Parse((string)row.Alterado),
            };
        }
    }
}
