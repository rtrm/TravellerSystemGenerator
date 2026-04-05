using System;
using System.Drawing;
using System.Windows.Forms;
using TravellerGenesis.Models;
using TravellerSystemGenerator;

namespace TravellerGenesis.Forms
{
    internal class PhysicalSurveyForm : Form
    {
        private readonly GeneratedSystem gs;
        private readonly SurveyData data;
        private readonly SystemOverviewForm? owner;
        private readonly MainworldData? mainworld;
        private readonly AdditionalInhabitedWorld? aiw;

        private TextBox txtName  = null!;
        private TextBox txtNotes = null!;
        private readonly string nameKey;

        private Form? _socialForm;

        public PhysicalSurveyForm(GeneratedSystem gs, SurveyData data, SystemOverviewForm? owner = null,
            MainworldData? mainworld = null, AdditionalInhabitedWorld? aiw = null)
        {
            this.gs        = gs;
            this.data      = data;
            this.owner     = owner;
            this.mainworld = mainworld;
            this.aiw       = aiw;

            nameKey = $"world:{data.WorldName.Split('<')[0].Trim()}";

            Size          = new Size(900, 780);
            StartPosition = FormStartPosition.WindowsDefaultLocation;
            AutoScroll    = true;

            BuildControls();
            RefreshTitle();
        }

        private void BuildControls()
        {
            // Name bar at top
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 32, Padding = new Padding(4) };
            topPanel.Controls.Add(new Label { Text = "Name:", AutoSize = true, Top = 8, Left = 4 });
            txtName = new TextBox { Left = 48, Top = 4, Width = 280 };
            txtName.Text = gs.Names.TryGetValue(nameKey, out var n) ? n : "";
            txtName.TextChanged += (s, e) => { gs.Names[nameKey] = txtName.Text; gs.IsDirty = true; RefreshTitle(); owner?.RefreshTitle(); };
            topPanel.Controls.Add(txtName);

            // Social Survey button for populated worlds
            if (mainworld != null || aiw != null)
            {
                var btnSocial = new Button
                {
                    Text   = "Social Survey →",
                    Left   = 340,
                    Top    = 4,
                    Width  = 120,
                    Height = 24
                };
                btnSocial.Click += BtnSocial_Click;
                topPanel.Controls.Add(btnSocial);
            }

            Controls.Add(topPanel);

            // Scrollable content
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var content = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                AutoSize    = true,
                ColumnCount = 2,
                Padding     = new Padding(8)
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // World Identification
            AddSectionHeader(content, "World Identification");
            AddRow(content, "SAH / UWP:", data.SAH_UWP);
            AddRow(content, "Designation:", data.WorldName.Split('<')[0].Trim());

            // Orbital Data
            AddSectionHeader(content, "Orbital Data");
            AddRow(content, "Orbit #:", data.OrbitNumber.ToString("F1"));
            AddRow(content, "Distance (AU):", data.AU.ToString("F3"));
            AddRow(content, "Eccentricity:", data.Eccentricity.ToString("F2"));
            AddRow(content, "Period:", data.Period);

            // Size
            AddSectionHeader(content, "Size");
            AddRow(content, "Diameter (km):", data.Diameter.ToString("N0"));
            AddRow(content, "Composition:", data.Composition);
            AddRow(content, "Density:", data.Density.ToString("F2"));
            AddRow(content, "Gravity (G):", data.Gravity.ToString("F2"));
            AddRow(content, "Mass (M⊕):", data.Mass.ToString("F3"));
            AddRow(content, "Escape Velocity:", $"{data.EscapeVelocity:F2} km/s");

            // Atmosphere
            AddSectionHeader(content, "Atmosphere");
            AddRow(content, "Atmosphere:", data.Atmosphere);
            AddRow(content, "Composition:", data.AtmosphereComposition);
            AddRow(content, "Pressure (bar):", data.AtmosphericPressure.ToString("F2"));
            AddRow(content, "Taint:", data.AtmosphericTaint);

            // Hydrographics
            AddSectionHeader(content, "Hydrographics");
            AddRow(content, "Code:", data.HydrographicsCode);
            AddRow(content, "Coverage:", $"{data.HydrographicsCoverage:F1}%");
            AddRow(content, "Distribution:", data.SurfaceDistribution);

