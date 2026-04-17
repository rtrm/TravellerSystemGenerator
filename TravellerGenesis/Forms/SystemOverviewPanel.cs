using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TravellerGenesis.Models;
using TravellerSystemGenerator;

namespace TravellerGenesis.Forms
{
    internal class SystemOverviewPanel : UserControl
    {
        public GeneratedSystem GeneratedSystem { get; }
        private readonly MainForm mainForm;
        private readonly AppSettings settings;

        // ── Controls ─────────────────────────────────────────────────
        private TextBox txtSystemName  = null!;
        private Label   lblStats       = null!;
        private Label   lblHex         = null!;
        private Label   lblAllegiance  = null!;
        private Label   lblTravelZone  = null!;
        private DataGridView dgvStars  = null!;
        private DataGridView dgvWorlds = null!;

        // ── Column indices ────────────────────────────────────────────
        private const int StarNameCol  = 0;
        private const int WorldNameCol = 0;

        // ── Detail window tracking ────────────────────────────────────
        private readonly Dictionary<string, Form> _openDetails = new();

        public SystemOverviewPanel(GeneratedSystem gs, MainForm parent, AppSettings settings)
        {
            GeneratedSystem = gs;
            mainForm        = parent;
            this.settings   = settings;

            Dock = DockStyle.Fill;

            BuildControls();
            PopulateData();
        }

        private void BuildControls()
        {
            // ── Top info bar (56px, DockStyle.Top) ──────────────────────────────
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(4) };
            var lblName = new Label { Text = "System Name:", AutoSize = true, Top = 10, Left = 4 };
            txtSystemName = new TextBox { Left = 120, Top = 6, Width = 240, TabIndex = 0 };
            txtSystemName.TextChanged += TxtSystemName_TextChanged;
            lblStats      = new Label { Left = 370, Top = 10, AutoSize = true, ForeColor = SystemColors.GrayText };
            lblHex        = new Label { Left = 4,   Top = 34, AutoSize = true, ForeColor = SystemColors.GrayText, Visible = false };
            lblAllegiance = new Label { Left = 100, Top = 34, AutoSize = true, ForeColor = SystemColors.GrayText, Visible = false };
            lblTravelZone = new Label { Left = 220, Top = 34, AutoSize = true, ForeColor = Color.DarkOrange,      Visible = false, Font = new Font(Font, FontStyle.Bold) };
            topPanel.Controls.Add(lblName);
            topPanel.Controls.Add(txtSystemName);
            topPanel.Controls.Add(lblStats);
            topPanel.Controls.Add(lblHex);
            topPanel.Controls.Add(lblAllegiance);
            topPanel.Controls.Add(lblTravelZone);

            // ── Stars section: heading + grid inside a fixed-height Top panel ──
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
            // In WinForms, the LAST control added with DockStyle.Top appears at the top.
            starsPanel.Controls.Add(dgvStars);   // Fill  — added first → fills remaining
            starsPanel.Controls.Add(lblStars);   // Top   — added last  → docks at top

            var splitter = new Splitter { Dock = DockStyle.Top, Height = 4 };

            // ── Worlds section: heading + grid inside a Fill panel ────────────
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
            dgvWorlds.Dock             = DockStyle.Fill;
            dgvWorlds.ShowCellToolTips = true;
            dgvWorlds.CellDoubleClick += DgvWorlds_CellDoubleClick;
            dgvWorlds.CellToolTipTextNeeded += DgvWorlds_CellToolTipTextNeeded;
            AddWorldColumns(dgvWorlds);

            var worldsPanel = new Panel { Dock = DockStyle.Fill };
            worldsPanel.Controls.Add(dgvWorlds);   // Fill — added first
            worldsPanel.Controls.Add(lblWorlds);   // Top  — added last → docks at top

            // ── Add to UserControl in reverse visual order ────────────────────
            // Last-added DockStyle.Top control appears at the top of the panel.
            Controls.Add(worldsPanel);   // Fill → gets all space below Top controls
            Controls.Add(splitter);      // Top  → below starsPanel
            Controls.Add(starsPanel);    // Top  → below topPanel
            Controls.Add(topPanel);      // Top  → added last = appears at very top
        }

