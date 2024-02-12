using System;
using System.Data;

namespace DatabaseConnection
{
    public class ReadRow    {

        public ReadRow(IDataReader reader)
        {
            this.reader = reader;
        }

        private readonly IDataReader reader;

        public bool? GetBoolean(string name)
        {
            return GetBoolean(reader.GetOrdinal(name));
        }

        public bool? GetBoolean(int i)
        {
            if (reader.IsDBNull(i)) return null;

            return reader.GetBoolean(i);
        }

        public DateTime? GetDateTime(string name)
        {
            return GetDateTime(reader.GetOrdinal(name));
        }

        public DateTime? GetDateTime(int i)
        {
            if (reader.IsDBNull(i)) return null;

            return reader.GetDateTime(i);
        }

        public decimal? GetDecimal(string name)
        {
            return GetDecimal(reader.GetOrdinal(name));
        }

        public decimal? GetDecimal(int i)
        {
            if (reader.IsDBNull(i)) return null;

            return reader.GetDecimal(i);
        }

        public int? GetInt(string name)
        {
            return GetInt(reader.GetOrdinal(name));
        }

        public int? GetInt(int i)
        {
            if (reader.IsDBNull(i)) return null;

            return reader.GetInt32(i);
        }

        public long? GetLong(string name)
        {
            return GetLong(reader.GetOrdinal(name));
        }

        public long? GetLong(int i)
        {
            if (reader.IsDBNull(i)) return null;

            return reader.GetInt64(i);
        }

        public string? GetString(string name)
        {
            return GetString(reader.GetOrdinal(name));
        }

        public string? GetString(int i)
        {
            if (reader.IsDBNull(i)) return null;

            return reader.GetString(i);
        }
    }
}