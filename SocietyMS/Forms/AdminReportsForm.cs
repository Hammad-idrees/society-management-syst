using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    /// <summary>Admin: university-wide reports - societies, events, memberships summary.</summary>
    public class AdminReportsForm : Form
    {
        private TabControl tabReports;

        public AdminReportsForm()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Dock = DockStyle.Fill;
            Controls.Add(UITheme.MakeHeader("📋 University-Wide Reports", 20, 20));

            tabReports = new TabControl
            {
                Location = new Point(20, 60), Size = new Size(940, 600),
                Font = UITheme.FontBody
            };
            tabReports.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabReports.DrawItem += TabReports_DrawItem;
            tabReports.Appearance = TabAppearance.FlatButtons;
            Controls.Add(tabReports);

            AddTab("👥 Members Report",    BuildMembersReport());
            AddTab("🏛 Societies Report",  BuildSocietiesReport());
            AddTab("📅 Events Report",     BuildEventsReport());
            AddTab("📊 Summary",           BuildSummaryReport());
        }

        private void TabReports_DrawItem(object sender, DrawItemEventArgs e)
        {
            var g = e.Graphics;
            var tab = (TabControl)sender;
            var page = tab.TabPages[e.Index];
            var rect = tab.GetTabRect(e.Index);
            bool selected = e.Index == tab.SelectedIndex;

            using (var bg = new SolidBrush(selected ? UITheme.AccentViolet : UITheme.BgPanel))
                g.FillRectangle(bg, rect);
            using (var fg = new SolidBrush(UITheme.TextPrimary))
                g.DrawString(page.Text, UITheme.FontSmall, fg,
                    rect.X + 4, rect.Y + 4);
        }

        private void AddTab(string title, Control content)
        {
            var page = new TabPage(title) { BackColor = UITheme.BgDeep };
            content.Dock = DockStyle.Fill;
            page.Controls.Add(content);
            tabReports.TabPages.Add(page);
        }

        private DataGridView MakeGrid()
        {
            var dgv = new DataGridView();
            UITheme.StyleDataGrid(dgv);
            return dgv;
        }

        private Panel BuildMembersReport()
        {
            var p = new Panel { BackColor = UITheme.BgDeep };
            try
            {
                string sql = @"SELECT s.Name AS [Society], u.FullName AS [Member], u.Email,
                                      u.RollNumber AS [Roll], u.Department AS [Dept],
                                      m.Role AS [Soc Role], m.Status, m.AppliedAt AS [Applied],
                                      m.ApprovedAt AS [Approved]
                               FROM Memberships m
                               JOIN Users u ON m.UserID=u.UserID
                               JOIN Societies s ON m.SocietyID=s.SocietyID
                               ORDER BY s.Name, u.FullName";
                var dgv = MakeGrid();
                dgv.DataSource = DatabaseManager.ExecuteQuery(sql);
                var btnExport = UITheme.MakePrimaryButton("📤 Export CSV", 10, 10, 150, 36);
                btnExport.Click += (s, e) => ExportCSV(dgv, "MembersReport");
                p.Controls.Add(btnExport);
                dgv.Location = new Point(10, 60); dgv.Size = new Size(910, 510);
                p.Controls.Add(dgv);
            }
            catch (Exception ex)
            {
                p.Controls.Add(UITheme.MakeLabel("⚠ " + ex.Message, 10, 10, UITheme.AccentRed));
            }
            return p;
        }

        private Panel BuildSocietiesReport()
        {
            var p = new Panel { BackColor = UITheme.BgDeep };
            try
            {
                string sql = @"SELECT s.Name AS [Society], s.Category, s.Status,
                                      ISNULL(u.FullName,'(None)') AS [Head],
                                      s.MaxMembers AS [Max Members],
                                      COUNT(m.MembershipID) AS [Active Members],
                                      COUNT(e.EventID) AS [Total Events],
                                      s.CreatedAt AS [Created]
                               FROM Societies s
                               LEFT JOIN Users u ON s.HeadUserID=u.UserID
                               LEFT JOIN Memberships m ON s.SocietyID=m.SocietyID AND m.Status='Approved'
                               LEFT JOIN Events e ON s.SocietyID=e.SocietyID AND e.Status='Approved'
                               GROUP BY s.SocietyID, s.Name, s.Category, s.Status,
                                        u.FullName, s.MaxMembers, s.CreatedAt
                               ORDER BY [Active Members] DESC";
                var dgv = MakeGrid();
                dgv.DataSource = DatabaseManager.ExecuteQuery(sql);
                var btnExport = UITheme.MakePrimaryButton("📤 Export CSV", 10, 10, 150, 36);
                btnExport.Click += (s, e) => ExportCSV(dgv, "SocietiesReport");
                p.Controls.Add(btnExport);
                dgv.Location = new Point(10, 60); dgv.Size = new Size(910, 510);
                p.Controls.Add(dgv);
            }
            catch (Exception ex)
            {
                p.Controls.Add(UITheme.MakeLabel("⚠ " + ex.Message, 10, 10, UITheme.AccentRed));
            }
            return p;
        }

        private Panel BuildEventsReport()
        {
            var p = new Panel { BackColor = UITheme.BgDeep };
            try
            {
                string sql = @"SELECT e.Title AS [Event], s.Name AS [Society], e.Venue,
                                      e.EventDate AS [Date], e.MaxAttendees AS [Max],
                                      COUNT(r.RegID) AS [Registered], e.Status
                               FROM Events e
                               JOIN Societies s ON e.SocietyID=s.SocietyID
                               LEFT JOIN EventRegistrations r ON e.EventID=r.EventID
                               GROUP BY e.EventID, e.Title, s.Name, e.Venue,
                                        e.EventDate, e.MaxAttendees, e.Status
                               ORDER BY e.EventDate DESC";
                var dgv = MakeGrid();
                dgv.DataSource = DatabaseManager.ExecuteQuery(sql);
                var btnExport = UITheme.MakePrimaryButton("📤 Export CSV", 10, 10, 150, 36);
                btnExport.Click += (s, e) => ExportCSV(dgv, "EventsReport");
                p.Controls.Add(btnExport);
                dgv.Location = new Point(10, 60); dgv.Size = new Size(910, 510);
                p.Controls.Add(dgv);
            }
            catch (Exception ex)
            {
                p.Controls.Add(UITheme.MakeLabel("⚠ " + ex.Message, 10, 10, UITheme.AccentRed));
            }
            return p;
        }

        private Panel BuildSummaryReport()
        {
            var p = new Panel { BackColor = UITheme.BgDeep };
            int y = 20;
            try
            {
                var items = new (string, string)[]
                {
                    ("Total Users",          DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Users")?.ToString()),
                    ("Active Users",         DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Users WHERE IsActive=1")?.ToString()),
                    ("Total Societies",      DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Societies")?.ToString()),
                    ("Active Societies",     DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Societies WHERE Status='Active'")?.ToString()),
                    ("Total Events",         DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Events")?.ToString()),
                    ("Approved Events",      DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Events WHERE Status='Approved'")?.ToString()),
                    ("Total Memberships",    DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Memberships")?.ToString()),
                    ("Approved Memberships", DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Memberships WHERE Status='Approved'")?.ToString()),
                    ("Total Registrations",  DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM EventRegistrations")?.ToString()),
                };

                foreach (var (label, value) in items)
                {
                    var card = UITheme.MakeCard(20 + (y > 200 ? 300 : 0), y % 250 + 20, 260, 70);
                    card.Controls.Add(new Label { Text = label, Location = new Point(12, 10),
                        AutoSize = true, Font = UITheme.FontSmall, ForeColor = UITheme.TextSecond });
                    card.Controls.Add(new Label { Text = value ?? "N/A", Location = new Point(12, 32),
                        AutoSize = true, Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                        ForeColor = UITheme.AccentViolet });
                    p.Controls.Add(card);
                    y += 90;
                }
            }
            catch (Exception ex)
            {
                p.Controls.Add(UITheme.MakeLabel("⚠ " + ex.Message, 10, 10, UITheme.AccentRed));
            }
            return p;
        }

        private void ExportCSV(DataGridView dgv, string name)
        {
            try
            {
                using (var sfd = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = $"{name}_{DateTime.Now:yyyyMMdd_HHmm}.csv"
                })
                {
                    if (sfd.ShowDialog() != DialogResult.OK) return;
                    var sb = new System.Text.StringBuilder();
                    // Header
                    foreach (DataGridViewColumn col in dgv.Columns)
                        sb.Append(col.HeaderText + ",");
                    sb.AppendLine();
                    // Rows
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                            sb.Append($"\"{cell.Value}\",");
                        sb.AppendLine();
                    }
                    System.IO.File.WriteAllText(sfd.FileName, sb.ToString());
                    MessageBox.Show("✓ Report exported successfully!", "Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}



