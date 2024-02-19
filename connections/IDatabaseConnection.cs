namespace StonksWebApp.connections;

public interface IDatabaseConnection
{
    public int RunUpsert(string query);
    public int RunUpsert(string query, params KeyValuePair<string, string>[] sqlParams);
    public QueryResult RunQuery(string query, params KeyValuePair<string,string>[] sqlParams);
    public void CreateTable(string tableSchema);
}


public class QueryResult
{
    public string[] Columns { get; set; }
    public QueryRow[] Rows { get; set; }
    public QueryResult(string[] columns, QueryRow[] rows)
    {
        Columns = columns;
        Rows = rows;
    }
}

public class QueryRow
{
    public dynamic[] Data;

    public QueryRow(dynamic[] data)
    {
        Data = data;
    }
}