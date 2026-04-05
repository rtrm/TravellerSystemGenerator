using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TravellerGenesis.Models;
using TravellerSystemGenerator;

namespace TravellerGenesis.Forms
{
    internal class WorldPropertiesForm : Form
    {
        private readonly GeneratedSystem gs;
        private readonly string desig;      // clean designation (no '*')
        private readonly string nameKey;    // "world:{desig}"
        private readonly SurveyData? survey;
        private readonly MainworldData? mainworld;
        private readonly AdditionalInhabitedWorld? aiw;
        private readonly SystemOverviewForm? owner;

        private TextBox txtName = null!;

        public WorldPropertiesForm(GeneratedSystem gs, string desig,
            SurveyData? survey, MainworldData? mainworld, AdditionalInhabitedWorld? aiw,
            SystemOverviewForm? owner = null)
        {
            this.gs        = gs;
            this.desig     = desig;
            this.survey    = survey;
            this.mainworld = mainworld;
            this.aiw       = aiw;
            this.owner     = owner;

            nameKey = $"world:{desig}";

            Size          = new Size(920, 820);
            StartPosition = FormStartPosition.WindowsDefaultLocation;
            MinimumSize   = new Size(640, 480);
            if (AppIcon.Get() is System.Drawing.Icon icon) Icon = icon;

            BuildControls();
            RefreshTitle();
        }

        // ── Layout ────────────────────────────────────────────────────

        private void BuildControls()
        {
            // Name bar
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 32, Padding = new Padding(4) };
            topPanel.Controls.Add(new Label { Text = "Name:", AutoSize = true, Top = 8, Left = 4 });
            txtName = new TextBox { Left = 48, Top = 4, Width = 320 };
            txtName.Text = gs.Names.TryGetValue(nameKey, out var n) ? n : "";
            txtName.TextChanged += (s, e) =>
            {
                gs.Names[nameKey] = txtName.Text;
                gs.IsDirty = true;
                RefreshTitle();
                owner?.RefreshTitle();
            };
            topPanel.Controls.Add(txtName);

            // Tab control — must be added to Controls before topPanel so Fill
            // is laid out first, leaving room for the Top-docked name bar.
            var tabs = new TabControl { Dock = DockStyle.Fill };

            if (survey != null)
                tabs.TabPages.Add(BuildPhysicalTab());

            if (mainworld != null)
                tabs.TabPages.Add(BuildSocialTab());
            else if (aiw != null)
                tabs.TabPages.Add(BuildInhabitedTab());

            Controls.Add(tabs);
            Controls.Add(topPanel);
        }

        // ── Physical Survey tab ───────────────────────────────────────

