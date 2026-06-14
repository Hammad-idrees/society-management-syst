using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    public class EventsForm : Form
    {
        private DataGridView dgvEvents;
        private Button btnRegister, btnRefresh;
        private Label lblMsg;

        public EventsForm()
        {
            InitializeUI();
            LoadEvents();
        }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Dock = DockStyle.Fill;

            // ── TOP BAR ─────────────────────────────────────────────────────────
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = UITheme.BgDeep };
            var lblTitle = UITheme.MakeHeader("📅 Upcoming Events", 20, 16);
            lblTitle.Font = UITheme.FontSubtitle;
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(UITheme.MakeLabel("Browse and register for society events.", 20, 52, UITheme.TextSecond));

            btnRefresh = UITheme.MakePrimaryButton("🔄 Refresh", 380, 48, 110, 36);
            btnRefresh.Click += (s, e) => LoadEvents();
            pnlTop.Controls.Add(btnRefresh);

            // Action Button moved to Top Bar
            btnRegister = UITheme.MakeSuccessButton("🎫 Register for Selected Event", 510, 46, 260, 40);
            btnRegister.Click += BtnRegister_Click;
            pnlTop.Controls.Add(btnRegister);

            lblMsg = new Label
            {
                Location = new Point(380, 20), Size = new Size(600, 24),
                Font = UITheme.FontBody, ForeColor = UITheme.AccentGreen
            };
            pnlTop.Controls.Add(lblMsg);
            Controls.Add(pnlTop);

            // ── GRID (Fill — added LAST) ─────────────────────────────────────────
            dgvEvents = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGrid(dgvEvents);
            Controls.Add(dgvEvents);

            // ── RIGHT CLICK MENU ────────────────────────────────────────────────
            var ctx = new ContextMenuStrip();
            ctx.BackColor = UITheme.BgCard;
            ctx.ForeColor = UITheme.TextPrimary;
            var mnuReg = new ToolStripMenuItem("🎫 Register for Event");
            mnuReg.Click += BtnRegister_Click;
            ctx.Items.Add(mnuReg);
            dgvEvents.ContextMenuStrip = ctx;
        }

        private void LoadEvents()
        {
            try
            {
                string sql = @"SELECT e.EventID AS ID, e.Title AS [Event], s.Name AS [Society],
                                      e.Venue, e.EventDate AS [Date & Time],
                                      e.MaxAttendees AS [Max],
                                      (SELECT COUNT(*) FROM EventRegistrations r WHERE r.EventID=e.EventID) AS [Registered]
                               FROM Events e JOIN Societies s ON e.SocietyID=s.SocietyID
                               WHERE e.Status='Approved' AND e.EventDate >= GETDATE()
                               ORDER BY e.EventDate";
                DataTable dt = DatabaseManager.ExecuteQuery(sql);
                dgvEvents.DataSource = dt;
                if (dgvEvents.Columns.Contains("ID")) dgvEvents.Columns["ID"].Visible = false;
                lblMsg.Text = "";
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";
            if (dgvEvents.CurrentRow == null)
            { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ Please select an event first."; return; }

            int eid = Convert.ToInt32(dgvEvents.CurrentRow.Cells["ID"].Value);
            int uid = SessionManager.CurrentUser.UserID;
            try
            {
                object already = DatabaseManager.ExecuteScalar(
                    "SELECT COUNT(*) FROM EventRegistrations WHERE EventID=@E AND UserID=@U",
                    new SqlParameter("@E", eid), new SqlParameter("@U", uid));
                if (Convert.ToInt32(already) > 0)
                { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ Already registered for this event."; return; }

                object regCount = DatabaseManager.ExecuteScalar(
                    "SELECT COUNT(*) FROM EventRegistrations WHERE EventID=@E", new SqlParameter("@E", eid));
                object maxCount = DatabaseManager.ExecuteScalar(
                    "SELECT MaxAttendees FROM Events WHERE EventID=@E", new SqlParameter("@E", eid));
                if (Convert.ToInt32(regCount) >= Convert.ToInt32(maxCount))
                { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = "⚠ Event is full!"; return; }

                string ticket = "TKT-" + eid.ToString("D4") + "-" + uid.ToString("D4");
                DatabaseManager.ExecuteNonQuery(
                    "INSERT INTO EventRegistrations (EventID, UserID, TicketNum) VALUES (@E, @U, @T)",
                    new SqlParameter("@E", eid), new SqlParameter("@U", uid), new SqlParameter("@T", ticket));
                lblMsg.ForeColor = UITheme.AccentGreen;
                lblMsg.Text = "✓ Registered! Your ticket: " + ticket + "  —  Check 'My Tickets' to print it.";
                LoadEvents();
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }
    }
}
