using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    public class MembershipRequestsForm : Form
    {
        private DataGridView dgvRequests;
        private Button btnApprove, btnReject, btnRefresh;
        private Label lblMsg;
        private int _societyId;

        public MembershipRequestsForm() { _societyId = GetSocietyId(); InitializeUI(); LoadRequests(); }

        private int GetSocietyId()
        {
            try
            {
                object id = DatabaseManager.ExecuteScalar("SELECT SocietyID FROM Societies WHERE HeadUserID=@U",
                    new SqlParameter("@U", SessionManager.CurrentUser.UserID));
                return (id != null && id != DBNull.Value) ? Convert.ToInt32(id) : 0;
            }
            catch { return 0; }
        }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Dock = DockStyle.Fill;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = UITheme.BgDeep };
            var lbl = UITheme.MakeHeader("📋 Membership Requests", 20, 14);
            lbl.Font = UITheme.FontSubtitle;
            pnlTop.Controls.Add(lbl);
            pnlTop.Controls.Add(UITheme.MakeLabel("Review and approve or reject pending applications.", 20, 52, UITheme.TextSecond));
            
            btnRefresh = UITheme.MakePrimaryButton("🔄 Refresh", 400, 48, 110, 36);
            btnRefresh.Click += (s, e) => LoadRequests();
            pnlTop.Controls.Add(btnRefresh);

            btnApprove = UITheme.MakeSuccessButton("✓ Approve", 530, 46, 140, 40);
            btnApprove.Click += (s, e) => SetStatus("Approved");
            pnlTop.Controls.Add(btnApprove);

            btnReject = UITheme.MakeDangerButton("✗ Reject", 690, 46, 120, 40);
            btnReject.Click += (s, e) => SetStatus("Rejected");
            pnlTop.Controls.Add(btnReject);

            lblMsg = new Label { Location = new Point(400, 80), AutoSize = true, Font = UITheme.FontSmall, ForeColor = UITheme.AccentGreen };
            pnlTop.Controls.Add(lblMsg);
            Controls.Add(pnlTop);

            dgvRequests = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGrid(dgvRequests);
            Controls.Add(dgvRequests);

            var ctx = new ContextMenuStrip();
            ctx.BackColor = UITheme.BgCard; ctx.ForeColor = UITheme.TextPrimary;
            var mnuApprove = new ToolStripMenuItem("✓ Approve"); mnuApprove.Click += (s, e) => SetStatus("Approved"); ctx.Items.Add(mnuApprove);
            var mnuReject = new ToolStripMenuItem("✗ Reject"); mnuReject.Click += (s, e) => SetStatus("Rejected"); ctx.Items.Add(mnuReject);
            dgvRequests.ContextMenuStrip = ctx;
        }

        private void LoadRequests()
        {
            if (_societyId == 0) { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ No society assigned to you yet."; return; }
            try
            {
                string sql = @"SELECT m.MembershipID AS ID, u.FullName AS [Student], u.Email,
                                      u.RollNumber AS [Roll], u.Department AS [Dept],
                                      m.Role AS [Applied Role], m.Status, m.AppliedAt AS [Applied On]
                               FROM Memberships m JOIN Users u ON m.UserID=u.UserID
                               WHERE m.SocietyID=@S
                               ORDER BY CASE m.Status WHEN 'Pending' THEN 0 ELSE 1 END, m.AppliedAt DESC";
                DataTable dt = DatabaseManager.ExecuteQuery(sql, new SqlParameter("@S", _societyId));
                dgvRequests.DataSource = dt;
                if (dgvRequests.Columns.Contains("ID")) dgvRequests.Columns["ID"].Visible = false;
                lblMsg.Text = "";
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }

        private void SetStatus(string status)
        {
            lblMsg.Text = "";
            if (dgvRequests.CurrentRow == null) { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ Select a request first."; return; }
            int mid = Convert.ToInt32(dgvRequests.CurrentRow.Cells["ID"].Value);
            try
            {
                string sql = status == "Approved"
                    ? "UPDATE Memberships SET Status=@S, ApprovedAt=GETDATE() WHERE MembershipID=@M"
                    : "UPDATE Memberships SET Status=@S WHERE MembershipID=@M";
                DatabaseManager.ExecuteNonQuery(sql, new SqlParameter("@S", status), new SqlParameter("@M", mid));
                lblMsg.ForeColor = UITheme.AccentGreen; lblMsg.Text = "✓ Request " + status.ToLower() + ".";
                LoadRequests();
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }
    }
}
