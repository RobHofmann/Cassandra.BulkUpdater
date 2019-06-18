namespace CassandraBulkUpdater.Base.Contracts.Helpers
{
	interface ICounter
	{
		void IncrementCounter();
		long ReadCounter();
	}
}
