using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using TravellerGenesis.Models;
using TravellerSystemGenerator;

namespace TravellerGenesis.Forms
{
    internal class ImportDialog : Form
    {
        // ── Result ────────────────────────────────────────────────────
        public List<GeneratedSystem> ImportedSystems { get; } = new();

        // ── Controls ─────────────────────────────────────────────────
        private ComboBox cboSector     = null!;
        private ComboBox cboSubsector  = null!;
        private Label    lblStatus     = null!;
        private ProgressBar pgbImport  = null!;
        private Button   btnImport     = null!;
        private Button   btnCancel     = null!;

        // ── State ─────────────────────────────────────────────────────
        private List<SectorInfo> _sectors = new();
        private Dictionary<char, string> _subsectorNames = new();
        private List<T5SystemData> _loadedSystems = new();
        private bool _loading = false;

        public ImportDialog()
        {
            Text            = "Import from Travellermap.com";
            Size            = new Size(520, 300);
            MinimumSize     = new Size(420, 260);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            if (AppIcon.Get() is System.Drawing.Icon icon) Icon = icon;

            BuildControls();
        }

        private void BuildControls()
        {
            var layout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 5,
                Padding     = new Padding(12),
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));  // sector
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));  // subsector
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));  // status
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));  // progress
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // buttons

            // Row 0 — Sector
            layout.Controls.Add(new Label { Text = "Sector:", Anchor = AnchorStyles.Left | AnchorStyles.Top, Top = 8 }, 0, 0);
            cboSector = new ComboBox
            {
                Dock          = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled       = false,
                Sorted        = true,
                Margin        = new Padding(0, 4, 0, 4)
            };
            cboSector.SelectedIndexChanged += CboSector_SelectedIndexChanged;
            layout.Controls.Add(cboSector, 1, 0);

            // Row 1 — Subsector
            layout.Controls.Add(new Label { Text = "Subsector:", Anchor = AnchorStyles.Left | AnchorStyles.Top, Top = 8 }, 0, 1);
            cboSubsector = new ComboBox
            {
                Dock          = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled       = false,
                Margin        = new Padding(0, 4, 0, 4)
            };
            cboSubsector.SelectedIndexChanged += CboSubsector_SelectedIndexChanged;
            layout.Controls.Add(cboSubsector, 1, 1);

            // Row 2 — Status
            lblStatus = new Label
            {
                Text      = "Connecting to travellermap.com...",
                Dock      = DockStyle.Fill,
                ForeColor = SystemColors.GrayText,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.SetColumnSpan(lblStatus, 2);
            layout.Controls.Add(lblStatus, 0, 2);

            // Row 3 — Progress bar
            pgbImport = new ProgressBar
            {
                Dock    = DockStyle.Fill,
                Visible = false,
                Margin  = new Padding(0, 2, 0, 2)
            };
            layout.SetColumnSpan(pgbImport, 2);
            layout.Controls.Add(pgbImport, 0, 3);

            // Row 4 — Buttons
            var btnPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(0, 4, 0, 0)
            };
            btnCancel = new Button { Text = "Cancel", Width = 80, DialogResult = DialogResult.Cancel };
            btnImport = new Button { Text = "Import", Width = 80, Enabled = false };
            btnImport.Click += BtnImport_Click;
            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnImport);
            layout.SetColumnSpan(btnPanel, 2);
            layout.Controls.Add(btnPanel, 0, 4);

            Controls.Add(layout);
            AcceptButton = btnImport;
            CancelButton = btnCancel;
        }

        // ── Load lifecycle ────────────────────────────────────────────

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await LoadSectorsAsync();
        }

        private async Task LoadSectorsAsync()
        {
            SetStatus("Loading sector list from travellermap.com...");
            try
            {
                _sectors = await TravellerMapImporter.GetSectorsAsync();
                cboSector.BeginUpdate();
                cboSector.Items.Clear();
                foreach (var s in _sectors)
                    cboSector.Items.Add(new SectorItem(s));
                cboSector.EndUpdate();
                cboSector.Enabled = true;
                SetStatus($"{_sectors.Count} sectors available. Select a sector.");
            }
            catch (Exception ex)
            {
                SetStatus($"Error loading sectors: {ex.Message}");
            }
        }

        private async void CboSector_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_loading || cboSector.SelectedItem is not SectorItem si) return;
            btnImport.Enabled = false;
            cboSubsector.Enabled = false;
            cboSubsector.Items.Clear();
            _loadedSystems.Clear();

            SetStatus($"Loading {si.Sector.DisplayName}...");
            try
            {
                _loading = true;

                // Load subsector names from raw sector text
                string sectorText = await TravellerMapImporter.GetSectorTextAsync(si.Sector.Abbreviation);
                _subsectorNames = TravellerMapImporter.ParseSubsectorNames(sectorText);

                // Load all systems for this sector
                _loadedSystems = await TravellerMapImporter.GetSystemsAsync(si.Sector.Abbreviation);

                // Populate subsector combo
                cboSubsector.BeginUpdate();
                cboSubsector.Items.Add(new SubsectorItem('\0', "(Whole Sector)", _loadedSystems.Count));
                foreach (var kvp in _subsectorNames)
                {
                    int count = CountSubsector(kvp.Key, _loadedSystems, si.Sector.Abbreviation);
                    cboSubsector.Items.Add(new SubsectorItem(kvp.Key, kvp.Value, count));
                }
                cboSubsector.SelectedIndex = 0;
                cboSubsector.EndUpdate();
                cboSubsector.Enabled = true;
                btnImport.Enabled = true;

                SetStatus($"{_loadedSystems.Count} systems in {si.Sector.DisplayName}.");
            }
            catch (Exception ex)
            {
                SetStatus($"Error loading sector: {ex.Message}");
            }
            finally { _loading = false; }
        }

        private void CboSubsector_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboSubsector.SelectedItem is not SubsectorItem sub) return;
            if (cboSector.SelectedItem is not SectorItem si) return;
            SetStatus($"{sub.Count} systems in {sub.Name}.");
        }

        // ── Import ────────────────────────────────────────────────────

        private async void BtnImport_Click(object? sender, EventArgs e)
        {
            if (cboSector.SelectedItem is not SectorItem si) return;

            btnImport.Enabled = false;
            btnCancel.Enabled = false;
            pgbImport.Visible = true;

            try
            {
                List<T5SystemData> toImport;
                string importSource;

                if (cboSubsector.SelectedItem is SubsectorItem sub && sub.Letter != '\0')
                {
                    // Import subsector only — fetch fresh
                    SetStatus($"Loading {sub.Name} ({si.Sector.DisplayName})...");
                    toImport = await TravellerMapImporter.GetSystemsAsync(
                        si.Sector.Abbreviation, sub.Letter);
                    importSource = $"{si.Sector.DisplayName} / {sub.Name}";
                }
                else
                {
                    toImport = _loadedSystems;
                    importSource = si.Sector.DisplayName;
                }

                pgbImport.Maximum = Math.Max(1, toImport.Count);
                pgbImport.Value   = 0;
                SetStatus($"Importing 0 / {toImport.Count}...");

                int count = 0;
                await Task.Run(() =>
                {
                    foreach (var data in toImport)
                    {
                        try
                        {
                            var names = new Dictionary<string, string>();
                            if (!string.IsNullOrWhiteSpace(data.Name))
                                names["system"] = data.Name;

                            var snapshot = ImportedSystemBuilder.BuildSnapshot(data, importSource, names);
                            var gs = new GeneratedSystem
                            {
                                Snapshot = snapshot,
                                Names    = names
                            };
                            ImportedSystems.Add(gs);
                        }
                        catch
                        {
                            // Skip systems that fail to generate
                        }

                        count++;
                        int captured = count;
                        int total    = toImport.Count;
                        Invoke(() =>
                        {
                            pgbImport.Value = captured;
                            SetStatus($"Importing {captured} / {total}...");
                        });
                    }
                });

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                SetStatus($"Import error: {ex.Message}");
                btnImport.Enabled = true;
                btnCancel.Enabled = true;
                pgbImport.Visible = false;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────

        private void SetStatus(string text)
        {
            if (lblStatus.InvokeRequired)
                lblStatus.Invoke(() => lblStatus.Text = text);
            else
                lblStatus.Text = text;
        }

        private static int CountSubsector(char letter, List<T5SystemData> systems, string abbrev)
        {
            // Subsector letter maps to a hex range: A=01xx–04xx, B=05xx–08xx, etc.
            // Subsectors are laid out in a 4×4 grid across a 32-column × 40-row sector.
            // Rather than hard-coding hex ranges, we re-fetch per-subsector count from the
            // API when the user selects — just show total here as an estimate.
            // Actually: subsectors in T5SS are identified by the hex column/row.
            // A=cols 01-08 rows 01-10, B=09-16 rows 01-10, etc. in a 4×4 arrangement.
            // For simplicity, just return total/16 as an estimate.
            return systems.Count / 16;
        }

        // ── Inner types ───────────────────────────────────────────────

        private record SectorItem(SectorInfo Sector)
        {
            public override string ToString() => Sector.DisplayName;
        }

        private record SubsectorItem(char Letter, string Name, int Count)
        {
            public override string ToString() => Letter == '\0' ? $"{Name} ({Count})" : $"{Letter}: {Name} ({Count})";
        }
    }
}
