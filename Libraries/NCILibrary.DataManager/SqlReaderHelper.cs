using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace NCI.DataManager
{
    public sealed class SqlFieldValueReader
    {
        private DateTime defaultDate;
        public SqlFieldValueReader(SqlDataReader reader)
        {
            this.defaultDate = DateTime.MinValue;
            this.reader = reader;
        }

        public int GetInt32(String column)
        {
            int data = (reader.IsDBNull(reader.GetOrdinal(column)))
                                    ? (int)0 : (int)reader[column];
            return data;
        }

        public short GetInt16(String column)
        {
            short data = (reader.IsDBNull(reader.GetOrdinal(column)))
                                  ? (short)0 : (short)reader[column];
            return data;
        }

        public float GetFloat(String column)
        {
            float data = (reader.IsDBNull(reader.GetOrdinal(column)))
                        ? 0 : float.Parse(reader[column].ToString());
            return data;
        }

        public bool GetBoolean(String column)
        {
            bool data = (reader.IsDBNull(reader.GetOrdinal(column)))
                                     ? false : (bool)reader[column];
            return data;
        }

        public String GetString(String column)
        {
            String data = (reader.IsDBNull(reader.GetOrdinal(column)))
                                   ? null : reader[column].ToString();
            return data;
        }

        public DateTime GetDateTime(String column)
        {
            DateTime data = (reader.IsDBNull(reader.GetOrdinal(column)))
                               ? defaultDate : (DateTime)reader[column];
            return data;
        }

        public bool Read()
        {
            return this.reader.Read();
        }
        private SqlDataReader reader;
    }
}
