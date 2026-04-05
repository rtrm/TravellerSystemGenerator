using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TravellerGenesis.Models;
using TravellerSystemGenerator;

namespace TravellerGenesis.Forms
{
    internal class SocialSurveyForm : Form
    {
        private readonly GeneratedSystem gs;
        private readonly MainworldData mw;
        private readonly SystemOverviewForm? owner;

        private TextBox txtName  = null!;
        private TextBox txtNotes = null!;
        private readonly string nameKey = "world:mainworld";

        public SocialSurveyForm(GeneratedSystem gs, MainworldData mw, SystemOverviewForm? owner = null)
        {
            this.gs    = gs;
            this.mw    = mw;
            this.owner = owner;

            Size          = new Size(900, 850);
            StartPosition = FormStartPosition.WindowsDefaultLocation;
            AutoScroll    = true;

            BuildControls();
            RefreshTitle();
        }

        private void BuildControls()
        {
            // Name bar
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 32, Padding = new Padding(4) };
            topPanel.Controls.Add(new Label { Text = "Name:", AutoSize = true, Top = 8, Left = 4 });
            txtName = new TextBox { Left = 48, Top = 4, Width = 300 };
            txtName.Text = gs.Names.TryGetValue("system", out var sn) ? sn
                         : gs.Snapshot.SystemName ?? "";
            txtName.TextChanged += (s, e) =>
            {
                gs.Names["system"] = txtName.Text;
                gs.IsDirty = true;
                RefreshTitle();
                owner?.RefreshTitle();
            };
            topPanel.Controls.Add(txtName);
            Controls.Add(topPanel);

            var scroll  = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var content = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                AutoSize    = true,
                ColumnCount = 2,
                Padding     = new Padding(8)
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // World
            AddSH(content, "World");
            AddRow(content, "UWP:", mw.UWP);
            AddRow(content, "Starport:", mw.Starport.ToString());

            // Population
            AddSH(content, "Population");
            AddRow(content, "Population Code:", mw.Population.ToString());
            AddRow(content, "Total Population:", mw.ActualPopulation.ToString("N0"));
            AddRow(content, "PCR:", $"{mw.PCR} — {mw.PCRDescription}");
            AddRow(content, "Urbanisation:", $"{mw.UrbanisationPercent}%  (urban pop: {mw.TotalUrbanPopulation:N0})");
            AddRow(content, "Major Cities:", mw.NumberOfMajorCities.ToString());

            // Major cities list
            if (mw.MajorCities.Count > 0)
            {
                var cityGrid = new DataGridView
                {
                    Dock               = DockStyle.Fill,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    RowHeadersVisible  = false,
                    Height             = Math.Min(200, mw.MajorCities.Count * 24 + 30),
                    ReadOnly           = true,
                    BackgroundColor    = SystemColors.Window
                };
                cityGrid.Columns.Add("City", "City");
                cityGrid.Columns.Add("Population", "Population");
                cityGrid.Columns.Add("Pct", "% of City Pop");
                foreach (var c in mw.MajorCities)
                    cityGrid.Rows.Add(c.Name, c.Population.ToString("N0"), $"{c.PercentOfMajorCityPop:F1}%");
                content.SetColumnSpan(cityGrid, 2);
                content.Controls.Add(cityGrid);
            }

            // Government
            AddSH(content, "Government");
            AddRow(content, "Government Code:", mw.Government.ToString());
            AddRow(content, "Type:", mw.GovernmentType);
            AddRow(content, "Profile:", mw.GovernmentProfile);
            AddRow(content, "Centralisation:", $"{mw.CentralisationCode} — {mw.CentralisationType}");
            AddRow(content, "Authority:", $"{mw.AuthorityCode} — {mw.AuthorityType}");
            AddRow(content, "Structure:", $"{mw.StructureCode} — {mw.StructureType}");

