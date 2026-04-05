using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using TravellerGenesis.Models;
using TravellerSystemGenerator;

namespace TravellerGenesis.Forms
{
    internal class MainForm : Form
    {
        // ── Controls ─────────────────────────────────────────────────
        private MenuStrip menuStrip = null!;
        private ToolStrip toolStrip = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel statusLabel = null!;
        private Panel leftPanel = null!;
        private ListView sessionListView = null!;
        private Splitter splitter = null!;

        // ── Session state ────────────────────────────────────────────
        private readonly List<GeneratedSystem> sessionSystems = new();

        // ── Settings ──────────────────────────────────────────────────
        internal readonly AppSettings Settings = AppSettings.Load();

        // ── JSON options ─────────────────────────────────────────────
        private static readonly JsonSerializerOptions JsonWrite = new() { WriteIndented = true };
        private static readonly JsonSerializerOptions JsonRead  = new() { PropertyNameCaseInsensitive = true };

        public MainForm()
        {
            Text = Version.GetFullVersionString();
            Size = new Size(1280, 768);
            IsMdiContainer = true;
            StartPosition = FormStartPosition.CenterScreen;

            string iconPath = Path.Combine(AppContext.BaseDirectory, "TravellerGenesis.ico");
            if (File.Exists(iconPath))
                Icon = new System.Drawing.Icon(iconPath);

            BuildMenu();
            BuildToolStrip();
            BuildStatusStrip();
            BuildLeftPanel();
        }

        // ── Menu ─────────────────────────────────────────────────────

        private void BuildMenu()
        {
            menuStrip = new MenuStrip();

            // File menu
            var fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add("New &Random System",    null, (s, e) => NewRandomSystem());
            fileMenu.DropDownItems.Add("New from &UWP...",      null, (s, e) => NewFromUWP());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("&Open...",              null, (s, e) => OpenSystem());
            fileMenu.DropDownItems.Add("&Save",                 null, (s, e) => SaveActiveSystem());
            fileMenu.DropDownItems.Add("Save &As...",           null, (s, e) => SaveSystemAs());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("&Options...",           null, (s, e) => OpenOptions());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("E&xit",                 null, (s, e) => Close());

            // Window menu
            var windowMenu = new ToolStripMenuItem("&Window");
            windowMenu.DropDownItems.Add("&Cascade",       null, (s, e) => LayoutMdi(MdiLayout.Cascade));
            windowMenu.DropDownItems.Add("Tile &Horizontal", null, (s, e) => LayoutMdi(MdiLayout.TileHorizontal));
            windowMenu.DropDownItems.Add("Tile &Vertical", null, (s, e) => LayoutMdi(MdiLayout.TileVertical));

            // Help menu
            var helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.DropDownItems.Add("&About", null, (s, e) =>
                MessageBox.Show(
                    $"{Version.GetFullVersionString()}\n\nTraveller Star System Generator\nEngine: Traveller Genesis Core v{TravellerSystemGenerator.Version.VersionString}",
                    "About Traveller Genesis",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information));

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(windowMenu);
            menuStrip.Items.Add(helpMenu);
            menuStrip.MdiWindowListItem = windowMenu;

            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);
        }

