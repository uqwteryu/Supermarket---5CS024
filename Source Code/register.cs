using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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
            textBox2.PasswordChar = '•';
        }
        public static int InitiateConnection()
        {
            // Read credentials from the file
            string[] credentials = File.ReadAllLines("C:\\Users\\Daryl\\source\\Panarama Market\\Panarama Market\\pass.txt");
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
        public static class RegistrationState
        {
            public static DateTime? LastRegistrationTime { get; set; } = null;
        }

        public static string EncryptStringAES(string plainText)
        {
            // Provided AES-128 encryption key and IV
            string hexKey = "66499f4d9595d448a66edb0b0125c4e5";
            string hexIV = "b14ca5898a4e4133bbce2ea2315a1916";

            // Convert the hexadecimal strings to byte arrays
            byte[] key = Enumerable.Range(0, hexKey.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hexKey.Substring(x, 2), 16))
                             .ToArray();
            byte[] iv = Enumerable.Range(0, hexIV.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hexIV.Substring(x, 2), 16))
                             .ToArray();

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 128; // Set the key size to 128 bits for AES-128
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        byte[] encrypted = msEncrypt.ToArray();
                        return Convert.ToBase64String(encrypted); // Return the encrypted bytes as a base64 string
                    }
                }
            }
        }
        private bool IsValidName(string name)
        {
            // Regular expression to match valid characters in a name
            return Regex.IsMatch(name, @"^[a-zA-Z\s\-']+$");
        }

        private bool IsValidEmail(string email)
        {
            // Regular expression for validating an email address
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
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
        private void textBox3_TextChanged(object sender, EventArgs e) // Full Name
        {
            if (!IsValidName(textBox3.Text))
            {
                MessageBox.Show("The full name contains invalid characters. Please use only letters, spaces, hyphens, and apostrophes.");
                // Consider clearing the textbox or highlighting it for user attention
                textBox3.Focus();
            }
            else
            {
                fullName = textBox3.Text;
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e) // Email
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
            // Assuming your registration form class is named register
            Login registerForm = new Login();

            // Hide the login form
            this.Hide();

            // Show the register form
            registerForm.Show();

            // Set the Closed event handler for the registerForm to re-open the login form when the register form is closed
            registerForm.FormClosed += (s, args) => this.Close();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            // Check if a registration has already occurred and enforce a 5-minute pause
            if (RegistrationState.LastRegistrationTime.HasValue &&
                DateTime.Now.Subtract(RegistrationState.LastRegistrationTime.Value).TotalMinutes < 5)
            {
                MessageBox.Show("Please wait a while before trying to register again.", "Registration Throttled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Exit the method without proceeding further
            }

            // Check if any of the textboxes are empty
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(surname))
            {
                MessageBox.Show("Please fill in all fields before submitting.", "Incomplete Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Exit the method without proceeding further
            }

            // Validate email format
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Exit the method without proceeding further
            }

            // Basic strength criteria
            bool isLengthValid = password.Length >= 10;
            bool hasUpperCase = password.Any(char.IsUpper);
            bool hasLowerCase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecialChar = password.Any(ch => !char.IsLetterOrDigit(ch));

            // Load common passwords directly in the function for simplicity (not recommended for large lists)
            var commonPasswords = File.ReadAllLines(@"C:\Users\Daryl\source\Panarama Market\Panarama Market\com_pass.txt").Select(p => p.Trim().ToLower()).ToList();
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
                string[] credentials = File.ReadAllLines(@"C:\Users\Daryl\source\Panarama Market\Panarama Market\pass.txt");
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

                            // Update the last registration timestamp
                            RegistrationState.LastRegistrationTime = DateTime.Now;

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
                    catch
                    {
                        // If there's an error, display it
                        MessageBox.Show("There was a problem when creating your account. Please try again later or contact a system administrator.");
                    }
                }
            }
            else
            {
                // If connection failed, inform the user
                MessageBox.Show("There was a problem when creating your account. Please try again later or contact a system administrator.", "Connection Error");
            }
        }
        private void textBox4_TextChanged(object sender, EventArgs e) // Surname
        {
            if (!IsValidName(textBox4.Text))
            {
                MessageBox.Show("The surname contains invalid characters. Please use only letters, spaces, hyphens, and apostrophes.");
                textBox4.Focus();
            }
            else
            {
                surname = textBox4.Text;
            }
        }

        private void register_Load(object sender, EventArgs e)
        {

            this.BackgroundImageLayout = ImageLayout.Stretch; // Make the image cover the whole form

            // Subscribe to the Paint event to draw the fade effect
            this.Paint += (s, pe) =>
            {
                // Fine-tune starting point and transparency for a smoother transition
                using (LinearGradientBrush brush = new LinearGradientBrush(
                       new Point(0, this.Height * 1 / 4), // Start the gradient further up the form
                       new Point(0, this.Height),
                       Color.FromArgb(0, Color.Black), // Start color: fully transparent black
                       Color.Black)) // End color: solid black
                {
                    // Fill from the starting point to the bottom with the gradient for the fade effect
                    pe.Graphics.FillRectangle(brush, 0, this.Height * 1 / 4, this.Width, this.Height * 3 / 4);
                }
            };
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
