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

        public static string EncryptAndHashString(string inputString)
        {
            // Retrieve the HMAC key from the environment variable and convert from hex to byte[]
            string aesKeyHex = Environment.GetEnvironmentVariable("HMAC_KEY");
            byte[] key = Enumerable.Range(0, aesKeyHex.Length)
                        .Where(x => x % 2 == 0)
                        .Select(x => Convert.ToByte(aesKeyHex.Substring(x, 2), 16))
                        .ToArray();

            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                // Compute the HMAC of the input string
                byte[] hmacBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputString));

                // Convert the byte array to a Base64 string instead of hexadecimal for the hash
                string hashBase64 = Convert.ToBase64String(hmacBytes);

                // Generate a random salt for each input
                byte[] saltBytes = new byte[10]; // 10 bytes for the salt
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(saltBytes);
                }
                string salt = Convert.ToBase64String(saltBytes); // Base64 encode to ensure readable characters
                salt = salt.Substring(0, 10); // Ensure the salt is exactly 10 characters long

                // Append the salt to the hash
                return hashBase64 + salt; // Here, salt is appended after the hash
            }
        }

        public static string DecryptStringAES(string encryptedText)
        {
            // Retrieve AES-128 encryption key and IV from environment variables
            string hexKey = Environment.GetEnvironmentVariable("PRGRM_KEY");
            string hexIV = Environment.GetEnvironmentVariable("PRGRM_IV");

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

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                try
                {
                    byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                    using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        public static string EncryptStringAES(string plainText)
        {
            // Retrieve AES-128 encryption key and IV from environment variables
            string hexKey = Environment.GetEnvironmentVariable("PRGRM_KEY");
            string hexIV = Environment.GetEnvironmentVariable("PRGRM_IV");

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

            // Encrypt the input email using AES
            string encryptedInputEmail = EncryptStringAES(inputEmail);

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
                        // Query to check for a user that matches the encrypted email
                        string query = "SELECT Password FROM Account WHERE Email = @Email";
                        using (MySqlCommand cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@Email", encryptedInputEmail);

                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read()) // Read the first record if available
                                {
                                    // Retrieve hashed and salted password from the database
                                    string storedHashedAndSaltedPassword = reader.GetString("Password");

                                    // Compute the HMAC hash of the input password
                                    string inputPasswordHashed = EncryptAndHashString(inputPassword);

                                    // Now we assume that the hash in the database has the salt appended after the hash
                                    // So we remove the last 10 characters (the salt) before comparing
                                    string storedHashWithoutSalt = storedHashedAndSaltedPassword.Substring(0, storedHashedAndSaltedPassword.Length - 10);
                                    string inputHashWithoutSalt = inputPasswordHashed.Substring(0, inputPasswordHashed.Length - 10);

                                    // Compare the hash values without the salt
                                    if (storedHashWithoutSalt == inputHashWithoutSalt)
                                    {
                                        // Successful login logic here...
                                        LoginState.RemainingTries = 3; // Reset tries upon successful login

                                        MessageBox.Show("Login Successful! You may now access the main page", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        // Proceed with other login success actions
                                        return;
                                    }
                                    else
                                    {
                                        // Passwords do not match
                                        LoginState.RemainingTries--;
                                        MessageBox.Show($"Login Failed. Please check your email and password. {LoginState.RemainingTries} attempts remaining.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        if (LoginState.RemainingTries <= 0)
                                        {
                                            DisableLogin();
                                            LoginState.StartLockout(); // Start or continue the lockout
                                        }
                                    }
                                }
                                else
                                {
                                    // Email not found in database
                                    MessageBox.Show("Login Failed. The email provided does not exist in our records.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show("There was an issue with the login process. Please contact a system administrator.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else // Connection failed
            {
                MessageBox.Show("There was a problem establishing a connection, please try again later.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

// CHANGE THE SALTING!!!