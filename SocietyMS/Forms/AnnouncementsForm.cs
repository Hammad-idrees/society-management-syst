using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    /// <summary>Post and view announcements for a society.</summary>
    public class AnnouncementsForm : Form
    {
        private DataGridView dgvAnn;
        private TextBox txtTitle, txtContent;
        private CheckBox chkGlobal;
        private Button btnPost, btnDelete;
        private Label lblMsg;
        private int _societyId;

        public AnnouncementsForm()
        {
            _societyId = GetSocietyId();
            InitializeUI();
            LoadAnnouncements();
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
            Controls.Add(UITheme.MakeHeader("📢 Society Announcements", 20, 20));

            // Left: compose
            int y = 68;
            Controls.Add(UITheme.MakeLabel("Title *", 20, y)); y += 24;
            txtTitle = UITheme.MakeTextBox(20, y, 380); Controls.Add(txtTitle); y += 50;

            Controls.Add(UITheme.MakeLabel("Content *", 20, y)); y += 24;
            txtContent = new TextBox { Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(380, 120), Multiline = true,
                BackColor = UITheme.BgCard, ForeColor = UITheme.TextPrimary, Font = UITheme.FontBody };
            Controls.Add(txtContent); y += 135;

            chkGlobal = new CheckBox { Text = "Post as University-Wide Announcement",
                Location = new System.Drawing.Point(20, y), AutoSize = true,
                ForeColor = UITheme.TextSecond, BackColor = UITheme.BgDeep,
                Font = UITheme.FontSmall };
            if (SessionManager.CurrentUser.IsAdmin) Controls.Add(chkGlobal);
            y += 30;

            lblMsg = new Label { Location = new System.Drawing.Point(20, y), Size = new System.Drawing.Size(380, 24),
                Font = UITheme.FontBody, ForeColor = UITheme.AccentGreen };
            Controls.Add(lblMsg); y += 30;

            btnPost = UITheme.MakePrimaryButton("📨 Post Announcement", 20, y, 210, 42);
            btnPost.Click += BtnPost_Click;
            Controls.Add(btnPost);

            // Right: list
            dgvAnn = new DataGridView { Location = new System.Drawing.Point(430, 68), Size = new System.Drawing.Size(520, 490) };
            UITheme.StyleDataGrid(dgvAnn);
            Controls.Add(dgvAnn);

            btnDelete = UITheme.MakeDangerButton("🗑 Delete", 430, 572, 150, 40);
            btnDelete.Click += BtnDelete_Click;
            Controls.Add(btnDelete);
        }

        private void LoadAnnouncements()
        {
            try
            {
                string sql = @"SELECT a.AnnouncementID AS ID, a.Title, LEFT(a.Content,60)+'...' AS [Content Preview],
                                      u.FullName AS [Posted By],
                                      CASE a.IsGlobal WHEN 1 THEN 'University-Wide' ELSE 'Society' END AS [Scope],
                                      a.CreatedAt AS [Date]
                               FROM Announcements a JOIN Users u ON a.PostedBy=u.UserID
                               WHERE a.SocietyID=@S OR a.PostedBy=@U
                               ORDER BY a.CreatedAt DESC";
                DataTable dt = DatabaseManager.ExecuteQuery(sql,
                    new SqlParameter("@S", _societyId == 0 ? (object)DBNull.Value : _societyId),
                    new SqlParameter("@U", SessionManager.CurrentUser.UserID));
                dgvAnn.DataSource = dt;
                if (dgvAnn.Columns.Contains("ID")) dgvAnn.Columns["ID"].Visible = false;
            }
            catch (Exception ex)
            {
                lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = "⚠ " + ex.Message;
            }
        }

        private void BtnPost_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";
            string title   = txtTitle.Text.Trim();
            string content = txtContent.Text.Trim();
            if (!ValidationHelper.IsNotEmpty(title) || !ValidationHelper.IsNotEmpty(content))
            {
                lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = "⚠ Title and content are required."; return;
            }
            try
            {
                DatabaseManager.ExecuteNonQuery(
                    @"INSERT INTO Announcements (SocietyID, PostedBy, Title, Content, IsGlobal)
                      VALUES (@S, @U, @T, @C, @G)",
                    new SqlParameter("@S", _societyId == 0 ? (object)DBNull.Value : _societyId),
                    new SqlParameter("@U", SessionManager.CurrentUser.UserID),
                    new SqlParameter("@T", title),
                    new SqlParameter("@C", content),
                    new SqlParameter("@G", chkGlobal.Checked ? 1 : 0));
                lblMsg.ForeColor = UITheme.AccentGreen; lblMsg.Text = "✓ Announcement posted!";
                txtTitle.Clear(); txtContent.Clear();
                LoadAnnouncements();
            }
            catch (Exception ex)
            {
                lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = "⚠ " + ex.Message;
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvAnn.CurrentRow == null) return;
            int aid = Convert.ToInt32(dgvAnn.CurrentRow.Cells["ID"].Value);
            try
            {
                DatabaseManager.ExecuteNonQuery("DELETE FROM Announcements WHERE AnnouncementID=@A",
                    new SqlParameter("@A", aid));
                lblMsg.ForeColor = UITheme.AccentGreen; lblMsg.Text = "✓ Announcement deleted.";
                LoadAnnouncements();
            }
            catch (Exception ex)
            {
                lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = "⚠ " + ex.Message;
            }
        }
    }

    /// <summary>Society head report: member list and event summary for their society.</summary>
    public class SocietyReportForm : Form
    {
        private TabControl tabReports;

        public SocietyReportForm()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Dock = DockStyle.Fill;
            Controls.Add(UITheme.MakeHeader("📄 Society Reports", 20, 20));

            tabReports = new TabControl
            {
                Location = new System.Drawing.Point(20, 60), Size = new System.Drawing.Size(940, 600),
                Font = UITheme.FontBody
            };
            Controls.Add(tabReports);

            int sid = GetSocietyId();

            // Members tab
            var membersPage = new TabPage("👥 Members") { BackColor = UITheme.BgDeep };
            try
            {
                string sql = @"SELECT u.FullName AS [Name], u.Email, u.RollNumber AS [Roll],
                                      m.Role AS [Society Role], m.Status,
                                      m.AppliedAt AS [Applied], m.ApprovedAt AS [Approved]
                               FROM Memberships m JOIN Users u ON m.UserID=u.UserID
                               WHERE m.SocietyID=@S ORDER BY m.Status, u.FullName";
                var dgv = new DataGridView { Dock = DockStyle.Fill };
                UITheme.StyleDataGrid(dgv);
                dgv.DataSource = DatabaseManager.ExecuteQuery(sql, new SqlParameter("@S", sid));
                membersPage.Controls.Add(dgv);
            }
            catch { }
            tabReports.TabPages.Add(membersPage);

            // Events tab
            var eventsPage = new TabPage("📅 Events") { BackColor = UITheme.BgDeep };
            try
            {
                string sql = @"SELECT e.Title, e.Venue, e.EventDate AS [Date], e.Status,
                                      e.MaxAttendees AS [Max],
                                      (SELECT COUNT(*) FROM EventRegistrations WHERE EventID=e.EventID) AS [Registered]
                               FROM Events e WHERE e.SocietyID=@S ORDER BY e.EventDate DESC";
                var dgv = new DataGridView { Dock = DockStyle.Fill };
                UITheme.StyleDataGrid(dgv);
                dgv.DataSource = DatabaseManager.ExecuteQuery(sql, new SqlParameter("@S", sid));
                eventsPage.Controls.Add(dgv);
            }
            catch { }
            tabReports.TabPages.Add(eventsPage);
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
    }
}



