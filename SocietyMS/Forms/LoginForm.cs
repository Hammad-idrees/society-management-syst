using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;
using SocietyMS.Models;

namespace SocietyMS.Forms
{
    /// <summary>
    /// Aurora-themed Login Form. Deep space left panel + glassmorphism card.
    /// Routes each role to its dedicated dashboard after successful login.
    /// </summary>
    public class LoginForm : Form
    {
        private TextBox txtEmail, txtPassword;
        private Button btnLogin, btnRegister;
        private Label lblError;
        private CheckBox chkShowPass;

        public LoginForm()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Text = "FAST Societies MS - Login";
            Size = new Size(1100, 680);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            // ── Left gradient panel ───────────────────────────────────────────────
            var pnlLeft = UITheme.MakeGradientPanel(0, 0, 520, 680);
            Controls.Add(pnlLeft);

            // Violet accent stripe
            var stripe = new Panel { Location = new Point(0,0), Size = new Size(5,680),
                BackColor = UITheme.AccentViolet };
            pnlLeft.Controls.Add(stripe);

            // Logo / title
            var lblBig = new Label
            {
                Text = "FAST\nSocieties\nManagement\nSystem",
                Location = new Point(50, 100), Size = new Size(420, 280),
                Font = new Font("Segoe UI", 32f, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary, BackColor = Color.Transparent
            };
            pnlLeft.Controls.Add(lblBig);

            var lblTag = new Label
            {
                Text = "Connecting students  ·  Empowering societies",
                Location = new Point(50, 400), Size = new Size(420, 30),
                Font = new Font("Segoe UI", 10f, FontStyle.Italic),
                ForeColor = UITheme.AccentCyan, BackColor = Color.Transparent
            };
            pnlLeft.Controls.Add(lblTag);

            // Emoji icons
            string[] icons = { "🎮", "⚽", "💻", "📖", "📸" };
            for (int i = 0; i < icons.Length; i++)
                pnlLeft.Controls.Add(new Label {
                    Text = icons[i], Font = new Font("Segoe UI Emoji",22f),
                    Location = new Point(50 + i * 88, 490), AutoSize = true,
                    BackColor = Color.Transparent
                });

            // ── Right card ────────────────────────────────────────────────────────
            var card = UITheme.MakeCard(565, 110, 470, 450);
            Controls.Add(card);

            var lblTitle = new Label { Text = "Welcome Back",
                Location = new Point(18,16), AutoSize = true,
                Font = UITheme.FontTitle, ForeColor = UITheme.TextPrimary,
                BackColor = Color.Transparent };
            card.Controls.Add(lblTitle);

            card.Controls.Add(UITheme.MakeLabel("Sign in to continue", 18, 60));

            // Violet separator line
            var sep = new Panel { Location = new Point(18,84), Size = new Size(434, 2),
                BackColor = UITheme.AccentViolet };
            card.Controls.Add(sep);

            card.Controls.Add(UITheme.MakeLabel("Email Address", 18, 100));
            txtEmail = UITheme.MakeTextBox(18, 122, 434);
            card.Controls.Add(txtEmail);

            card.Controls.Add(UITheme.MakeLabel("Password", 18, 172));
            txtPassword = UITheme.MakeTextBox(18, 194, 434, password: true);
            card.Controls.Add(txtPassword);

            chkShowPass = new CheckBox { Text = "Show password",
                Location = new Point(18, 240), AutoSize = true,
                ForeColor = UITheme.TextSecond, BackColor = Color.Transparent,
                Font = UITheme.FontSmall };
            chkShowPass.CheckedChanged += (s, e) =>
                txtPassword.PasswordChar = chkShowPass.Checked ? '\0' : '●';
            card.Controls.Add(chkShowPass);

            lblError = new Label { Text = "", Location = new Point(18, 265),
                Size = new Size(434, 22), ForeColor = UITheme.AccentRed,
                Font = UITheme.FontSmall, BackColor = Color.Transparent };
            card.Controls.Add(lblError);

            btnLogin = UITheme.MakePrimaryButton("Sign In", 18, 295, 434, 48);
            btnLogin.Click += BtnLogin_Click;
            card.Controls.Add(btnLogin);

            btnRegister = UITheme.MakeSecondaryButton("Create New Account", 18, 357, 434, 44);
            btnRegister.Click += (s, e) => new RegisterForm().ShowDialog(this);
            card.Controls.Add(btnRegister);

            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnLogin_Click(s, e); };
            txtEmail.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtPassword.Focus(); };

            Controls.Add(new Label {
                Text = "v1.0  |  SE-4011 Software Measurement and Metrics  |  FAST-NUCES 2026",
                Location = new Point(0, 648), Size = new Size(1100, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = UITheme.TextMuted, Font = UITheme.FontSmall,
                BackColor = Color.Transparent
            });
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            string email = txtEmail.Text.Trim();
            string pass  = txtPassword.Text;

            if (!ValidationHelper.IsNotEmpty(email) || !ValidationHelper.IsNotEmpty(pass))
            { lblError.Text = "Please fill in all fields."; return; }
            if (!ValidationHelper.IsValidEmail(email))
            { lblError.Text = "Invalid email format."; return; }

            try
            {
                btnLogin.Enabled = false; btnLogin.Text = "Signing in...";
                string hash = ValidationHelper.HashPassword(pass);
                DataTable dt = DatabaseManager.ExecuteQuery(
                    "SELECT * FROM Users WHERE Email=@E AND IsActive=1",
                    new System.Data.SqlClient.SqlParameter("@E", email));

                if (dt.Rows.Count == 0) { lblError.Text = "No account found for this email."; return; }
                DataRow row = dt.Rows[0];
                if (row["PasswordHash"].ToString() != hash) { lblError.Text = "Incorrect password."; return; }

                User user = new User
                {
                    UserID       = (int)row["UserID"],
                    FullName     = row["FullName"].ToString(),
                    Email        = row["Email"].ToString(),
                    PasswordHash = row["PasswordHash"].ToString(),
                    Role         = row["Role"].ToString(),
                    RollNumber   = row["RollNumber"] == DBNull.Value ? "" : row["RollNumber"].ToString(),
                    Department   = row["Department"] == DBNull.Value ? "" : row["Department"].ToString(),
                    Semester     = row["Semester"] == DBNull.Value ? (int?)null : (int)row["Semester"],
                    PhoneNumber  = row["PhoneNumber"] == DBNull.Value ? "" : row["PhoneNumber"].ToString(),
                    IsActive     = (bool)row["IsActive"],
                    CreatedAt    = (DateTime)row["CreatedAt"]
                };

                SessionManager.Login(user);
                Form dash;
                if (user.Role == "Admin")            dash = new AdminDashboard();
                else if (user.Role == "SocietyHead") dash = new SocietyHeadDashboard();
                else                                  dash = new StudentDashboard();

                Hide();
                dash.FormClosed += (s2, e2) => Show();
                dash.Show();
            }
            catch (Exception ex) { lblError.Text = "Login failed: " + ex.Message; }
            finally { btnLogin.Enabled = true; btnLogin.Text = "Sign In"; }
        }
    }
}

