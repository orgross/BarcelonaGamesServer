using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BarcelonaGamesServer.Services.Data
{
    public class SqlService
    {
        public static SqlConnection CreateSqlConnection(IConfiguration configuration)
        {
            string environment = configuration.GetValue<string>("Environment");
            string connectionStringKey = environment == "development" ? "DefaultConnection" : "ProductionConnection";
            string connectionString = configuration.GetConnectionString(connectionStringKey);

            try
            {
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                Console.WriteLine("Connected to SQL Database successfully");
                return connection;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to connect to SQL Database: {e.Message}");
                throw;
            }
        }
    }
}