            // Factions
            if (gs.Snapshot.WorldFactions.Count > 0)
            {
                AddSH(content, "Factions");
                foreach (var f in gs.Snapshot.WorldFactions)
                    AddRow(content, $"Faction {f.Number}:", $"{f.StrengthType} — {f.Government.Type}  ({f.Profile})");
                if (gs.Snapshot.FactionRelationships.Count > 0)
                {
                    AddRow(content, "Relationships:", string.Join("  |  ",
                        gs.Snapshot.FactionRelationships.Select(r => $"{r.Faction1Number}↔{r.Faction2Number}: {r.Type}")));
                }
            }

            // Law Level
            AddSH(content, "Law Level");
            AddRow(content, "Law Level:", mw.LawLevel.ToString());
            AddRow(content, "Profile:", mw.LawLevels.Profile);
            AddRow(content, "Weapons:", mw.LawLevels.WeaponsLevel.ToString());
            AddRow(content, "Economic:", mw.LawLevels.EconomicLevel.ToString());
            AddRow(content, "Criminal:", mw.LawLevels.CriminalLevel.ToString());
            AddRow(content, "Private:", mw.LawLevels.PrivateLevel.ToString());
            AddRow(content, "Personal Rights:", mw.LawLevels.PersonalRightsLevel.ToString());

            // Tech Level
            AddSH(content, "Tech Level");
            AddRow(content, "Tech Level (UWP):", mw.TechLevel.ToString());
            AddRow(content, "High Common TL:", mw.TechLevels.HighCommonTL.ToString());
            AddRow(content, "Low Common TL:", mw.TechLevels.LowCommonTL.ToString());
            AddRow(content, "TL Profile:", mw.TechLevels.Profile);

            // Judicial
            AddSH(content, "Judicial System");
            AddRow(content, "System:", $"{mw.Judicial.JudicialSystemCode} — {mw.Judicial.JudicialSystemType}");
            AddRow(content, "Profile:", mw.Judicial.Profile);
            AddRow(content, "Uniformity:", mw.Judicial.UniformityType);
            AddRow(content, "Presumption:", mw.Judicial.PresumptionOfInnocence ? "Innocent until proven guilty" : "Guilty until proven innocent");
            AddRow(content, "Death Penalty:", mw.Judicial.DeathPenalty ? "Yes" : "No");

            // Culture
            AddSH(content, "Culture");
            AddRow(content, "Profile:", mw.Culture.Profile);
            AddRow(content, "Diversity:", mw.Culture.Diversity.ToString());
            AddRow(content, "Xenophilia:", mw.Culture.Xenophilia.ToString());
            AddRow(content, "Uniqueness:", mw.Culture.Uniqueness.ToString());
            AddRow(content, "Symbology:", mw.Culture.Symbology.ToString());
            AddRow(content, "Cohesion:", mw.Culture.Cohesion.ToString());
            AddRow(content, "Progressiveness:", mw.Culture.Progressiveness.ToString());
            AddRow(content, "Expansionism:", mw.Culture.Expansionism.ToString());
            AddRow(content, "Militancy:", mw.Culture.Militancy.ToString());

            // Bases & Starport
            AddSH(content, "Bases & Starport");
            AddRow(content, "Highport:", mw.HasHighport ? "Yes" : "No");
            var bases = string.Join(", ", new[] {
                mw.HasNavalBase ? "Naval" : null,
                mw.HasScoutBase ? "Scout" : null,
                mw.HasMilitaryBase ? "Military" : null,
                mw.HasCorsairBase ? "Corsair" : null
            }.Where(b => b != null));
            AddRow(content, "Bases:", string.IsNullOrEmpty(bases) ? "None" : bases);
            AddRow(content, "Berthing Fees:", mw.BerthingFees);
            AddRow(content, "Weekly Traffic:", mw.ExpectedWeeklyTraffic > 0 ? $"{mw.ExpectedWeeklyTraffic:N0} dt" : "-");
            if (mw.HasHighport)
            {
                AddRow(content, "Highport Docking:", $"{mw.HighportTotalDocking:N0} dt");
                AddRow(content, "Downport Docking:", $"{mw.DownportTotalDocking:N0} dt");
            }
            else if (mw.DownportTotalDocking > 0)
                AddRow(content, "Downport Docking:", $"{mw.DownportTotalDocking:N0} dt");
            if (mw.StarportBuildCapacity > 0)
            {
                AddRow(content, "Shipyard Capacity:", $"{mw.StarportBuildCapacity:N0} dt");
                AddRow(content, "Annual Output:", $"{mw.AnnualShipyardOutput:N0} dt");
            }

