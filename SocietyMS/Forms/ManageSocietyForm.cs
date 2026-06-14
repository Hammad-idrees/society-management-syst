using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    /// <summary>Manage society profile: name, description, category, head assignment.</summary>
    public class ManageSocietyForm : Form
    {
        private TextBox txtName, txtDesc;
        private ComboBox cmbCategory, cmbStatus;
        private NumericUpDown nudMax;
        private Button btnSave;
        private Label lblMsg;
        private int _societyId;

        public ManageSocietyForm()
        {
            _societyId = GetSocietyId();
            InitializeUI();
            if (_societyId > 0) LoadSociety();
        }

        private int GetSocietyId()
        {
            try
            {
                object id = DatabaseManager.ExecuteScalar(
                    "SELECT SocietyID FROM Societies WHERE HeadUserID=@U",
                    new SqlParameter("@U", SessionManager.CurrentUser.UserID));
                return id != null && id != DBNull.Value ? Convert.ToInt32(id) : 0;
            }
            catch { return 0; }
        }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Dock = DockStyle.Fill;

            Controls.Add(UITheme.MakeHeader("✏ Manage Society Profile", 20, 20));
            int y = 70, lx = 20, fx = 20, fw = 500;

            Controls.Add(UITheme.MakeLabel("Society Name *", lx, y)); y += 24;
            txtName = UITheme.MakeTextBox(fx, y, fw); Controls.Add(txtName); y += 52;

            Controls.Add(UITheme.MakeLabel("Category", lx, y)); y += 24;
            cmbCategory = new ComboBox { Location = new Point(fx, y), Size = new Size(fw, 36) };
            UITheme.StyleComboBox(cmbCategory);
            cmbCategory.Items.AddRange(new[] { "Technology","Entertainment","Arts","Sports","Media","Other" });
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(cmbCategory); y += 52;

            Controls.Add(UITheme.MakeLabel("Description", lx, y)); y += 24;
            txtDesc = new TextBox { Location = new Point(fx, y), Size = new Size(fw, 100),
                Multiline = true, ScrollBars = ScrollBars.Vertical,
                BackColor = UITheme.BgCard, ForeColor = UITheme.TextPrimary, Font = UITheme.FontBody };
            Controls.Add(txtDesc); y += 115;

            Controls.Add(UITheme.MakeLabel("Max Members", lx, y)); y += 24;
            nudMax = new NumericUpDown { Location = new Point(fx, y), Size = new Size(120, 36),
                Minimum = 5, Maximum = 500, Value = 50,
                BackColor = UITheme.BgCard, ForeColor = UITheme.TextPrimary, Font = UITheme.FontBody };
            Controls.Add(nudMax); y += 52;

            lblMsg = new Label { Location = new Point(lx, y), Size = new Size(fw, 24),
                Font = UITheme.FontBody, ForeColor = UITheme.AccentGreen }; Controls.Add(lblMsg); y += 32;

            btnSave = UITheme.MakePrimaryButton("💾 Save Changes", lx, y, 200, 44);
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);
        }

        private void LoadSociety()
        {
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(
                    "SELECT * FROM Societies WHERE SocietyID=@S",
                    new SqlParameter("@S", _societyId));
                if (dt.Rows.Count == 0) return;
                DataRow r = dt.Rows[0];
                txtName.Text = r["Name"].ToString();
                txtDesc.Text = r["Description"].ToString();
                nudMax.Value = Convert.ToDecimal(r["MaxMembers"]);
                cmbCategory.SelectedItem = r["Category"].ToString();
                if (cmbCategory.SelectedIndex < 0) cmbCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                lblMsg.ForeColor = UITheme.AccentRed;
                lblMsg.Text = "⚠ " + ex.Message;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";
            string name = txtName.Text.Trim();
            if (!ValidationHelper.IsNotEmpty(name))
            {
                lblMsg.ForeColor = UITheme.AccentRed;
                lblMsg.Text = "⚠ Society name is required."; return;
            }

            try
            {
                if (_societyId == 0)
                {
                    // Create new society request
                    DatabaseManager.ExecuteNonQuery(
                        @"INSERT INTO Societies (Name, Description, Category, MaxMembers, HeadUserID, Status)
                          VALUES (@N, @D, @C, @M, @H, 'Pending')",
                        new SqlParameter("@N", name),
                        new SqlParameter("@D", txtDesc.Text.Trim()),
                        new SqlParameter("@C", cmbCategory.SelectedItem?.ToString() ?? "Other"),
                        new SqlParameter("@M", (int)nudMax.Value),
                        new SqlParameter("@H", SessionManager.CurrentUser.UserID));
                    lblMsg.ForeColor = UITheme.AccentGreen;
                    lblMsg.Text = "✓ Society creation request submitted for admin approval.";
                    _societyId = Convert.ToInt32(DatabaseManager.ExecuteScalar(
                        "SELECT TOP 1 SocietyID FROM Societies WHERE HeadUserID=@H ORDER BY CreatedAt DESC",
                        new SqlParameter("@H", SessionManager.CurrentUser.UserID)));
                }
                else
                {
                    DatabaseManager.ExecuteNonQuery(
                        "UPDATE Societies SET Name=@N, Description=@D, Category=@C, MaxMembers=@M WHERE SocietyID=@S",
                        new SqlParameter("@N", name),
                        new SqlParameter("@D", txtDesc.Text.Trim()),
                        new SqlParameter("@C", cmbCategory.SelectedItem?.ToString() ?? "Other"),
                        new SqlParameter("@M", (int)nudMax.Value),
                        new SqlParameter("@S", _societyId));
                    lblMsg.ForeColor = UITheme.AccentGreen;
                    lblMsg.Text = "✓ Society profile updated successfully.";
                }
            }
            catch (Exception ex)
            {
                lblMsg.ForeColor = UITheme.AccentRed;
                lblMsg.Text = "⚠ " + ex.Message;
            }
        }
    }
}



