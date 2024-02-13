using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Runtime.Versioning;

namespace DatabaseConnection
{
    [SupportedOSPlatform("windows")]
    public sealed class OleDbConnector : IDatabaseConnector
    {
        private readonly string connectionString;
        private TransactionalConnection scope;

        internal OleDbConnector(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, new Dictionary<string, object>());
        }

        public int ExecuteNonQuery(string sql, IDictionary<string, object> parameters)
        {
            if (scope == null) {
                using var connection = new OleDbConnection(connectionString);
                connection.Open();
                using var command = new OleDbCommand(sql, connection);

                command.Parameters.AddRange(parameters.Select(
                    p => new OleDbParameter(p.Key, p.Value)).ToArray());

                return command.ExecuteNonQuery();
            } else {
                using var command = scope.GetCommand(sql);

                command.Parameters.AddRange(parameters.Select(
                    p => new OleDbParameter(p.Key, p.Value)).ToArray());

                return command.ExecuteNonQuery();
            }
        }

        public IEnumerable<T> ExecuteQuery<T>(string sql, Func<ReadRow, T> convert)
        {
            return ExecuteQuery(sql, new Dictionary<string, object>(), convert);
        }

        public IEnumerable<T> ExecuteQuery<T>(
            string sql, IDictionary<string, object> parameters, Func<ReadRow, T> convert)
        {
            if (scope == null) {
                using var connection = new OleDbConnection(connectionString);
                connection.Open();
                using var command = new OleDbCommand(sql, connection);
                command.Parameters.AddRange(parameters.Select(
                    p => new OleDbParameter(p.Key, p.Value)).ToArray());
                using var reader = command.ExecuteReader();
                while (reader.Read()) {
                    yield return convert(new ReadRow(reader));
                }
            } else {
                using var command = scope.GetCommand(sql);
                command.Parameters.AddRange(parameters.Select(
                    p => new OleDbParameter(p.Key, p.Value)).ToArray());
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

            scope = new TransactionalConnection(this);

            return scope;
        }

        public void CompleteTransaction()
        {
            if (scope == null) {
                throw new InvalidOperationException();
            }

            scope.Complete();
        }

        private sealed class TransactionalConnection : IDisposable
        {
            private readonly OleDbConnector connector;
            private readonly OleDbConnection connection;
            private readonly OleDbTransaction transaction;
            private bool _complete;

            public TransactionalConnection(OleDbConnector connector)
            {
                this.connector = connector;
                connection = new OleDbConnection(connector.connectionString);
                connection.Open();
                transaction = connection.BeginTransaction();
            }

            public void Dispose()
            {
                if (_complete) {
                    transaction.Commit();
                } else {
                    transaction.Rollback();
                }

                transaction.Dispose();
                connection.Dispose();

                connector.scope = null;
            }

            internal void Complete()
            {
                _complete = true;
            }

            internal OleDbCommand GetCommand(string sql)
            {
                return new OleDbCommand(sql, connection, transaction);
            }
        }
    }
}