            // Climate
            AddSectionHeader(content, "Climate & Temperature");
            AddRow(content, "Mean Temp:", $"{data.MeanTemperatureC:+0;-0;0}°C  ({data.MeanTemperatureK} K)");
            AddRow(content, "High Temp:", $"{data.HighTemperatureC:+0;-0;0}°C  ({data.HighTemperatureK} K)");
            AddRow(content, "Low Temp:",  $"{data.LowTemperatureC:+0;-0;0}°C  ({data.LowTemperatureK} K)");
            AddRow(content, "Albedo:", data.Albedo.ToString("F2"));
            AddRow(content, "Greenhouse:", data.Greenhouse.ToString("F2"));

            // Rotation
            AddSectionHeader(content, "Rotation & Axial Tilt");
            AddRow(content, "Sidereal Period:", $"{data.BasicRotationRateHours:F2} hours");
            AddRow(content, "Solar Day:", $"{data.SolarDayHours:F2} hours");
            AddRow(content, "Solar Days/Year:", data.SolarDaysInLocalYear.ToString("F1"));
            AddRow(content, "Axial Tilt:", $"{data.AxialTilt:F1}°");
            AddRow(content, "Tidal Lock:", data.TidalLockStatus);

            // Seismology
            AddSectionHeader(content, "Seismology");
            AddRow(content, "Seismic Stress:", data.TotalSeismicStress.ToString("F2"));
            AddRow(content, "Tidal Force:", $"{data.TotalTidalForce:F4} m");
            AddRow(content, "Tectonic Plates:", data.NumberOfMajorTectonicPlates.ToString());

            // Biosphere
            AddSectionHeader(content, "Biosphere");
            AddRow(content, "Biomass:", data.BiomassRating.ToString());
            AddRow(content, "Biocomplexity:", $"{data.BiocomplexityRating} — {data.BiocomplexityDescription}");
            AddRow(content, "Biodiversity:", data.BiodiversityRating.ToString());
            AddRow(content, "Compatibility:", data.CompatibilityRating.ToString());
            AddRow(content, "Habitability:", data.HabitabilityRating.ToString());
            AddRow(content, "Native Sophonts:", data.CurrentNativeSophont);
            if (data.ExtinctNativeSophont)
                AddRow(content, "Extinct Sophonts:", "Evidence of extinct sophonts");

            // Notes
            AddSectionHeader(content, "Notes");
            txtNotes = new TextBox
            {
                Multiline  = true,
                Height     = 80,
                Dock       = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical
            };
            txtNotes.Text = gs.Names.TryGetValue($"{nameKey}:notes", out var nt) ? nt : "";
            txtNotes.TextChanged += (s, e) => { gs.Names[$"{nameKey}:notes"] = txtNotes.Text; gs.IsDirty = true; };
            content.SetColumnSpan(txtNotes, 2);
            content.Controls.Add(txtNotes);

            scroll.Controls.Add(content);
            Controls.Add(scroll);
        }

        private void BtnSocial_Click(object? sender, EventArgs e)
        {
            if (_socialForm != null && !_socialForm.IsDisposed)
            {
                _socialForm.Activate();
                return;
            }
            if (mainworld != null)
                _socialForm = new SocialSurveyForm(gs, mainworld, owner);
            else if (aiw != null)
                _socialForm = new InhabitedWorldForm(gs, aiw, owner);
            else
                return;

            _socialForm.FormClosed += (s, e) => _socialForm = null;
            _socialForm.Show();
        }

        private static void AddSectionHeader(TableLayoutPanel tlp, string title)
        {
            var lbl = new Label
            {
                Text      = title,
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 220, 220),
                Height    = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0),
                Margin    = new Padding(0, 6, 0, 0)
            };
            tlp.SetColumnSpan(lbl, 2);
            tlp.Controls.Add(lbl);
        }

        private static void AddRow(TableLayoutPanel tlp, string label, string value)
        {
            tlp.Controls.Add(new Label
            {
                Text      = label,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Padding   = new Padding(0, 0, 6, 0)
            });
            tlp.Controls.Add(new Label
            {
                Text      = value,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            });
        }

        public void RefreshTitle()
        {
            string name  = gs.Names.TryGetValue(nameKey, out var n) && !string.IsNullOrWhiteSpace(n) ? n : data.WorldName.Split('<')[0].Trim();
            string dirty = gs.IsDirty ? " *" : "";
            Text = $"Physical Survey — {data.SAH_UWP} — {name}{dirty}";
        }
    }
}
