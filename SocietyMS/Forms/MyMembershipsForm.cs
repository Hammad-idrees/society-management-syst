using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    public class MyMembershipsForm : Form
    {
        private DataGridView dgvMemberships;
        private Button btnWithdraw, btnRefresh;
        private Label lblMsg;

        public MyMembershipsForm() { InitializeUI(); LoadMemberships(); }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Dock = DockStyle.Fill;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = UITheme.BgDeep };
            var lbl = UITheme.MakeHeader("🎫 My Society Memberships", 20, 16);
            lbl.Font = UITheme.FontSubtitle;
            pnlTop.Controls.Add(lbl);
            pnlTop.Controls.Add(UITheme.MakeLabel("View and manage your society memberships.", 20, 52, UITheme.TextSecond));
            
            btnRefresh = UITheme.MakePrimaryButton("🔄 Refresh", 380, 48, 110, 36);
            btnRefresh.Click += (s, e) => LoadMemberships();
            pnlTop.Controls.Add(btnRefresh);

            btnWithdraw = UITheme.MakeDangerButton("🚪 Withdraw Application", 510, 46, 220, 40);
            btnWithdraw.Click += BtnWithdraw_Click;
            pnlTop.Controls.Add(btnWithdraw);

            lblMsg = new Label { Location = new Point(380, 20), Size = new Size(600, 24), Font = UITheme.FontBody, ForeColor = UITheme.AccentGreen };
            pnlTop.Controls.Add(lblMsg);
            Controls.Add(pnlTop);

            dgvMemberships = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGrid(dgvMemberships);
            Controls.Add(dgvMemberships);

            var ctx = new ContextMenuStrip();
            ctx.BackColor = UITheme.BgCard; ctx.ForeColor = UITheme.TextPrimary;
            var mnu = new ToolStripMenuItem("🚪 Withdraw Application");
            mnu.Click += BtnWithdraw_Click;
            ctx.Items.Add(mnu);
            dgvMemberships.ContextMenuStrip = ctx;
        }

        private void LoadMemberships()
        {
            try
            {
                string sql = @"SELECT m.MembershipID AS ID, s.Name AS [Society], s.Category,
                                      m.Role AS [Your Role], m.Status,
                                      m.AppliedAt AS [Applied On], m.ApprovedAt AS [Approved On]
                               FROM Memberships m JOIN Societies s ON m.SocietyID=s.SocietyID
                               WHERE m.UserID=@U ORDER BY m.AppliedAt DESC";
                DataTable dt = DatabaseManager.ExecuteQuery(sql, new SqlParameter("@U", SessionManager.CurrentUser.UserID));
                dgvMemberships.DataSource = dt;
                if (dgvMemberships.Columns.Contains("ID")) dgvMemberships.Columns["ID"].Visible = false;
                lblMsg.Text = "";
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }

        private void BtnWithdraw_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";
            if (dgvMemberships.CurrentRow == null) { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ Select a membership first."; return; }
            string status = dgvMemberships.CurrentRow.Cells["Status"].Value?.ToString() ?? "";
            if (status == "Approved") { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ Cannot withdraw from an approved membership. Contact society head."; return; }
            int mid = Convert.ToInt32(dgvMemberships.CurrentRow.Cells["ID"].Value);
            if (MessageBox.Show("Withdraw this application?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                DatabaseManager.ExecuteNonQuery("DELETE FROM Memberships WHERE MembershipID=@M", new SqlParameter("@M", mid));
                lblMsg.ForeColor = UITheme.AccentGreen; lblMsg.Text = "✓ Application withdrawn.";
                LoadMemberships();
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }
    }
}