        private TabPage BuildPhysicalTab()
        {
            var tab    = new TabPage("Physical Survey");
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var tlp    = MakeTlp(200);

            AddSH(tlp, "World Identification");
            AddRow(tlp, "SAH / UWP:", survey!.SAH_UWP);
            AddRow(tlp, "Designation:", desig);

            AddSH(tlp, "Orbital Data");
            AddRow(tlp, "Orbit #:", survey.OrbitNumber.ToString("F1"));
            AddRow(tlp, "Distance (AU):", survey.AU.ToString("F3"));
            AddRow(tlp, "Eccentricity:", survey.Eccentricity.ToString("F2"));
            AddRow(tlp, "Period:", survey.Period);

            AddSH(tlp, "Size");
            AddRow(tlp, "Diameter (km):", survey.Diameter.ToString("N0"));
            AddRow(tlp, "Composition:", survey.Composition);
            AddRow(tlp, "Density:", survey.Density.ToString("F2"));
            AddRow(tlp, "Gravity (G):", survey.Gravity.ToString("F2"));
            AddRow(tlp, "Mass (M⊕):", survey.Mass.ToString("F3"));
            AddRow(tlp, "Escape Velocity:", $"{survey.EscapeVelocity:F2} km/s");

            AddSH(tlp, "Atmosphere");
            AddRow(tlp, "Atmosphere:", survey.Atmosphere);
            AddRow(tlp, "Composition:", survey.AtmosphereComposition);
            AddRow(tlp, "Pressure (bar):", survey.AtmosphericPressure.ToString("F2"));
            AddRow(tlp, "Taint:", survey.AtmosphericTaint);

            AddSH(tlp, "Hydrographics");
            AddRow(tlp, "Code:", survey.HydrographicsCode);
            AddRow(tlp, "Coverage:", $"{survey.HydrographicsCoverage:F1}%");
            AddRow(tlp, "Distribution:", survey.SurfaceDistribution);

            AddSH(tlp, "Climate & Temperature");
            AddRow(tlp, "Mean Temp:", $"{survey.MeanTemperatureC:+0;-0;0}°C  ({survey.MeanTemperatureK} K)");
            AddRow(tlp, "High Temp:", $"{survey.HighTemperatureC:+0;-0;0}°C  ({survey.HighTemperatureK} K)");
            AddRow(tlp, "Low Temp:",  $"{survey.LowTemperatureC:+0;-0;0}°C  ({survey.LowTemperatureK} K)");
            AddRow(tlp, "Albedo:", survey.Albedo.ToString("F2"));
            AddRow(tlp, "Greenhouse:", survey.Greenhouse.ToString("F2"));

            AddSH(tlp, "Rotation & Axial Tilt");
            AddRow(tlp, "Sidereal Period:", $"{survey.BasicRotationRateHours:F2} hours");
            AddRow(tlp, "Solar Day:", $"{survey.SolarDayHours:F2} hours");
            AddRow(tlp, "Solar Days/Year:", survey.SolarDaysInLocalYear.ToString("F1"));
            AddRow(tlp, "Axial Tilt:", $"{survey.AxialTilt:F1}°");
            AddRow(tlp, "Tidal Lock:", survey.TidalLockStatus);

            AddSH(tlp, "Seismology");
            AddRow(tlp, "Seismic Stress:", survey.TotalSeismicStress.ToString("F2"));
            AddRow(tlp, "Tidal Force:", $"{survey.TotalTidalForce:F4} m");
            AddRow(tlp, "Tectonic Plates:", survey.NumberOfMajorTectonicPlates.ToString());

            AddSH(tlp, "Biosphere");
            AddRow(tlp, "Biomass:", survey.BiomassRating.ToString());
            AddRow(tlp, "Biocomplexity:", $"{survey.BiocomplexityRating} — {survey.BiocomplexityDescription}");
            AddRow(tlp, "Biodiversity:", survey.BiodiversityRating.ToString());
            AddRow(tlp, "Compatibility:", survey.CompatibilityRating.ToString());
            AddRow(tlp, "Habitability:", survey.HabitabilityRating.ToString());
            AddRow(tlp, "Native Sophonts:", survey.CurrentNativeSophont);
            if (survey.ExtinctNativeSophont)
                AddRow(tlp, "Extinct Sophonts:", "Evidence of extinct sophonts");

            AddNotes(tlp, $"{nameKey}:notes");

            scroll.Controls.Add(tlp);
            tab.Controls.Add(scroll);
            return tab;
        }

        // ── Social Survey tab (mainworld) ─────────────────────────────

