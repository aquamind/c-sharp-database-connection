using System.Runtime.Versioning;

namespace DatabaseConnection
{
    public static class DatabaseConnector
    {
        /// <summary>
        /// SQL Server 接続
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IDatabaseConnector GetSqlServerConnector(string connectionString)
        {
            return new SqlServerConnector(connectionString);
        }

        /// <summary>
        /// OLE DB 接続
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static IDatabaseConnector GetOleDbConnector(string connectionString)
        {
            return new OleDbConnector(connectionString);
        }
    }
}
