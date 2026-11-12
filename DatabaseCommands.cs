using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;

namespace ClassPractice
{
    public static class DatabaseCommands
    {
        public static int ExecuteNonQuery(string sql, params MySqlParameter[] parameters)
        {
            using (var connection = Database.CreateConnection())
            using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddRange(parameters);
                return command.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql, params MySqlParameter[] parameters)
        {
            using (var connection = Database.CreateConnection())
            using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddRange(parameters);
                return command.ExecuteScalar();
            }
        }

        public static DataTable Query(string sql, params MySqlParameter[] parameters)
        {
            using (var connection = Database.CreateConnection())
            using (var command = new MySqlCommand(sql, connection))
            using (var adapter = new MySqlDataAdapter(command))
            {
                command.Parameters.AddRange(parameters);
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public static MySqlParameter Parameter(string name, object value)
        {
            return new MySqlParameter(name, value ?? DBNull.Value);
        }
    }
}
