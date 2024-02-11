using System;
using System.Windows.Forms;

namespace MyApp
{
    // Define a class that inherits from Form to create your GUI window
    public class MainForm : Form
    {
        // Constructor for your main form
        public MainForm()
        {
            // Initialize your form here
            // Example: this.Text = "My Application";
        }

        // Main entry point of your application
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Run an instance of your MainForm
            Application.Run(new MainForm());
        }
    }
}
