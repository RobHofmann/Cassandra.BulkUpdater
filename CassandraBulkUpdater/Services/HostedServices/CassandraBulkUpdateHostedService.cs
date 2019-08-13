using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using CassandraBulkUpdater.Base.Contracts.Helpers;
using CassandraBulkUpdater.Base.Contracts.Services;
using System;

namespace CassandraBulkUpdater.Services.HostedServices
{
    internal class CassandraBulkUpdateHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly ICounter _counter;
        private readonly ICassandraBulkUpdaterService _cassandraBulkUpdaterService;
        private readonly IApplicationLifetime _appLifetime;
        private readonly ICommandlineArgsHelper _commandlineArgsHelper;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public CassandraBulkUpdateHostedService(ILogger logger, ICounter counter, ICassandraBulkUpdaterService cassandraBulkUpdaterService,
            IApplicationLifetime appLifetime, ICommandlineArgsHelper commandlineArgsHelper)
        {
            _logger = logger;
            _counter = counter;
            _cassandraBulkUpdaterService = cassandraBulkUpdaterService;
            _appLifetime = appLifetime;
            _commandlineArgsHelper = commandlineArgsHelper;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);
        }

        private void OnStarted()
        {
            Run().GetAwaiter().GetResult();
        }

        private async Task Run()
        {
            try
            {
                string cassandraHostName = _commandlineArgsHelper.GetCommandlineArgumentValue<string>("CassandraHostName");
                string cassandraUserName = _commandlineArgsHelper.GetCommandlineArgumentValue<string>("CassandraUserName", false);
                string cassandraPassword = _commandlineArgsHelper.GetCommandlineArgumentValue<string>("CassandraPassword", false);
                string keyspace = _commandlineArgsHelper.GetCommandlineArgumentValue<string>("Keyspace");
                string table = _commandlineArgsHelper.GetCommandlineArgumentValue<string>("Table");
                string columnToUpdateName = _commandlineArgsHelper.GetCommandlineArgumentValue<string>("ColumnToUpdateName");
                string columnToUpdateType = _commandlineArgsHelper.GetCommandlineArgumentValue<string>("ColumnToUpdateType", true, "System.String", "System.Int16", "System.Int32", "System.Int64", "System.Boolean");
                string columnToUpdateValue = _commandlineArgsHelper.GetCommandlineArgumentValue<string>("ColumnToUpdateValue");
                string primaryKeyColumnName = _commandlineArgsHelper.GetCommandlineArgumentValue<string>("PrimaryKeyColumnName");
                string primaryKeyColumnType = _commandlineArgsHelper.GetCommandlineArgumentValue<string>("PrimaryKeyColumnType", true, "System.String", "System.Int16", "System.Int32", "System.Int64");
                int numberOfSimultaneousThreads = _commandlineArgsHelper.GetCommandlineArgumentValue<int>("NumberOfThreads");

                _logger.Log("Hello World! Welcome to the Cassandra Bulk Updater");
                _logger.Log("UPDATE ALL THE THINGS!!!!");

                if (_cts.IsCancellationRequested) return;

                // Create connection
                _logger.Log("Connecting to Cassandra cluster...");
                if (cassandraUserName?.Length > 0 || cassandraPassword?.Length > 0)
                    _logger.Log("Using username & password");
                var session = _cassandraBulkUpdaterService.GetSessionAsync(cassandraHostName, cassandraUserName, cassandraPassword, keyspace).Result;
                _logger.Log("Connected to Cassandra cluster...");

                if (_cts.IsCancellationRequested) return;
                
                // Fetching Rows
                _logger.Log("Fetching rows");
                var rowSet = await _cassandraBulkUpdaterService.GetRowsAsync(session, table, primaryKeyColumnName);
                _logger.Log("Fetched rows");

                if (_cts.IsCancellationRequested) return;

                // Create prepared statement (this gives a faster result) & Start updating the rows
                var preparedStatement = _cassandraBulkUpdaterService.CreatePreparedStatement(session, keyspace, table, columnToUpdateName, primaryKeyColumnName);
                await _cassandraBulkUpdaterService.UpdateRowsAsync(session, preparedStatement, rowSet, numberOfSimultaneousThreads, columnToUpdateType, columnToUpdateValue, primaryKeyColumnName, primaryKeyColumnType, _cts.Token);

                if (_cts.IsCancellationRequested) return;

                _logger.Log("Done!");
            }
            catch (Exception ex)
            {
                _logger.Log(ex.ToString());

                if (ex is ArgumentOutOfRangeException || ex is ArgumentNullException)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    _logger.Log(_commandlineArgsHelper.GetCommandlineUsageText());
                }
            }
        }

        private void OnStopping()
        {
            _logger.Log("Exiting...");
            _cts.Cancel();
            // Perform on-stopping activities here
        }

        private void OnStopped()
        {
            _logger.Log("Exited...");
            // Perform post-stopped activities here
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {

        }
    }
}
