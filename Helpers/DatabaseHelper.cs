using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ArtGalleryStore.Helpers
{
    public static class DatabaseHelper
    {
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["ArtGalleryDB"].ConnectionString;

        /// <summary>
        /// Выполняет запрос к базе данных, который не возвращает результатов (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="commandText">SQL-запрос</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Количество затронутых строк</returns>
        public static int ExecuteNonQuery(string commandText, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(commandText, connection);
                command.Parameters.AddRange(parameters);

                connection.Open();
                int result = command.ExecuteNonQuery();
                return result;
            }
        }

        /// <summary>
        /// Выполняет запрос к базе данных, который возвращает одно значение (например, COUNT(*))
        /// </summary>
        /// <param name="commandText">SQL-запрос</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса (можно привести к нужному типу)</returns>
        public static object ExecuteScalar(string commandText, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(commandText, connection);
                command.Parameters.AddRange(parameters);

                connection.Open();
                object result = command.ExecuteScalar();
                return result;
            }
        }

        /// <summary>
        /// Выполняет запрос к базе данных, который возвращает набор результатов (SELECT)
        /// </summary>
        /// <param name="commandText">SQL-запрос</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Заполненный набор данных</returns>
        public static DataTable ExecuteQuery(string commandText, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(commandText, connection);
                command.Parameters.AddRange(parameters);

                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        /// <summary>
        /// Проверяет подключение к базе данных
        /// </summary>
        /// <returns>true, если подключение успешно</returns>
        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}