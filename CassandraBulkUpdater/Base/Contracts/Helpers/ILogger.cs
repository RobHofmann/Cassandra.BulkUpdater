namespace CassandraBulkUpdater.Base.Contracts.Helpers
{
    public interface ILogger
    {
        void Log(string logEntry, params string[] prefixes);
    }
}
