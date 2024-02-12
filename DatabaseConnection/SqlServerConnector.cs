using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;

namespace DatabaseConnection
{
    public sealed class SqlServerConnector : IDatabaseConnector
    {
        private readonly string connectionString;
        private TransactionScopeWrapper? scope;

        internal SqlServerConnector(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, new Dictionary<string, object>());
        }

        public int ExecuteNonQuery(string sql, IDictionary<string, object> parameters)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddRange(parameters.Select(
                p => new SqlParameter(p.Key, p.Value)).ToArray());

            return command.ExecuteNonQuery();
        }

        public IEnumerable<T> ExecuteQuery<T>(string sql, Func<ReadRow, T> convert)
        {
            return ExecuteQuery(sql, new Dictionary<string, object>(), convert);
        }

        public IEnumerable<T> ExecuteQuery<T>(
            string sql, IDictionary<string, object> parameters, Func<ReadRow, T> convert)
        {
            using (var connection = new SqlConnection(connectionString)) {
                connection.Open();
                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddRange(parameters.Select(
                    p => new SqlParameter(p.Key, p.Value)).ToArray());
                using var reader = command.ExecuteReader();
                while (reader.Read()) {
                    yield return convert(new ReadRow(reader));
                }
            }
        }

        public IDisposable SetTransaction()
        {
            if (scope != null) {
                throw new InvalidOperationException();
            }

            scope = new TransactionScopeWrapper(this);

            return scope;
        }

        public void CompleteTransaction()
        {
            if (scope == null) {
                throw new InvalidOperationException();
            }

            scope.Complete();
        }

        private sealed class TransactionScopeWrapper: IDisposable
        {
            public TransactionScopeWrapper(SqlServerConnector connector)
            {
                this.connector = connector;
            } 

            private readonly SqlServerConnector connector;
            private readonly TransactionScope scope = new();

            public void Dispose()
            {
                scope.Dispose();
                connector.scope = null;
            }

            internal void Complete()
            {
                scope.Complete();
            }
        }
    }
}
