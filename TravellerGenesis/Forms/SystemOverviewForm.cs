using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TravellerGenesis.Models;
using TravellerSystemGenerator;

namespace TravellerGenesis.Forms
{
    internal class SystemOverviewForm : Form
    {
        public GeneratedSystem GeneratedSystem { get; }
        private readonly MainForm mainForm;

        // ── Controls ─────────────────────────────────────────────────
        private TextBox txtSystemName = null!;
        private Label   lblStats      = null!;
        private DataGridView dgvStars  = null!;
        private DataGridView dgvWorlds = null!;

        // ── Column indices ────────────────────────────────────────────
        private const int StarNameCol  = 0;
        private const int WorldNameCol = 0;

        public SystemOverviewForm(GeneratedSystem gs, MainForm parent)
        {
            GeneratedSystem = gs;
            mainForm        = parent;

            Size            = new Size(1200, 700);
            StartPosition   = FormStartPosition.WindowsDefaultLocation;
            MinimumSize     = new Size(800, 500);

            BuildControls();
            PopulateData();
            RefreshTitle();
        }

        private void BuildControls()
        {
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 36, Padding = new Padding(4) };

            var lblName = new Label { Text = "System Name:", AutoSize = true, Top = 10, Left = 4 };
            txtSystemName = new TextBox { Left = 95, Top = 6, Width = 260, TabIndex = 0 };
            txtSystemName.TextChanged += TxtSystemName_TextChanged;

            lblStats = new Label { Left = 370, Top = 10, AutoSize = true, ForeColor = SystemColors.GrayText };

            topPanel.Controls.Add(lblName);
            topPanel.Controls.Add(txtSystemName);
            topPanel.Controls.Add(lblStats);

            // Stars grid
            var lblStars = new Label
            {
                Text      = "Stars",
                Dock      = DockStyle.Top,
                Height    = 20,
                Font      = new Font(Font, FontStyle.Bold),
                BackColor = SystemColors.ControlLight,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0)
            };

            dgvStars = BuildGrid();
            dgvStars.Dock = DockStyle.Fill;
            AddStarColumns(dgvStars);

            var starsPanel = new Panel { Dock = DockStyle.Top, Height = 140 };
            starsPanel.Controls.Add(dgvStars);
            starsPanel.Controls.Add(lblStars);

            var splitter = new Splitter { Dock = DockStyle.Top, Height = 4 };

