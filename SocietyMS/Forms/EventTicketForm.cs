using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    /// <summary>
    /// Event Ticket viewer: shows all event registrations for the logged-in student
    /// with printable ticket details including unique ticket number.
    /// </summary>
    public class EventTicketForm : Form
    {
        private DataGridView dgvTickets;
        private Panel pnlTicketPreview;
        private Button btnPrint, btnRefresh;
        private Label lblMsg;

        public EventTicketForm()
        {
            InitializeUI();
            LoadTickets();
        }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Dock = DockStyle.Fill;
            Controls.Add(UITheme.MakeHeader("My Event Tickets", 20, 20));
            Controls.Add(UITheme.MakeLabel("Click a ticket to view details.", 20, 55, UITheme.TextSecond));

            btnRefresh = UITheme.MakePrimaryButton("Refresh", 830, 20, 120, 36);
            btnRefresh.Click += (s, e) => LoadTickets();
            Controls.Add(btnRefresh);

            dgvTickets = new DataGridView { Location = new Point(20, 90), Size = new Size(460, 530) };
            UITheme.StyleDataGrid(dgvTickets);
            dgvTickets.SelectionChanged += DgvTickets_SelectionChanged;
            Controls.Add(dgvTickets);

            pnlTicketPreview = UITheme.MakeCard(500, 90, 450, 400);
            Controls.Add(pnlTicketPreview);

            lblMsg = new Label
            {
                Location = new Point(500, 504), Size = new Size(450, 24),
                Font = UITheme.FontBody, ForeColor = UITheme.AccentGreen
            };
            Controls.Add(lblMsg);

            btnPrint = UITheme.MakePrimaryButton("Print Ticket", 500, 530, 200, 44);
            btnPrint.Click += BtnPrint_Click;
            Controls.Add(btnPrint);
        }

        private void LoadTickets()
        {
            try
            {
                int uid = SessionManager.CurrentUser.UserID;
                string sql = @"SELECT r.RegID AS ID, e.Title AS [Event], s.Name AS [Society],
                                      e.Venue, e.EventDate AS [Date], r.TicketNum AS [Ticket No],
                                      r.RegisteredAt AS [Registered On]
                               FROM EventRegistrations r
                               JOIN Events e ON r.EventID=e.EventID
                               JOIN Societies s ON e.SocietyID=s.SocietyID
                               WHERE r.UserID=@U ORDER BY r.RegisteredAt DESC";
                DataTable dt = DatabaseManager.ExecuteQuery(sql, new SqlParameter("@U", uid));
                dgvTickets.DataSource = dt;
                if (dgvTickets.Columns.Contains("ID")) dgvTickets.Columns["ID"].Visible = false;
                pnlTicketPreview.Controls.Clear();
            }
            catch (Exception ex)
            {
                lblMsg.ForeColor = UITheme.AccentRed;
                lblMsg.Text = "Error: " + ex.Message;
            }
        }

        private void DgvTickets_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvTickets.CurrentRow == null) return;
            pnlTicketPreview.Controls.Clear();

            string eventName = dgvTickets.CurrentRow.Cells["Event"].Value != null
                ? dgvTickets.CurrentRow.Cells["Event"].Value.ToString() : "";
            string society = dgvTickets.CurrentRow.Cells["Society"].Value != null
                ? dgvTickets.CurrentRow.Cells["Society"].Value.ToString() : "";
            string venue = dgvTickets.CurrentRow.Cells["Venue"].Value != null
                ? dgvTickets.CurrentRow.Cells["Venue"].Value.ToString() : "";
            string date = dgvTickets.CurrentRow.Cells["Date"].Value != null
                ? dgvTickets.CurrentRow.Cells["Date"].Value.ToString() : "";
            string ticketNum = dgvTickets.CurrentRow.Cells["Ticket No"].Value != null
                ? dgvTickets.CurrentRow.Cells["Ticket No"].Value.ToString() : "";
            string regDate = dgvTickets.CurrentRow.Cells["Registered On"].Value != null
                ? dgvTickets.CurrentRow.Cells["Registered On"].Value.ToString() : "";

            RenderTicket(eventName, society, venue, date, ticketNum, regDate);
        }

        // ─── Render Ticket Card ───────────────────────────────────────────────────
        private void RenderTicket(string ev, string soc, string venue,
            string date, string ticket, string regDate)
        {
            var stripe = new Panel
            {
                Location = new Point(0, 0), Size = new Size(450, 8),
                BackColor = UITheme.AccentViolet
            };
            pnlTicketPreview.Controls.Add(stripe);

            int y = 20;

            var lblHeader = new Label
            {
                Text = "EVENT TICKET", Location = new Point(14, y), AutoSize = true,
                Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = UITheme.AccentViolet
            };
            pnlTicketPreview.Controls.Add(lblHeader);
            y += 44;

            AddTicketRow("EVENT", ev, UITheme.TextPrimary, ref y);
            AddTicketRow("SOCIETY", soc, UITheme.AccentPink, ref y);
            AddTicketRow("DATE & TIME", date, UITheme.AccentCyan, ref y);
            AddTicketRow("VENUE", venue, UITheme.TextPrimary, ref y);
            AddTicketRow("HOLDER", SessionManager.CurrentUser.FullName, UITheme.TextPrimary, ref y);
            AddTicketRow("REGISTERED ON", regDate, UITheme.TextSecond, ref y);

            // Ticket number box
            var pnlTN = new Panel
            {
                Location = new Point(14, y), Size = new Size(416, 56),
                BackColor = UITheme.BgPanel
            };
            pnlTN.Controls.Add(new Label
            {
                Text = "TICKET NUMBER", Location = new Point(10, 6), AutoSize = true,
                Font = UITheme.FontSmall, ForeColor = UITheme.TextMuted
            });
            pnlTN.Controls.Add(new Label
            {
                Text = ticket, Location = new Point(10, 24), AutoSize = true,
                Font = new Font("Courier New", 14f, FontStyle.Bold), ForeColor = UITheme.AccentGreen
            });
            pnlTicketPreview.Controls.Add(pnlTN);
        }

        /// <summary>Helper to add a label+value row to the ticket card.</summary>
        private void AddTicketRow(string label, string val, Color color, ref int y)
        {
            pnlTicketPreview.Controls.Add(new Label
            {
                Text = label, Location = new Point(14, y), AutoSize = true,
                Font = UITheme.FontSmall, ForeColor = UITheme.TextMuted
            });
            y += 18;
            pnlTicketPreview.Controls.Add(new Label
            {
                Text = val, Location = new Point(14, y), Size = new Size(416, 24),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = color
            });
            y += 30;
        }

        // ─── Print ────────────────────────────────────────────────────────────────
        private void BtnPrint_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";
            if (dgvTickets.CurrentRow == null)
            {
                lblMsg.ForeColor = UITheme.AccentAmber;
                lblMsg.Text = "Select a ticket first.";
                return;
            }

            string ev     = dgvTickets.CurrentRow.Cells["Event"].Value != null
                ? dgvTickets.CurrentRow.Cells["Event"].Value.ToString() : "";
            string ticket = dgvTickets.CurrentRow.Cells["Ticket No"].Value != null
                ? dgvTickets.CurrentRow.Cells["Ticket No"].Value.ToString() : "";
            string date   = dgvTickets.CurrentRow.Cells["Date"].Value != null
                ? dgvTickets.CurrentRow.Cells["Date"].Value.ToString() : "";
            string venue  = dgvTickets.CurrentRow.Cells["Venue"].Value != null
                ? dgvTickets.CurrentRow.Cells["Venue"].Value.ToString() : "";
            string holder = SessionManager.CurrentUser.FullName;

            var pd = new PrintDocument();
            pd.PrintPage += delegate(object ps, PrintPageEventArgs pe)
            {
                var g = pe.Graphics;
                int py = 80;
                g.DrawString("FAST SOCIETIES MANAGEMENT SYSTEM",
                    new Font("Arial", 16, FontStyle.Bold), Brushes.Black, 80, py); py += 40;
                g.DrawString("EVENT TICKET",
                    new Font("Arial", 14, FontStyle.Bold), Brushes.DarkBlue, 80, py); py += 40;
                g.DrawLine(Pens.Gray, 80, py, 680, py); py += 20;
                g.DrawString("Event:  " + ev,    new Font("Arial", 12), Brushes.Black, 80, py); py += 30;
                g.DrawString("Date:   " + date,  new Font("Arial", 12), Brushes.Black, 80, py); py += 30;
                g.DrawString("Venue:  " + venue, new Font("Arial", 12), Brushes.Black, 80, py); py += 30;
                g.DrawString("Holder: " + holder,new Font("Arial", 12), Brushes.Black, 80, py); py += 30;
                g.DrawLine(Pens.Gray, 80, py, 680, py); py += 20;
                g.DrawString("Ticket: " + ticket,
                    new Font("Courier New", 14, FontStyle.Bold), Brushes.DarkGreen, 80, py);
            };

            using (var ppd = new PrintPreviewDialog
            {
                Document = pd, Width = 800, Height = 600
            })
            {
                ppd.ShowDialog(this);
            }
        }
    }
}


