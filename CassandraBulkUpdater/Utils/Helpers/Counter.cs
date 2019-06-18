using System.Threading;
using CassandraBulkUpdater.Base.Contracts.Helpers;

namespace CassandraBulkUpdater.Utils.Helpers
{
	public class Counter : ICounter
	{
		private long _sharedInteger;

		public void IncrementCounter()
		{
			Interlocked.Increment(ref _sharedInteger);
		}

		public long ReadCounter()
		{
			return Interlocked.Read(ref _sharedInteger);
		}
	}
}
