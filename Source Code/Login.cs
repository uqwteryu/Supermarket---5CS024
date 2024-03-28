using MySql.Data.MySqlClient;
using System;
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
    public partial class Login : Form
    {
        private int remainingTries = 3; // Class level variable, initialize with max attempts
        private System.Windows.Forms.Timer lockoutTimer = new System.Windows.Forms.Timer();

        public Login()
        {
            InitializeComponent();
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

        public static class LoginState
        {
            public static int RemainingTries = 3;
            public static System.Windows.Forms.Timer LockoutTimer = new System.Windows.Forms.Timer();
            public static bool IsLockedOut => LockoutTimer.Enabled;

            static LoginState()
            {
                LockoutTimer.Interval = 60000; // Lockout period in milliseconds (e.g., 60000ms = 60s)
                LockoutTimer.Tick += (sender, e) =>
                {
                    RemainingTries = 3; // Reset the counter after the lockout period
                    LockoutTimer.Stop();
                };
            }

            public static void StartLockout()
            {
                if (!LockoutTimer.Enabled) // Start the timer only if it's not already running
                {
                    LockoutTimer.Start();
                }
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
            // Assuming your registration form class is named register
            register registerForm = new register();

            // Hide the login form
            this.Hide();

            // Show the register form
            registerForm.Show();

            // Set the Closed event handler for the registerForm to re-open the login form when the register form is closed
            registerForm.FormClosed += (s, args) => this.Close();
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

        private bool IsValidEmail(string email)
        {
            // Regular expression for validating an email address
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (LoginState.IsLockedOut)
            {
                MessageBox.Show("Account locked. Please wait a while before trying again.", "Account Locked", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string inputEmail = textBox1.Text; // User's input email
            string inputPassword = textBox2.Text; // User's input password

            if (string.IsNullOrWhiteSpace(inputEmail) || string.IsNullOrWhiteSpace(inputPassword))
            {
                MessageBox.Show("Please fill in all fields before submitting.", "Incomplete Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Exit the method without proceeding further
            }

            // Validate the input email format
            if (!IsValidEmail(inputEmail))
            {
                MessageBox.Show("Please enter a valid email address.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Encrypt the input email to match the encryption used during registration
            string encryptedInputEmail = EncryptStringAES(inputEmail);

            // Hash the input password to match the hashing used during registration
            string hashedInputPassword = HashStringSHA256(inputPassword);

            int connectionStatus = InitiateConnection();
            if (connectionStatus == 0) // Connection successful
            {
                string[] credentials = File.ReadAllLines("C:\\Users\\Daryl\\source\\Panarama Market\\Panarama Market\\pass.txt");
                string connectionString = $"server=localhost;user={credentials[0]};password={credentials[1]};database=ecommerce";

                using (var connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        // Query to check for a user that matches the encrypted email and hashed password
                        string query = "SELECT AccountID FROM Account WHERE Email = @Email AND Password = @Password";
                        using (MySqlCommand cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@Email", encryptedInputEmail);
                            cmd.Parameters.AddWithValue("@Password", hashedInputPassword);

                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read()) // True if a matching record is found
                                {
                                    // Successful login logic here...
                                    LoginState.RemainingTries = 3; // Reset tries upon successful login

                                    // Note: Ensure the SQL query above is correctly fetching the AccountID
                                    int accountID = reader.GetInt32(0); // Get AccountID assuming it's the first column in the result set

                                    string auditDescription = $"A user logged in with the email: {inputEmail}."; // Use unencrypted email for clarity in audit

                                    // Try to insert audit log for successful login
                                    try
                                    {
                                        using (var auditCmd = new MySqlCommand("INSERT INTO Audit (SessionStart, SessionEnd, Description, AccountID) VALUES (NOW(), NOW(), @desc, @AccountID)", connection))
                                        {
                                            auditCmd.Parameters.AddWithValue("@desc", auditDescription);
                                            auditCmd.Parameters.AddWithValue("@AccountID", accountID);

                                            // Execute the command to insert the audit entry
                                            auditCmd.ExecuteNonQuery();
                                        }
                                    }
                                    catch
                                    {
                                        //Do nothing
                                    }

                                    MessageBox.Show("Login Successful! You may now access the main page", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    // Proceed with other login success actions
                                }
                                else // No matching record found
                                {
                                    LoginState.RemainingTries--;
                                    MessageBox.Show($"Login Failed. Please check your email and password. {LoginState.RemainingTries} attempts remaining.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    if (LoginState.RemainingTries <= 0)
                                    {
                                        DisableLogin();
                                        LoginState.StartLockout(); // Start or continue the lockout
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show("There was an issue with the login process. Please contact a system administrator", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else // Connection failed
            {
                MessageBox.Show("There was a problem establishing a connection, please try again later", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisableLogin()
        {
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            button3.Enabled = false; 
        }

        private void Login_Load(object sender, EventArgs e)
        {
            if (LoginState.IsLockedOut)
            {
                DisableLogin();
            }

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
