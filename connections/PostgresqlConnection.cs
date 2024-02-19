using System.Data;
using System.Runtime.InteropServices;
using Npgsql;
namespace StonksWebApp.connections;

public class PostgresqlConnection: IDatabaseConnection
{
    private string _connectionString;

    public PostgresqlConnection(string username, string password, string instance, string db)
    {
        NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder();
        builder.Username = username;
        builder.Password = password;
        builder.Host = instance;
        builder.Database = db;
        builder.Pooling = true;
        builder.MaxPoolSize = 3;
        builder.SslMode = SslMode.Require;

        _connectionString = builder.ToString();

    }

    public int RunUpsert(string query)
    {
        using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        var command = new NpgsqlCommand(query, connection);
        return command.ExecuteNonQuery();
    }

    public int RunUpsert(string query, params KeyValuePair<string, string>[] sqlParams)
    {
        using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        var command = new NpgsqlCommand(query, connection);
        foreach (var pair in sqlParams)
        {
            command.Parameters.AddWithValue(pair.Key, pair.Value);
        }
        return command.ExecuteNonQuery();
    }

    public QueryResult RunQuery(string query, params KeyValuePair<string,string>[] sqlParams)
    {
        using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        var command = new NpgsqlCommand(query,connection);
        foreach (var pair in sqlParams)
        {
            command.Parameters.AddWithValue(pair.Key, pair.Value);
        }
        var reader = command.ExecuteReader();
        if (!reader.HasRows)
        {
            return new QueryResult(Array.Empty<string>(), Array.Empty<QueryRow>());
        }
        var results = new List<QueryRow>();
        var fields = new string[reader.FieldCount];
        var schema = reader.GetColumnSchema();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            fields[i] = schema[i].ColumnName ?? "";
        }

        while (reader.Read())
        {
            dynamic[] row = new dynamic[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[i] = reader.GetValue(i);
            }

            results.Add(new QueryRow(row));
        }

        return new QueryResult(fields,results.ToArray());
    }

    public void CreateTable(string tableSchema)
    {
        using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        NpgsqlCommand command = new NpgsqlCommand(tableSchema,connection);
        command.ExecuteNonQuery();
        connection.Close();
    }
}