        private TabPage BuildSocialTab()
        {
            var mw     = mainworld!;
            var tab    = new TabPage("Social Survey");
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var tlp    = MakeTlp(220);

            AddSH(tlp, "World");
            AddRow(tlp, "UWP:", mw.UWP);
            AddRow(tlp, "Starport:", mw.Starport.ToString());

            AddSH(tlp, "Population");
            AddRow(tlp, "Population Code:", mw.Population.ToString());
            AddRow(tlp, "Total Population:", mw.ActualPopulation.ToString("N0"));
            AddRow(tlp, "PCR:", $"{mw.PCR} — {mw.PCRDescription}");
            AddRow(tlp, "Urbanisation:", $"{mw.UrbanisationPercent}%  (urban pop: {mw.TotalUrbanPopulation:N0})");
            AddRow(tlp, "Major Cities:", mw.NumberOfMajorCities.ToString());

            if (mw.MajorCities.Count > 0)
            {
                var cityGrid = new DataGridView
                {
                    Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                    RowHeadersVisible = false, ReadOnly = true, BackgroundColor = SystemColors.Window,
                    Height = Math.Min(200, mw.MajorCities.Count * 24 + 30)
                };
                cityGrid.Columns.Add("City", "City");
                cityGrid.Columns.Add("Population", "Population");
                cityGrid.Columns.Add("Pct", "% of City Pop");
                foreach (var c in mw.MajorCities)
                    cityGrid.Rows.Add(c.Name, c.Population.ToString("N0"), $"{c.PercentOfMajorCityPop:F1}%");
                tlp.SetColumnSpan(cityGrid, 2);
                tlp.Controls.Add(cityGrid);
            }

            AddSH(tlp, "Government");
            AddRow(tlp, "Government Code:", mw.Government.ToString());
            AddRow(tlp, "Type:", mw.GovernmentType);
            AddRow(tlp, "Profile:", mw.GovernmentProfile);
            AddRow(tlp, "Centralisation:", $"{mw.CentralisationCode} — {mw.CentralisationType}");
            AddRow(tlp, "Authority:", $"{mw.AuthorityCode} — {mw.AuthorityType}");
            AddRow(tlp, "Structure:", $"{mw.StructureCode} — {mw.StructureType}");

            if (gs.Snapshot.WorldFactions.Count > 0)
            {
                AddSH(tlp, "Factions");
                foreach (var f in gs.Snapshot.WorldFactions)
                    AddRow(tlp, $"Faction {f.Number}:", $"{f.StrengthType} — {f.Government.Type}  ({f.Profile})");
                if (gs.Snapshot.FactionRelationships.Count > 0)
                    AddRow(tlp, "Relationships:", string.Join("  |  ",
                        gs.Snapshot.FactionRelationships.Select(r => $"{r.Faction1Number}↔{r.Faction2Number}: {r.Type}")));
            }

            AddSH(tlp, "Law Level");
            AddRow(tlp, "Law Level:", mw.LawLevel.ToString());
            AddRow(tlp, "Profile:", mw.LawLevels.Profile);
            AddRow(tlp, "Weapons:", mw.LawLevels.WeaponsLevel.ToString());
            AddRow(tlp, "Economic:", mw.LawLevels.EconomicLevel.ToString());
            AddRow(tlp, "Criminal:", mw.LawLevels.CriminalLevel.ToString());
            AddRow(tlp, "Private:", mw.LawLevels.PrivateLevel.ToString());
            AddRow(tlp, "Personal Rights:", mw.LawLevels.PersonalRightsLevel.ToString());

            AddSH(tlp, "Tech Level");
            AddRow(tlp, "Tech Level (UWP):", mw.TechLevel.ToString());
            AddRow(tlp, "High Common TL:", mw.TechLevels.HighCommonTL.ToString());
            AddRow(tlp, "Low Common TL:", mw.TechLevels.LowCommonTL.ToString());
            AddRow(tlp, "TL Profile:", mw.TechLevels.Profile);

            AddSH(tlp, "Judicial System");
            AddRow(tlp, "System:", $"{mw.Judicial.JudicialSystemCode} — {mw.Judicial.JudicialSystemType}");
            AddRow(tlp, "Profile:", mw.Judicial.Profile);
            AddRow(tlp, "Uniformity:", mw.Judicial.UniformityType);
            AddRow(tlp, "Presumption:", mw.Judicial.PresumptionOfInnocence ? "Innocent until proven guilty" : "Guilty until proven innocent");
            AddRow(tlp, "Death Penalty:", mw.Judicial.DeathPenalty ? "Yes" : "No");

            AddSH(tlp, "Culture");
            AddRow(tlp, "Profile:", mw.Culture.Profile);
            AddRow(tlp, "Diversity:", mw.Culture.Diversity.ToString());
            AddRow(tlp, "Xenophilia:", mw.Culture.Xenophilia.ToString());
            AddRow(tlp, "Uniqueness:", mw.Culture.Uniqueness.ToString());
            AddRow(tlp, "Symbology:", mw.Culture.Symbology.ToString());
            AddRow(tlp, "Cohesion:", mw.Culture.Cohesion.ToString());
            AddRow(tlp, "Progressiveness:", mw.Culture.Progressiveness.ToString());
            AddRow(tlp, "Expansionism:", mw.Culture.Expansionism.ToString());
            AddRow(tlp, "Militancy:", mw.Culture.Militancy.ToString());

            AddSH(tlp, "Bases & Starport");
            AddRow(tlp, "Highport:", mw.HasHighport ? "Yes" : "No");
            var bases = string.Join(", ", new[] {
                mw.HasNavalBase ? "Naval" : null, mw.HasScoutBase ? "Scout" : null,
                mw.HasMilitaryBase ? "Military" : null, mw.HasCorsairBase ? "Corsair" : null
            }.Where(b => b != null));
            AddRow(tlp, "Bases:", string.IsNullOrEmpty(bases) ? "None" : bases);
            AddRow(tlp, "Berthing Fees:", mw.BerthingFees);
            AddRow(tlp, "Weekly Traffic:", mw.ExpectedWeeklyTraffic > 0 ? $"{mw.ExpectedWeeklyTraffic:N0} dt" : "-");
            if (mw.HasHighport)
            {
                AddRow(tlp, "Highport Docking:", $"{mw.HighportTotalDocking:N0} dt");
                AddRow(tlp, "Downport Docking:", $"{mw.DownportTotalDocking:N0} dt");
            }
            else if (mw.DownportTotalDocking > 0)
                AddRow(tlp, "Downport Docking:", $"{mw.DownportTotalDocking:N0} dt");
            if (mw.StarportBuildCapacity > 0)
            {
                AddRow(tlp, "Shipyard Capacity:", $"{mw.StarportBuildCapacity:N0} dt");
                AddRow(tlp, "Annual Output:", $"{mw.AnnualShipyardOutput:N0} dt");
            }

            AddSH(tlp, "Economics");
            AddRow(tlp, "Importance:", mw.Economics.Importance.ToString("+0;-0;0"));
            AddRow(tlp, "Resource Factor:", mw.Economics.ResourceFactor.ToString());
            AddRow(tlp, "Labour Factor:", mw.Economics.LabourFactor.ToString());
            AddRow(tlp, "Infrastructure:", mw.Economics.InfrastructureFactor.ToString());
            AddRow(tlp, "Efficiency:", mw.Economics.EfficiencyFactor.ToString("+0;-0;0"));
            AddRow(tlp, "Resource Units:", mw.Economics.ResourceUnits.ToString());
            AddRow(tlp, "GWP / capita:", $"{mw.Economics.GWPPerCapita:N0} Cr");
            AddRow(tlp, "Total GWP:", $"{mw.Economics.TotalGWPMCr:N2} MCr");
            AddRow(tlp, "WTN:", mw.Economics.WorldTradeNumber.ToString());
            AddRow(tlp, "Inequality:", mw.Economics.InequalityRating.ToString());
            AddRow(tlp, "Development Score:", mw.Economics.DevelopmentScore.ToString("F1"));
            AddRow(tlp, "Tariffs:", mw.Economics.Tariffs);

            AddSH(tlp, "Military");
            AddRow(tlp, "Budget:", $"{mw.Military.BasicMilitaryBudget:F2}% of GWP");
            if (mw.Military.EnforcementBranch   > 0) AddRow(tlp, "Enforcement:",   EHex(mw.Military.EnforcementBranch));
            if (mw.Military.MilitiaBranch       > 0) AddRow(tlp, "Militia:",        EHex(mw.Military.MilitiaBranch));
            if (mw.Military.ArmyBranch          > 0) AddRow(tlp, "Army:",           EHex(mw.Military.ArmyBranch));
            if (mw.Military.WetNavyBranch       > 0) AddRow(tlp, "Wet Navy:",       EHex(mw.Military.WetNavyBranch));
            if (mw.Military.AirForceBranch      > 0) AddRow(tlp, "Air Force:",      EHex(mw.Military.AirForceBranch));
            if (mw.Military.SystemDefenceBranch > 0) AddRow(tlp, "System Defence:", EHex(mw.Military.SystemDefenceBranch));
            if (mw.Military.NavyBranch          > 0) AddRow(tlp, "Navy:",           EHex(mw.Military.NavyBranch));
            if (mw.Military.MarineBranch        > 0) AddRow(tlp, "Marine:",         EHex(mw.Military.MarineBranch));

            AddSH(tlp, "Trade Codes");
            AddRow(tlp, "Codes:", mw.TradeCodes.Count > 0
                ? string.Join("  ", mw.TradeCodes.Select(t => t.Code)) : "None");

            AddNotes(tlp, $"{nameKey}:social:notes");

            scroll.Controls.Add(tlp);
            tab.Controls.Add(scroll);
            return tab;
        }

