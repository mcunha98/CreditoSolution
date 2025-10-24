using Microsoft.Data.Sqlite;
using System.Data;

namespace Data.Factory;

public static class DbContextFactory
{
    private const string DatabaseFileName = "creditosolution.db";

    public static IDbConnection CreateConnection()
    {
        var basePath = Path.GetTempPath(); 
        var dbPath = Path.Combine(basePath, DatabaseFileName);

        Console.WriteLine($"Banco de dados : {dbPath}");
        var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();
        return connection;
    }
}