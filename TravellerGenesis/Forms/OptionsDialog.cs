using System.Drawing;
using System.Windows.Forms;

namespace TravellerGenesis.Forms
{
    internal class OptionsDialog : Form
    {
        private readonly AppSettings settings;

        private RadioButton rdoSdi = null!;
        private RadioButton rdoMdi = null!;

        public OptionsDialog(AppSettings settings)
        {
            this.settings = settings;

            Text            = "Options";
            Size            = new Size(380, 200);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;

            BuildControls();
        }

        private void BuildControls()
        {
            var grpDetail = new GroupBox
            {
                Text    = "Detail Windows  (Physical Survey, Social Survey, Inhabited World)",
                Left    = 12, Top = 12, Width = 338, Height = 80
            };

            rdoSdi = new RadioButton
            {
                Text    = "Open as independent windows  (SDI)",
                Left    = 10, Top = 20, Width = 300, AutoSize = true,
                Checked = !settings.DetailWindowsMdi
            };
            rdoMdi = new RadioButton
            {
                Text    = "Open inside main window  (MDI)",
                Left    = 10, Top = 45, Width = 300, AutoSize = true,
                Checked = settings.DetailWindowsMdi
            };

            grpDetail.Controls.Add(rdoSdi);
            grpDetail.Controls.Add(rdoMdi);
            Controls.Add(grpDetail);

            var btnOk = new Button
            {
                Text         = "OK",
                Left         = 196, Top = 120, Width = 75,
                DialogResult = DialogResult.OK
            };
            var btnCancel = new Button
            {
                Text         = "Cancel",
                Left         = 278, Top = 120, Width = 75,
                DialogResult = DialogResult.Cancel
            };

            btnOk.Click += (s, e) =>
            {
                settings.DetailWindowsMdi = rdoMdi.Checked;
                settings.Save();
            };

            Controls.Add(btnOk);
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}