        // ── Social Survey tab (AIW secondary world) ───────────────────

        private TabPage BuildInhabitedTab()
        {
            var w      = aiw!;
            var tab    = new TabPage("Social Survey");
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var tlp    = MakeTlp(220);

            AddSH(tlp, "World");
            AddRow(tlp, "UWP:", w.UWP);
            AddRow(tlp, "Authority:", w.IsIndependent ? "Independent World" : $"Under authority of {w.AuthorityDesignation}");

            AddSH(tlp, "Population");
            AddRow(tlp, "Population Code:", w.PopulationCode.ToString());
            AddRow(tlp, "Total Population:", w.ActualPopulation.ToString("N0"));

            AddSH(tlp, "Government");
            AddRow(tlp, "Government Code:", w.GovernmentCode.ToString());
            AddRow(tlp, "Type:", w.GovernmentType);
            AddRow(tlp, "Profile:", w.GovernmentProfile);
            AddRow(tlp, "Centralisation:", $"{w.CentralisationCode} — {w.CentralisationType}");
            AddRow(tlp, "Authority:", $"{w.AuthorityCode} — {w.AuthorityType}");
            AddRow(tlp, "Structure:", $"{w.StructureCode} — {w.StructureType}");

            if (w.Factions.Count > 0)
            {
                AddSH(tlp, "Factions");
                foreach (var f in w.Factions)
                    AddRow(tlp, $"Faction {f.Number}:", $"{f.StrengthType} — {f.Government.Type}  ({f.Profile})");
                if (w.FactionRelationships.Count > 0)
                    AddRow(tlp, "Relationships:", string.Join("  |  ",
                        w.FactionRelationships.Select(r => $"{r.Faction1Number}↔{r.Faction2Number}: {r.Type}")));
            }

            AddSH(tlp, "Law Level");
            AddRow(tlp, "Law Level:", w.LawLevel.ToString());
            AddRow(tlp, "Profile:", w.LawLevels.Profile);
            AddRow(tlp, "Weapons:", w.LawLevels.WeaponsLevel.ToString());
            AddRow(tlp, "Economic:", w.LawLevels.EconomicLevel.ToString());
            AddRow(tlp, "Criminal:", w.LawLevels.CriminalLevel.ToString());
            AddRow(tlp, "Private:", w.LawLevels.PrivateLevel.ToString());
            AddRow(tlp, "Personal Rights:", w.LawLevels.PersonalRightsLevel.ToString());

            AddSH(tlp, "Tech Level");
            AddRow(tlp, "Tech Level:", w.TechLevel.ToString());
            AddRow(tlp, "High Common TL:", w.TechLevels.HighCommonTL.ToString());
            AddRow(tlp, "Low Common TL:", w.TechLevels.LowCommonTL.ToString());
            AddRow(tlp, "TL Profile:", w.TechLevels.Profile);

            AddSH(tlp, "Judicial System");
            AddRow(tlp, "System:", $"{w.Judicial.JudicialSystemCode} — {w.Judicial.JudicialSystemType}");
            AddRow(tlp, "Profile:", w.Judicial.Profile);
            AddRow(tlp, "Uniformity:", w.Judicial.UniformityType);
            AddRow(tlp, "Presumption:", w.Judicial.PresumptionOfInnocence ? "Innocent until proven guilty" : "Guilty until proven innocent");
            AddRow(tlp, "Death Penalty:", w.Judicial.DeathPenalty ? "Yes" : "No");

            AddSH(tlp, "Culture");
            AddRow(tlp, "Profile:", w.Culture.Profile);
            AddRow(tlp, "Diversity:", w.Culture.Diversity.ToString());
            AddRow(tlp, "Xenophilia:", w.Culture.Xenophilia.ToString());
            AddRow(tlp, "Uniqueness:", w.Culture.Uniqueness.ToString());
            AddRow(tlp, "Symbology:", w.Culture.Symbology.ToString());
            AddRow(tlp, "Cohesion:", w.Culture.Cohesion.ToString());
            AddRow(tlp, "Progressiveness:", w.Culture.Progressiveness.ToString());
            AddRow(tlp, "Expansionism:", w.Culture.Expansionism.ToString());
            AddRow(tlp, "Militancy:", w.Culture.Militancy.ToString());

            AddSH(tlp, "Bases & Spaceport");
            AddRow(tlp, "Spaceport:", $"{w.SpaceportClass} (equiv. Starport {w.EquivalentStarportClass})");
            AddRow(tlp, "Highport:", w.HasHighport ? "Yes" : "No");
            var bases = string.Join(", ", new[] {
                w.HasNavalBase ? "Naval" : null, w.HasScoutBase ? "Scout" : null,
                w.HasMilitaryBase ? "Military" : null, w.HasCorsairBase ? "Corsair" : null
            }.Where(b => b != null));
            AddRow(tlp, "Bases:", string.IsNullOrEmpty(bases) ? "None" : bases);
            AddRow(tlp, "Berthing Fees:", w.BerthingFees);
            AddRow(tlp, "Weekly Traffic:", w.ExpectedWeeklyTraffic > 0 ? $"{w.ExpectedWeeklyTraffic:N0} dt" : "-");
            if (w.HasHighport)
            {
                AddRow(tlp, "Highport Docking:", $"{w.HighportTotalDocking:N0} dt");
                AddRow(tlp, "Downport Docking:", $"{w.DownportTotalDocking:N0} dt");
            }
            else if (w.DownportTotalDocking > 0)
                AddRow(tlp, "Downport Docking:", $"{w.DownportTotalDocking:N0} dt");
            if (w.StarportBuildCapacity > 0)
            {
                AddRow(tlp, "Shipyard Capacity:", $"{w.StarportBuildCapacity:N0} dt");
                AddRow(tlp, "Annual Output:", $"{w.AnnualShipyardOutput:N0} dt");
            }

            AddSH(tlp, "Economics");
            AddRow(tlp, "Importance:", w.Economics.Importance.ToString("+0;-0;0"));
            AddRow(tlp, "Resource Factor:", w.Economics.ResourceFactor.ToString());
            AddRow(tlp, "Labour Factor:", w.Economics.LabourFactor.ToString());
            AddRow(tlp, "Infrastructure:", w.Economics.InfrastructureFactor.ToString());
            AddRow(tlp, "Efficiency:", w.Economics.EfficiencyFactor.ToString("+0;-0;0"));
            AddRow(tlp, "Resource Units:", w.Economics.ResourceUnits.ToString());
            AddRow(tlp, "GWP / capita:", $"{w.Economics.GWPPerCapita:N0} Cr");
            AddRow(tlp, "Total GWP:", $"{w.Economics.TotalGWPMCr:N2} MCr");
            AddRow(tlp, "WTN:", w.Economics.WorldTradeNumber.ToString());
            AddRow(tlp, "Tariffs:", w.Economics.Tariffs);

            AddSH(tlp, "Military");
            AddRow(tlp, "Budget:", $"{w.Military.BasicMilitaryBudget:F2}% of GWP");
            if (w.Military.EnforcementBranch   > 0) AddRow(tlp, "Enforcement:",   EHex(w.Military.EnforcementBranch));
            if (w.Military.MilitiaBranch       > 0) AddRow(tlp, "Militia:",        EHex(w.Military.MilitiaBranch));
            if (w.Military.ArmyBranch          > 0) AddRow(tlp, "Army:",           EHex(w.Military.ArmyBranch));
            if (w.Military.WetNavyBranch       > 0) AddRow(tlp, "Wet Navy:",       EHex(w.Military.WetNavyBranch));
            if (w.Military.AirForceBranch      > 0) AddRow(tlp, "Air Force:",      EHex(w.Military.AirForceBranch));
            if (w.Military.SystemDefenceBranch > 0) AddRow(tlp, "System Defence:", EHex(w.Military.SystemDefenceBranch));
            if (w.Military.NavyBranch          > 0) AddRow(tlp, "Navy:",           EHex(w.Military.NavyBranch));
            if (w.Military.MarineBranch        > 0) AddRow(tlp, "Marine:",         EHex(w.Military.MarineBranch));

            AddSH(tlp, "Trade Codes");
            AddRow(tlp, "Codes:", w.TradeCodes.Count > 0
                ? string.Join("  ", w.TradeCodes.Select(t => t.Code)) : "None");

            AddNotes(tlp, $"{nameKey}:social:notes");

            scroll.Controls.Add(tlp);
            tab.Controls.Add(scroll);
            return tab;
        }