        private static DataGridView BuildGrid()
        {
            var dgv = new DataGridView
            {
                AllowUserToAddRows          = false,
                AllowUserToDeleteRows       = false,
                RowHeadersVisible           = false,
                AutoSizeColumnsMode         = DataGridViewAutoSizeColumnsMode.None,
                SelectionMode               = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect                 = false,
                EditMode                    = DataGridViewEditMode.EditOnKeystroke,
                BackgroundColor             = SystemColors.Window,
                BorderStyle                 = BorderStyle.None,
                ScrollBars                  = ScrollBars.Both,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing
            };

            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            // Data cells: larger Courier New for readability
            dgv.DefaultCellStyle.Font = new Font("Courier New", 10f);

            return dgv;
        }

        private void AddStarColumns(DataGridView dgv)
        {
            void R(string h, int w) => dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, Width = w, ReadOnly = true });
            void E(string h, int w) => dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, Width = w, ReadOnly = false });

            E("Name",        110);  // 0 editable
            R("Component",    80);
            R("Class",        76);
            R("Mass (M☉)",    82);
            R("Temp (K)",     72);
            R("Diam (D☉)",    82);
            R("Luminosity",   88);
            R("Orbit#",       60);
            R("AU",           64);
            R("Ecc",          52);
            R("Period",       90);
            R("MAO",          56);
            R("HZCO",         60);
        }

        private void AddWorldColumns(DataGridView dgv)
        {
            void R(string h, int w) => dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, Width = w, ReadOnly = true });
            void E(string h, int w) => dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, Width = w, ReadOnly = false });

            E("Name",        120);  // 0 editable
            R("Primary",      68);
            R("Object",       84);
            R("Type",        126);
            R("SAH/UWP",      92);
            R("Orbit#",       60);
            R("AU",           64);
            R("Ecc",          52);
            R("Period",       90);
            R("Sub",          44);
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

            // Import metadata labels
            if (!string.IsNullOrWhiteSpace(snap.HexLocation))
            {
                lblHex.Text    = $"Hex: {snap.HexLocation}";
                lblHex.Visible = true;
            }
            if (!string.IsNullOrWhiteSpace(snap.Allegiance))
            {
                lblAllegiance.Text    = $"Alleg: {snap.Allegiance}";
                lblAllegiance.Visible = true;
            }
            if (!string.IsNullOrWhiteSpace(snap.TravelZone))
            {
                lblTravelZone.Text      = snap.TravelZone == "R" ? "Zone: Red" : "Zone: Amber";
                lblTravelZone.ForeColor = snap.TravelZone == "R" ? Color.Red : Color.DarkOrange;
                lblTravelZone.Visible   = true;
            }

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
                dgvStars.Rows[dgvStars.Rows.Count - 1].Tag = nameKey;
            }

            // Worlds grid
            dgvWorlds.Rows.Clear();
            var existingMoonRows = new HashSet<string>(snap.Worlds
                .Where(w => w.Type == "Moon")
                .Select(w => w.Object.TrimEnd('*')));

            foreach (var w in snap.Worlds)
            {
                string nameKey = $"world:{w.Object}";
                string wname = GeneratedSystem.Names.TryGetValue(nameKey, out var wn) ? wn : "";
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

                // Add rows only for populated moons (AIW) not already present as their own row
                string parentClean = w.Object.TrimEnd('*');
                foreach (var moon in w.Moons.Where(m => m.Size != "R"))
                {
                    string moonObj = $"{parentClean} {moon.Designation}";
                    if (existingMoonRows.Contains(moonObj)) continue; // mainworld moon already has its own row
                    bool isPopulated = snap.AdditionalInhabitedWorlds.Any(a => a.WorldDesignation == moonObj);
                    if (!isPopulated) continue;

                    string moonKey    = $"world:{moonObj}";
                    string moonName   = GeneratedSystem.Names.TryGetValue(moonKey, out var mn) ? mn : "";
                    string moonSah    = moon.Size + moon.Atmosphere + moon.HydrographicsCode;
                    float  moonAU     = moon.OrbitDistanceKm / 149597870.9f;
                    string moonPeriod = FormatMoonPeriod(moon.OrbitalPeriod);
                    dgvWorlds.Rows.Add(
                        moonName,
                        parentClean,
                        moonObj,
                        "Moon",
                        moonSah,
                        moon.Orbit.ToString("F2"),
                        moonAU.ToString("F4"),
                        moon.Eccentricity.ToString("F2"),
                        moonPeriod,
                        "",
                        ""
                    );
                    dgvWorlds.Rows[dgvWorlds.Rows.Count - 1].Tag = moonKey;
                }
            }

            // Wire cell-edit and Notes-click events
            dgvStars.CellEndEdit  += DgvStars_CellEndEdit;
            dgvWorlds.CellEndEdit += DgvWorlds_CellEndEdit;
            dgvWorlds.CellClick   += DgvWorlds_NotesCellClick;
        }

        private static string StripHtml(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return System.Text.RegularExpressions.Regex.Replace(s, "<[^>]+>", "");
        }

        private static string FormatMoonPeriod(float hours)
        {
            if (hours < 24f)   return $"{hours:F1}h";
            float days = hours / 24f;
            if (days < 365f)   return $"{days:F1}d";
            return $"{days / 365.25f:F2}y";
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

        private const int NotesCol = 10;

        private void DgvWorlds_NotesCellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != NotesCol) return;

            var row      = dgvWorlds.Rows[e.RowIndex];
            string obj   = row.Cells[2].Value?.ToString() ?? "";
            string objClean = obj.TrimEnd('*');

            var snap = GeneratedSystem.Snapshot;
            var wd = snap.Worlds.FirstOrDefault(w => w.Object.TrimEnd('*') == objClean);
            if (wd == null) return;

            var moons = wd.Moons.Where(m => m.Size != "R").ToList();
            if (moons.Count == 0) return;

            var menu = new ContextMenuStrip();
            foreach (var moon in moons)
            {
                string moonObj = $"{objClean} {moon.Designation}";
                var item = new ToolStripMenuItem(moonObj);
                item.Click += (s, ev) =>
                {
                    var survey = snap.Surveys.FirstOrDefault(sd => MatchesSurvey(sd.WorldName, moonObj));
                    if (survey != null)
                        OpenOrActivate($"{moonObj}:properties",
                            () => new WorldPropertiesForm(GeneratedSystem, moonObj, survey, null,
                                snap.AdditionalInhabitedWorlds.FirstOrDefault(a => a.WorldDesignation == moonObj), this));
                };
                menu.Items.Add(item);
            }

            var cellRect = dgvWorlds.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            menu.Show(dgvWorlds, new System.Drawing.Point(cellRect.Left, cellRect.Bottom));
        }

        private const int TypeCol  = 3;
        private const int SahCol   = 4;

        private const string TT_GG_TYPE =
            "Gas Giant size codes:\n" +
            "GS — Small  (< 50,000 km diameter)\n" +
            "GM — Medium (50,000 – 100,000 km)\n" +
            "GL — Large  (> 100,000 km)";

        private const string TT_BELT_NOTES =
            "Belt profile format:\n" +
            "Span – Cm.Cs.Cc.Co – Bulk – Resources – Bodies\n\n" +
            "Span:    distance across the belt (AU)\n" +
            "Cm:      Metallic composition %\n" +
            "Cs:      Silicate composition %\n" +
            "Cc:      Carbonaceous composition %\n" +
            "Co:      Other composition %\n" +
            "Bulk:    total belt mass (eHex)\n" +
            "Resources: resource rating (eHex)\n" +
            "Bodies:  size-1 count . size-S count (eHex)";

        private const string TT_UWP =
            "UWP format:  Starport Size Atm Hyd Pop Gov Law – TL\n\n" +
            "Starport:  A (best) → E (frontier) → X (none)\n" +
            "Size:      0 (asteroid) → F (8,000 km)\n" +
            "Atm:       0 (none) → F (exotic)\n" +
            "Hyd:       0 (desert) → A (water world)\n" +
            "Pop:       0 (uninhabited) → A+ (billions)\n" +
            "Gov:       0 (none) → F (totalitarian)\n" +
            "Law:       0 (no law) → F (all weapons banned)\n" +
            "TL:        0 (stone age) → F+ (advanced)";

        private const string TT_SAH =
            "SAH:  Size  Atmosphere  Hydrographics\n\n" +
            "Size:  0 (asteroid) → F (8,000 km)\n" +
            "Atm:   0 (none/vacuum) → F (exotic)\n" +
            "Hyd:   0 (dry) → A (water world)";

        private void DgvWorlds_CellToolTipTextNeeded(object? sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string type = dgvWorlds.Rows[e.RowIndex].Cells[TypeCol].Value?.ToString() ?? "";
            bool isGG    = type.Contains("Gas Giant");
            bool isBelt  = type.Contains("Planetoid Belt") || type.Contains("Belt");
            bool isMW    = dgvWorlds.Rows[e.RowIndex].Cells[2].Value?.ToString()?.EndsWith('*') == true;

            if (e.ColumnIndex == TypeCol && isGG)
                e.ToolTipText = TT_GG_TYPE;
            else if (e.ColumnIndex == NotesCol && isBelt)
                e.ToolTipText = TT_BELT_NOTES;
            else if (e.ColumnIndex == SahCol && isMW)
                e.ToolTipText = TT_UWP;
            else if (e.ColumnIndex == SahCol && !isGG && !isBelt)
                e.ToolTipText = TT_SAH;
        }

        private void TxtSystemName_TextChanged(object? sender, EventArgs e)
        {
            GeneratedSystem.Names["system"] = txtSystemName.Text;
            MarkDirty();
            mainForm.RefreshSessionRow(GeneratedSystem);
        }

        // ── Double-click routing ──────────────────────────────────────

        private void DgvWorlds_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row      = dgvWorlds.Rows[e.RowIndex];
            string obj   = row.Cells[2].Value?.ToString() ?? "";   // designation (mainworld has trailing '*')
            string objClean = obj.TrimEnd('*');

            var snap = GeneratedSystem.Snapshot;

            // Physical survey data
            var survey = snap.Surveys.FirstOrDefault(s => MatchesSurvey(s.WorldName, objClean));

            // Social data: '*' suffix marks the mainworld; otherwise check AIWs
            MainworldData? mw = null;
            AdditionalInhabitedWorld? aiw = null;
            if (obj.EndsWith('*') && snap.Mainworld?.Population > 0)
                mw = snap.Mainworld;
            else
                aiw = snap.AdditionalInhabitedWorlds.FirstOrDefault(a => a.WorldDesignation == objClean);

            if (survey != null || mw != null || aiw != null)
                OpenOrActivate($"{objClean}:properties",
                    () => new WorldPropertiesForm(GeneratedSystem, objClean, survey, mw, aiw, this));
        }

        // ── Survey name matching ──────────────────────────────────────

        // WorldName can be:
        //   "A III"                            plain designation
        //   "Sol (A III)"                      mainworld with system name
        //   "A III a"                          moon
        //   "A II<br>Independent World"        AIW — HTML appended in post-processing
        private static bool MatchesSurvey(string worldName, string desig)
        {
            // Strip HTML suffix appended for AIW worlds (e.g. "<br>Independent World")
            int br = worldName.IndexOf('<');
            string clean = br >= 0 ? worldName.Substring(0, br).Trim() : worldName;

            // Strip leading "SystemName (" and trailing ")" for mainworld-with-name form
            int paren = clean.IndexOf('(');
            if (paren >= 0 && clean.EndsWith(")"))
                clean = clean.Substring(paren + 1, clean.Length - paren - 2).Trim();

            return clean == desig || clean.StartsWith(desig + " ");
        }

        // ── Window management ─────────────────────────────────────────

        private void OpenOrActivate(string key, Func<Form> factory)
        {
            if (_openDetails.TryGetValue(key, out var existing) && !existing.IsDisposed)
            {
                existing.Activate();
                return;
            }
            var f = factory();
            f.FormClosed += (s, e) => _openDetails.Remove(key);
            _openDetails[key] = f;
            f.Show(mainForm);
        }

        // ── Helpers ────────────────────────────────────────────────────

        private void MarkDirty()
        {
            GeneratedSystem.IsDirty = true;
            mainForm.NotifySystemChanged(GeneratedSystem);
        }

        public void RefreshTitle()
        {
            // Nothing to do on the control itself — MainForm owns the title
            mainForm.NotifySystemChanged(GeneratedSystem);
        }
    }
}
