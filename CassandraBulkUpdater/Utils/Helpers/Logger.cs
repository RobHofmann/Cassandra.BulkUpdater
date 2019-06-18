using CassandraBulkUpdater.Base.Contracts.Helpers;
using System;
using System.Linq;

namespace CassandraBulkUpdater.Utils.Helpers
{
    internal class ConsoleLogger : ILogger
    {
        public void Log(string logEntry, params string[] prefixes)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")}]{string.Join("", prefixes.Select(p => $"[{p}]"))} {logEntry}");
        }
    }
}
