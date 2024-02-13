using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using StonksWebApp.connections;
using StonksWebApp.models;

namespace StonksWebApp.Services;

public class DatabaseConnectionService
{
    private static DatabaseConnectionService? _instance;
    public static DatabaseConnectionService Instance => _instance ?? throw new Exception("No DB connection intitialized");

    private IDatabaseConnection _connection;
    public DatabaseConnectionService(IDatabaseConnection connection)
    {
        _connection = connection;
        _instance = this;
    }

    public CompanyFinancialModel[] GetCompanies(string search)
    {
        var query = "SELECT * FROM Companies";
        if (!string.IsNullOrEmpty(search))
        {
            query += " WHERE LOWER(CName) LIKE LOWER(@search) OR LOWER(Ticker) LIKE LOWER(@search) OR LOWER(Sector) LIKE LOWER(@search)";
        }

        query += " LIMIT 25;";
        var result = _connection.RunQuery(query,new KeyValuePair<string, string>("@search",$"%{search}%"));

        return CompanyFinancialModel.ConvertQueryResults(result);
    }

    public void DeleteCompany(int cik)
    {
        var query = $@"START TRANSACTION;
            DELETE FROM Filings 
            WHERE CompanyId IN (SELECT Id FROM Companies WHERE CIK = {cik});
            DELETE FROM Companies 
            WHERE CIK = {cik};
            COMMIT;
            ";
        int numRows = _connection.RunUpsert(query);
        Console.WriteLine(query);
        Console.WriteLine($"{numRows} rows deleted");
    }

    public int UpsertRows(string query)
    {
        return _connection.RunUpsert(query);
    }

    public void CreateCompanyTable()
    {
        var query = @"
            START TRANSACTION;
            CREATE TABLE IF NOT EXISTS Companies (
            Id SERIAL PRIMARY KEY,
            CName VARCHAR(255),
            CIK INT,
            Ticker VARCHAR(255),
            FiscalYearEnd DATE,
            Shares INT,
            Sector VARCHAR(50)
            );
            ALTER TABLE Companies ADD CONSTRAINT unique_cik_constraint UNIQUE (CIK);
            COMMIT;
        ";
        _connection.CreateTable(query);
        Console.WriteLine("Companies table initialized.");
    }

