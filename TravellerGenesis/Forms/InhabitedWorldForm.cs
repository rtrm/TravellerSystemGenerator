using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TravellerGenesis.Models;
using TravellerSystemGenerator;

namespace TravellerGenesis.Forms
{
    internal class InhabitedWorldForm : Form
    {
        private readonly GeneratedSystem gs;
        private readonly AdditionalInhabitedWorld aiw;
        private readonly SystemOverviewForm? owner;

        private TextBox txtName  = null!;
        private TextBox txtNotes = null!;
        private readonly string nameKey;

        public InhabitedWorldForm(GeneratedSystem gs, AdditionalInhabitedWorld aiw, SystemOverviewForm? owner = null)
        {
            this.gs    = gs;
            this.aiw   = aiw;
            this.owner = owner;

            nameKey       = $"world:{aiw.WorldDesignation}";
            Size          = new Size(860, 780);
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
            txtName.Text = gs.Names.TryGetValue(nameKey, out var n) ? n : "";
            txtName.TextChanged += (s, e) =>
            {
                gs.Names[nameKey] = txtName.Text;
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
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // UWP & Authority
            AddSH(content, "World");
            AddRow(content, "Designation:", aiw.WorldDesignation);
            AddRow(content, "UWP:", aiw.UWP);
            AddRow(content, "Authority:", aiw.IsIndependent ? "Independent World" : $"Under authority of {aiw.AuthorityDesignation}");

            // Population
            AddSH(content, "Population");
            AddRow(content, "Population Code:", aiw.PopulationCode.ToString());
            AddRow(content, "Total Population:", aiw.ActualPopulation.ToString("N0"));

            // Government
            AddSH(content, "Government");
            AddRow(content, "Government Type:", aiw.GovernmentType);
            AddRow(content, "Gov Profile:", aiw.GovernmentProfile);
            AddRow(content, "Centralisation:", $"{aiw.CentralisationCode} — {aiw.CentralisationType}");
            AddRow(content, "Authority:", $"{aiw.AuthorityCode} — {aiw.AuthorityType}");
            AddRow(content, "Structure:", $"{aiw.StructureCode} — {aiw.StructureType}");

            // Law Level
            AddSH(content, "Law Level");
            AddRow(content, "Law Level:", aiw.LawLevel.ToString());
            AddRow(content, "Law Profile:", aiw.LawLevels.Profile);
            AddRow(content, "Weapons:", aiw.LawLevels.WeaponsLevel.ToString());
            AddRow(content, "Economic:", aiw.LawLevels.EconomicLevel.ToString());
            AddRow(content, "Criminal:", aiw.LawLevels.CriminalLevel.ToString());
            AddRow(content, "Private:", aiw.LawLevels.PrivateLevel.ToString());
            AddRow(content, "Personal Rights:", aiw.LawLevels.PersonalRightsLevel.ToString());

            // Tech Level
            AddSH(content, "Tech Level");
            AddRow(content, "Tech Level:", aiw.TechLevel.ToString());
            AddRow(content, "TL Profile:", aiw.TechLevels.Profile);
            AddRow(content, "High Common TL:", aiw.TechLevels.HighCommonTL.ToString());
            AddRow(content, "Low Common TL:", aiw.TechLevels.LowCommonTL.ToString());

            // Spaceport & Bases
            AddSH(content, "Spaceport & Bases");
            AddRow(content, "Spaceport Class:", aiw.SpaceportClass.ToString());
            AddRow(content, "Equivalent Starport:", aiw.EquivalentStarportClass.ToString());
            AddRow(content, "Berthing Fees:", aiw.BerthingFees);
            AddRow(content, "Highport:", aiw.HasHighport ? "Yes" : "No");
            var bases = string.Join(", ", new[] {
                aiw.HasNavalBase ? "Naval" : null,
                aiw.HasScoutBase ? "Scout" : null,
                aiw.HasMilitaryBase ? "Military" : null,
                aiw.HasCorsairBase ? "Corsair" : null
            }.Where(b => b != null));
            AddRow(content, "Bases:", string.IsNullOrEmpty(bases) ? "None" : bases);

            // Trade Codes
            AddSH(content, "Trade Codes");
            if (aiw.TradeCodes.Count > 0)
                AddRow(content, "Codes:", string.Join("  ", aiw.TradeCodes.Select(t => t.Code)));
            else
                AddRow(content, "Codes:", "None");

            // Judicial System
            AddSH(content, "Judicial System");
            AddRow(content, "System:", $"{aiw.Judicial.JudicialSystemCode} — {aiw.Judicial.JudicialSystemType}");
            AddRow(content, "Profile:", aiw.Judicial.Profile);
            AddRow(content, "Uniformity:", aiw.Judicial.UniformityType);
            AddRow(content, "Presumption:", aiw.Judicial.PresumptionOfInnocence ? "Innocent until proven guilty" : "Guilty until proven innocent");
            AddRow(content, "Death Penalty:", aiw.Judicial.DeathPenalty ? "Yes" : "No");

            // Culture
            AddSH(content, "Culture");
            AddRow(content, "Profile:", aiw.Culture.Profile);
            AddRow(content, "Diversity:", aiw.Culture.Diversity.ToString());
            AddRow(content, "Xenophilia:", aiw.Culture.Xenophilia.ToString());
            AddRow(content, "Uniqueness:", aiw.Culture.Uniqueness.ToString());
            AddRow(content, "Symbology:", aiw.Culture.Symbology.ToString());
            AddRow(content, "Cohesion:", aiw.Culture.Cohesion.ToString());
            AddRow(content, "Progressiveness:", aiw.Culture.Progressiveness.ToString());
            AddRow(content, "Expansionism:", aiw.Culture.Expansionism.ToString());
            AddRow(content, "Militancy:", aiw.Culture.Militancy.ToString());

            // Factions
            if (aiw.Factions.Count > 0)
            {
                AddSH(content, "Factions");
                foreach (var f in aiw.Factions)
                    AddRow(content, $"Faction {f.Number}:", $"{f.StrengthType} — {f.Government.Type} ({f.Profile})");
            }

            // Economics
            AddSH(content, "Economics");
            AddRow(content, "Importance:", aiw.Economics.Importance.ToString("+0;-0;0"));
            AddRow(content, "Resource Factor:", aiw.Economics.ResourceFactor.ToString());
            AddRow(content, "Labour Factor:", aiw.Economics.LabourFactor.ToString());
            AddRow(content, "Infrastructure:", aiw.Economics.InfrastructureFactor.ToString());
            AddRow(content, "Efficiency:", aiw.Economics.EfficiencyFactor.ToString("+0;-0;0"));
            AddRow(content, "Resource Units:", aiw.Economics.ResourceUnits.ToString());
            AddRow(content, "GWP/capita:", $"{aiw.Economics.GWPPerCapita:N0} Cr");
            AddRow(content, "Total GWP:", $"{aiw.Economics.TotalGWPMCr:N2} MCr");
            AddRow(content, "WTN:", aiw.Economics.WorldTradeNumber.ToString());
            AddRow(content, "Tariffs:", aiw.Economics.Tariffs);

            // Military
            AddSH(content, "Military");
            AddRow(content, "Budget:", $"{aiw.Military.BasicMilitaryBudget:F2}% of GWP");
            if (aiw.Military.EnforcementBranch  > 0) AddRow(content, "Enforcement:",  EHex(aiw.Military.EnforcementBranch));
            if (aiw.Military.MilitiaBranch      > 0) AddRow(content, "Militia:",       EHex(aiw.Military.MilitiaBranch));
            if (aiw.Military.ArmyBranch         > 0) AddRow(content, "Army:",          EHex(aiw.Military.ArmyBranch));
            if (aiw.Military.WetNavyBranch      > 0) AddRow(content, "Wet Navy:",      EHex(aiw.Military.WetNavyBranch));
            if (aiw.Military.AirForceBranch     > 0) AddRow(content, "Air Force:",     EHex(aiw.Military.AirForceBranch));
            if (aiw.Military.SystemDefenceBranch> 0) AddRow(content, "System Defence:",EHex(aiw.Military.SystemDefenceBranch));
            if (aiw.Military.NavyBranch         > 0) AddRow(content, "Navy:",          EHex(aiw.Military.NavyBranch));
            if (aiw.Military.MarineBranch       > 0) AddRow(content, "Marine:",        EHex(aiw.Military.MarineBranch));

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

        private static string EHex(int v)
        {
            if (v < 10) return v.ToString();
            return ((char)('A' + v - 10)).ToString();
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

        public void RefreshTitle()
        {
            string name  = gs.Names.TryGetValue(nameKey, out var n) && !string.IsNullOrWhiteSpace(n) ? n : aiw.WorldDesignation;
            string dirty = gs.IsDirty ? " *" : "";
            Text = $"Inhabited — {aiw.WorldDesignation} — {name}{dirty}";
        }
    }
}