            // Worlds grid
            var lblWorlds = new Label
            {
                Text      = "Objects",
                Dock      = DockStyle.Top,
                Height    = 20,
                Font      = new Font(Font, FontStyle.Bold),
                BackColor = SystemColors.ControlLight,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0)
            };

            dgvWorlds = BuildGrid();
            dgvWorlds.Dock = DockStyle.Fill;
            dgvWorlds.CellDoubleClick += DgvWorlds_CellDoubleClick;
            AddWorldColumns(dgvWorlds);

            var worldsPanel = new Panel { Dock = DockStyle.Fill };
            worldsPanel.Controls.Add(dgvWorlds);
            worldsPanel.Controls.Add(lblWorlds);

            Controls.Add(worldsPanel);
            Controls.Add(splitter);
            Controls.Add(starsPanel);
            Controls.Add(topPanel);
        }

        private static DataGridView BuildGrid()
        {
            var dgv = new DataGridView
            {
                AllowUserToAddRows       = false,
                AllowUserToDeleteRows    = false,
                RowHeadersVisible        = false,
                AutoSizeColumnsMode      = DataGridViewAutoSizeColumnsMode.None,
                SelectionMode            = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect              = false,
                EditMode                 = DataGridViewEditMode.EditOnKeystroke,
                BackgroundColor          = SystemColors.Window,
                BorderStyle              = BorderStyle.None,
                ScrollBars               = ScrollBars.Both
            };
            dgv.DefaultCellStyle.Font = new Font("Courier New", 9f);
            return dgv;
        }

        private void AddStarColumns(DataGridView dgv)
        {
            void R(string h, int w) => dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, Width = w, ReadOnly = true });
            void E(string h, int w) => dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, Width = w, ReadOnly = false });

            E("Name",        100);  // 0 editable
            R("Component",    76);
            R("Class",        76);
            R("Mass (M☉)",    76);
            R("Temp (K)",     70);
            R("Diam (D☉)",    76);
            R("Luminosity",   80);
            R("Orbit#",       56);
            R("AU",           64);
            R("Ecc",          50);
            R("Period",       90);
            R("MAO",          56);
            R("HZCO",         56);
        }

        private void AddWorldColumns(DataGridView dgv)
        {
            void R(string h, int w) => dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, Width = w, ReadOnly = true });
            void E(string h, int w) => dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, Width = w, ReadOnly = false });

            E("Name",        110);  // 0 editable
            R("Primary",      56);
            R("Object",       80);
            R("Type",        120);
            R("SAH/UWP",      90);
            R("Orbit#",       56);
            R("AU",           64);
            R("Ecc",          50);
            R("Period",       90);
            R("Sub",          40);
            R("Notes",       200);
        }

        // ── Data population ────────────────────────────────────────────

        private void PopulateData()
        {
            var snap = GeneratedSystem.Snapshot;

            // System name
            txtSystemName.Text = GeneratedSystem.Names.TryGetValue("system", out var n) ? n
                : snap.SystemName ?? "";

            // Stats
            lblStats.Text = $"Stars: {snap.Stars.Count}  |  GG: {snap.GasGiantCount}  |  Belts: {snap.PlanetoidBeltCount}  |  Worlds: {snap.TerrestrialPlanetCount}";

            // Stars grid
            dgvStars.Rows.Clear();
            foreach (var s in snap.Stars)
            {
                string nameKey = $"star:{s.Component}";
                string name = GeneratedSystem.Names.TryGetValue(nameKey, out var sn) ? sn : "";
                dgvStars.Rows.Add(
                    name,
                    s.Component,
                    s.Class,
                    s.Mass.ToString("F2"),
                    s.Temp.ToString("F0"),
                    s.Diameter.ToString("F3"),
                    s.Luminosity.ToString("F4"),
                    s.Orbit.HasValue ? s.Orbit.Value.ToString("F1") : "-",
                    s.AU.HasValue   ? s.AU.Value.ToString("F3")    : "-",
                    s.Ecc.HasValue  ? s.Ecc.Value.ToString("F2")   : "-",
                    s.Period ?? "-",
                    s.MAO.ToString("F1"),
                    s.HZCO > 0 ? s.HZCO.ToString("F1") : "-"
                );
                // Store key in tag
                dgvStars.Rows[dgvStars.Rows.Count - 1].Tag = nameKey;
            }

            // Worlds grid
            dgvWorlds.Rows.Clear();
            foreach (var w in snap.Worlds)
            {
                string nameKey = $"world:{w.Object}";
                string wname = GeneratedSystem.Names.TryGetValue(nameKey, out var wn) ? wn : "";
                // Strip HTML from notes for display
                string notes = StripHtml(w.Notes);
                dgvWorlds.Rows.Add(
                    wname,
                    w.Primary,
                    w.Object,
                    w.Type,
                    w.Size,
                    w.Orbit,
                    w.AU.ToString("F3"),
                    w.Ecc.ToString("F2"),
                    w.Period,
                    w.Sub,
                    notes
                );
                dgvWorlds.Rows[dgvWorlds.Rows.Count - 1].Tag = nameKey;
            }

            // Wire cell-edit events
            dgvStars.CellEndEdit  += DgvStars_CellEndEdit;
            dgvWorlds.CellEndEdit += DgvWorlds_CellEndEdit;
        }

        private static string StripHtml(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return System.Text.RegularExpressions.Regex.Replace(s, "<[^>]+>", "");
        }

        // ── Cell edit handlers ────────────────────────────────────────

        private void DgvStars_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != StarNameCol) return;
            string key  = (string)dgvStars.Rows[e.RowIndex].Tag!;
            string val  = dgvStars.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "";
            GeneratedSystem.Names[key] = val;
            MarkDirty();
        }

        private void DgvWorlds_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != WorldNameCol) return;
            string key  = (string)dgvWorlds.Rows[e.RowIndex].Tag!;
            string val  = dgvWorlds.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "";
            GeneratedSystem.Names[key] = val;
            MarkDirty();
        }

        private void TxtSystemName_TextChanged(object? sender, EventArgs e)
        {
            GeneratedSystem.Names["system"] = txtSystemName.Text;
            MarkDirty();
            mainForm.RefreshSessionRow(GeneratedSystem);
            RefreshTitle();
        }

        // ── Double-click routing ──────────────────────────────────────

        private void DgvWorlds_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row      = dgvWorlds.Rows[e.RowIndex];
            string obj   = row.Cells[2].Value?.ToString() ?? "";
            string type  = row.Cells[3].Value?.ToString() ?? "";
            string uwp   = row.Cells[4].Value?.ToString() ?? "";

            var snap = GeneratedSystem.Snapshot;

            // Mainworld (full UWP contains '-')
            if (uwp.Contains('-') && snap.Mainworld != null && snap.Mainworld.Population > 0)
            {
                OpenOrActivate<PopulatedWorldDetailsForm>(
                    () => new PopulatedWorldDetailsForm(GeneratedSystem, snap.Mainworld, this));
                return;
            }

            // Additional Inhabited World
            var aiw = snap.AdditionalInhabitedWorlds.FirstOrDefault(a => a.WorldDesignation == obj);
            if (aiw != null)
            {
                OpenOrActivate<InhabitedWorldForm>(
                    () => new InhabitedWorldForm(GeneratedSystem, aiw, this),
                    obj);
                return;
            }

            // Survey form (terrestrial, moon, gas giant, belt)
            var survey = snap.Surveys.FirstOrDefault(s => s.WorldName.StartsWith(obj) || s.SAH_UWP == uwp.Substring(0, Math.Min(3, uwp.Length)));
            if (survey == null) survey = snap.Surveys.FirstOrDefault(); // fallback
            if (survey != null)
            {
                OpenOrActivate<SurveyForm>(
                    () => new SurveyForm(GeneratedSystem, survey, this),
                    obj);
            }
        }

        private void OpenOrActivate<T>(Func<T> factory, string? key = null) where T : Form
        {
            foreach (Form child in MdiParent!.MdiChildren)
            {
                if (child is T t && (key == null || child.Tag?.ToString() == key))
                {
                    child.Activate();
                    return;
                }
            }
            var f = factory();
            f.Tag       = key;
            f.MdiParent = MdiParent;
            f.Show();
        }

        // ── Helpers ────────────────────────────────────────────────────

        private void MarkDirty()
        {
            GeneratedSystem.IsDirty = true;
            RefreshTitle();
        }

        public void RefreshTitle()
        {
            string dirty = GeneratedSystem.IsDirty ? " *" : "";
            Text = $"System — {GeneratedSystem.Seed} — {GeneratedSystem.DisplayName}{dirty}";
        }
    }
}
