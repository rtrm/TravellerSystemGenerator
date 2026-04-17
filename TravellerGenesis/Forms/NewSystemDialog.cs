using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TravellerGenesis.Forms
{
    internal class NewSystemDialog : Form
    {
        // ── Result properties ─────────────────────────────────────────
        public int?   Seed         { get; private set; }
        public string? SystemName  { get; private set; }
        public string? MainworldUWP { get; private set; }
        public bool   NoMainworld  { get; private set; }

        // ── Controls ─────────────────────────────────────────────────
        private TextBox txtSeed       = null!;
        private TextBox txtName       = null!;
        private TextBox txtUWP        = null!;
        private CheckBox chkNoMainworld = null!;
        private Label lblUWPError     = null!;
        private Button btnOK          = null!;
        private Button btnCancel      = null!;

        public NewSystemDialog()
        {
            Text            = "New System";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            Size            = new Size(420, 290);
            MaximizeBox     = false;
            MinimizeBox     = false;

            BuildControls();
        }

        private void BuildControls()
        {
            var tlp = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 7,
                Padding     = new Padding(12)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;

            // Seed
            tlp.Controls.Add(new Label { Text = "Seed (optional):", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, row);
            txtSeed = new TextBox { Dock = DockStyle.Fill };
            tlp.Controls.Add(txtSeed, 1, row++);

            // Name
            tlp.Controls.Add(new Label { Text = "System Name:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, row);
            txtName = new TextBox { Dock = DockStyle.Fill };
            tlp.Controls.Add(txtName, 1, row++);

            // UWP
            tlp.Controls.Add(new Label { Text = "Mainworld UWP:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, row);
            var uwpPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            txtUWP = new TextBox { Width = 160, MaxLength = 14, CharacterCasing = CharacterCasing.Upper };
            var hint = new Label { Text = "Format: A123456-7 (optional)", ForeColor = SystemColors.GrayText, AutoSize = true };
            lblUWPError = new Label { ForeColor = Color.Red, AutoSize = true, Visible = false };
            uwpPanel.Controls.AddRange(new Control[] { txtUWP, hint, lblUWPError });
            tlp.Controls.Add(uwpPanel, 1, row++);

            // No Mainworld
            tlp.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill }, 0, row);
            chkNoMainworld = new CheckBox { Text = "No automatic mainworld selection", Dock = DockStyle.Fill };
            tlp.Controls.Add(chkNoMainworld, 1, row++);

            // Spacer
            tlp.Controls.Add(new Label(), 0, row);
            tlp.Controls.Add(new Label(), 1, row++);

            // Buttons
            btnOK     = new Button { Text = "OK",     DialogResult = DialogResult.OK,     Width = 80 };
            btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 80 };
            btnOK.Click += BtnOK_Click;

            var btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock          = DockStyle.Fill
            };
            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnOK);
            tlp.SetColumnSpan(btnPanel, 2);
            tlp.Controls.Add(btnPanel, 0, row);

            Controls.Add(tlp);
            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            // Validate seed
            if (!string.IsNullOrWhiteSpace(txtSeed.Text))
            {
                if (!int.TryParse(txtSeed.Text.Trim(), out int s))
                {
                    MessageBox.Show("Seed must be an integer.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }
                Seed = s;
            }

            // Validate UWP
            string uwpRaw = txtUWP.Text.Trim();
            if (!string.IsNullOrEmpty(uwpRaw))
            {
                string? err = ValidateUWP(uwpRaw);
                if (err != null)
                {
                    lblUWPError.Text    = err;
                    lblUWPError.Visible = true;
                    DialogResult = DialogResult.None;
                    return;
                }
                MainworldUWP = uwpRaw;
            }

            lblUWPError.Visible = false;
            SystemName  = string.IsNullOrWhiteSpace(txtName.Text) ? null : txtName.Text.Trim();
            NoMainworld = chkNoMainworld.Checked;
        }

        // ── UWP validation (mirrors Program.cs logic) ─────────────────

        private static string? ValidateUWP(string uwp)
        {
            string c = uwp.Replace(" ", "").ToUpper();
            if (c.Length < 8)  return "Too short. Minimum: A123456-7";
            if (c.Length > 12) return "Too long. Maximum: A123456-7890";
            if (!"ABCDEX".Contains(c[0])) return $"Invalid starport '{c[0]}'";
            if (!IsEhex(c[1],0,15)) return $"Invalid size '{c[1]}'";
            if (!IsEhex(c[2],0,17)) return $"Invalid atmosphere '{c[2]}'";
            if (!IsEhex(c[3],0,10)) return $"Invalid hydrographics '{c[3]}'";
            if (!IsEhex(c[4],0,12)) return $"Invalid population '{c[4]}'";
            if (!IsEhex(c[5],0,15)) return $"Invalid government '{c[5]}'";
            if (!IsEhex(c[6],0,35)) return $"Invalid law level '{c[6]}'";
            if (c[7] == '-')
            {
                if (c.Length < 9)           return "Tech level missing after '-'";
                if (!IsEhex(c[8],0,16))     return $"Invalid tech level '{c[8]}'";
            }
            else
            {
                if (!IsEhex(c[7],0,16))     return $"Invalid tech level '{c[7]}'";
            }
            return null;
        }

        private static bool IsEhex(char ch, int min, int max)
        {
            ch = char.ToUpper(ch);
            int v = char.IsDigit(ch) ? ch - '0' : ch >= 'A' && ch <= 'Z' ? ch - 'A' + 10 : -1;
            return v >= min && v <= max;
        }
    }
}
