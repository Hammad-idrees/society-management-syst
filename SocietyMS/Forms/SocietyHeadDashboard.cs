using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    public class SocietyHeadDashboard : Form
    {
        private Panel pnlSidebar, pnlContent;
        private int _societyId;

        public SocietyHeadDashboard()
        {
            _societyId = GetHeadSocietyId();
            InitializeUI();
            LoadStats();
        }

        private int GetHeadSocietyId()
        {
            try
            {
                object id = DatabaseManager.ExecuteScalar(
                    "SELECT SocietyID FROM Societies WHERE HeadUserID=@U AND Status='Active'",
                    new System.Data.SqlClient.SqlParameter("@U", SessionManager.CurrentUser.UserID));
                return id != null && id != DBNull.Value ? Convert.ToInt32(id) : 0;
            }
            catch { return 0; }
        }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Text = "Society Head Dashboard – FAST Societies MS";
            Size = new Size(1240, 740);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);

            pnlSidebar = new Panel
            {
                Location = new Point(0, 0), Size = new Size(240, Height), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = UITheme.BgPanel
            };
            Controls.Add(pnlSidebar);

            var pnlLogo = new Panel
            {
                Location = new Point(0, 0), Size = new Size(240, 80),
                BackColor = UITheme.AccentGreen
            };
            pnlLogo.Controls.Add(new Label
            {
                Text = "🏛 Society Head", Location = new Point(12, 22), AutoSize = true,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = Color.White
            });
            pnlSidebar.Controls.Add(pnlLogo);

            pnlSidebar.Controls.Add(new Label
            {
                Text = $"👤 {SessionManager.CurrentUser.FullName}",
                Location = new Point(16, 92), Size = new Size(210, 40),
                ForeColor = UITheme.TextSecond, Font = UITheme.FontSmall
            });

            string[] navItems = { "📊 Dashboard", "✏ Manage Society", "👥 Membership Requests",
                                   "📅 Manage Events", "📋 Assign Tasks", "📢 Announcements",
                                   "📄 Reports", "🚪 Logout" };
            EventHandler[] handlers =
            {
                (s,e) => LoadStats(),
                (s,e) => OpenChild(new ManageSocietyForm()),
                (s,e) => OpenChild(new MembershipRequestsForm()),
                (s,e) => OpenChild(new ManageEventsForm()),
                (s,e) => OpenChild(new AssignTasksForm()),
                (s,e) => OpenChild(new AnnouncementsForm()),
                (s,e) => OpenChild(new SocietyReportForm()),
                (s,e) => Logout()
            };

            for (int i = 0; i < navItems.Length; i++)
            {
                int idx = i;
                var btn = new Button
                {
                    Text = navItems[i], Location = new Point(0, 150 + i * 50),
                    Size = new Size(240, 48), FlatStyle = FlatStyle.Flat,
                    BackColor = UITheme.BgPanel, ForeColor = UITheme.TextPrimary,
                    Font = UITheme.FontBody, TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(18, 0, 0, 0), Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = UITheme.BgCard;
                btn.Click += handlers[idx];
                if (navItems[i].Contains("Logout")) btn.ForeColor = UITheme.AccentRed;
                pnlSidebar.Controls.Add(btn);
            }

            pnlContent = new Panel
            {
                Location = new Point(240, 0), Size = new Size(Width - 230, Height), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, BackColor = UITheme.BgDeep, AutoScroll = false
            };
            Controls.Add(pnlContent);
        }

        private void LoadStats()
        {
            pnlContent.Controls.Clear();
            var hdr = UITheme.MakeHeader("Society Overview", 30, 30);
            hdr.Font = UITheme.FontTitle;
            pnlContent.Controls.Add(hdr);

            if (_societyId == 0)
            {
                pnlContent.Controls.Add(UITheme.MakeLabel(
                    "⚠ You are not assigned as head of any active society.\nContact admin to set up your society.",
                    30, 80, UITheme.AccentAmber));
                return;
            }

            try
            {
                string sName = DatabaseManager.ExecuteScalar(
                    "SELECT Name FROM Societies WHERE SocietyID=@S",
                    new System.Data.SqlClient.SqlParameter("@S", _societyId))?.ToString() ?? "Society";

                pnlContent.Controls.Add(UITheme.MakeLabel($"Managing: {sName}", 30, 72, UITheme.AccentViolet));

                object mCount = DatabaseManager.ExecuteScalar(
                    "SELECT COUNT(*) FROM Memberships WHERE SocietyID=@S AND Status='Approved'",
                    new System.Data.SqlClient.SqlParameter("@S", _societyId));
                object pCount = DatabaseManager.ExecuteScalar(
                    "SELECT COUNT(*) FROM Memberships WHERE SocietyID=@S AND Status='Pending'",
                    new System.Data.SqlClient.SqlParameter("@S", _societyId));
                object eCount = DatabaseManager.ExecuteScalar(
                    "SELECT COUNT(*) FROM Events WHERE SocietyID=@S AND Status='Approved'",
                    new System.Data.SqlClient.SqlParameter("@S", _societyId));
                object tCount = DatabaseManager.ExecuteScalar(
                    "SELECT COUNT(*) FROM SocietyTasks WHERE SocietyID=@S AND Status='Pending'",
                    new System.Data.SqlClient.SqlParameter("@S", _societyId));

                var cards = new (string t, string v, string ico, Color c)[]
                {
                    ("Active Members",   mCount?.ToString() ?? "0", "👥", UITheme.AccentViolet),
                    ("Pending Requests", pCount?.ToString() ?? "0", "⏳", UITheme.AccentAmber),
                    ("Approved Events",  eCount?.ToString() ?? "0", "📅", UITheme.AccentGreen),
                    ("Open Tasks",       tCount?.ToString() ?? "0", "📋", UITheme.AccentCyan),
                };

                int sx = 30, sy = 110;
                foreach (var (t, v, ico, c) in cards)
                {
                    var card = UITheme.MakeCard(sx, sy, 210, 110);
                    card.Controls.Add(new Label { Text = ico, Location = new Point(12,12), AutoSize=true,
                        Font = new Font("Segoe UI Emoji",22f), ForeColor = c });
                    card.Controls.Add(new Label { Text = v, Location = new Point(12,50), AutoSize=true,
                        Font = new Font("Segoe UI",24f,FontStyle.Bold), ForeColor = c });
                    card.Controls.Add(new Label { Text = t, Location = new Point(12,80), AutoSize=true,
                        Font = UITheme.FontSmall, ForeColor = UITheme.TextSecond });
                    pnlContent.Controls.Add(card);
                    sx += 240;
                }

                LoadPendingMembers(sy + 130);
            }
            catch (Exception ex)
            {
                pnlContent.Controls.Add(UITheme.MakeLabel("⚠ " + ex.Message, 30, 120, UITheme.AccentRed));
            }
        }

        private void LoadPendingMembers(int y)
        {
            pnlContent.Controls.Add(UITheme.MakeHeader("⏳ Pending Membership Requests", 30, y));
            try
            {
                string sql = @"SELECT u.FullName, u.Email, u.RollNumber, m.AppliedAt
                               FROM Memberships m JOIN Users u ON m.UserID=u.UserID
                               WHERE m.SocietyID=@S AND m.Status='Pending'
                               ORDER BY m.AppliedAt DESC";
                DataTable dt = DatabaseManager.ExecuteQuery(sql,
                    new System.Data.SqlClient.SqlParameter("@S", _societyId));
                var dgv = new DataGridView { Location = new Point(30, y + 36), Size = new Size(920, 200) };
                UITheme.StyleDataGrid(dgv);
                dgv.DataSource = dt;
                pnlContent.Controls.Add(dgv);
            }
            catch { }
        }

        private void OpenChild(Form child)
        {
            pnlContent.Controls.Clear();
            child.TopLevel = false;
            child.FormBorderStyle = FormBorderStyle.None;
            child.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(child);
            child.Show();
        }

        private void Logout()
        {
            if (MessageBox.Show("Log out?", "Logout", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                SessionManager.Logout();
                Close();
            }
        }
    }
}
