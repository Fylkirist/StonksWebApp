using StonksWebApp.connections;
using StonksWebApp.Services;
using Microsoft.Extensions.Configuration;

namespace StonksWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            FetchingService.TickerInfoMap = FetchingService.GetTickerInfoMap();

            var builder = WebApplication.CreateBuilder(args);

            
            builder.Services.AddRazorPages();

            builder.Configuration.AddJsonFile(
                "appsettings.Development.json",
                optional: false,
                reloadOnChange: true
            );
            var app = builder.Build();
            var db = new DatabaseConnectionService(
                new PostgresqlConnection(app.Configuration["Credentials:Sql:Username"], app.Configuration["Credentials:Sql:Password"], app.Configuration["Credentials:Sql:Instance"], app.Configuration["Credentials:Sql:Database"]
                    )
                );
            FetchingService.FinnhubKey = app.Configuration["Credentials:Finnhub"];
            // These functions recreate tables, only use on migrations.
            
            /*
            db.DropAllTables();
            db.CreateCompanyTable(); 
            db.CreateFilingsTable();
            db.CreateUsersTable();
            db.CreateAdminUser(app.Configuration);
            */

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}