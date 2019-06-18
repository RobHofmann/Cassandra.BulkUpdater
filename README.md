# CassandraBulkUpdater
Cassandra does not support an UPDATE statement without a WHERE clause. It always requires you to use a WHERE statement with the primarykey. Example: In SQL you can do the following:
```
UPDATE [table] SET [column] = 'my new value';
```
Above is not possible within Cassandra. This tool enables you to do this anyway. Please take a look at the usage below to find out how to use this tool.

# Usage
```
Usage: cassandrabulkupdater.exe /CassandraHostName="[CassandraHostName]" /CassandraUserName="[CassandraUserName]" /CassandraPassword="[CassandraPassword]" /Keyspace="[Keyspace]" /Table="[Table]" /ColumnToUpdateName="[ColumnToUpdateName]" /ColumnToUpdateType="[ColumnToUpdateType]" /ColumnToUpdateValue="[ColumnToUpdateValue]" /PrimaryKeyColumnName="[PrimaryKeyColumnName]" /PrimaryKeyColumnType="[PrimaryKeyColumnType]" /NumberOfThreads=[NumberOfThreads]

CassandraHostName: The address where to reach cassandra (just provide one of the reachable nodes)
CassandraUserName: The cassandra username to use (OPTIONAL)
CassandraPassword: The cassandra password to use (OPTIONAL)
Keyspace: The keyspace to use
Table: The table to use
ColumnToUpdateName: The column name of the field that should be updated
ColumnToUpdateType: The column type of the field that should be updated (Supported: System.String, System.Int16, System.Int32, System.Int64, System.Boolean)
ColumnToUpdateValue: The value to insert in the COLUMNTOUPDATE field
PrimaryKeyColumnName: The column name of the primarykey
PrimaryKeyColumnType: The column type of the primarykey  (Supported: System.String, System.Int16, System.Int32, System.Int64)
NumberOfThreads: The number of threads to use which the bulkupdater uses against Cassandra. Advise is to use 20 here.
```

Please be cautious as always. This tool hasn't been heavily tested.