using DatabaseConnection;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using Xunit;

namespace DatabaseConnectionTest
{
    [SupportedOSPlatform("windows")]
    public class OleDbConnectorTest : IDisposable
    {
        private readonly IDatabaseConnector conn;

        public OleDbConnectorTest()
        {
            Assembly myAssembly = Assembly.GetEntryAssembly();
            string path = Path.GetDirectoryName(myAssembly.Location);

            string dbSource = Path.Combine(path, @"test.mdb");

            var cs = new OleDbConnectionStringBuilder() {
                Provider = "Microsoft.ACE.OLEDB.16.0",
                DataSource = dbSource,
            };

            conn = DatabaseConnector.GetOleDbConnector(cs.ConnectionString);
            conn.ExecuteNonQuery("CREATE TABLE test (name nvarchar(50));");
        }

        public void Dispose()
        {
            conn.ExecuteNonQuery(@"DROP TABLE test;");
        }

        [Fact(DisplayName="SELECT句でデータが取得できること")]
        public void TestExecuteQuery()
        {
            conn.ExecuteNonQuery(@"INSERT INTO test VALUES ('TEST');");

            var list = conn.ExecuteQuery(
                @"SELECT name FROM test;",
                row => {
                    return row.GetString("name");
                }
            );

            Assert.Equal("TEST", list.First());
        }

        [Fact(DisplayName="パラメータ付きのSELECT句でデータが取得できること")]
        public void TestExecuteQueryWithParameters()
        {
            conn.ExecuteNonQuery(@"INSERT INTO test VALUES ('TEST');");

            var parameters = new Dictionary<string, object> {
                { "name", "TEST" }
            };

            var list = conn.ExecuteQuery(
                "SELECT name FROM test WHERE name = @name;",
                parameters,
                row => {
                    return row.GetString("name");
                }
            );

            Assert.Equal("TEST", list.First());
        }
        
        [Fact(DisplayName="INSERT句でデータが登録できること")]
        public void TestExecuteNonQuery()
        {
            conn.ExecuteNonQuery(@"INSERT INTO test VALUES ('INSERT_TEST');");

            var list = conn.ExecuteQuery(
                @"SELECT name FROM test;",
                row => {
                    return row.GetString("name");
                }
            );

            Assert.Equal("INSERT_TEST", list.First());
        }

        [Fact(DisplayName="パラメータ付きのINSERT句でデータが登録できること")]
        public void TestExecuteNonQueryWithParameters()
        {
            var parameters = new Dictionary<string, object> {
                { "name", "INSERT_TEST" }
            };

            int result = conn.ExecuteNonQuery("INSERT INTO test VALUES (@name);", parameters);

            var list = conn.ExecuteQuery(
                @"SELECT name FROM test;",
                row => {
                    return row.GetString("name");
                }
            );

            Assert.Equal("INSERT_TEST", list.First());
        }

        [Fact(DisplayName="トランザクションがコミットできること")]
        public void TestTransactionCommit()
        {
            var parameters = new Dictionary<string, object> {
                { "name", "TEST" }
            };
            conn.ExecuteNonQuery("INSERT INTO test VALUES (@name);", parameters);

            using (conn.SetTransaction()) {
                conn.ExecuteNonQuery("DELETE FROM test;");
                int result = conn.ExecuteNonQuery("INSERT INTO test VALUES (@name);", parameters);
                conn.CompleteTransaction();

                Assert.Equal(1, result);
            }
        }

        [Fact(DisplayName="トランザクションがロールバックできること")]
        public void TestTransactionRollback()
        {
            var parameters = new Dictionary<string, object> {
                { "name", "TEST" }
            };
            conn.ExecuteNonQuery("INSERT INTO test VALUES (@name);", parameters);

            try {
                using (conn.SetTransaction()) {
                    conn.ExecuteNonQuery("DELETE FROM test;");
                    throw new Exception();
                    conn.CompleteTransaction();
                }
            } catch (Exception) {
            }

            var list = conn.ExecuteQuery(
                "SELECT name FROM test WHERE name = @name;",
                parameters,
                row => {
                    return row.GetString("name");
                }
            );

            Assert.Equal("TEST", list.First());
        }
    }
}