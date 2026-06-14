using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using SocietyMS.Database;
using SocietyMS.Helpers;

namespace SocietyMS.Forms
{
    /// <summary>Assign tasks to society members with priority and due date.</summary>
    public class AssignTasksForm : Form
    {
        private DataGridView dgvTasks;
        private ComboBox cmbMember;
        private TextBox txtTitle, txtDesc;
        private ComboBox cmbPriority;
        private DateTimePicker dtpDue;
        private Button btnAssign, btnComplete;
        private Label lblMsg;
        private int _societyId;

        public AssignTasksForm()
        {
            _societyId = GetSocietyId();
            InitializeUI();
            LoadMembers();
            LoadTasks();
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
            Controls.Add(UITheme.MakeHeader("📋 Assign Tasks to Members", 20, 20));

            // Left: form
            int y = 68, lx = 20, fw = 380;
            Controls.Add(UITheme.MakeLabel("Assign To *", lx, y)); y += 24;
            cmbMember = new ComboBox { Location = new System.Drawing.Point(lx, y), Size = new System.Drawing.Size(fw, 36) };
            UITheme.StyleComboBox(cmbMember);
            cmbMember.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(cmbMember); y += 50;

            Controls.Add(UITheme.MakeLabel("Task Title *", lx, y)); y += 24;
            txtTitle = UITheme.MakeTextBox(lx, y, fw); Controls.Add(txtTitle); y += 50;

            Controls.Add(UITheme.MakeLabel("Priority", lx, y)); y += 24;
            cmbPriority = new ComboBox { Location = new System.Drawing.Point(lx, y), Size = new System.Drawing.Size(180, 36) };
            UITheme.StyleComboBox(cmbPriority);
            cmbPriority.Items.AddRange(new[] { "Low", "Medium", "High" });
            cmbPriority.SelectedIndex = 1;
            cmbPriority.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(cmbPriority); y += 50;

            Controls.Add(UITheme.MakeLabel("Due Date", lx, y)); y += 24;
            dtpDue = new DateTimePicker
            {
                Location = new System.Drawing.Point(lx, y),
                Size = new System.Drawing.Size(fw, 36),
                MinDate = DateTime.Today,
                Format = DateTimePickerFormat.Short
            };
            Controls.Add(dtpDue); y += 50;

            Controls.Add(UITheme.MakeLabel("Description", lx, y)); y += 24;
            txtDesc = new TextBox
            {
                Location = new System.Drawing.Point(lx, y), Size = new System.Drawing.Size(fw, 80),
                Multiline = true, BackColor = UITheme.BgCard,
                ForeColor = UITheme.TextPrimary, Font = UITheme.FontBody
            };
            Controls.Add(txtDesc); y += 95;

            lblMsg = new Label { Location = new System.Drawing.Point(lx, y), Size = new System.Drawing.Size(fw, 24),
                Font = UITheme.FontBody, ForeColor = UITheme.AccentGreen };
            Controls.Add(lblMsg); y += 30;

            btnAssign = UITheme.MakePrimaryButton("📌 Assign Task", lx, y, 190, 42);
            btnAssign.Click += BtnAssign_Click;
            Controls.Add(btnAssign);

            // Right: task list
            dgvTasks = new DataGridView
            {
                Location = new System.Drawing.Point(430, 68),
                Size = new System.Drawing.Size(530, 490)
            };
            UITheme.StyleDataGrid(dgvTasks);
            Controls.Add(dgvTasks);

            btnComplete = UITheme.MakeSuccessButton("✓ Mark Complete", 430, 572, 180, 40);
            btnComplete.Click += BtnComplete_Click;
            Controls.Add(btnComplete);
        }

        private void LoadMembers()
        {
            if (_societyId == 0) return;
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(
                    @"SELECT u.UserID, u.FullName FROM Users u
                      JOIN Memberships m ON u.UserID=m.UserID
                      WHERE m.SocietyID=@S AND m.Status='Approved'
                      ORDER BY u.FullName",
                    new SqlParameter("@S", _societyId));
                cmbMember.DisplayMember = "FullName";
                cmbMember.ValueMember = "UserID";
                cmbMember.DataSource = dt;
            }
            catch { }
        }

        private void LoadTasks()
        {
            if (_societyId == 0) return;
            try
            {
                string sql = @"SELECT t.TaskID AS ID, u.FullName AS [Assigned To],
                                      t.Title, t.Priority, t.Status, t.DueDate AS [Due]
                               FROM SocietyTasks t JOIN Users u ON t.AssignedTo=u.UserID
                               WHERE t.SocietyID=@S
                               ORDER BY CASE t.Status WHEN 'Pending' THEN 0 WHEN 'InProgress' THEN 1 ELSE 2 END,
                                        t.DueDate ASC";
                DataTable dt = DatabaseManager.ExecuteQuery(sql, new SqlParameter("@S", _societyId));
                dgvTasks.DataSource = dt;
                if (dgvTasks.Columns.Contains("ID")) dgvTasks.Columns["ID"].Visible = false;
            }
            catch (Exception ex)
            {
                lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = "⚠ " + ex.Message;
            }
        }

        private void BtnAssign_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";
            string title = txtTitle.Text.Trim();
            if (!ValidationHelper.IsNotEmpty(title))
            {
                lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = "⚠ Task title required."; return;
            }
            if (cmbMember.SelectedValue == null)
            {
                lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = "⚠ Select a member."; return;
            }
            if (_societyId == 0)
            {
                lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = "⚠ No active society."; return;
            }
            try
            {
                int assignedTo = Convert.ToInt32(cmbMember.SelectedValue);
                DatabaseManager.ExecuteNonQuery(
                    @"INSERT INTO SocietyTasks (SocietyID, AssignedTo, AssignedBy, Title, Description, DueDate, Priority)
                      VALUES (@S, @AT, @AB, @T, @D, @Due, @P)",
                    new SqlParameter("@S",   _societyId),
                    new SqlParameter("@AT",  assignedTo),
                    new SqlParameter("@AB",  SessionManager.CurrentUser.UserID),
                    new SqlParameter("@T",   title),
                    new SqlParameter("@D",   txtDesc.Text.Trim()),
                    new SqlParameter("@Due", dtpDue.Value),
                    new SqlParameter("@P",   cmbPriority.SelectedItem?.ToString() ?? "Medium"));
                lblMsg.ForeColor = UITheme.AccentGreen;
                lblMsg.Text = "✓ Task assigned successfully.";
                txtTitle.Clear(); txtDesc.Clear();
                LoadTasks();
            }
            catch (Exception ex)
            {
                lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = "⚠ " + ex.Message;
            }
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            if (dgvTasks.CurrentRow == null) return;
            int tid = Convert.ToInt32(dgvTasks.CurrentRow.Cells["ID"].Value);
            try
            {
                DatabaseManager.ExecuteNonQuery(
                    "UPDATE SocietyTasks SET Status='Completed' WHERE TaskID=@T",
                    new SqlParameter("@T", tid));
                lblMsg.ForeColor = UITheme.AccentGreen; lblMsg.Text = "✓ Task marked complete.";
                LoadTasks();
            }
            catch (Exception ex)
            {
                lblMsg.ForeColor = UITheme.AccentRed; lblMsg.Text = "⚠ " + ex.Message;
            }
        }
    }
}



