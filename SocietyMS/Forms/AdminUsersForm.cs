using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    public class AdminUsersForm : Form
    {
        private DataGridView dgvUsers;
        private TextBox txtSearch;
        private Button btnActivate, btnDeactivate, btnRefresh;
        private Label lblMsg;

        public AdminUsersForm() { InitializeUI(); LoadUsers(); }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Dock = DockStyle.Fill;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = UITheme.BgDeep };
            var lbl = UITheme.MakeHeader("👥 Manage User Accounts", 20, 16);
            lbl.Font = UITheme.FontSubtitle;
            pnlTop.Controls.Add(lbl);
            
            pnlTop.Controls.Add(UITheme.MakeLabel("Search:", 20, 56));
            txtSearch = UITheme.MakeTextBox(82, 52, 280);
            txtSearch.TextChanged += (s, e) => LoadUsers();
            pnlTop.Controls.Add(txtSearch);
            
            btnRefresh = UITheme.MakePrimaryButton("🔄 Refresh", 380, 50, 100, 36);
            btnRefresh.Click += (s, e) => LoadUsers();
            pnlTop.Controls.Add(btnRefresh);

            btnActivate = UITheme.MakeSuccessButton("✓ Activate", 490, 48, 140, 40);
            btnActivate.Click += (s, e) => SetActive(true);
            pnlTop.Controls.Add(btnActivate);

            btnDeactivate = UITheme.MakeDangerButton("⛔ Deactivate", 640, 48, 140, 40);
            btnDeactivate.Click += (s, e) => SetActive(false);
            pnlTop.Controls.Add(btnDeactivate);

            lblMsg = new Label { Location = new Point(380, 80), AutoSize = true, Font = UITheme.FontSmall, ForeColor = UITheme.AccentGreen };
            pnlTop.Controls.Add(lblMsg);
            
            Controls.Add(pnlTop);

            dgvUsers = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGrid(dgvUsers);
            Controls.Add(dgvUsers);

            var ctx = new ContextMenuStrip();
            ctx.BackColor = UITheme.BgCard; ctx.ForeColor = UITheme.TextPrimary;
            var mnuAct = new ToolStripMenuItem("✓ Activate");
            mnuAct.Click += (s, e) => SetActive(true);
            ctx.Items.Add(mnuAct);
            var mnuDeact = new ToolStripMenuItem("⛔ Deactivate");
            mnuDeact.Click += (s, e) => SetActive(false);
            ctx.Items.Add(mnuDeact);
            dgvUsers.ContextMenuStrip = ctx;
        }

        private void LoadUsers()
        {
            try
            {
                string sql = @"SELECT UserID AS ID, FullName AS [Name], Email, Role,
                                      RollNumber AS [Roll No], Department AS [Dept],
                                      CASE IsActive WHEN 1 THEN 'Active' ELSE 'Inactive' END AS [Status],
                                      CreatedAt AS [Joined]
                               FROM Users WHERE @S='' OR FullName LIKE '%'+@S+'%' OR Email LIKE '%'+@S+'%'
                               ORDER BY CreatedAt DESC";
                DataTable dt = DatabaseManager.ExecuteQuery(sql, new SqlParameter("@S", txtSearch.Text.Trim()));
                dgvUsers.DataSource = dt;
                if (dgvUsers.Columns.Contains("ID")) dgvUsers.Columns["ID"].Visible = false;
                lblMsg.Text = "";
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }

        private void SetActive(bool active)
        {
            lblMsg.Text = "";
            if (dgvUsers.CurrentRow == null) { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ Select a user first."; return; }
            int uid = Convert.ToInt32(dgvUsers.CurrentRow.Cells["ID"].Value);
            try
            {
                DatabaseManager.ExecuteNonQuery("UPDATE Users SET IsActive=@A WHERE UserID=@U",
                    new SqlParameter("@A", active ? 1 : 0), new SqlParameter("@U", uid));
                lblMsg.ForeColor = UITheme.AccentGreen;
                lblMsg.Text = "✓ User " + (active ? "activated" : "deactivated") + ".";
                LoadUsers();
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }
    }
}
