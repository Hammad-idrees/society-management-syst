using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    public class BrowseSocietiesForm : Form
    {
        private DataGridView dgvSocieties;
        private TextBox txtSearch;
        private Button btnApply, btnRefresh;
        private Label lblMsg;

        public BrowseSocietiesForm()
        {
            InitializeUI();
            LoadSocieties();
        }

        private void InitializeUI()
        {
            UITheme.ApplyToForm(this);
            Dock = DockStyle.Fill;

            // ── TOP BAR (DockStyle.Top) ─────────────────────────────────────────
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = UITheme.BgDeep };

            var lblTitle = UITheme.MakeHeader("🏛 Browse Societies", 20, 16);
            lblTitle.Font = UITheme.FontSubtitle;
            pnlTop.Controls.Add(lblTitle);

            pnlTop.Controls.Add(UITheme.MakeLabel("Search:", 20, 68));
            txtSearch = UITheme.MakeTextBox(82, 64, 250);
            txtSearch.TextChanged += (s, e) => LoadSocieties();
            pnlTop.Controls.Add(txtSearch);

            btnRefresh = UITheme.MakePrimaryButton("🔄 Refresh", 350, 62, 110, 36);
            btnRefresh.Click += (s, e) => LoadSocieties();
            pnlTop.Controls.Add(btnRefresh);

            // Action button moved to TOP BAR!
            btnApply = UITheme.MakeSuccessButton("✋ Join Selected Society", 480, 60, 220, 40);
            btnApply.Click += BtnApply_Click;
            pnlTop.Controls.Add(btnApply);

            lblMsg = new Label
            {
                Location = new Point(480, 25), Size = new Size(500, 24),
                Font = UITheme.FontBody, ForeColor = UITheme.AccentGreen
            };
            pnlTop.Controls.Add(lblMsg);

            Controls.Add(pnlTop);

            // ── DATA GRID ───────────────────────────────────────────────────────
            dgvSocieties = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGrid(dgvSocieties);
            Controls.Add(dgvSocieties);

            // ── RIGHT CLICK MENU ────────────────────────────────────────────────
            var ctx = new ContextMenuStrip();
            ctx.BackColor = UITheme.BgCard;
            ctx.ForeColor = UITheme.TextPrimary;
            var mnuJoin = new ToolStripMenuItem("✋ Join Selected Society");
            mnuJoin.Click += BtnApply_Click;
            ctx.Items.Add(mnuJoin);
            dgvSocieties.ContextMenuStrip = ctx;
        }

        private void LoadSocieties()
        {
            try
            {
                string search = txtSearch.Text.Trim();
                string sql = @"SELECT s.SocietyID AS ID, s.Name AS [Society], s.Category,
                                      ISNULL(u.FullName,'—') AS [Head], s.Status,
                                      s.MaxMembers AS [Max],
                                      (SELECT COUNT(*) FROM Memberships m
                                       WHERE m.SocietyID=s.SocietyID AND m.Status='Approved') AS [Members],
                                      s.Description AS [About]
                               FROM Societies s LEFT JOIN Users u ON s.HeadUserID=u.UserID
                               WHERE s.Status='Active'
                               AND (@S='' OR s.Name LIKE '%'+@S+'%' OR s.Category LIKE '%'+@S+'%')
                               ORDER BY s.Name";
                DataTable dt = DatabaseManager.ExecuteQuery(sql, new SqlParameter("@S", search));
                dgvSocieties.DataSource = dt;
                if (dgvSocieties.Columns.Contains("ID")) dgvSocieties.Columns["ID"].Visible = false;
                lblMsg.Text = "";
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";
            if (dgvSocieties.CurrentRow == null)
            { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ Please select a society first."; return; }

            int sid = Convert.ToInt32(dgvSocieties.CurrentRow.Cells["ID"].Value);
            int uid = SessionManager.CurrentUser.UserID;
            try
            {
                object already = DatabaseManager.ExecuteScalar(
                    "SELECT COUNT(*) FROM Memberships WHERE UserID=@U AND SocietyID=@S",
                    new SqlParameter("@U", uid), new SqlParameter("@S", sid));
                if (Convert.ToInt32(already) > 0)
                { lblMsg.ForeColor = UITheme.AccentAmber; lblMsg.Text = "⚠ Already applied or a member of this society."; return; }

                DatabaseManager.ExecuteNonQuery(
                    "INSERT INTO Memberships (UserID, SocietyID) VALUES (@U, @S)",
                    new SqlParameter("@U", uid), new SqlParameter("@S", sid));
                lblMsg.ForeColor = UITheme.AccentGreen;
                lblMsg.Text = "✓ Application submitted! Awaiting society head approval.";
            }
            catch (Exception ex) { lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = ex.Message; }
        }
    }
}
