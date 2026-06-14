using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    /// <summary>
    /// Student Dashboard: navigation hub for all student features.
    /// Displays summary stats and provides access to societies, events, memberships.
    /// </summary>
    public class StudentDashboard : Form
    {
        private Panel pnlSidebar, pnlContent;
        private Label lblWelcome;

        public StudentDashboard()
        {
            InitializeUI();
            LoadStats();
        }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Text = "Student Dashboard - FAST Societies MS";
            Size = new Size(1200, 740);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);
            FormBorderStyle = FormBorderStyle.Sizable;

            pnlSidebar = new Panel { Location = new Point(0, 0), Size = new Size(240, Height), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = UITheme.BgPanel };
            Controls.Add(pnlSidebar);

            var pnlLogo = new Panel { Location = new Point(0, 0), Size = new Size(240, 80),
                BackColor = UITheme.AccentViolet };
            pnlLogo.Controls.Add(new Label { Text = "FAST SMS", Location = new Point(16, 22),
                AutoSize = true, Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.White });
            pnlSidebar.Controls.Add(pnlLogo);

            lblWelcome = new Label
            {
                Text = "Student: " + SessionManager.CurrentUser.FullName,
                Location = new Point(16, 95), Size = new Size(198, 50),
                ForeColor = UITheme.TextSecond, Font = UITheme.FontSmall
            };
            pnlSidebar.Controls.Add(lblWelcome);

            AddNavButton("Dashboard", 160, BtnDashboard_Click);
            AddNavButton("Browse Societies", 212, BtnBrowse_Click);
            AddNavButton("Events", 264, BtnEvents_Click);
            AddNavButton("My Memberships", 316, BtnMemberships_Click);
            AddNavButton("My Tickets", 368, BtnTickets_Click);
            AddNavButton("Logout", 420, BtnLogout_Click, UITheme.AccentRed);

            pnlContent = new Panel { Location = new Point(230, 0), Size = new Size(Width - 230, Height), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, BackColor = UITheme.BgDeep, AutoScroll = false };
            Controls.Add(pnlContent);
        }

        private void AddNavButton(string text, int y, EventHandler handler, Color foreColor = default(Color))
        {
            var btn = new Button
            {
                Text = text, Location = new Point(0, y), Size = new Size(230, 48),
                FlatStyle = FlatStyle.Flat, BackColor = UITheme.BgPanel,
                ForeColor = foreColor == default(Color) ? UITheme.TextPrimary : foreColor,
                Font = UITheme.FontBody, TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0), Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = UITheme.BgCard;
            btn.Click += handler;
            pnlSidebar.Controls.Add(btn);
        }

        private void BtnDashboard_Click(object sender, EventArgs e) { LoadStats(); }
        private void BtnBrowse_Click(object sender, EventArgs e) { OpenChild(new BrowseSocietiesForm()); }
        private void BtnEvents_Click(object sender, EventArgs e) { OpenChild(new EventsForm()); }
        private void BtnMemberships_Click(object sender, EventArgs e) { OpenChild(new MyMembershipsForm()); }
        private void BtnTickets_Click(object sender, EventArgs e) { OpenChild(new EventTicketForm()); }
        private void BtnLogout_Click(object sender, EventArgs e) { Logout(); }

        private void LoadStats()
        {
            pnlContent.Controls.Clear();
            var hdr = UITheme.MakeHeader("Welcome, " + SessionManager.CurrentUser.FullName + "!", 30, 30);
            hdr.Font = UITheme.FontTitle;
            pnlContent.Controls.Add(hdr);
            pnlContent.Controls.Add(UITheme.MakeLabel(DateTime.Now.ToString("dddd, MMMM dd, yyyy"), 30, 72, UITheme.TextSecond));

            try
            {
                int uid = SessionManager.CurrentUser.UserID;

                object mCount = DatabaseManager.ExecuteScalar(
                    "SELECT COUNT(*) FROM Memberships WHERE UserID=@U AND Status='Approved'",
                    new System.Data.SqlClient.SqlParameter("@U", uid));
                object eCount = DatabaseManager.ExecuteScalar(
                    "SELECT COUNT(*) FROM EventRegistrations WHERE UserID=@U",
                    new System.Data.SqlClient.SqlParameter("@U", uid));
                object pCount = DatabaseManager.ExecuteScalar(
                    "SELECT COUNT(*) FROM Memberships WHERE UserID=@U AND Status='Pending'",
                    new System.Data.SqlClient.SqlParameter("@U", uid));
                object sCount = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Societies WHERE Status='Active'");

                AddStatCard("Societies Joined",    mCount != null ? mCount.ToString() : "0", UITheme.AccentViolet,   30,  110);
                AddStatCard("Events Registered",   eCount != null ? eCount.ToString() : "0", UITheme.AccentGreen,  260,  110);
                AddStatCard("Pending Requests",    pCount != null ? pCount.ToString() : "0", UITheme.AccentAmber,  490,  110);
                AddStatCard("Active Societies",    sCount != null ? sCount.ToString() : "0", UITheme.AccentCyan,     720,  110);

                LoadAnnouncements(uid);
            }
            catch (Exception ex)
            {
                pnlContent.Controls.Add(UITheme.MakeLabel("Could not load stats: " + ex.Message, 30, 120, UITheme.AccentRed));
            }
        }

        private void AddStatCard(string title, string value, Color accent, int x, int y)
        {
            var card = UITheme.MakeCard(x, y, 210, 110);
            card.Controls.Add(new Label { Text = value, Location = new Point(12, 12), AutoSize = true,
                Font = new Font("Segoe UI", 28f, FontStyle.Bold), ForeColor = accent });
            card.Controls.Add(new Label { Text = title, Location = new Point(12, 68), AutoSize = true,
                Font = UITheme.FontSmall, ForeColor = UITheme.TextSecond });
            pnlContent.Controls.Add(card);
        }

        private void LoadAnnouncements(int uid)
        {
            pnlContent.Controls.Add(UITheme.MakeHeader("Recent Announcements", 30, 248));
            try
            {
                string sql = "SELECT TOP 5 a.Title, a.Content, u.FullName, a.CreatedAt, " +
                             "ISNULL(s.Name,'University-Wide') AS Source " +
                             "FROM Announcements a " +
                             "JOIN Users u ON a.PostedBy = u.UserID " +
                             "LEFT JOIN Societies s ON a.SocietyID = s.SocietyID " +
                             "WHERE a.IsGlobal=1 OR a.SocietyID IN " +
                             "(SELECT SocietyID FROM Memberships WHERE UserID=@U AND Status='Approved') " +
                             "ORDER BY a.CreatedAt DESC";
                DataTable dt = DatabaseManager.ExecuteQuery(sql,
                    new System.Data.SqlClient.SqlParameter("@U", uid));
                int ay = 285;
                if (dt.Rows.Count == 0)
                {
                    pnlContent.Controls.Add(UITheme.MakeLabel("No announcements yet. Join societies to see updates!", 30, ay, UITheme.TextMuted));
                    return;
                }
                foreach (DataRow row in dt.Rows)
                {
                    var card = UITheme.MakeCard(30, ay, 900, 85);
                    card.Controls.Add(new Label { Text = row["Title"].ToString(), Location = new Point(14, 10),
                        Size = new Size(700, 22), Font = UITheme.FontSubtitle, ForeColor = UITheme.TextPrimary });
                    card.Controls.Add(new Label { Text = row["Content"].ToString(), Location = new Point(14, 34),
                        Size = new Size(700, 22), Font = UITheme.FontBody, ForeColor = UITheme.TextSecond });
                    card.Controls.Add(new Label
                    {
                        Text = row["Source"].ToString() + " - " + Convert.ToDateTime(row["CreatedAt"]).ToString("MMM dd, yyyy"),
                        Location = new Point(14, 58), Size = new Size(500, 18),
                        Font = UITheme.FontSmall, ForeColor = UITheme.AccentCyan
                    });
                    pnlContent.Controls.Add(card);
                    ay += 100;
                }
            }
            catch (Exception ex)
            {
                pnlContent.Controls.Add(UITheme.MakeLabel("Could not load announcements: " + ex.Message, 30, 285, UITheme.AccentRed));
            }
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
            if (MessageBox.Show("Are you sure you want to log out?", "Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                SessionManager.Logout();
                Close();
            }
        }
    }
}







