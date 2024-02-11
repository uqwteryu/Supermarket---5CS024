using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        // Update the connection string below with your server name, database name,
        // and explicitly include the user ID and password for the database connection.
        string connectionString = "Data Source=ServerName;Initial Catalog=DatabaseName;User ID=YourUsername;Password=YourPassword;";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                Console.WriteLine("Connection successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
