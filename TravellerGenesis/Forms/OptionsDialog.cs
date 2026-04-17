using System.Drawing;
using System.Windows.Forms;

namespace TravellerGenesis.Forms
{
    internal class OptionsDialog : Form
    {
        private readonly AppSettings settings;

        private RadioButton rdoSdi      = null!;
        private RadioButton rdoMdi      = null!;
        private CheckBox    chkBenford  = null!;

        public OptionsDialog(AppSettings settings)
        {
            this.settings = settings;

            Text            = "Options";
            Size            = new Size(420, 310);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;

            BuildControls();
        }

        private void BuildControls()
        {
            // ── Detail Windows group ──────────────────────────────────────
            var grpDetail = new GroupBox
            {
                Text   = "Detail Windows  (Physical Survey, Social Survey, Inhabited World)",
                Left   = 12, Top = 12, Width = 380, Height = 80
            };

            rdoSdi = new RadioButton
            {
                Text    = "Open as independent windows  (SDI)",
                Left    = 10, Top = 20, Width = 340, AutoSize = true,
                Checked = !settings.DetailWindowsMdi
            };
            rdoMdi = new RadioButton
            {
                Text    = "Open inside main window  (MDI)",
                Left    = 10, Top = 45, Width = 340, AutoSize = true,
                Checked = settings.DetailWindowsMdi
            };

            grpDetail.Controls.Add(rdoSdi);
            grpDetail.Controls.Add(rdoMdi);
            Controls.Add(grpDetail);

            // ── Generation group ──────────────────────────────────────────
            var grpGen = new GroupBox
            {
                Text   = "Generation",
                Left   = 12, Top = 104, Width = 380, Height = 110
            };

            chkBenford = new CheckBox
            {
                Text    = "Apply Benford's Law to population figures",
                Left    = 10, Top = 22, Width = 350, AutoSize = true,
                Checked = settings.UseBenfordsLaw
            };

            var lblBenfordNote = new Label
            {
                Text      = "When enabled, the leading digit of each population figure is re-sampled\r\n" +
                            "from Benford's distribution (log₁₀(1 + 1/d)). Not applied to populations\r\n" +
                            "below 1,000.",
                Left      = 26, Top = 46, Width = 340,
                AutoSize  = true,
                ForeColor = SystemColors.GrayText
            };

            grpGen.Controls.Add(chkBenford);
            grpGen.Controls.Add(lblBenfordNote);
            Controls.Add(grpGen);

            // ── Buttons ───────────────────────────────────────────────────
            var btnOk = new Button
            {
                Text         = "OK",
                Left         = 234, Top = 228, Width = 75,
                DialogResult = DialogResult.OK
            };
            var btnCancel = new Button
            {
                Text         = "Cancel",
                Left         = 316, Top = 228, Width = 75,
                DialogResult = DialogResult.Cancel
            };

            btnOk.Click += (s, e) =>
            {
                settings.DetailWindowsMdi = rdoMdi.Checked;
                settings.UseBenfordsLaw   = chkBenford.Checked;
                settings.Save();
            };

            Controls.Add(btnOk);
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}
