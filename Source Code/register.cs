﻿using MySql.Data.MySqlClient;
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
using System.Security.Cryptography;

namespace Panarama_Market
{

    public partial class register : Form
    {
        private string fullName = "";
        private string email = "";
        private string password = "";
        private string surname = "";

        public register()
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
        public static string EncryptStringAES(string plainText)
         
        {
            // Path to the AES key file
            string KeyFilePath = @"C:\Users\sapia\Desktop\Panarama Market\key.txt";

            // Read the AES key from the file
            string encryptionKey = File.ReadAllText(KeyFilePath).Trim(); // Ensure the key is correctly trimmed

            // Generate a random IV
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 128; // Set the key size to 128 bits for AES-128
                aesAlg.GenerateIV(); // Automatically generate a random IV
                byte[] iv = aesAlg.IV;

                // Convert key and IV to byte arrays
                byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
                byte[] encrypted;

                // Create an encryptor to perform the stream transform
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(keyBytes, iv);

                // Create the streams used for encryption
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // Write all data to the stream
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }

                // Return the encrypted bytes and IV as base64 strings, concatenated together
                // This is important: for decryption, you'll need the IV, so it's typically sent along with the encrypted data
                return Convert.ToBase64String(iv) + ":" + Convert.ToBase64String(encrypted);
            }
        }
        public static string HashStringSHA256(string inputString)
        {
            // Check if the input string is null or empty
            if (string.IsNullOrEmpty(inputString))
                MessageBox.Show("Ensure all texboxes are filled in before registering");

            // Create a SHA256 instance
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Compute the hash of the input string
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(inputString));

                // Convert the byte array to a hexadecimal string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                // Return the hexadecimal string
                return builder.ToString();
            }
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            fullName = textBox3.Text;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            email = textBox1.Text;
        }
        private HashSet<string> commonPasswords = new HashSet<string>();
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            password = textBox2.Text; // Update the password variable with the text from textBox2
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }
        private void button3_Click(object sender, EventArgs e)
        {
            // Check if any of the textboxes are empty
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(surname))
            {
                MessageBox.Show("Please fill in all fields before submitting.", "Incomplete Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Exit the method without proceeding further
            }

            // Basic strength criteria
            bool isLengthValid = password.Length >= 10;
            bool hasUpperCase = password.Any(char.IsUpper);
            bool hasLowerCase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecialChar = password.Any(ch => !char.IsLetterOrDigit(ch));

            // Load common passwords directly in the function for simplicity (not recommended for large lists)
            var commonPasswords = File.ReadAllLines(@"C:\Users\sapia\Desktop\Panarama Market\com_pass.txt").Select(p => p.Trim().ToLower()).ToList();
            bool isCommonPassword = commonPasswords.Contains(password.ToLower());

            // Check if the password is weak by policy or is a common password
            if (!isLengthValid || !hasUpperCase || !hasLowerCase || !hasDigit || !hasSpecialChar || isCommonPassword)
            {
                MessageBox.Show("Your password is considered weak. It must be at least 10 characters long, include uppercase and lowercase letters, a digit, a special character, and not be a commonly used password.", "Weak Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox2.Clear(); // Clear the text box to force the user to enter a new password
                password = ""; // Clear the stored password since it's invalid
                return; // Exit the method without proceeding further
            }

            // First, try to initiate the database connection
            int connectionStatus = InitiateConnection();

            // Check if connection was successful
            if (connectionStatus == 0)
            {
                // Encrypt the email and hash the password
                string encryptedEmail = EncryptStringAES(email); // Encrypt the email
                string hashedPassword = HashStringSHA256(password); // Hash the password

                // Split fullName into firstName and lastName
                string[] nameParts = fullName.Split(new[] { ' ' }, 2); // Split by first space only
                string firstName = nameParts.Length > 0 ? nameParts[0] : "";
                string lastName = nameParts.Length > 1 ? nameParts[1] : surname;

                // Define the connection string again
                string[] credentials = File.ReadAllLines(@"C:\Users\sapia\Desktop\Panarama Market\pass.txt");
                string connectionString = $"server=localhost;user={credentials[0]};password={credentials[1]};database=ecommerce";

                using (var connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        using (var cmd = new MySqlCommand("CreateUser", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            // Set parameter values for the stored procedure
                            cmd.Parameters.AddWithValue("em", encryptedEmail);
                            cmd.Parameters.AddWithValue("na", firstName);
                            cmd.Parameters.AddWithValue("su", lastName);
                            cmd.Parameters.AddWithValue("pa", hashedPassword);

                            // Execute the stored procedure
                            cmd.ExecuteNonQuery();

                            // Inform the user of success
                            MessageBox.Show("Account created! You may now try to login with your credentials.", "Success");

                            // Now, add entry to Audit table indicating a new account was created
                            string auditDescription = $"A new user account was created with the name: {firstName} {lastName} and email: {email}.";
                            using (var auditCmd = new MySqlCommand("INSERT INTO Audit (SessionStart, SessionEnd, Description, AccountID) VALUES (NOW(), NOW(), @desc, LAST_INSERT_ID())", connection))
                            {
                                auditCmd.Parameters.AddWithValue("@desc", auditDescription);

                                // Execute the command to insert the audit entry
                                auditCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // If there's an error, display it
                        MessageBox.Show("There was a problem when creating your account. Please try again later or contact a system administrator. Error: " + ex.Message, "Error");
                    }
                }
            }
            else
            {
                // If connection failed, inform the user
                MessageBox.Show("There was a problem when creating your account. Please try again later or contact a system administrator.", "Connection Error");
            }
        }
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            surname = textBox4.Text;
        }

        private void register_Load(object sender, EventArgs e)
        {

        }
    }
}
