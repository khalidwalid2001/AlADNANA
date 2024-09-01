 using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;

namespace AlADNANA.SqlConnectionManager
{
    public class SqlConnectionManag : IDisposable
    {
        string connectionString = @"Data Source=SQL8005.site4now.net;Initial Catalog=db_aaac07_aladnana;User Id=db_aaac07_aladnana_admin;Password=Khalid11@khalid";

        private readonly string _connectionString;
        private SqlConnection _sqlConnection;

        public SqlConnectionManag( )
        {
   

            _connectionString = connectionString;
        }

        public void OpenConnection()
        {
            _sqlConnection = new SqlConnection(_connectionString);
            _sqlConnection.Open();
        }

        public void CloseConnection()
        {
            if (_sqlConnection != null && _sqlConnection.State == ConnectionState.Open)
            {
                _sqlConnection.Close();
                _sqlConnection.Dispose();
            }
        }

        public SqlConnection GetConnection()
        {
            if (_sqlConnection == null)
            {
                _sqlConnection = new SqlConnection(_connectionString);
            }
            return _sqlConnection;
        }

        public void Dispose()
        {
            CloseConnection();
        }
        public string getstringConection()
        { 
        
        
        return _connectionString;
        }
    }
}