        // ── Helpers ────────────────────────────────────────────────────

        private static TableLayoutPanel MakeTlp(int labelColWidth)
        {
            var tlp = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                AutoSize    = true,
                ColumnCount = 2,
                Padding     = new Padding(8)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, labelColWidth));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            return tlp;
        }

        private void AddNotes(TableLayoutPanel tlp, string key)
        {
            AddSH(tlp, "Notes");
            var txt = new TextBox
            {
                Multiline  = true,
                Height     = 80,
                Dock       = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical
            };
            txt.Text = gs.Names.TryGetValue(key, out var nt) ? nt : "";
            txt.TextChanged += (s, e) => { gs.Names[key] = txt.Text; gs.IsDirty = true; };
            tlp.SetColumnSpan(txt, 2);
            tlp.Controls.Add(txt);
        }

        private static void AddSH(TableLayoutPanel tlp, string title)
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
            tlp.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 6, 0) });
            tlp.Controls.Add(new Label { Text = value, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft });
        }

        private static string EHex(int v) =>
            v < 10 ? v.ToString() : ((char)('A' + v - 10)).ToString();

        public void RefreshTitle()
        {
            string name  = gs.Names.TryGetValue(nameKey, out var n) && !string.IsNullOrWhiteSpace(n) ? n : desig;
            string dirty = gs.IsDirty ? " *" : "";
            Text = $"{desig} — {name}{dirty}";
        }
    }
}
