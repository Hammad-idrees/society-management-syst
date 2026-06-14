using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SocietyMS.Helpers
{
    /// <summary>
    /// FAST SMS – Aurora Glassmorphism Theme.
    /// Deep space background with vibrant violet-to-cyan gradient accents,
    /// glowing cards, and custom-painted controls for a truly premium look.
    /// </summary>
    public static class UITheme
    {
        // ─── Color Palette ────────────────────────────────────────────────────────
        public static Color BgDeep       = Color.FromArgb(8,   6,  24);    // #080618 near-black
        public static Color BgPanel      = Color.FromArgb(15,  12,  40);   // #0F0C28
        public static Color BgCard       = Color.FromArgb(22,  18,  58);   // #16123A
        public static Color BgCardHover  = Color.FromArgb(32,  26,  78);   // slightly lighter
        public static Color AccentViolet = Color.FromArgb(139, 92, 246);   // #8B5CF6 violet
        public static Color AccentCyan   = Color.FromArgb(6,   182, 212);  // #06B6D4 cyan
        public static Color AccentPink   = Color.FromArgb(236, 72, 153);   // #EC4899 pink
        public static Color AccentGreen  = Color.FromArgb(16,  185, 129);  // #10B981 emerald
        public static Color AccentAmber  = Color.FromArgb(245, 158,  11);  // #F59E0B amber
        public static Color AccentRed    = Color.FromArgb(239,  68,  68);  // #EF4444 red
        public static Color TextPrimary  = Color.FromArgb(248, 246, 255);  // near white
        public static Color TextSecond   = Color.FromArgb(167, 157, 210);  // muted lavender
        public static Color TextMuted    = Color.FromArgb(80,   72, 130);  // very muted
        public static Color Border       = Color.FromArgb(60,   50, 110);  // dark violet border
        public static Color GlassEdge    = Color.FromArgb(80, 139, 92, 246); // semi-transparent violet

        // Gradient colors for sidebar
        public static Color GradStart    = Color.FromArgb(45,  10,  90);   // deep purple
        public static Color GradEnd      = Color.FromArgb(8,   40,  80);   // deep teal

        // ─── Fonts ────────────────────────────────────────────────────────────────
        public static Font FontTitle    = new Font("Segoe UI", 20f, FontStyle.Bold);
        public static Font FontSubtitle = new Font("Segoe UI", 13f, FontStyle.Bold);
        public static Font FontBody     = new Font("Segoe UI", 10f, FontStyle.Regular);
        public static Font FontSmall    = new Font("Segoe UI",  8.5f, FontStyle.Regular);
        public static Font FontButton   = new Font("Segoe UI", 10f, FontStyle.Bold);
        public static Font FontLabel    = new Font("Segoe UI", 10f, FontStyle.Regular);
        public static Font FontMono     = new Font("Consolas",  10f, FontStyle.Regular);

        // ─── Form Apply ───────────────────────────────────────────────────────────
        public static void ApplyToForm(Form form)
        {
            form.BackColor = BgDeep;
            form.ForeColor = TextPrimary;
            form.Font      = FontBody;
        }

        // ─── Gradient Sidebar Panel ───────────────────────────────────────────────
        /// <summary>Creates a panel that auto-paints a vertical violet-to-teal gradient.</summary>
        public static Panel MakeGradientPanel(int x, int y, int w, int h)
        {
            var p = new Panel { Location = new Point(x, y), Size = new Size(w, h) };
            p.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(p.ClientRectangle,
                    GradStart, GradEnd, LinearGradientMode.Vertical))
                    e.Graphics.FillRectangle(br, p.ClientRectangle);
            };
            return p;
        }

        // ─── Glow Card Panel ──────────────────────────────────────────────────────
        /// <summary>Creates a card panel with a glowing violet border effect.</summary>
        public static Panel MakeCard(int x, int y, int w, int h)
        {
            var card = new Panel
            {
                Location = new Point(x, y), Size = new Size(w, h),
                BackColor = BgCard, Padding = new Padding(14)
            };
            card.Paint += (s, e) =>
            {
                var r = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using (var pen = new Pen(Color.FromArgb(90, AccentViolet), 1))
                    e.Graphics.DrawRectangle(pen, r);
            };
            return card;
        }

        // ─── Stat Card ────────────────────────────────────────────────────────────
        public static Panel MakeStatCard(int x, int y, string label, string value,
            Color accent, string emoji)
        {
            var card = MakeCard(x, y, 210, 110);
            card.Paint += (s, e) =>
            {
                // Gradient top stripe
                var stripeR = new Rectangle(0, 0, 4, 110);
                using (var br = new SolidBrush(accent))
                    e.Graphics.FillRectangle(br, stripeR);
            };

            card.Controls.Add(new Label
            {
                Text = emoji, Location = new Point(16, 8), AutoSize = true,
                Font = new Font("Segoe UI Emoji", 18f), ForeColor = accent,
                BackColor = Color.Transparent
            });
            card.Controls.Add(new Label
            {
                Text = value, Location = new Point(16, 42), AutoSize = true,
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = accent, BackColor = Color.Transparent
            });
            card.Controls.Add(new Label
            {
                Text = label, Location = new Point(16, 78), Size = new Size(190, 20),
                Font = FontSmall, ForeColor = TextSecond, BackColor = Color.Transparent
            });
            return card;
        }

        // ─── Primary Button ───────────────────────────────────────────────────────
        public static Button MakePrimaryButton(string text, int x, int y, int w = 180, int h = 42)
        {
            var btn = new GradientButton(AccentViolet, AccentCyan)
            {
                Text = text, Location = new Point(x, y), Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White,
                Font = FontButton, Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ─── Secondary Button ─────────────────────────────────────────────────────
        public static Button MakeSecondaryButton(string text, int x, int y, int w = 180, int h = 42)
        {
            var btn = new Button
            {
                Text = text, Location = new Point(x, y), Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat, BackColor = BgCard,
                ForeColor = TextPrimary, Font = FontButton, Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = AccentViolet;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = BgCardHover;
            return btn;
        }

        // ─── Danger Button ────────────────────────────────────────────────────────
        public static Button MakeDangerButton(string text, int x, int y, int w = 140, int h = 38)
        {
            var btn = new Button
            {
                Text = text, Location = new Point(x, y), Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat, BackColor = AccentRed,
                ForeColor = Color.White, Font = FontButton, Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(185, 28, 28);
            return btn;
        }

        // ─── Success Button ───────────────────────────────────────────────────────
        public static Button MakeSuccessButton(string text, int x, int y, int w = 140, int h = 38)
        {
            var btn = new Button
            {
                Text = text, Location = new Point(x, y), Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat, BackColor = AccentGreen,
                ForeColor = Color.White, Font = FontButton, Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(5, 150, 105);
            return btn;
        }

        // ─── TextBox ──────────────────────────────────────────────────────────────
        public static TextBox MakeTextBox(int x, int y, int w = 280, bool password = false)
        {
            var tb = new TextBox
            {
                Location = new Point(x, y), Size = new Size(w, 36),
                BackColor = Color.FromArgb(30, 24, 70),
                ForeColor = TextPrimary, Font = FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };
            if (password) tb.PasswordChar = '●';
            return tb;
        }

        // ─── Label Helpers ────────────────────────────────────────────────────────
        public static Label MakeLabel(string text, int x, int y, Color? color = null)
        {
            return new Label
            {
                Text = text, Location = new Point(x, y), AutoSize = true,
                Font = FontLabel, ForeColor = color ?? TextSecond,
                BackColor = Color.Transparent
            };
        }

        public static Label MakeHeader(string text, int x, int y)
        {
            return new Label
            {
                Text = text, Location = new Point(x, y), AutoSize = true,
                Font = FontSubtitle, ForeColor = TextPrimary,
                BackColor = Color.Transparent
            };
        }

        // ─── Nav Button (Sidebar) ─────────────────────────────────────────────────
        public static Button MakeNavButton(string text, int y, EventHandler onClick,
            bool isDanger = false)
        {
            var btn = new Button
            {
                Text = text, Location = new Point(0, y), Size = new Size(240, 48),
                FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent,
                ForeColor = isDanger ? AccentRed : TextPrimary,
                Font = FontBody, TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 0), Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 139, 92, 246);
            btn.Click += onClick;
            return btn;
        }

        // ─── DataGridView Styling ─────────────────────────────────────────────────
        public static void StyleDataGrid(DataGridView dgv)
        {
            dgv.BackgroundColor      = BgPanel;
            dgv.ForeColor            = TextPrimary;
            dgv.GridColor            = Border;
            dgv.BorderStyle          = BorderStyle.None;
            dgv.RowHeadersVisible    = false;
            dgv.AllowUserToAddRows   = false;
            dgv.AllowUserToDeleteRows= false;
            dgv.ReadOnly             = true;
            dgv.SelectionMode        = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect          = false;
            dgv.AutoSizeColumnsMode  = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.EnableHeadersVisualStyles = false;
            dgv.Font                 = FontBody;
            dgv.ColumnHeadersDefaultCellStyle.BackColor      = Color.FromArgb(28, 10, 60);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor      = AccentViolet;
            dgv.ColumnHeadersDefaultCellStyle.Font           = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(28, 10, 60);
            dgv.ColumnHeadersDefaultCellStyle.Padding        = new Padding(6, 0, 0, 0);
            dgv.ColumnHeadersHeight  = 38;
            dgv.DefaultCellStyle.BackColor         = BgCard;
            dgv.DefaultCellStyle.ForeColor         = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor= AccentViolet;
            dgv.DefaultCellStyle.SelectionForeColor= Color.White;
            dgv.DefaultCellStyle.Padding           = new Padding(4, 0, 0, 0);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(18, 14, 48);
            dgv.RowTemplate.Height   = 36;
        }

        // ─── ComboBox Styling ─────────────────────────────────────────────────────
        public static void StyleComboBox(ComboBox cmb)
        {
            cmb.BackColor = Color.FromArgb(30, 24, 70);
            cmb.ForeColor = TextPrimary;
            cmb.Font      = FontBody;
            cmb.FlatStyle = FlatStyle.Flat;
        }
    }

    // ─── Gradient Button ──────────────────────────────────────────────────────────
    /// <summary>Custom button that paints a left-to-right gradient background.</summary>
    public class GradientButton : Button
    {
        private Color _colorStart, _colorEnd;

        public GradientButton(Color start, Color end)
        {
            _colorStart = start;
            _colorEnd   = end;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var r = new Rectangle(0, 0, Width, Height);
            using (var br = new LinearGradientBrush(r, _colorStart, _colorEnd,
                LinearGradientMode.Horizontal))
                e.Graphics.FillRectangle(br, r);

            TextRenderer.DrawText(e.Graphics, Text, Font, r, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _colorStart = Color.FromArgb(
                Math.Min(255, _colorStart.R + 20),
                Math.Min(255, _colorStart.G + 20),
                Math.Min(255, _colorStart.B + 20));
            Invalidate();
        }
    }
}
