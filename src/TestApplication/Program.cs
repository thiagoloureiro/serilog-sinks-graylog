using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using System;
using System.Threading.Tasks;

namespace TestApplication
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .Build();

            Logger logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(configuration)
                .CreateLogger();

            logger.Warning("some warning: {test}", "test message");

            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }
}