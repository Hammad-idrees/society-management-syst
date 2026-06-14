using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    public class ManageEventsForm : Form
    {
        private DataGridView dgvEvents;
        private TextBox txtTitle, txtDesc, txtVenue;
        private DateTimePicker dtpDate;
        private NumericUpDown nudMax;
        private Button btnAdd, btnRefresh;
        private Label lblMsg;
        private bool _adminMode;
        private int _societyId;

        public ManageEventsForm(bool adminMode = false)
        {
            _adminMode = adminMode;
            if (!adminMode) _societyId = GetSocietyId();
            InitializeUI();
            LoadEvents();
        }

        private int GetSocietyId()
        {
            try
            {
                object id = DatabaseManager.ExecuteScalar(
                    "SELECT SocietyID FROM Societies WHERE HeadUserID=@U",
                    new SqlParameter("@U", SessionManager.CurrentUser.UserID));
                return (id != null && id != DBNull.Value) ? Convert.ToInt32(id) : 0;
            }
            catch { return 0; }
        }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Dock = DockStyle.Fill;
            string title = _adminMode ? "📅 Event Approval Queue" : "📅 Manage Society Events";

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = UITheme.BgDeep };
            var lbl = UITheme.MakeHeader(title, 20, 20);
            pnlTop.Controls.Add(lbl);

            btnRefresh = UITheme.MakePrimaryButton("🔄 Refresh", 350, 20, 110, 36);
            btnRefresh.Click += (s, e) => LoadEvents();
            pnlTop.Controls.Add(btnRefresh);

            lblMsg = new Label { Location = new System.Drawing.Point(350, 60), AutoSize = true, Font = UITheme.FontBody, ForeColor = UITheme.AccentGreen };
            pnlTop.Controls.Add(lblMsg);

            // Action buttons moved to pnlTop!
            if (_adminMode)
            {
                var btnApprove = UITheme.MakeSuccessButton("✓ Approve Event", 800, 20, 150, 36);
                btnApprove.Click += BtnApprove_Click;
                pnlTop.Controls.Add(btnApprove);

                var btnReject = UITheme.MakeDangerButton("✗ Reject", 960, 20, 110, 36);
                btnReject.Click += BtnCancelEvent_Click;
                pnlTop.Controls.Add(btnReject);
            }
            else
            {
                var btnCancelEvent = UITheme.MakeDangerButton("✗ Cancel Event", 800, 20, 150, 36);
                btnCancelEvent.Click += BtnCancelEvent_Click;
                pnlTop.Controls.Add(btnCancelEvent);
            }

            Controls.Add(pnlTop);

            var ctx = new ContextMenuStrip();
            ctx.BackColor = UITheme.BgCard; ctx.ForeColor = UITheme.TextPrimary;

            if (!_adminMode)
            {
                // ─ Left panel: create event form ─────────────────────────────────
                var pnlLeft = new Panel { Dock = DockStyle.Left, Width = 480, BackColor = UITheme.BgDeep };
                int y = 20;
                pnlLeft.Controls.Add(UITheme.MakeLabel("Event Title *", 20, y)); y += 22;
                txtTitle = UITheme.MakeTextBox(20, y, 440); pnlLeft.Controls.Add(txtTitle); y += 44;
                pnlLeft.Controls.Add(UITheme.MakeLabel("Venue", 20, y)); y += 22;
                txtVenue = UITheme.MakeTextBox(20, y, 440); pnlLeft.Controls.Add(txtVenue); y += 44;
                pnlLeft.Controls.Add(UITheme.MakeLabel("Event Date & Time", 20, y)); y += 22;
                dtpDate = new DateTimePicker
                {
                    Location = new System.Drawing.Point(20, y), Size = new System.Drawing.Size(260, 36),
                    Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm",
                    MinDate = DateTime.Today.AddDays(1)
                };
                pnlLeft.Controls.Add(dtpDate); y += 44;
                pnlLeft.Controls.Add(UITheme.MakeLabel("Max Attendees", 20, y)); y += 22;
                nudMax = new NumericUpDown
                {
                    Location = new System.Drawing.Point(20, y), Size = new System.Drawing.Size(120, 34),
                    Minimum = 5, Maximum = 1000, Value = 50,
                    BackColor = UITheme.BgCard, ForeColor = UITheme.TextPrimary, Font = UITheme.FontBody
                };
                pnlLeft.Controls.Add(nudMax); y += 44;
                pnlLeft.Controls.Add(UITheme.MakeLabel("Description", 20, y)); y += 22;
                txtDesc = new TextBox
                {
                    Location = new System.Drawing.Point(20, y), Size = new System.Drawing.Size(440, 64),
                    Multiline = true, BackColor = UITheme.BgCard, ForeColor = UITheme.TextPrimary, Font = UITheme.FontBody
                };
                pnlLeft.Controls.Add(txtDesc); y += 78;

                btnAdd = UITheme.MakePrimaryButton("➕ Submit Event Request", 20, y, 240, 42);
                btnAdd.Click += BtnAdd_Click;
                pnlLeft.Controls.Add(btnAdd);
                Controls.Add(pnlLeft);

                var mnuCancel = new ToolStripMenuItem("✗ Cancel Event"); mnuCancel.Click += BtnCancelEvent_Click; ctx.Items.Add(mnuCancel);
            }
            else
            {
                var mnuApprove = new ToolStripMenuItem("✓ Approve Event"); mnuApprove.Click += BtnApprove_Click; ctx.Items.Add(mnuApprove);
                var mnuReject = new ToolStripMenuItem("✗ Reject/Cancel"); mnuReject.Click += BtnCancelEvent_Click; ctx.Items.Add(mnuReject);
            }

            dgvEvents = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGrid(dgvEvents);
            dgvEvents.ContextMenuStrip = ctx;
            Controls.Add(dgvEvents);
        }

        private void LoadEvents()
        {
            try
            {
                string sql = _adminMode ? 
                    @"SELECT e.EventID AS ID, e.Title AS [Event], s.Name AS [Society],
                             e.Venue, e.EventDate AS [Date], e.MaxAttendees AS [Max], e.Status
                      FROM Events e JOIN Societies s ON e.SocietyID=s.SocietyID
                      ORDER BY CASE e.Status WHEN 'Pending' THEN 0 ELSE 1 END, e.EventDate" 
                    : 
                    @"SELECT EventID AS ID, Title AS [Event], Venue, EventDate AS [Date],
                             MaxAttendees AS [Max], Status
                      FROM Events WHERE SocietyID=@S ORDER BY EventDate DESC";

                DataTable dt = DatabaseManager.ExecuteQuery(sql, new SqlParameter("@S", _societyId));
                dgvEvents.DataSource = dt;
                if (dgvEvents.Columns.Contains("ID")) dgvEvents.Columns["ID"].Visible = false;
                lblMsg.Text = "";
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (_societyId == 0) { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ You are not assigned to an active society."; return; }
            string title = txtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title)) { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ Title is required."; return; }

            try
            {
                string sql = "INSERT INTO Events (SocietyID, Title, Description, EventDate, Venue, MaxAttendees, Status) " +
                             "VALUES (@S, @T, @D, @Dt, @V, @M, 'Pending')";
                DatabaseManager.ExecuteNonQuery(sql,
                    new SqlParameter("@S", _societyId),
                    new SqlParameter("@T", title),
                    new SqlParameter("@D", txtDesc.Text.Trim()),
                    new SqlParameter("@Dt", dtpDate.Value),
                    new SqlParameter("@V", txtVenue.Text.Trim()),
                    new SqlParameter("@M", nudMax.Value)
                );
                lblMsg.ForeColor = UITheme.AccentGreen; lblMsg.Text = "✓ Event requested. Awaiting admin approval.";
                txtTitle.Text = ""; txtDesc.Text = ""; txtVenue.Text = "";
                LoadEvents();
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }

        private void BtnCancelEvent_Click(object sender, EventArgs e)
        {
            if (dgvEvents.CurrentRow == null) { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ Select an event first."; return; }
            int eid = Convert.ToInt32(dgvEvents.CurrentRow.Cells["ID"].Value);
            if (MessageBox.Show("Are you sure you want to cancel/delete this event?", "Confirm Cancel",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                DatabaseManager.ExecuteNonQuery("DELETE FROM EventRegistrations WHERE EventID=@E", new SqlParameter("@E", eid));
                DatabaseManager.ExecuteNonQuery("DELETE FROM Events WHERE EventID=@E", new SqlParameter("@E", eid));
                lblMsg.ForeColor = UITheme.AccentGreen; lblMsg.Text = "✓ Event cancelled/deleted.";
                LoadEvents();
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }

        private void BtnApprove_Click(object sender, EventArgs e)
        {
            if (!_adminMode || dgvEvents.CurrentRow == null) return;
            int eid = Convert.ToInt32(dgvEvents.CurrentRow.Cells["ID"].Value);
            try
            {
                DatabaseManager.ExecuteNonQuery("UPDATE Events SET Status='Approved' WHERE EventID=@E", new SqlParameter("@E", eid));
                lblMsg.ForeColor = UITheme.AccentGreen; lblMsg.Text = "✓ Event approved.";
                LoadEvents();
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }
    }
}