        private void BuildToolStrip()
        {
            toolStrip = new ToolStrip();
            toolStrip.Items.Add(new ToolStripButton("New Random", null,  (s, e) => NewRandomSystem())  { ToolTipText = "Generate a new random system" });
            toolStrip.Items.Add(new ToolStripButton("New from UWP", null, (s, e) => NewFromUWP())      { ToolTipText = "Generate a system with a specified mainworld UWP" });
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("Open", null,  (s, e) => OpenSystem())             { ToolTipText = "Open a saved JSON snapshot" });
            toolStrip.Items.Add(new ToolStripButton("Save", null,  (s, e) => SaveActiveSystem())       { ToolTipText = "Save the active system" });
            Controls.Add(toolStrip);
        }

        private void BuildStatusStrip()
        {
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Ready");
            statusStrip.Items.Add(statusLabel);
            Controls.Add(statusStrip);
        }

        private void BuildLeftPanel()
        {
            leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300,
                BorderStyle = BorderStyle.FixedSingle
            };

            var header = new Label
            {
                Text = "Session Systems",
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font(Font, FontStyle.Bold),
                Padding = new Padding(4, 0, 0, 0),
                BackColor = SystemColors.ControlLight
            };

            sessionListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };
            sessionListView.Columns.Add("Seed",     60);
            sessionListView.Columns.Add("Name",    110);
            sessionListView.Columns.Add("Stars",    42);
            sessionListView.Columns.Add("GG",       36);
            sessionListView.Columns.Add("Belts",    42);
            sessionListView.Columns.Add("Worlds",   46);
            sessionListView.DoubleClick += SessionListView_DoubleClick;

            leftPanel.Controls.Add(sessionListView);
            leftPanel.Controls.Add(header);

            splitter = new Splitter { Dock = DockStyle.Left, Width = 4 };

            Controls.Add(splitter);
            Controls.Add(leftPanel);
        }

        // ── Session list helpers ──────────────────────────────────────

        private void AddToSessionList(GeneratedSystem gs)
        {
            sessionSystems.Add(gs);
            RefreshSessionRow(gs);
            UpdateStatus();
        }

        internal void RefreshSessionRow(GeneratedSystem gs)
        {
            // Find existing item or create new one
            ListViewItem? existing = null;
            foreach (ListViewItem lvi in sessionListView.Items)
                if (lvi.Tag == gs) { existing = lvi; break; }

            string[] cols =
            {
                gs.Seed.ToString(),
                gs.DisplayName,
                gs.StarCount.ToString(),
                gs.GasGiantCount.ToString(),
                gs.PlanetoidBeltCount.ToString(),
                gs.TerrestrialPlanetCount.ToString()
            };

            if (existing == null)
            {
                var item = new ListViewItem(cols) { Tag = gs };
                sessionListView.Items.Add(item);
            }
            else
            {
                for (int i = 0; i < cols.Length; i++)
                    existing.SubItems[i].Text = cols[i];
            }
        }

        private void UpdateStatus()
        {
            int n = sessionSystems.Count;
            string active = ActiveMdiChild is SystemOverviewForm sof ? $" | {sof.GeneratedSystem.DisplayName}" : "";
            statusLabel.Text = $"{n} system{(n == 1 ? "" : "s")} in session{active}";
        }

        private void SessionListView_DoubleClick(object? sender, EventArgs e)
        {
            if (sessionListView.SelectedItems.Count == 0) return;
            var gs = (GeneratedSystem)sessionListView.SelectedItems[0].Tag!;
            OpenOrActivateOverview(gs);
        }

        private void OpenOrActivateOverview(GeneratedSystem gs)
        {
            // Look for an already-open child
            foreach (Form child in MdiChildren)
                if (child is SystemOverviewForm sof && sof.GeneratedSystem == gs)
                {
                    sof.Activate();
                    return;
                }

            var form = new SystemOverviewForm(gs, this, Settings);
            form.MdiParent = this;
            form.Show();
            UpdateStatus();
        }

        // ── Actions ───────────────────────────────────────────────────

        private void OpenOptions()
        {
            using var dlg = new OptionsDialog(Settings);
            dlg.ShowDialog(this);
        }

        private void NewRandomSystem()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                var ss = new StarSystem(generateFiles: false);
                var snapshot = BuildSnapshot(ss, new Dictionary<string, string>());
                var gs = new GeneratedSystem { System = ss, Snapshot = snapshot };
                AddToSessionList(gs);
                OpenOrActivateOverview(gs);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating system: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { Cursor = Cursors.Default; }
        }

        private void NewFromUWP()
        {
            using var dlg = new NewSystemDialog();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            Cursor = Cursors.WaitCursor;
            try
            {
                var ss = new StarSystem(
                    seed: dlg.Seed,
                    mainworldUWP: dlg.MainworldUWP,
                    name: dlg.SystemName,
                    noMainworld: dlg.NoMainworld,
                    generateFiles: false);

                var names = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(dlg.SystemName))
                    names["system"] = dlg.SystemName;

                var snapshot = BuildSnapshot(ss, names);
                var gs = new GeneratedSystem { System = ss, Snapshot = snapshot, Names = names };
                AddToSessionList(gs);
                OpenOrActivateOverview(gs);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating system: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { Cursor = Cursors.Default; }
        }

        private void OpenSystem()
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Open System Snapshot",
                Filter = "JSON Snapshot (*.json)|*.json|All files (*.*)|*.*",
                InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "systems")
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var snapshot = SystemSave.Load(dlg.FileName);
                var gs = new GeneratedSystem
                {
                    Snapshot = snapshot,
                    FilePath = dlg.FileName,
                    Names = new Dictionary<string, string>(snapshot.Names)
                };
                AddToSessionList(gs);
                OpenOrActivateOverview(gs);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveActiveSystem()
        {
            if (ActiveMdiChild is SystemOverviewForm sof)
                SaveSystem(sof.GeneratedSystem, forceDialog: false);
        }

        private void SaveSystemAs()
        {
            if (ActiveMdiChild is SystemOverviewForm sof)
                SaveSystem(sof.GeneratedSystem, forceDialog: true);
        }

        internal void SaveSystem(GeneratedSystem gs, bool forceDialog)
        {
            if (gs.FilePath == null || forceDialog)
            {
                string defaultName = $"{gs.DisplayName.Replace(" ", "_").Replace("/", "-")}_{gs.Seed}.json";
                using var dlg = new SaveFileDialog
                {
                    Title = "Save System Snapshot",
                    Filter = "JSON Snapshot (*.json)|*.json|All files (*.*)|*.*",
                    FileName = defaultName,
                    InitialDirectory = EnsureSystemsFolder()
                };
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                gs.FilePath = dlg.FileName;
            }

            try
            {
                gs.Snapshot.Names = new Dictionary<string, string>(gs.Names);
                string json = JsonSerializer.Serialize(gs.Snapshot, JsonWrite);
                File.WriteAllText(gs.FilePath, json);
                gs.IsDirty = false;
                RefreshSessionRow(gs);
                // Update any open child title
                foreach (Form child in MdiChildren)
                    if (child is SystemOverviewForm sof && sof.GeneratedSystem == gs)
                        sof.RefreshTitle();
                statusLabel.Text = $"Saved: {Path.GetFileName(gs.FilePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Snapshot builder ──────────────────────────────────────────

        private static SystemSnapshot BuildSnapshot(StarSystem ss, Dictionary<string, string> names)
        {
            return new SystemSnapshot
            {
                Version                   = TravellerSystemGenerator.Version.VersionString,
                Seed                      = ss.Seed,
                SystemName                = ss.systemName,
                GeneratedAt               = DateTime.UtcNow.ToString("o"),
                Stars                     = ss.StarData,
                Worlds                    = ss.WorldData,
                Surveys                   = ss.SurveyDataList,
                Mainworld                 = ss.mainworld,
                AdditionalInhabitedWorlds = ss.additionalInhabitedWorlds,
                WorldFactions             = ss.worldFactions,
                FactionRelationships      = ss.factionRelationships,
                GasGiantCount             = ss.GasGiantCount,
                PlanetoidBeltCount        = ss.PlanetoidBeltCount,
                TerrestrialPlanetCount    = ss.TerrestrialPlanetCount,
                Names                     = names
            };
        }

        private static string EnsureSystemsFolder()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "systems");
            Directory.CreateDirectory(path);
            return path;
        }

        protected override void OnMdiChildActivate(EventArgs e)
        {
            base.OnMdiChildActivate(e);
            UpdateStatus();
        }
    }
}