            // Economics
            AddSH(content, "Economics");
            AddRow(content, "Importance:", mw.Economics.Importance.ToString("+0;-0;0"));
            AddRow(content, "Resource Factor:", mw.Economics.ResourceFactor.ToString());
            AddRow(content, "Labour Factor:", mw.Economics.LabourFactor.ToString());
            AddRow(content, "Infrastructure:", mw.Economics.InfrastructureFactor.ToString());
            AddRow(content, "Efficiency:", mw.Economics.EfficiencyFactor.ToString("+0;-0;0"));
            AddRow(content, "Resource Units:", mw.Economics.ResourceUnits.ToString());
            AddRow(content, "GWP / capita:", $"{mw.Economics.GWPPerCapita:N0} Cr");
            AddRow(content, "Total GWP:", $"{mw.Economics.TotalGWPMCr:N2} MCr");
            AddRow(content, "WTN:", mw.Economics.WorldTradeNumber.ToString());
            AddRow(content, "Inequality:", mw.Economics.InequalityRating.ToString());
            AddRow(content, "Development Score:", mw.Economics.DevelopmentScore.ToString("F1"));
            AddRow(content, "Tariffs:", mw.Economics.Tariffs);

            // Military
            AddSH(content, "Military");
            AddRow(content, "Budget:", $"{mw.Military.BasicMilitaryBudget:F2}% of GWP");
            if (mw.Military.EnforcementBranch   > 0) AddRow(content, "Enforcement:",   EHex(mw.Military.EnforcementBranch));
            if (mw.Military.MilitiaBranch       > 0) AddRow(content, "Militia:",        EHex(mw.Military.MilitiaBranch));
            if (mw.Military.ArmyBranch          > 0) AddRow(content, "Army:",           EHex(mw.Military.ArmyBranch));
            if (mw.Military.WetNavyBranch       > 0) AddRow(content, "Wet Navy:",       EHex(mw.Military.WetNavyBranch));
            if (mw.Military.AirForceBranch      > 0) AddRow(content, "Air Force:",      EHex(mw.Military.AirForceBranch));
            if (mw.Military.SystemDefenceBranch > 0) AddRow(content, "System Defence:", EHex(mw.Military.SystemDefenceBranch));
            if (mw.Military.NavyBranch          > 0) AddRow(content, "Navy:",           EHex(mw.Military.NavyBranch));
            if (mw.Military.MarineBranch        > 0) AddRow(content, "Marine:",         EHex(mw.Military.MarineBranch));

            // Trade Codes
            AddSH(content, "Trade Codes");
            AddRow(content, "Codes:", mw.TradeCodes.Count > 0
                ? string.Join("  ", mw.TradeCodes.Select(t => t.Code))
                : "None");

            // Notes
            AddSH(content, "Notes");
            txtNotes = new TextBox { Multiline = true, Height = 80, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
            txtNotes.Text = gs.Names.TryGetValue($"{nameKey}:notes", out var nt) ? nt : "";
            txtNotes.TextChanged += (s, e) => { gs.Names[$"{nameKey}:notes"] = txtNotes.Text; gs.IsDirty = true; };
            content.SetColumnSpan(txtNotes, 2);
            content.Controls.Add(txtNotes);

            scroll.Controls.Add(content);
            Controls.Add(scroll);
        }

        private static string EHex(int v) =>
            v < 10 ? v.ToString() : ((char)('A' + v - 10)).ToString();

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

        public void RefreshTitle()
        {
            string name  = gs.DisplayName;
            string dirty = gs.IsDirty ? " *" : "";
            Text = $"Social Survey — {name}{dirty}";
        }
    }
}
