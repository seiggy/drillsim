using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace YPLCalibrationFromRheometer.Service;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration["ConnectionStrings:Sqlite"] ??=
            $"Data Source={Path.Combine("..", "home", "YPLCalibrationFromRheometer.db")}";
        builder.AddServiceDefaults();
        builder.Logging.ClearProviders();
        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
        builder.Logging.AddDebug();
        builder.Logging.AddConsole();

        var startup = new Startup(builder.Configuration);
        startup.ConfigureServices(builder.Services);

        var app = builder.Build();
        startup.Configure(app, app.Environment);
        app.MapDefaultEndpoints();

        var dataSource = new SqliteConnectionStringBuilder(
            builder.Configuration["ConnectionStrings:Sqlite"]!).DataSource;
        SQLConnectionManager.Initialize(
            app.Services.GetRequiredService<ILoggerFactory>(),
            dataSource);

        app.Run();
    }
}
