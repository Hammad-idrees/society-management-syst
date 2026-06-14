using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SocietyMS.Database
{
    /// <summary>
    /// Central database manager: handles all SQL Server connections, CRUD operations,
    /// and schema initialization for the FAST Societies Management System.
    /// </summary>
    public class DatabaseManager
    {
        // ─── Connection ────────────────────────────────────────────────────────────
        private static string _connectionString =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SocietyMS;Integrated Security=True;Connect Timeout=30;";

        public static string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        // ─── Connection Test ───────────────────────────────────────────────────────
        /// <summary>Tests the database connection. Returns true if successful.</summary>
        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    return conn.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        // ─── Get Open Connection ──────────────────────────────────────────────────
        /// <summary>Returns an open SqlConnection. Caller must dispose it.</summary>
        public static SqlConnection GetConnection()
        {
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        // ─── Execute Non-Query ────────────────────────────────────────────────────
        /// <summary>Executes INSERT/UPDATE/DELETE. Returns rows affected.</summary>
        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                LogError("ExecuteNonQuery", ex);
                throw;
            }
        }

        // ─── Execute Scalar ───────────────────────────────────────────────────────
        /// <summary>Executes a query returning a single value.</summary>
        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                LogError("ExecuteScalar", ex);
                throw;
            }
        }

        // ─── Execute Query ────────────────────────────────────────────────────────
        /// <summary>Executes a SELECT and returns a DataTable.</summary>
        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                    return dt;
                }
            }
            catch (Exception ex)
            {
                LogError("ExecuteQuery", ex);
                throw;
            }
        }

        // ─── Error Logger ─────────────────────────────────────────────────────────
        private static void LogError(string source, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SocietyMS DB Error] {source}: {ex.Message}");
        }

        // ─── Initialize Database Schema ───────────────────────────────────────────
        /// <summary>Creates all tables and seeds default admin if they don't exist.</summary>
        public static void InitializeDatabase()
        {
            try
            {
                string[] scripts = GetSchemaScripts();
                using (SqlConnection conn = GetConnection())
                {
                    foreach (string script in scripts)
                    {
                        if (string.IsNullOrWhiteSpace(script)) continue;
                        using (SqlCommand cmd = new SqlCommand(script, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Database initialization failed:\n{ex.Message}\n\nPlease ensure SQL Server LocalDB is installed.",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        // ─── Schema SQL Scripts ───────────────────────────────────────────────────
        private static string[] GetSchemaScripts()
        {
            return new string[]
            {
                // Users table
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
                CREATE TABLE Users (
                    UserID       INT IDENTITY(1,1) PRIMARY KEY,
                    FullName     NVARCHAR(100) NOT NULL,
                    Email        NVARCHAR(150) NOT NULL UNIQUE,
                    PasswordHash NVARCHAR(256) NOT NULL,
                    Role         NVARCHAR(20)  NOT NULL DEFAULT 'Student',
                    RollNumber   NVARCHAR(20)  NULL,
                    Department   NVARCHAR(100) NULL,
                    Semester     INT           NULL,
                    PhoneNumber  NVARCHAR(20)  NULL,
                    IsActive     BIT           NOT NULL DEFAULT 1,
                    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE()
                )",

                // Societies table
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Societies' AND xtype='U')
                CREATE TABLE Societies (
                    SocietyID   INT IDENTITY(1,1) PRIMARY KEY,
                    Name        NVARCHAR(100) NOT NULL,
                    Description NVARCHAR(500) NULL,
                    Category    NVARCHAR(50)  NULL,
                    HeadUserID  INT           NULL REFERENCES Users(UserID),
                    LogoPath    NVARCHAR(255) NULL,
                    Status      NVARCHAR(20)  NOT NULL DEFAULT 'Pending',
                    MaxMembers  INT           NOT NULL DEFAULT 50,
                    CreatedAt   DATETIME      NOT NULL DEFAULT GETDATE()
                )",

                // Memberships table
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Memberships' AND xtype='U')
                CREATE TABLE Memberships (
                    MembershipID INT IDENTITY(1,1) PRIMARY KEY,
                    UserID       INT NOT NULL REFERENCES Users(UserID),
                    SocietyID    INT NOT NULL REFERENCES Societies(SocietyID),
                    Role         NVARCHAR(30) NOT NULL DEFAULT 'Member',
                    Status       NVARCHAR(20) NOT NULL DEFAULT 'Pending',
                    AppliedAt    DATETIME NOT NULL DEFAULT GETDATE(),
                    ApprovedAt   DATETIME NULL,
                    UNIQUE(UserID, SocietyID)
                )",

                // Events table
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Events' AND xtype='U')
                CREATE TABLE Events (
                    EventID      INT IDENTITY(1,1) PRIMARY KEY,
                    SocietyID    INT NOT NULL REFERENCES Societies(SocietyID),
                    Title        NVARCHAR(150) NOT NULL,
                    Description  NVARCHAR(1000) NULL,
                    EventDate    DATETIME NOT NULL,
                    Venue        NVARCHAR(200) NULL,
                    MaxAttendees INT NOT NULL DEFAULT 100,
                    Status       NVARCHAR(20) NOT NULL DEFAULT 'Pending',
                    CreatedAt    DATETIME NOT NULL DEFAULT GETDATE()
                )",

                // EventRegistrations table
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='EventRegistrations' AND xtype='U')
                CREATE TABLE EventRegistrations (
                    RegID     INT IDENTITY(1,1) PRIMARY KEY,
                    EventID   INT NOT NULL REFERENCES Events(EventID),
                    UserID    INT NOT NULL REFERENCES Users(UserID),
                    TicketNum NVARCHAR(20) NOT NULL,
                    RegisteredAt DATETIME NOT NULL DEFAULT GETDATE(),
                    UNIQUE(EventID, UserID)
                )",

                // Tasks table
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SocietyTasks' AND xtype='U')
                CREATE TABLE SocietyTasks (
                    TaskID      INT IDENTITY(1,1) PRIMARY KEY,
                    SocietyID   INT NOT NULL REFERENCES Societies(SocietyID),
                    AssignedTo  INT NOT NULL REFERENCES Users(UserID),
                    AssignedBy  INT NOT NULL REFERENCES Users(UserID),
                    Title       NVARCHAR(150) NOT NULL,
                    Description NVARCHAR(500) NULL,
                    DueDate     DATETIME NULL,
                    Priority    NVARCHAR(10) NOT NULL DEFAULT 'Medium',
                    Status      NVARCHAR(20) NOT NULL DEFAULT 'Pending',
                    CreatedAt   DATETIME NOT NULL DEFAULT GETDATE()
                )",

                // Announcements table
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Announcements' AND xtype='U')
                CREATE TABLE Announcements (
                    AnnouncementID INT IDENTITY(1,1) PRIMARY KEY,
                    SocietyID      INT NULL REFERENCES Societies(SocietyID),
                    PostedBy       INT NOT NULL REFERENCES Users(UserID),
                    Title          NVARCHAR(150) NOT NULL,
                    Content        NVARCHAR(2000) NOT NULL,
                    IsGlobal       BIT NOT NULL DEFAULT 0,
                    CreatedAt      DATETIME NOT NULL DEFAULT GETDATE()
                )",

                // Seed default Admin account (password: Admin@123 -> SHA256 hash)
                @"IF NOT EXISTS (SELECT 1 FROM Users WHERE Role='Admin')
                INSERT INTO Users (FullName, Email, PasswordHash, Role)
                VALUES ('System Administrator', 'admin@fast.edu.pk',
                    'e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7',
                    'Admin')",

                // Seed sample societies
                @"IF NOT EXISTS (SELECT 1 FROM Societies)
                BEGIN
                    INSERT INTO Societies (Name, Description, Category, Status, MaxMembers)
                    VALUES
                    ('Developers Club',  'A hub for software enthusiasts and tech innovators.', 'Technology',  'Active', 80),
                    ('Gaming Society',   'Competitive gaming, esports, and game development.',  'Entertainment','Active', 60),
                    ('Literary Society', 'Debates, creative writing, and public speaking.',      'Arts',        'Active', 50),
                    ('Sports Society',   'Cricket, football, badminton and more.',               'Sports',      'Active', 100),
                    ('Media Society',    'Photography, filmmaking, and digital content.',        'Media',       'Active', 45)
                END"
            };
        }
    }
}


