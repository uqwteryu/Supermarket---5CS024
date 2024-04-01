using MySql.Data.MySqlClient;
using System.IO;

namespace MyConsoleApp
{
    public class DatabaseConnector
    {
        // Function to create and return a database connection
        public static MySqlConnection GetDatabaseConnection()
        {
            // Read credentials from the file
            string[] credentials = File.ReadAllLines("C:\\Users\\sapia\\Desktop\\Panarama Market\\pass.txt");
            string username = credentials.Length > 0 ? credentials[0] : "";
            string password = credentials.Length > 1 ? credentials[1] : "";

            // Construct the connection string
            string connectionString = $"server=localhost;user={username};password={password};database=ecommerce";

            // Create a new MySqlConnection object
            var connection = new MySqlConnection(connectionString);

            return connection;
        }
    }
}