    public void CreateUsersTable()
    {
        string query = @"CREATE TABLE IF NOT EXISTS Users(
            Id SERIAL PRIMARY KEY,
            Email VARCHAR(255),
            Pass VARCHAR(255),
            DateCreated DATE,
            Watchlist TEXT,
            UserRole VARCHAR(10),
            Validated INTEGER
        );";
        _connection.CreateTable(query);
        Console.WriteLine("Users table initialized");
    }

    public void CreateFilingsTable()
    {
        string query = @"CREATE TABLE IF NOT EXISTS Filings(
            Id SERIAL PRIMARY KEY,
            CompanyId INTEGER NOT NULL,
            Link VARCHAR(2048) NOT NULL,
            FilingDate DATE NOT NULL,
            FilingType VARCHAR(30) NOT NULL,
            Org VARCHAR(10) NOT NULL,
            FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
        );";
        _connection.CreateTable(query);
        Console.WriteLine("Filings table initialized.");
    }

    public void CreateAdminUser(IConfiguration config)
    {
        try
        {
            CreateNewUser(config["Credentials:Admin:Email"], config["Credentials:Admin:Password"], true);
            Console.WriteLine($"Admin user generated with username: {config["Credentials:Admin:Email"]}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Admin user could not be generated: {e.Message}");
        }
    }

    public void CreateHistoricalPriceTable()
    {
        string query = @"CREATE TABLE Prices(
            Id SERIAL PRIMARY KEY,
            CompanyId INTEGER NOT NULL,
            PDate DATE NOT NULL,
            Open REAL NOT NULL,
            Close REAL NOT NULL,
            High REAL NOT NULL,
            Low REAL NOT NULL,
            Volume INTEGER NOT NULL,
            FOREIGN KEY(CompanyId) REFERENCES Companies(Id)
        );";
        _connection.CreateTable(query);
        Console.WriteLine("Prices table initialized");
    }

    public void CreatePortfolioTables()
    {
        string query1 = @"CREATE TABLE Portfolios(
            Id SERIAL PRIMARY KEY,
            UserId INTEGER,
            DateAdded DATE,
            PortfolioName VARCHAR(255),
            Capital REAL,
            FOREIGN KEY (UserId) REFERENCES Users(Id)
        );";

        string query2 = @"CREATE TABLE PortfolioOrders(
            Id SERIAL PRIMARY KEY,
            PortfolioId INTEGER,
            CompanyId INTEGER,
            OrderType INTEGER,
            OrderDate DATE,
            OrderPrice REAL,
            OrderSize INTEGER,
            FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
            FOREIGN KEY (PortfolioId) REFERENCES Portfolios(Id)
        );";

        _connection.CreateTable(query1);
        Console.WriteLine("Portfolios table initialized");
        _connection.CreateTable(query2);
        Console.WriteLine("Orders table initialized");
    }

    public void DropAllTables()
    {
        string query = $@"DO $$ DECLARE
            rec RECORD;
        BEGIN
            FOR rec IN (SELECT tablename FROM pg_catalog.pg_tables WHERE schemaname = 'public') LOOP
                EXECUTE format('DROP TABLE IF EXISTS %I.%I CASCADE;', 'public', rec.tablename);
            END LOOP;
        END $$;";
        _connection.RunUpsert(query);
        Console.WriteLine("All Db tables dropped.");
    }

    public UserModel? GetUser(string username)
    {
        string query = @"SELECT * FROM Users WHERE LOWER(Email) LIKE LOWER(@Username) LIMIT 1;";
        var result = _connection.RunQuery(query,  new KeyValuePair<string, string>("Username", username));
        UserModel[] users = UserModel.TranslateQueryResult(result);
        return users.Length >= 1? users[0]: null;
    }

    public bool ToggleWatchlistItem(string username, string ticker, bool add)
    {
        var user = GetUser(username);
        List<string> result = user.Watchlist.ToList();
        result.Remove("");
        bool added;
        if (add)
        {
            if (!result.Contains(ticker))
            {
                result.Add(ticker);
                UpsertRows(@$"
                    UPDATE Users
                    SET Watchlist = '{string.Join(',',result)}'
                    WHERE LOWER(Email) = LOWER('{username}');
                ");
            }
            
            added = true;
        }
        else
        {
            result.Remove(ticker);
            added = false;
            UpsertRows($@"
                UPDATE Users
                SET Watchlist = '{string.Join(',', result)}'
                WHERE LOWER(Email) = LOWER('{username}');
            ");
        }

        return added;
    }

    public void CreateNewUser(string email, string password, bool admin)
    {
        DateTime creationDate = DateTime.UtcNow;
        string hashedPassword = LoginManagerService.HashPassword(password);
        string role = admin ? "admin" : "user";
        string query = @$"INSERT INTO Users (Id, Email, Pass, DateCreated, Watchlist, UserRole, Validated) 
                        VALUES (DEFAULT, '{email}', '{hashedPassword}', '{creationDate}', '', '{role}', 0)";
        _connection.RunUpsert(query);
    }

    public PriceCandleModel[] GetHistoricalPrices(string ticker, DateTime from, DateTime to)
    {
        var query = $@"SELECT Prices.*
            FROM Prices
            INNER JOIN Companies ON Prices.company_id = Companies.id
            WHERE Companies.ticker = @ticker
              AND Prices.date BETWEEN {from} AND {to};
            ";

        var result = _connection.RunQuery(query, new KeyValuePair<string, string>("ticker",ticker));
        var converted = PriceCandleModel.ConvertQueryResult(result).ToList();
        var dateTimes = GenerateDatetimes(from, to, converted);

        if (dateTimes.Count > 0)
        { 
            var newPrices = FetchingService.GetHistoricalPrices(ticker, from, to);
            var uniquePrices = newPrices.Where(e => converted.All(c => c.Date != e.Date)).ToArray();
            int cId = _connection.RunQuery($"SELECT Id FROM Companies WHERE Ticker = '{ticker}'").Rows[0].Data[0];
            foreach (var price in uniquePrices)
            {
                _connection.RunUpsert($@"
                    INSERT INTO Prices(Id, CompanyId, PDate, Open, Close, High, Low, Volume) 
                    VALUES (DEFAULT, {cId}, {price.Date}, {price.Open}, {price.Close}, {price.High}, {price.Low}, {price.Volume});
                ");
            }
            converted.AddRange(uniquePrices);
        }

        return converted.ToArray();
    }
     
    private static List<DateTime> GenerateDatetimes(DateTime fromDate, DateTime toDate, List<PriceCandleModel> models)
    {
        List<DateTime> allDatetimes = new List<DateTime>();
        DateTime currentDatetime = fromDate;
        while (currentDatetime <= toDate)
        {
            if (models.All(e => e.Date != currentDatetime))
            {
                allDatetimes.Add(currentDatetime);
            }
            currentDatetime = currentDatetime.AddHours(1);
        }
        return allDatetimes;
    }

    public FilingModel[] GetFilings(string search, string type)
    {
        var query = @"SELECT F.*, C.CName
            FROM Filings F
            JOIN Companies C ON F.CompanyId = C.Id
            WHERE (C.CName LIKE @Search OR C.Ticker LIKE @Search)
            AND F.FilingType LIKE @FilingType
            LIMIT 50;
            ";
        var searchParam = new KeyValuePair<string, string>("Search", $"%{search}%");
        var typeParam = new KeyValuePair<string, string>("FilingType", type);
        var result = _connection.RunQuery(query, searchParam, typeParam);
        var deserialized = FilingModel.TranslateQueryResults(result);
        return deserialized;
    }

    public FilingModel[] GetFilings(string name, DateTime from, DateTime to, string type)
    {
        var query = @$"SELECT F.*, C.CName
            FROM Filings F
            JOIN Companies C ON F.CompanyId = C.Id
            WHERE (C.CName LIKE @Search OR C.Ticker LIKE @Search)
            AND F.FilingDate >= DATE '{from}' 
            AND F.FilingDate <= DATE '{to}'
            AND F.FilingType = @FilingType
            LIMIT 50;
            ";
        var searchParam = new KeyValuePair<string, string>("Search", $"%{name}%");
        var typeParam = new KeyValuePair<string, string>("FilingType", type);
        return FilingModel.TranslateQueryResults(_connection.RunQuery(query, searchParam, typeParam));
    }

    public int UpsertAndReturnId(string query)
    {
        var result = _connection.RunQuery(query);
        return (int)result.Rows[0].Data[0];
    }
}