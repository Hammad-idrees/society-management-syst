using System;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Forms;

namespace SocietyMS
{
    /// <summary>Application entry point. Initialises DB then shows Login form.</summary>
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ── Attempt DB connection / schema creation ─────────────────────────
            try
            {
                if (!DatabaseManager.TestConnection())
                {
                    // Fallback: try with named instance or prompt user
                    bool connected = TryAlternativeConnections();
                    if (!connected)
                    {
                        MessageBox.Show(
                            "Cannot connect to SQL Server.\n\n" +
                            "Please ensure SQL Server LocalDB or SQL Server Express is installed.\n" +
                            "Install from: https://aka.ms/sqllocaldb\n\n" +
                            "The application will now close.",
                            "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                DatabaseManager.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new LoginForm());
        }

        /// <summary>Tries alternate SQL Server connection strings.</summary>
        private static bool TryAlternativeConnections()
        {
            string[] alternatives = new[]
            {
                @"Data Source=.\SQLEXPRESS;Initial Catalog=SocietyMS;Integrated Security=True;Connect Timeout=10;",
                @"Data Source=localhost;Initial Catalog=SocietyMS;Integrated Security=True;Connect Timeout=10;",
                @"Data Source=(local);Initial Catalog=SocietyMS;Integrated Security=True;Connect Timeout=10;"
            };

            foreach (string cs in alternatives)
            {
                DatabaseManager.ConnectionString = cs;
                if (DatabaseManager.TestConnection()) return true;
            }
            return false;
        }
    }
}


