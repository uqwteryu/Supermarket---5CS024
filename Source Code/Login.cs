using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MySql.Data.MySqlClient;

namespace Panarama_Market
{
    public partial class Login : Form
    {

        public Login()
        {
            InitializeComponent();
        }
        public static int InitiateConnection()
        {
            // Read credentials from the file
            string[] credentials = File.ReadAllLines("C:\\Users\\sapia\\Desktop\\Panarama Market\\pass.txt");
            string username = credentials.Length > 0 ? credentials[0] : "";
            string password = credentials.Length > 1 ? credentials[1] : "";

            // Construct the connection string
            string connectionString = $"server=localhost;user={username};password={password};database=ecommerce";

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // If connection is successful, return 0
                    return 0;
                }
            }
            catch
            {
                // If there is a failure in connection, return 1
                return 1;
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }
        private void button3_Click(object sender, EventArgs e)
        {

        }

    }
}
