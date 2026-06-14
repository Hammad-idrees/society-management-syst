using System;
using System.Drawing;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    /// <summary>
    /// Registration form: allows new students to create accounts.
    /// Collects profile info, validates input, hashes password, and inserts into DB.
    /// </summary>
    public class RegisterForm : Form
    {
        private TextBox txtName, txtEmail, txtPassword, txtConfirm,
                        txtRoll, txtPhone;
        private ComboBox cmbDept, cmbSemester;
        private Button btnRegister, btnCancel;
        private Label lblError, lblSuccess;

        public RegisterForm()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Text = "Create New Account – FAST Societies MS";
            Size = new Size(560, 680);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            int lx = 30, fx = 30, fw = 480, ly = 20;

            // Title
            var lbl = UITheme.MakeHeader("Create Your Account", lx, ly);
            lbl.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            Controls.Add(lbl);
            ly += 45;

            var sub = UITheme.MakeLabel("Join FAST Societies Management System", lx, ly);
            Controls.Add(sub); ly += 40;

            // Full Name
            Controls.Add(UITheme.MakeLabel("Full Name *", lx, ly)); ly += 22;
            Controls.Add(txtName); ly += 48;

            // Email
            Controls.Add(UITheme.MakeLabel("University Email *", lx, ly)); ly += 22;
            Controls.Add(txtEmail); ly += 48;

            // Roll Number + Phone (side by side)
            Controls.Add(UITheme.MakeLabel("Roll Number", lx, ly));
            Controls.Add(UITheme.MakeLabel("Phone Number", lx + 250, ly)); ly += 22;
            Controls.Add(txtRoll); Controls.Add(txtPhone); ly += 48;

            // Department + Semester
            Controls.Add(UITheme.MakeLabel("Department", lx, ly));
            Controls.Add(UITheme.MakeLabel("Semester", lx + 280, ly)); ly += 22;
            cmbDept = new ComboBox { Location = new Point(fx, ly), Size = new Size(250, 36) };
            UITheme.StyleComboBox(cmbDept);
            cmbDept.Items.AddRange(new[] { "CS", "SE", "AI", "CY", "DS", "EE", "BBA", "Other" });
            cmbDept.SelectedIndex = 0; cmbDept.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(cmbDept);

            cmbSemester = new ComboBox { Location = new Point(fx + 280, ly), Size = new Size(200, 36) };
            UITheme.StyleComboBox(cmbSemester);
            for (int i = 1; i <= 8; i++) cmbSemester.Items.Add("Semester " + i);
            cmbSemester.SelectedIndex = 0; cmbSemester.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(cmbSemester); ly += 48;

            // Password
            Controls.Add(UITheme.MakeLabel("Password * (min 8 chars, letters+digits)", lx, ly)); ly += 22;
            Controls.Add(txtPassword); ly += 48;

            // Confirm Password
            Controls.Add(UITheme.MakeLabel("Confirm Password *", lx, ly)); ly += 22;
            Controls.Add(txtConfirm); ly += 48;

            // Feedback labels
            lblError = new Label { Text = "", Location = new Point(lx, ly), Size = new Size(fw, 22),
                ForeColor = UITheme.AccentRed, Font = UITheme.FontSmall };
            Controls.Add(lblError);
            lblSuccess = new Label { Text = "", Location = new Point(lx, ly), Size = new Size(fw, 22),
                ForeColor = UITheme.AccentGreen, Font = UITheme.FontSmall };
            Controls.Add(lblSuccess); ly += 30;

            // Buttons
            btnRegister = UITheme.MakePrimaryButton("Create Account", lx, ly, 230, 44);
            btnRegister.Click += BtnRegister_Click;
            Controls.Add(btnRegister);

            btnCancel = UITheme.MakeSecondaryButton("Cancel", lx + 250, ly, 260, 44);
            btnCancel.Click += (s, e) => Close();
            Controls.Add(btnCancel);
        }

        // ─── Registration Logic ───────────────────────────────────────────────────
        private void BtnRegister_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            lblSuccess.Text = "";

            // Gather inputs
            string name     = txtName.Text.Trim();
            string email    = txtEmail.Text.Trim();
            string roll     = txtRoll.Text.Trim();
            string phone    = txtPhone.Text.Trim();
            string dept     = cmbDept.SelectedItem?.ToString() ?? "";
            int    semester = cmbSemester.SelectedIndex + 1;
            string pass     = txtPassword.Text;
            string confirm  = txtConfirm.Text;

            // Validation chain
            if (!ValidationHelper.IsNotEmpty(name))   { lblError.Text = "⚠ Full name is required."; return; }
            if (!ValidationHelper.IsValidEmail(email)) { lblError.Text = "⚠ Invalid email address."; return; }
            if (!ValidationHelper.IsStrongPassword(pass))
            { lblError.Text = "⚠ Password must be 8+ chars with letters and digits."; return; }
            if (pass != confirm) { lblError.Text = "⚠ Passwords do not match."; return; }

            try
            {
                btnRegister.Enabled = false;
                btnRegister.Text = "Creating...";

                // Check for duplicate email
                object existing = DatabaseManager.ExecuteScalar(
                    "SELECT COUNT(1) FROM Users WHERE Email = @Email",
                    new System.Data.SqlClient.SqlParameter("@Email", email));

                if (Convert.ToInt32(existing) > 0)
                {
                    lblError.Text = "⚠ An account with this email already exists.";
                    return;
                }

                string hash = ValidationHelper.HashPassword(pass);
                string sql = @"INSERT INTO Users
                    (FullName, Email, PasswordHash, Role, RollNumber, Department, Semester, PhoneNumber)
                    VALUES (@Name, @Email, @Hash, 'Student', @Roll, @Dept, @Sem, @Phone)";

                DatabaseManager.ExecuteNonQuery(sql,
                    new System.Data.SqlClient.SqlParameter("@Name",  name),
                    new System.Data.SqlClient.SqlParameter("@Email", email),
                    new System.Data.SqlClient.SqlParameter("@Hash",  hash),
                    new System.Data.SqlClient.SqlParameter("@Roll",  string.IsNullOrEmpty(roll) ? (object)DBNull.Value : roll),
                    new System.Data.SqlClient.SqlParameter("@Dept",  dept),
                    new System.Data.SqlClient.SqlParameter("@Sem",   semester),
                    new System.Data.SqlClient.SqlParameter("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone));

                lblSuccess.Text = "✓ Account created! You can now log in.";
                lblError.Text = "";
                System.Threading.Thread.Sleep(1500);
                Close();
            }
            catch (Exception ex)
            {
                lblError.Text = "⚠ Registration failed: " + ex.Message;
            }
            finally
            {
                btnRegister.Enabled = true;
                btnRegister.Text = "Create Account";
            }
        }
    }
}



