using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    public class AdminDashboard : Form
    {
        private Panel pnlSidebar, pnlContent;
        
        public AdminDashboard()
        {
            InitializeUI();
            LoadStats();
        }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Text = "Admin Dashboard - FAST Societies MS";
            Size = new Size(1200, 740);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);

            // ── Sidebar ────────────────────────────────────────────────────────
            pnlSidebar = new Panel
            {
                Location = new Point(0, 0), Size = new Size(240, Height), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = UITheme.BgPanel
            };
            Controls.Add(pnlSidebar);

            var pnlLogo = new Panel
            {
                Location = new Point(0, 0), Size = new Size(240, 80),
                BackColor = UITheme.AccentPink
            };
            pnlLogo.Controls.Add(new Label
            {
                Text = "⚙ Admin Panel", Location = new Point(16, 22), AutoSize = true,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.White
            });
            pnlSidebar.Controls.Add(pnlLogo);

            pnlSidebar.Controls.Add(new Label
            {
                Text = $"👑 {SessionManager.CurrentUser.FullName}\nSystem Administrator",
                Location = new Point(16, 92), Size = new Size(210, 50),
                ForeColor = UITheme.TextSecond, Font = UITheme.FontSmall
            });

            string[] navItems = { "📊 Dashboard", "👥 Manage Users", "🏛 Manage Societies",
                                   "📅 Approve Events", "📋 Reports", "🚪 Logout" };
            int y = 160;
            foreach (string item in navItems)
            {
                var btn = new Button
                {
                    Text = item, Location = new Point(0, y), Size = new Size(240, 48),
                    FlatStyle = FlatStyle.Flat, BackColor = UITheme.BgPanel, ForeColor = UITheme.TextPrimary,
                    Font = UITheme.FontBody, TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(16, 0, 0, 0), Cursor = Cursors.Hand
                };
                if (item.Contains("Logout")) btn.ForeColor = UITheme.AccentRed;
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = UITheme.BgCard;
                
                string target = item;
                btn.Click += (s, e) => NavClick(target);
                pnlSidebar.Controls.Add(btn);
                y += 52;
            }

            // ── Content Area ───────────────────────────────────────────────────
            pnlContent = new Panel
            {
                Location = new Point(240, 0), Size = new Size(Width - 240, Height), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = UITheme.BgDeep, AutoScroll = false
            };
            Controls.Add(pnlContent);
        }

        private void NavClick(string item)
        {
            if (item.Contains("Dashboard")) LoadStats();
            else if (item.Contains("Manage Users")) OpenChild(new AdminUsersForm());
            else if (item.Contains("Manage Societies")) OpenChild(new AdminSocietiesForm());
            else if (item.Contains("Approve Events")) OpenChild(new ManageEventsForm(adminMode: true));
            else if (item.Contains("Reports")) OpenChild(new AdminReportsForm());
            else if (item.Contains("Logout"))
            {
                if (MessageBox.Show("Are you sure you want to log out?", "Logout", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SessionManager.Logout();
                    Close();
                }
            }
        }

        private void LoadStats()
        {
            pnlContent.Controls.Clear();
            var hdr = UITheme.MakeHeader("System Overview", 30, 30);
            hdr.Font = UITheme.FontTitle;
            pnlContent.Controls.Add(hdr);
            pnlContent.Controls.Add(UITheme.MakeLabel(DateTime.Now.ToString("dddd, MMMM dd, yyyy"), 30, 72, UITheme.TextSecond));

            try
            {
                object uCount = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Users WHERE IsActive=1");
                object sCount = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Societies WHERE Status='Active'");
                object eCount = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Events WHERE Status='Pending'");
                object mCount = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Memberships WHERE Status='Pending'");
                object rCount = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM EventRegistrations");
                object susCount = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Societies WHERE Status='Suspended'");

                AddStatCard("Active Users",         uCount.ToString(), UITheme.AccentViolet,  30, 110);
                AddStatCard("Active Societies",     sCount.ToString(), UITheme.AccentCyan,   260, 110);
                AddStatCard("Events Awaiting Approval", eCount.ToString(), UITheme.AccentAmber,  490, 110);
                AddStatCard("Pending Memberships",  mCount.ToString(), UITheme.AccentCyan,   720, 110);
                
                AddStatCard("Total Registrations",  rCount.ToString(), UITheme.AccentPink,    30, 230);
                AddStatCard("Suspended Societies",  susCount.ToString(), UITheme.AccentRed,  260, 230);

                LoadRecentRequests();
            }
            catch (Exception ex)
            {
                pnlContent.Controls.Add(UITheme.MakeLabel("Error loading stats: " + ex.Message, 30, 120, UITheme.AccentRed));
            }
        }

        private void AddStatCard(string title, string value, Color accent, int x, int y)
        {
            var card = UITheme.MakeCard(x, y, 210, 100);
            card.Controls.Add(new Label { Text = value, Location = new Point(12, 12), AutoSize = true, Font = new Font("Segoe UI", 24f, FontStyle.Bold), ForeColor = accent });
            card.Controls.Add(new Label { Text = title, Location = new Point(12, 60), AutoSize = true, Font = UITheme.FontSmall, ForeColor = UITheme.TextSecond });
            pnlContent.Controls.Add(card);
        }

        private void LoadRecentRequests()
        {
            pnlContent.Controls.Add(UITheme.MakeHeader("🏛 Recent Society Requests", 30, 360));
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(
                    "SELECT TOP 5 Name, Category, Status, CreatedAt FROM Societies ORDER BY CreatedAt DESC");
                var dgv = new DataGridView
                {
                    Location = new Point(30, 400), Size = new Size(900, 200),
                    DataSource = dt
                };
                UITheme.StyleDataGrid(dgv);
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
    }
}
