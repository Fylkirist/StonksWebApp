using StonksWebApp.connections;

namespace StonksWebApp.models;

public class UserModel
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    public string[] Watchlist { get; set; }
    public DateTime DateCreated { get; set; }

    public UserModel(string username, string password, string role, string[] watchlist, DateTime dateCreated, int id)
    {
        Username = username;
        Password = password;
        Role = role;
        Watchlist = watchlist;
        DateCreated = dateCreated;
        Id = id;
    }

    public static UserModel[] TranslateQueryResult(QueryResult result)
    {
        var models = new List<UserModel>();
        Dictionary<string, int> columnMapping = new Dictionary<string, int>();
        for (int i = 0; i < result.Columns.Length; i++)
        {
            columnMapping[result.Columns[i]] = i;
        }

        foreach (var row in result.Rows)
        {
            int id = columnMapping.ContainsKey("id") ? row.Data[columnMapping["id"]] : -1;
            string username = columnMapping.ContainsKey("email") ? row.Data[columnMapping["email"]]: "";
            string password = columnMapping.ContainsKey("pass") ? row.Data[columnMapping["pass"]]: "";
            string role = columnMapping.ContainsKey("userrole") ? row.Data[columnMapping["userrole"]] : "";
            string[] watchlist = columnMapping.ContainsKey("watchlist") ? row.Data[columnMapping["watchlist"]].Split(",") : Array.Empty<string>();
            DateTime timeCreated = columnMapping.ContainsKey("datecreated") ? row.Data[columnMapping["datecreated"]] : DateTime.MinValue;
            var model = new UserModel(username, password, role, watchlist, timeCreated, id);
            models.Add(model);
        }

        return models.ToArray();
    }
}