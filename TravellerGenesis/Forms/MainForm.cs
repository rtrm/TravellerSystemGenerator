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
        private Label leftPanelHeader = null!;
        private ListView sessionListView = null!;
        private Splitter splitter = null!;
        private Panel rightPanel = null!;

        // ── Session state ────────────────────────────────────────────
        private readonly List<GeneratedSystem> sessionSystems = new();
        private readonly Dictionary<GeneratedSystem, SystemOverviewPanel> _panels = new();
        private SystemOverviewPanel? _activePanel;

        // ── Settings ──────────────────────────────────────────────────
        internal readonly AppSettings Settings = AppSettings.Load();

        // ── JSON options ─────────────────────────────────────────────
        private static readonly JsonSerializerOptions JsonWrite = new() { WriteIndented = true };
        private static readonly JsonSerializerOptions JsonRead  = new() { PropertyNameCaseInsensitive = true };

        public MainForm()
        {
            Text = Version.GetFullVersionString();
            Size = new Size(1280, 768);
            StartPosition = FormStartPosition.CenterScreen;

            if (AppIcon.Get() is System.Drawing.Icon icon) Icon = icon;

            // Add order matters: dock layout processes back-to-front (last added first).
            // Menu and toolbar must be added LAST so they claim full-width rows at the
            // top of the form before the left panel claims x=0..300.
            BuildRightPanel();
            BuildLeftPanel();
            BuildStatusStrip();
            BuildToolStrip();    // Top — processed 2nd → below menu
            BuildMenu();         // Top — added last → processed 1st → at very top

            // Begin fetching the travellermap.com sector list in the background
            TravellerMapImporter.BeginPrefetchSectors();
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
            fileMenu.DropDownItems.Add("&Import from Travellermap...", null, (s, e) => ImportFromTravellermap());
            fileMenu.DropDownItems.Add("&Save",                 null, (s, e) => SaveActiveSystem());
            fileMenu.DropDownItems.Add("Save &As...",           null, (s, e) => SaveSystemAs());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("&Options...",           null, (s, e) => OpenOptions());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("E&xit",                 null, (s, e) => Close());

            // Help menu
            var helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.DropDownItems.Add("&About", null, (s, e) =>
                MessageBox.Show(
                    $"{Version.GetFullVersionString()}\n\nTraveller Star System Generator\nEngine: Traveller Genesis Core v{TravellerSystemGenerator.Version.VersionString}",
                    "About Traveller Genesis",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information));

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(helpMenu);

            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);
        }

        private void BuildToolStrip()
        {
            toolStrip = new ToolStrip { ImageScalingSize = new Size(32, 32), Dock = DockStyle.Top };

            toolStrip.Items.Add(new ToolStripButton("New Random",   LoadIcon("Random"),   (s, e) => NewRandomSystem())       { ToolTipText = "Generate a new random system",                  TextImageRelation = TextImageRelation.ImageAboveText, DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });
            toolStrip.Items.Add(new ToolStripButton("New from UWP", LoadIcon("UWP"),      (s, e) => NewFromUWP())            { ToolTipText = "Generate a system with a specified mainworld UWP", TextImageRelation = TextImageRelation.ImageAboveText, DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("Open",         LoadIcon("Open"),     (s, e) => OpenSystem())            { ToolTipText = "Open a saved JSON snapshot",                    TextImageRelation = TextImageRelation.ImageAboveText, DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });
            toolStrip.Items.Add(new ToolStripButton("Import",       LoadIcon("Download"), (s, e) => ImportFromTravellermap()) { ToolTipText = "Import systems from travellermap.com",           TextImageRelation = TextImageRelation.ImageAboveText, DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });
            toolStrip.Items.Add(new ToolStripButton("Save",         LoadIcon("Save"),     (s, e) => SaveActiveSystem())      { ToolTipText = "Save the active system",                        TextImageRelation = TextImageRelation.ImageAboveText, DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });

            Controls.Add(toolStrip);
        }

        private static Image? LoadIcon(string name)
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            string resourceName = $"TravellerGenesis.{name}.png";
            using var stream = asm.GetManifestResourceStream(resourceName);
            if (stream == null) return null;
            return Image.FromStream(stream);
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

            leftPanelHeader = new Label
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
                MultiSelect = false,
                ShowGroups = true
            };
            sessionListView.Columns.Add("Seed",     60);
            sessionListView.Columns.Add("Name",    110);
            sessionListView.Columns.Add("Stars",    42);
            sessionListView.Columns.Add("GG",       36);
            sessionListView.Columns.Add("Belts",    42);
            sessionListView.Columns.Add("Worlds",   46);
            sessionListView.SelectedIndexChanged += SessionListView_SelectedIndexChanged;

            var btnDelete = new Button
            {
                Text    = "Clear",
                Dock    = DockStyle.Left,
                Width   = 80,
                Height  = 28
            };
            btnDelete.Click += (s, e) => DeleteSelectedSystem();

            var btnClearAll = new Button
            {
                Text    = "Clear All",
                Dock    = DockStyle.Left,
                Width   = 80,
                Height  = 28
            };
            btnClearAll.Click += (s, e) => ClearAllSystems();

            var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 32, Padding = new Padding(2) };
            btnPanel.Controls.Add(btnClearAll);
            btnPanel.Controls.Add(btnDelete);

            leftPanel.Controls.Add(sessionListView);
            leftPanel.Controls.Add(btnPanel);
            leftPanel.Controls.Add(leftPanelHeader);

            splitter = new Splitter { Dock = DockStyle.Left, Width = 4 };

            Controls.Add(splitter);
            Controls.Add(leftPanel);
        }

        private void BuildRightPanel()
        {
            rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None
            };
            Controls.Add(rightPanel);
        }

        // ── Session list helpers ──────────────────────────────────────

        private void DeleteSelectedSystem()
        {
            if (sessionListView.SelectedItems.Count == 0) return;
            var item = sessionListView.SelectedItems[0];
            var gs   = (GeneratedSystem)item.Tag!;

            sessionSystems.Remove(gs);
            _panels.Remove(gs);
            sessionListView.Items.Remove(item);

            if (_activePanel?.GeneratedSystem == gs)
            {
                _activePanel = null;
                rightPanel.Controls.Clear();
            }

            UpdateStatus();
        }

        private void ClearAllSystems()
        {
            if (sessionSystems.Count == 0) return;
            if (MessageBox.Show("Remove all systems from the session?", "Clear All",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            sessionSystems.Clear();
            _panels.Clear();
            sessionListView.Items.Clear();
            sessionListView.Groups.Clear();
            _activePanel = null;
            rightPanel.Controls.Clear();
            UpdateStatus();
        }

        private void AddToSessionList(GeneratedSystem gs)
        {
            sessionSystems.Add(gs);
            RefreshSessionRow(gs);
            ShowSystemPanel(gs);
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
                var item = new ListViewItem(cols) { Tag = gs, Group = GetOrCreateGroup(gs) };
                sessionListView.Items.Add(item);
                // Select the newly added item
                item.Selected = true;
                item.EnsureVisible();
            }
            else
            {
                for (int i = 0; i < cols.Length; i++)
                    existing.SubItems[i].Text = cols[i];
            }
        }

        private void RefreshLeftPanelHeader()
        {
            if (sessionListView.Groups.Count == 1)
                leftPanelHeader.Text = sessionListView.Groups[0].Header;
            else
                leftPanelHeader.Text = "Session Systems";
        }

        private ListViewGroup GetOrCreateGroup(GeneratedSystem gs)
        {
            string groupKey = gs.Snapshot.ImportSource ?? "Session";
            string groupHeader = groupKey == "Session" ? "Session" : groupKey;

            foreach (ListViewGroup g in sessionListView.Groups)
                if (g.Name == groupKey) return g;

            var group = new ListViewGroup(groupKey, groupHeader);
            sessionListView.Groups.Add(group);
            return group;
        }

        private void UpdateStatus()
        {
            int n = sessionSystems.Count;
            if (_activePanel != null)
            {
                var gs = _activePanel.GeneratedSystem;
                string dirty = gs.IsDirty ? " *" : "";
                string name = $"System — {gs.Seed} — {gs.DisplayName}{dirty}";
                statusLabel.Text = $"{n} system{(n == 1 ? "" : "s")} in session  |  {name}";
            }
            else
            {
                statusLabel.Text = $"{n} system{(n == 1 ? "" : "s")} in session";
            }
        }

        // Called by SystemOverviewPanel when data changes
        internal void NotifySystemChanged(GeneratedSystem gs)
        {
            RefreshSessionRow(gs);
            UpdateStatus();
        }

        private void SessionListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (sessionListView.SelectedItems.Count == 0) return;
            var gs = (GeneratedSystem)sessionListView.SelectedItems[0].Tag!;
            ShowSystemPanel(gs);
        }

        private void ShowSystemPanel(GeneratedSystem gs)
        {
            // Reuse existing panel for this system
            if (!_panels.TryGetValue(gs, out var panel))
            {
                panel = new SystemOverviewPanel(gs, this, Settings);
                _panels[gs] = panel;
            }

            if (_activePanel == panel) return;

            // Swap the panel
            rightPanel.SuspendLayout();
            rightPanel.Controls.Clear();
            rightPanel.Controls.Add(panel);
            rightPanel.ResumeLayout();

            _activePanel = panel;
            UpdateStatus();

            // Sync list selection
            foreach (ListViewItem lvi in sessionListView.Items)
                if (lvi.Tag == gs && !lvi.Selected) { lvi.Selected = true; break; }
        }

        // ── Actions ───────────────────────────────────────────────────

        private void OpenOptions()
        {
            using var dlg = new OptionsDialog(Settings);
            dlg.ShowDialog(this);
        }

        private void ImportFromTravellermap()
        {
            using var dlg = new ImportDialog();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            Cursor = Cursors.WaitCursor;
            try
            {
                foreach (var gs in dlg.ImportedSystems)
                {
                    MaybeApplyBenford(gs);
                    AddToSessionList(gs);
                }
                if (dlg.ImportedSystems.Count > 0)
                    statusLabel.Text = $"Imported {dlg.ImportedSystems.Count} system{(dlg.ImportedSystems.Count == 1 ? "" : "s")}.";
                RefreshLeftPanelHeader();
            }
            finally { Cursor = Cursors.Default; }
        }

        private void NewRandomSystem()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                var ss = new StarSystem(generateFiles: false);
                var snapshot = BuildSnapshot(ss, new Dictionary<string, string>());
                var gs = new GeneratedSystem { System = ss, Snapshot = snapshot };
                MaybeApplyBenford(gs);
                AddToSessionList(gs);
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
                MaybeApplyBenford(gs);
                AddToSessionList(gs);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveActiveSystem()
        {
            if (_activePanel != null)
                SaveSystem(_activePanel.GeneratedSystem, forceDialog: false);
        }

        private void SaveSystemAs()
        {
            if (_activePanel != null)
                SaveSystem(_activePanel.GeneratedSystem, forceDialog: true);
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
                UpdateStatus();
                statusLabel.Text = $"Saved: {Path.GetFileName(gs.FilePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Benford's Law post-processor ─────────────────────────────

        private void MaybeApplyBenford(GeneratedSystem gs)
        {
            if (!Settings.UseBenfordsLaw) return;
            // Use a seed derived from the system seed for reproducibility
            var rng  = new Random(gs.Seed ^ 0x42BEFF);
            var snap = gs.Snapshot;

            if (snap.Mainworld != null)
            {
                snap.Mainworld.ActualPopulation    = BenfordsLaw.Apply(snap.Mainworld.ActualPopulation, rng);
                snap.Mainworld.TotalUrbanPopulation = BenfordsLaw.Apply(snap.Mainworld.TotalUrbanPopulation, rng);
                foreach (var city in snap.Mainworld.MajorCities)
                    city.Population = BenfordsLaw.Apply(city.Population, rng);
                // Re-sum city total to stay consistent
                snap.Mainworld.MajorCityPopulation = 0;
                foreach (var city in snap.Mainworld.MajorCities)
                    snap.Mainworld.MajorCityPopulation += city.Population;
            }

            foreach (var aiw in snap.AdditionalInhabitedWorlds)
                aiw.ActualPopulation = BenfordsLaw.Apply(aiw.ActualPopulation, rng);
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
    }
}
