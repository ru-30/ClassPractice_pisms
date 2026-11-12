using System;
using MySqlConnector;

namespace ClassPractice
{
    public static class Database
    {
        private const string ConnectionString = "Server=127.0.0.1;Port=3306;Database=pisms_db;User ID=root;Password=;SslMode=None;";

        public static MySqlConnection CreateConnection()
        {
            var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public static bool CanConnect(out string errorMessage)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    errorMessage = string.Empty;
                    return connection.State == System.Data.ConnectionState.Open;
                }
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }
        }
    }
}
