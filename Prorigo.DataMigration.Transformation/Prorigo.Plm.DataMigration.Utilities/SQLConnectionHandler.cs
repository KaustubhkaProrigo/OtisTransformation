using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prorigo.Plm.DataMigration.Utilities
{
    public class SQLConnectionHandler : IDisposable
    {
        private SqlConnection _conn;

        public SQLConnectionHandler(string conString)
        {
            _conn = new SqlConnection(conString);
            if (_conn.State != ConnectionState.Open)
                _conn.Open();
        }

        public SqlDataReader ExecuteQuery(string query)
        {
            var cmd = new SqlCommand(query, _conn);
            cmd.CommandType = CommandType.Text;
            return cmd.ExecuteReader();
        }

        public void Dispose()
        {
            if (_conn.State != ConnectionState.Closed)
            {
                _conn.Close();
            }
            _conn.Dispose();
        }

    }
}

