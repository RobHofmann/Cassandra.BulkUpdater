using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CassandraBulkUpdater.Services.HostedServices;
using CassandraBulkUpdater.Services.Cassandra;
using CassandraBulkUpdater.Utils.Helpers;
using CassandraBulkUpdater.Base.Contracts.Services;
using CassandraBulkUpdater.Base.Contracts.Helpers;

namespace CassandraBulkUpdater
{
	public class Program
	{
        public static async Task Main(string[] args)
		{
            var host = new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ILogger, ConsoleLogger>();
                    services.AddSingleton<ICounter, Counter>();
                    services.AddSingleton<ICassandraBulkUpdaterService, CassandraBulkUpdaterService>();
                    services.AddSingleton<ICommandlineArgsHelper, CommandlineArgsHelper>();
                    services.AddHostedService<CassandraBulkUpdateHostedService>();
                })
                .UseConsoleLifetime();
            await host.RunConsoleAsync();
        }
	}
}
