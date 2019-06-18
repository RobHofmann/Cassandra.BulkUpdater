using Cassandra;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CassandraBulkUpdater.Base.Contracts.Services
{
    internal interface ICassandraBulkUpdaterService
    {
        Task UpdateRowsAsync(ISession session, PreparedStatement preparedStatement, RowSet rowSet, int numberOfSimultaneousThreads, string columnToUpdateType, string columnToUpdateValue, string primaryKeyColumnName, string primaryKeyColumnType, long totalCount, CancellationToken cancellationToken);

        PreparedStatement CreatePreparedStatement(ISession session, string keyspace, string table, string columnToUpdateName, string primaryKeyColumnName);

        ConcurrentQueue<string> CreateQueueFromRowSet(RowSet rowSet, string primaryKeyColumnName);

        Task<RowSet> GetRowsAsync(ISession session, string table, string primaryKeyColumnName);

        Task<ISession> GetSessionAsync(string cassandraHostName, string cassandraUsername, string cassandraPassword, string keyspace);

        Task<long> GetRowCountAsync(ISession session, string table, string primaryKeyColumnName);

        Task UpdateRowAsync(ISession session, PreparedStatement preparedStatement, RowSet rowSet, string columnToUpdateType, string columnToUpdateValue, string primaryKeyColumnName, string primaryKeyColumnType, long totalCount, CancellationToken cancellationToken);
    }
}
