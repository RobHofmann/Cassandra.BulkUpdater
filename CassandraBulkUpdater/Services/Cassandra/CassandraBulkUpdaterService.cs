using Cassandra;
using CassandraBulkUpdater.Base.Contracts.Helpers;
using CassandraBulkUpdater.Base.Contracts.Services;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CassandraBulkUpdater.Services.Cassandra
{
    internal class CassandraBulkUpdaterService : ICassandraBulkUpdaterService
    {
        ILogger _logger;
        ICounter _counter;

        public CassandraBulkUpdaterService(ILogger logger, ICounter counter)
        {
            _logger = logger;
            _counter = counter;
        }

        public async Task UpdateRowsAsync(ISession session, PreparedStatement preparedStatement, RowSet rowSet, int numberOfSimultaneousThreads, string columnToUpdateType, string columnToUpdateValue, string primaryKeyColumnName, string primaryKeyColumnType, CancellationToken cancellationToken)
        {
            await ConcurrentUtils.ConcurrentUtils.Times(times: numberOfSimultaneousThreads, limit: numberOfSimultaneousThreads, index => UpdateRowAsync(session, preparedStatement, rowSet, columnToUpdateType, columnToUpdateValue, primaryKeyColumnName, primaryKeyColumnType, cancellationToken));
        }

        public PreparedStatement CreatePreparedStatement(ISession session, string keyspace, string table, string columnToUpdateName, string primaryKeyColumnName)
        {
            return session.Prepare($"UPDATE {keyspace}.{table} SET {columnToUpdateName} = ? WHERE {primaryKeyColumnName} = ?;");
        }

        public ConcurrentQueue<string> CreateQueueFromRowSet(RowSet rowSet, string primaryKeyColumnName)
        {
            var primaryKeyValues = new ConcurrentQueue<string>();
            foreach (var row in rowSet)
                primaryKeyValues.Enqueue(row.GetValue<string>(primaryKeyColumnName));
            return primaryKeyValues;
        }

        public async Task<RowSet> GetRowsAsync(ISession session, string table, string primaryKeyColumnName)
        {
            return await session.ExecuteAsync(new SimpleStatement($"SELECT {primaryKeyColumnName} FROM {table}"));
        }

        public async Task<ISession> GetSessionAsync(string cassandraHostName, string cassandraUsername, string cassandraPassword, string keyspace)
        {
            var clusterBuilder = Cluster.Builder()
                .AddContactPoints(cassandraHostName)
                .WithLoadBalancingPolicy(new TokenAwarePolicy(new DCAwareRoundRobinPolicy()));

            if (cassandraUsername?.Length > 0 || cassandraPassword?.Length > 0)
                clusterBuilder.WithCredentials(cassandraUsername, cassandraPassword);

            var cluster = clusterBuilder.Build();
            return await cluster.ConnectAsync(keyspace);
        }

        public async Task<long> GetRowCountAsync(ISession session, string table, string primaryKeyColumnName)
        {
            var rsCount = await session.ExecuteAsync(new SimpleStatement($"SELECT count({primaryKeyColumnName}) as count FROM {table}"));
            return rsCount.Select(x => x.GetValue<long>("count")).FirstOrDefault();
        }

        public async Task UpdateRowAsync(ISession session, PreparedStatement preparedStatement, RowSet rowSet, string columnToUpdateType, string columnToUpdateValue, string primaryKeyColumnName, string primaryKeyColumnType, CancellationToken cancellationToken)
        {
            // Get queue in preparation of multithreaded updating
            ConcurrentQueue<string> primaryKeyValues = CreateQueueFromRowSet(rowSet, primaryKeyColumnName);

            while (primaryKeyValues.TryDequeue(out var primaryKeyColumnValue))
            {
                if (cancellationToken.IsCancellationRequested) return;
                _counter.IncrementCounter();
                dynamic castedColumnToUpdateValue = Convert.ChangeType(columnToUpdateValue, Type.GetType(columnToUpdateType));
                dynamic castedPrimaryKeyColumnValue = Convert.ChangeType(primaryKeyColumnValue, Type.GetType(primaryKeyColumnType));
                var boundStatement = preparedStatement.Bind(castedColumnToUpdateValue, castedPrimaryKeyColumnValue);
                await session.ExecuteAsync(boundStatement);
                _logger.Log($"Updated row with {primaryKeyColumnName} {primaryKeyColumnValue}", $"{_counter.ReadCounter()}");
            }
        }
    }
}
