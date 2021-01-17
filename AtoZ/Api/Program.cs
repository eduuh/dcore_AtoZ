using System.IO;
using System;
using Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence;
using Serilog;

namespace Api
{
    public class Program
    {
        public static IConfiguration configuration { get; } = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build();
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
           .ReadFrom.Configuration(configuration)
           .WriteTo.Console(
             outputTemplate: "[{Timestamp:HH:mm:ss} {level:u3}] {Message:lg} {Properties:j}{NewLine}{Exception}")
             .CreateLogger();

            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    Log.Information("Getting dcore application Up");
                    var context = services.GetRequiredService<DataContext>();
                    var usermanager = services.GetRequiredService<UserManager<AppUser>>();
                    Log.Information("Migrating the database from the migrations");
                    context.Database.Migrate();
                    SeedData.SeedActivities(context, usermanager).Wait();
                    host.Run();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred during Migrations");
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
