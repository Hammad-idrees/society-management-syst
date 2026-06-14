using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    public class AdminSocietiesForm : Form
    {
        private DataGridView dgvSocieties;
        private Button btnApprove, btnSuspend, btnDelete, btnAssign, btnRefresh;
        private ComboBox cmbUsers;
        private Label lblMsg;

        public AdminSocietiesForm() { InitializeUI(); LoadSocieties(); LoadUsers(); }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Dock = DockStyle.Fill;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 140, BackColor = UITheme.BgDeep };
            var lbl = UITheme.MakeHeader("🏛 Manage All Societies", 20, 14);
            lbl.Font = UITheme.FontSubtitle;
            pnlTop.Controls.Add(lbl);
            
            btnRefresh = UITheme.MakePrimaryButton("🔄 Refresh", 350, 12, 110, 36);
            btnRefresh.Click += (s, e) => { LoadSocieties(); LoadUsers(); };
            pnlTop.Controls.Add(btnRefresh);

            lblMsg = new Label { Location = new Point(20, 115), AutoSize = true, Font = UITheme.FontSmall, ForeColor = UITheme.AccentGreen };
            pnlTop.Controls.Add(lblMsg);

            // Controls moved to Top Bar
            btnApprove = UITheme.MakeSuccessButton("✓ Approve", 20, 70, 120, 40);
            btnApprove.Click += (s, e) => SetStatus("Active");
            pnlTop.Controls.Add(btnApprove);

            btnSuspend = new Button { Text = "⏸ Suspend", Location = new Point(150, 70), Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat, BackColor = UITheme.AccentAmber, ForeColor = Color.White,
                Font = UITheme.FontButton, Cursor = Cursors.Hand };
            btnSuspend.FlatAppearance.BorderSize = 0;
            btnSuspend.Click += (s, e) => SetStatus("Suspended");
            pnlTop.Controls.Add(btnSuspend);

            btnDelete = UITheme.MakeDangerButton("🗑 Delete", 280, 70, 110, 40);
            btnDelete.Click += BtnDelete_Click;
            pnlTop.Controls.Add(btnDelete);

            pnlTop.Controls.Add(new Label { Text = "Assign Head:", Location = new Point(410, 80), AutoSize = true, ForeColor = UITheme.TextSecond, Font = UITheme.FontBody });
            cmbUsers = new ComboBox { Location = new Point(510, 74), Size = new Size(240, 32), DropDownStyle = ComboBoxStyle.DropDownList };
            UITheme.StyleComboBox(cmbUsers);
            pnlTop.Controls.Add(cmbUsers);

            btnAssign = UITheme.MakePrimaryButton("Assign", 760, 70, 90, 40);
            btnAssign.Click += BtnAssign_Click;
            pnlTop.Controls.Add(btnAssign);

            Controls.Add(pnlTop);

            dgvSocieties = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGrid(dgvSocieties);
            Controls.Add(dgvSocieties);

            var ctx = new ContextMenuStrip();
            ctx.BackColor = UITheme.BgCard; ctx.ForeColor = UITheme.TextPrimary;
            var mnuApprove = new ToolStripMenuItem("✓ Approve"); mnuApprove.Click += (s, e) => SetStatus("Active"); ctx.Items.Add(mnuApprove);
            var mnuSuspend = new ToolStripMenuItem("⏸ Suspend"); mnuSuspend.Click += (s, e) => SetStatus("Suspended"); ctx.Items.Add(mnuSuspend);
            var mnuDelete = new ToolStripMenuItem("🗑 Delete"); mnuDelete.Click += BtnDelete_Click; ctx.Items.Add(mnuDelete);
            dgvSocieties.ContextMenuStrip = ctx;
        }

        private void LoadSocieties()
        {
            try
            {
                string sql = @"SELECT s.SocietyID AS ID, s.Name AS [Society], s.Category, s.Status,
                                      s.MaxMembers AS [Max], ISNULL(u.FullName,'—') AS [Head],
                                      (SELECT COUNT(*) FROM Memberships m WHERE m.SocietyID=s.SocietyID AND m.Status='Approved') AS [Members],
                                      s.CreatedAt AS [Created]
                               FROM Societies s LEFT JOIN Users u ON s.HeadUserID=u.UserID
                               ORDER BY s.CreatedAt DESC";
                DataTable dt = DatabaseManager.ExecuteQuery(sql);
                dgvSocieties.DataSource = dt;
                if (dgvSocieties.Columns.Contains("ID")) dgvSocieties.Columns["ID"].Visible = false;
                lblMsg.Text = "";
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }

        private void LoadUsers()
        {
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery("SELECT UserID, FullName FROM Users WHERE IsActive=1 ORDER BY FullName");
                cmbUsers.DisplayMember = "FullName"; cmbUsers.ValueMember = "UserID"; cmbUsers.DataSource = dt;
            }
            catch { }
        }

        private void SetStatus(string status)
        {
            lblMsg.Text = "";
            if (dgvSocieties.CurrentRow == null) { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ Select a society."; return; }
            int sid = Convert.ToInt32(dgvSocieties.CurrentRow.Cells["ID"].Value);
            try
            {
                DatabaseManager.ExecuteNonQuery("UPDATE Societies SET Status=@S WHERE SocietyID=@ID", new SqlParameter("@S", status), new SqlParameter("@ID", sid));
                lblMsg.ForeColor = UITheme.AccentGreen; lblMsg.Text = "✓ Society status updated to " + status + ".";
                LoadSocieties();
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";
            if (dgvSocieties.CurrentRow == null) { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ Select a society."; return; }
            if (MessageBox.Show("Delete this society permanently?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            int sid = Convert.ToInt32(dgvSocieties.CurrentRow.Cells["ID"].Value);
            try
            {
                DatabaseManager.ExecuteNonQuery("DELETE FROM Memberships WHERE SocietyID=@ID", new SqlParameter("@ID", sid));
                DatabaseManager.ExecuteNonQuery("DELETE FROM Announcements WHERE SocietyID=@ID", new SqlParameter("@ID", sid));
                DatabaseManager.ExecuteNonQuery("DELETE FROM Events WHERE SocietyID=@ID", new SqlParameter("@ID", sid));
                DatabaseManager.ExecuteNonQuery("DELETE FROM Societies WHERE SocietyID=@ID", new SqlParameter("@ID", sid));
                lblMsg.ForeColor = UITheme.AccentGreen; lblMsg.Text = "✓ Society deleted.";
                LoadSocieties();
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }

        private void BtnAssign_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";
            if (dgvSocieties.CurrentRow == null || cmbUsers.SelectedValue == null) { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ Select society and user."; return; }
            int sid = Convert.ToInt32(dgvSocieties.CurrentRow.Cells["ID"].Value);
            int uid = Convert.ToInt32(cmbUsers.SelectedValue);
            try
            {
                DatabaseManager.ExecuteNonQuery("UPDATE Societies SET HeadUserID=@U WHERE SocietyID=@S", new SqlParameter("@U", uid), new SqlParameter("@S", sid));
                DatabaseManager.ExecuteNonQuery("UPDATE Users SET Role='SocietyHead' WHERE UserID=@U", new SqlParameter("@U", uid));
                lblMsg.ForeColor = UITheme.AccentGreen; lblMsg.Text = "✓ Head assigned.";
                LoadSocieties();
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }
    }
}
