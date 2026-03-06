
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using System.IO;

namespace IllRequestPortal.Web
{    
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var envConfigPath = $"nlog.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.config";
                if (File.Exists(envConfigPath))
                {
                    LogManager.Setup().LoadConfigurationFromFile(envConfigPath);
                }
                else
                {
                    LogManager.Setup().LoadConfigurationFromFile("nlog.config");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kunde inte ladda NLog-konfiguration: " + ex.Message);
            }

            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                logger.Info("Init main");

                //uncomment this if you want to use SQLite
                //const string DB_NAME = "<name>.sqlite";
                //var config = host.Services.GetService(typeof(IConfiguration)) as IConfiguration;
                //var dbFilePath = Path.Combine(Environment.CurrentDirectory, "App_Data", DB_NAME);
                //Directory.CreateDirectory(Path.GetDirectoryName(dbFilePath)!);
                //var resolvedConnectionString = $"Data Source={dbFilePath}";
                //logger.Info("Resolved connection string: " + resolvedConnectionString);

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    //webBuilder.ConfigureKestrel(options =>
                    //{
                    //});
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders(); 
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                })
                .UseNLog(); 
    }
}