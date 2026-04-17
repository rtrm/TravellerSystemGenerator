using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TravellerSystemGenerator
{
    internal static class ImportedSystemBuilder
    {
        /// <summary>
        /// Build a SystemSnapshot from T5 Second Survey data, then overlay the
        /// imported values on top of the Traveller Genesis-generated baseline.
        /// </summary>
        internal static SystemSnapshot BuildSnapshot(
            T5SystemData data,
            string importSource,
            Dictionary<string, string> names)
        {
            // Seed = hex location parsed as integer (e.g. "1910" → 1910)
            int.TryParse(data.Hex, out int seed);

            // Generate the baseline using existing engine logic
            var ss = new StarSystem(
                seed: seed,
                mainworldUWP: data.UWP,
                name: data.Name,
                generateFiles: false);

            var snapshot = new SystemSnapshot
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

            // ── Apply T5SS overrides ──────────────────────────────────

            snapshot.HexLocation  = data.Hex;
            snapshot.Allegiance   = string.IsNullOrWhiteSpace(data.Allegiance) ? null : data.Allegiance;
            snapshot.TravelZone   = string.IsNullOrWhiteSpace(data.TravelZone) ? null : data.TravelZone;
            snapshot.ImportSource = importSource;
            snapshot.Nobility     = string.IsNullOrWhiteSpace(data.Nobility)   ? null : data.Nobility;

            // ── Canonical UWP override (must come before derived-field overrides) ──
            // StarSystem only preserves Size/Atm/Hyd from the input UWP and re-rolls
            // everything else (Pop/Gov/Law/Starport/TL), so we forcibly restore them.
            ApplyCanonicalUWP(snapshot, data.UWP);

            // Importance Extension {Ix}
            if (snapshot.Mainworld != null)
                snapshot.Mainworld.Economics.Importance = data.Importance;

            // Economic Extension (Ex): format is "(RLI±E)" e.g. "(D7E+5)"
            ApplyEconomicExtension(snapshot, data.EconEx);

            // Cultural Extension [Cx]: 4 eHex chars e.g. "[9C6D]"
            ApplyCulturalExtension(snapshot, data.CulturalEx);

            // Bases
            ApplyBases(snapshot, data.Bases);

            // PBG — Population multiplier, Belts, Gas Giants
            ApplyPBG(snapshot, data.PBG);

            // World count → derive TerrestrialPlanetCount
            if (data.WorldCount > 0)
            {
                int terrestrial = Math.Max(0,
                    data.WorldCount - snapshot.GasGiantCount - snapshot.PlanetoidBeltCount - 1);
                snapshot.TerrestrialPlanetCount = terrestrial;
            }

            // Stars — override spectral class on generated stars
            ApplyStars(snapshot, data.Stars);

            // Remarks — trade codes and ownership
            ApplyRemarks(snapshot, data.Remarks);

            // ── Pre-populate mainworld name with system name ──────────
            if (!string.IsNullOrWhiteSpace(data.Name))
            {
                // Grid key includes '*' suffix; Properties form key does not — set both
                string? mainObj = snapshot.Worlds.FirstOrDefault(w => w.Object.EndsWith('*'))?.Object;
                if (mainObj != null)
                    names[$"world:{mainObj}"] = data.Name;         // overview grid
                string mainDesigClean = (mainObj ?? "").TrimEnd('*');
                if (!string.IsNullOrEmpty(mainDesigClean))
                    names[$"world:{mainDesigClean}"] = data.Name;  // Properties sheet
            }

            // ── Recalculate GWP/WTN from canonical economic and UWP values ──
            // ApplyEconomicExtension set canonical R/L/I/EF; ApplyCanonicalUWP set canonical
            // TL/pop/starport/trade-codes. GWP and WTN were computed during generation with
            // random values — recompute them now so all downstream calculations are consistent.
            RecalculateEconomicDerivedFields(snapshot);

            // ── Recalculate starport-dependent fields using canonical values ──
            // The baseline generation used a randomly-rolled starport; now that we've
            // applied the canonical T5SS UWP and bases, redo highport, berthing fees,
            // traffic, docking capacity and shipyard capacity with correct inputs.
            RecalculateStarportDependents(snapshot, new Random(snapshot.Seed ^ 0x53504F52));

            // ── Recalculate tech level subcategories using canonical TL ──
            // The baseline generated TechLevels from the random TL; redo with canonical TL.
            RecalculateTechLevels(snapshot, new Random(snapshot.Seed ^ 0x544C5645));

            // ── Recalculate military using canonical TL, starport, bases ──
            RecalculateMilitary(snapshot, new Random(snapshot.Seed ^ 0x4D494C59));

            return snapshot;
        }

        // ── Canonical UWP override ───────────────────────────────────

        private static void ApplyCanonicalUWP(SystemSnapshot snap, string uwp)
        {
            if (snap.Mainworld == null || string.IsNullOrWhiteSpace(uwp)) return;

            string clean = uwp.Replace(" ", "");
            if (clean.Length < 8) return;

            // Parse each position
            snap.Mainworld.Starport      = char.ToUpperInvariant(clean[0]);
            snap.Mainworld.Size          = TravellerMapImporter.EhexDecode(clean[1]);
            snap.Mainworld.Atmosphere    = TravellerMapImporter.EhexDecode(clean[2]);
            snap.Mainworld.Hydrographics = TravellerMapImporter.EhexDecode(clean[3]);
            snap.Mainworld.Population    = TravellerMapImporter.EhexDecode(clean[4]);
            snap.Mainworld.Government    = TravellerMapImporter.EhexDecode(clean[5]);
            snap.Mainworld.LawLevel      = TravellerMapImporter.EhexDecode(clean[6]);
            // Position 7 is '-', position 8 is TL (or position 7 if no dash)
            int tlIdx = clean[7] == '-' ? 8 : 7;
            if (tlIdx < clean.Length)
                snap.Mainworld.TechLevel = TravellerMapImporter.EhexDecode(clean[tlIdx]);

            // Restore the canonical UWP string
            snap.Mainworld.UWP = clean[7] == '-'
                ? clean.Substring(0, 9)
                : clean.Substring(0, 7) + "-" + clean[7];

            // Update government type name from code
            snap.Mainworld.GovernmentType = GetGovernmentType(snap.Mainworld.Government);

            // Recalculate actual population from the canonical population code + existing PopulationP
            if (snap.Mainworld.Population > 0)
            {
                long basePop = (long)Math.Pow(10, snap.Mainworld.Population);
                int p = snap.Mainworld.PopulationP;
                if (p <= 0) p = 1;
                snap.Mainworld.ActualPopulation = basePop * p;
            }
            else
            {
                snap.Mainworld.ActualPopulation = 0;
            }

            string canonicalUWP = snap.Mainworld.UWP;

            // Fix WorldDisplayData — mainworld row has Object ending with '*', Size holds the UWP
            foreach (var w in snap.Worlds)
            {
                if (w.Object.EndsWith('*'))
                {
                    w.Size = canonicalUWP;
                    break;
                }
            }

            // Fix SurveyData — mainworld survey SAH_UWP
            // Find the mainworld survey: its WorldName contains '(' (e.g. "Regina (A III)")
            // or matches the mainworld object designation (without '*')
            string mainDesig = snap.Worlds
                .FirstOrDefault(w => w.Object.EndsWith('*'))?.Object.TrimEnd('*') ?? "";
            foreach (var s in snap.Surveys)
            {
                bool isMainworld = !string.IsNullOrEmpty(mainDesig) &&
                    (s.WorldName.Contains('(') || s.WorldName.StartsWith(mainDesig));
                if (isMainworld)
                {
                    s.SAH_UWP = canonicalUWP;
                    break;
                }
            }
        }

        private static string GetGovernmentType(int code) => code switch
        {
            0  => "None",
            1  => "Company / Corporation",
            2  => "Participating Democracy",
            3  => "Self-Perpetuating Oligarchy",
            4  => "Representative Democracy",
            5  => "Feudal Technocracy",
            6  => "Captive Government",
            7  => "Balkanisation",
            8  => "Civil Service Bureaucracy",
            9  => "Impersonal Bureaucracy",
            10 => "Charismatic Dictatorship",
            11 => "Non-Charismatic Dictatorship",
            12 => "Charismatic Oligarchy",
            13 => "Religious Dictatorship",
            14 => "Religious Autocracy",
            15 => "Totalitarian Oligarchy",
            _  => $"Government {code}"
        };

        // ── Economic Extension ────────────────────────────────────────

        private static void ApplyEconomicExtension(SystemSnapshot snap, string econEx)
        {
            if (snap.Mainworld == null) return;

            // Strip parens: "(D7E+5)" → "D7E+5"
            string raw = econEx.Trim('(', ')', ' ');
            if (raw.Length < 3) return;

            // Find the efficiency sign
            int signIdx = raw.IndexOfAny(new[] { '+', '-' });
            string rliPart = signIdx > 0 ? raw.Substring(0, signIdx) : raw;
            string effPart = signIdx >= 0 ? raw.Substring(signIdx) : "";

            if (rliPart.Length >= 1)
                snap.Mainworld.Economics.ResourceFactor = TravellerMapImporter.EhexDecode(rliPart[0]);
            if (rliPart.Length >= 2)
                snap.Mainworld.Economics.LabourFactor = TravellerMapImporter.EhexDecode(rliPart[1]);
            if (rliPart.Length >= 3)
                snap.Mainworld.Economics.InfrastructureFactor = TravellerMapImporter.EhexDecode(rliPart[2]);

            if (int.TryParse(effPart, out int eff))
                snap.Mainworld.Economics.EfficiencyFactor = eff;

            // Recalculate RU
            snap.Mainworld.Economics.ResourceUnits =
                snap.Mainworld.Economics.ResourceFactor *
                snap.Mainworld.Economics.LabourFactor *
                snap.Mainworld.Economics.InfrastructureFactor *
                snap.Mainworld.Economics.EfficiencyFactor;
        }

        // ── Cultural Extension ────────────────────────────────────────

        private static void ApplyCulturalExtension(SystemSnapshot snap, string culturalEx)
        {
            if (snap.Mainworld == null) return;

            // Strip brackets: "[9C6D]" → "9C6D"
            string raw = culturalEx.Trim('[', ']', ' ');
            if (raw.Length < 4) return;

            snap.Mainworld.Culture.Diversity   = TravellerMapImporter.EhexDecode(raw[0]); // Heterogeneity
            snap.Mainworld.Culture.Xenophilia  = TravellerMapImporter.EhexDecode(raw[1]); // Acceptance
            snap.Mainworld.Culture.Uniqueness  = TravellerMapImporter.EhexDecode(raw[2]); // Strangeness
            snap.Mainworld.Culture.Symbology   = TravellerMapImporter.EhexDecode(raw[3]); // Symbols
        }

        // ── Bases ─────────────────────────────────────────────────────

        private static void ApplyBases(SystemSnapshot snap, string bases)
        {
            if (snap.Mainworld == null) return;

            // Always reset — T5SS data is authoritative; absence means no bases
            snap.Mainworld.HasNavalBase       = false;
            snap.Mainworld.HasScoutBase       = false;
            snap.Mainworld.HasMilitaryBase    = false;
            snap.Mainworld.HasCorsairBase     = false;
            snap.Mainworld.HasXBoatWaystation = false;

            if (string.IsNullOrWhiteSpace(bases) || bases == "-") return;

            foreach (char c in bases.ToUpperInvariant())
            {
                switch (c)
                {
                    case 'N': case 'K': snap.Mainworld.HasNavalBase      = true; break;
                    case 'S':           snap.Mainworld.HasScoutBase      = true; break;
                    case 'M':           snap.Mainworld.HasMilitaryBase   = true; break;
                    case 'C':           snap.Mainworld.HasCorsairBase    = true; break;
                    case 'W':           snap.Mainworld.HasXBoatWaystation = true; break;
                }
            }
        }

        // ── PBG ───────────────────────────────────────────────────────

        private static void ApplyPBG(SystemSnapshot snap, string pbg)
        {
            if (string.IsNullOrWhiteSpace(pbg) || pbg == "---") return;

            // PBG = 3 eHex chars: population-multiplier, belts, gas giants
            if (pbg.Length >= 1 && snap.Mainworld != null)
                snap.Mainworld.PopulationP = TravellerMapImporter.EhexDecode(pbg[0]);

            if (pbg.Length >= 2)
            {
                snap.PlanetoidBeltCount = TravellerMapImporter.EhexDecode(pbg[1]);
            }

            if (pbg.Length >= 3)
            {
                snap.GasGiantCount = TravellerMapImporter.EhexDecode(pbg[2]);
            }
        }

        // ── Stars ─────────────────────────────────────────────────────

        private static void ApplyStars(SystemSnapshot snap, string starsRaw)
        {
            if (string.IsNullOrWhiteSpace(starsRaw)) return;

            // Stars field: space-separated tokens alternating type+subtype / luminosity
            // e.g. "G2 V M1 V BD"
            // Pairs: "G2"+"V" = "G2 V", "M1"+"V" = "M1 V", "BD" alone = brown dwarf
            var tokens = starsRaw.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            var starClasses = new List<string>();
            int i = 0;
            while (i < tokens.Length)
            {
                string tok = tokens[i];
                // Luminosity-only tokens (BD = Brown Dwarf, D = White Dwarf)
                if (tok == "BD" || tok == "D" || tok == "NS" || tok == "BH")
                {
                    starClasses.Add(tok);
                    i++;
                }
                // Spectral type + subtype (e.g. "G2") followed by luminosity class (e.g. "V", "IV", "III", "Ia")
                else if (i + 1 < tokens.Length && IsLuminosityClass(tokens[i + 1]))
                {
                    starClasses.Add($"{tok} {tokens[i + 1]}");
                    i += 2;
                }
                else
                {
                    starClasses.Add(tok);
                    i++;
                }
            }

            // Override Class field on generated StarData entries
            for (int s = 0; s < Math.Min(starClasses.Count, snap.Stars.Count); s++)
                snap.Stars[s].Class = starClasses[s];
        }

        private static bool IsLuminosityClass(string tok) =>
            tok == "V" || tok == "IV" || tok == "III" || tok == "II" || tok == "I" ||
            tok == "Ia" || tok == "Ib" || tok == "VI" || tok == "VII" || tok == "D";

        // ── Economic derived-field recalculation ──────────────────────
        // Recomputes GWP and WTN from canonical R/I/EF (set by ApplyEconomicExtension),
        // TL, starport, pop (set by ApplyCanonicalUWP), and trade codes (set by ApplyRemarks).
        // Everything else in the Economics object (R/L/I/EF/RU/Importance) was already set
        // from T5SS data; we only touch the derived calculations.

        private static void RecalculateEconomicDerivedFields(SystemSnapshot snap)
        {
            if (snap.Mainworld == null || snap.Mainworld.Population == 0) return;
            var mw = snap.Mainworld;
            var econ = mw.Economics;

            int tl       = mw.TechLevel;
            char starport = mw.Starport;
            int govCode  = mw.Government;
            int ef       = econ.EfficiencyFactor;
            int inf      = econ.InfrastructureFactor;
            int rf       = econ.ResourceFactor;

            bool hasAg = mw.TradeCodes.Any(tc => tc.Code == "Ag");
            bool hasAs = mw.TradeCodes.Any(tc => tc.Code == "As");
            bool hasGa = mw.TradeCodes.Any(tc => tc.Code == "Ga");
            bool hasIn = mw.TradeCodes.Any(tc => tc.Code == "In");
            bool hasNi = mw.TradeCodes.Any(tc => tc.Code == "Ni");
            bool hasPo = mw.TradeCodes.Any(tc => tc.Code == "Po");
            bool hasRi = mw.TradeCodes.Any(tc => tc.Code == "Ri");

            // GWP
            int baseValue = inf + rf;
            double techMod = tl / 10.0;
            double portMod = starport switch
            {
                'A' => 1.5, 'B' => 1.2, 'C' => 1.0, 'D' => 0.8, 'E' => 0.5,
                'F' => 0.9, 'G' => 0.7, 'H' => 0.4, 'Y' => 0.2, _ => 0.2
            };
            double govMod = govCode switch
            {
                0  => 1.0, 1  => 1.5, 2  => 1.2, 3  => 0.8, 4  => 1.2,
                5  => 1.3, 6  => 0.6, 7  => 1.0, 8  => 0.9, 9  => 0.8,
                10 => 1.0, 11 => 0.7, 12 => 1.0, 13 => 0.6, 14 => 0.5,
                15 => 0.8, _ => 1.0
            };
            double tradeMod = 1.0;
            if (hasAg) tradeMod *= 0.9;
            if (hasAs) tradeMod *= 1.2;
            if (hasGa) tradeMod *= 1.2;
            if (hasIn) tradeMod *= 1.1;
            if (hasNi) tradeMod *= 0.9;
            if (hasPo) tradeMod *= 0.8;
            if (hasRi) tradeMod *= 1.2;
            double totalMod = techMod * portMod * govMod * tradeMod;
            double gwpPC = ef > 0
                ? 1000.0 * baseValue * totalMod * ef
                : 1000.0 * (baseValue * totalMod) / -(ef - 1);
            gwpPC = Math.Max(0.05, gwpPC);
            econ.GWPPerCapita = gwpPC;
            econ.TotalGWPMCr  = gwpPC * mw.ActualPopulation / 1_000_000.0;

            // WTN
            int wtnDM = 0;
            if (tl <= 1)                 wtnDM -= 1;
            if (tl >= 5 && tl <= 8)     wtnDM += 1;
            if (tl >= 9 && tl <= 13)    wtnDM += 2;
            if (tl >= 15)               wtnDM += 3;
            int baseWTN = mw.Population + wtnDM;
            econ.WTNStarportModifier = StarSystem.GetWTNStarportModifier(baseWTN, starport);
            econ.WorldTradeNumber    = Math.Max(0, baseWTN + econ.WTNStarportModifier);

            // DevelopmentScore depends on GWP and InequalityRating (InequalityRating is not
            // recalculated here — it requires a dice roll and uses PCR; leave it as generated)
            econ.DevelopmentScore = (econ.GWPPerCapita / 1000.0) * (1.0 - econ.InequalityRating / 100.0);
        }

        // ── Tech level subcategory recalculation ─────────────────────

        private static void RecalculateTechLevels(SystemSnapshot snap, Random dice)
        {
            if (snap.Mainworld == null || snap.Mainworld.Population == 0) return;
            var mw = snap.Mainworld;

            bool hasIn = mw.TradeCodes.Any(tc => tc.Code == "In");
            bool hasRi = mw.TradeCodes.Any(tc => tc.Code == "Ri");
            bool hasPo = mw.TradeCodes.Any(tc => tc.Code == "Po");

            int worldHighTL = mw.TechLevel;
            int worldLowTL  = StarSystem.CalculateLowCommonTL(worldHighTL, mw.Government, mw.Population, mw.PCR, dice);

            int habRating = 0;
            if (mw.PlacedWorld is TerrestrialPlanet tp) habRating = tp.HabitabilityRating;
            else if (mw.PlacedWorld is Moon moon)       habRating = moon.HabitabilityRating;

            mw.TechLevels = StarSystem.GenerateTechLevelData(
                worldHighTL, worldLowTL, mw.Government, mw.Population,
                mw.Atmosphere, mw.Hydrographics, mw.PCR,
                habRating, mw.Starport,
                mw.LawLevel, mw.LawLevels.WeaponsLevel,
                mw.Size, hasIn, hasRi, hasPo, isOnBalkanisedWorld: false, dice);
        }

        // ── Military recalculation ────────────────────────────────────

        private static void RecalculateMilitary(SystemSnapshot snap, Random dice)
        {
            if (snap.Mainworld == null || snap.Mainworld.Population == 0) return;
            var mw = snap.Mainworld;

            bool subMilBase = snap.AdditionalInhabitedWorlds.Any(a => !a.IsIndependent && a.HasMilitaryBase);

            mw.Military = StarSystem.CalculateWorldMilitary(
                popCode:                      mw.Population,
                govCode:                      mw.Government,
                lawLevel:                     mw.LawLevel,
                hydro:                        mw.Hydrographics,
                atmosphere:                   mw.Atmosphere,
                techLevel:                    mw.TechLevel,
                pcr:                          mw.PCR,
                starport:                     mw.Starport,
                hasHighport:                  mw.HasHighport,
                hasNavalBase:                 mw.HasNavalBase,
                hasMilitaryBase:              mw.HasMilitaryBase,
                hasMilitaryBaseOnSubordinate: subMilBase,
                militancy:                    mw.Culture.Militancy,
                expansionism:                 mw.Culture.Expansionism,
                factions:                     snap.WorldFactions,
                relationships:                snap.FactionRelationships,
                isUnderMainworldAuthority:    false,
                dice:                         dice);

            mw.Military.BudgetDM = StarSystem.CalcBudgetDM(
                mw.Government, mw.LawLevel,
                mw.HasNavalBase, mw.HasMilitaryBase,
                mw.Culture.Militancy, mw.Military);

            mw.Military.BasicMilitaryBudget = StarSystem.CalcBasicMilitaryBudget(
                mw.Economics.EfficiencyFactor, mw.Military.BudgetDM, dice);
        }

        // ── Starport-dependent recalculation ─────────────────────────

        private static void RecalculateStarportDependents(SystemSnapshot snap, Random dice)
        {
            if (snap.Mainworld == null) return;

            var  mw  = snap.Mainworld;
            char sp  = mw.Starport;
            int  pop = mw.Population;
            int  tl  = mw.TechLevel;
            int  ll  = mw.LawLevel;

            // ── Highport ──────────────────────────────────────────────────────
            // T5SS Bases field does not include highport; re-roll using canonical values.
            int highportDM = 0;
            if (pop <= 6) highportDM -= 1;
            if (pop >= 9) highportDM += 1;
            if (tl >= 9 && tl <= 11) highportDM += 1;
            if (tl >= 12)            highportDM += 2;
            int highportRoll = Starhelper.diceRoll(6, 2, dice) + highportDM;
            mw.HasHighport = sp switch
            {
                'A' => highportRoll >= 6,
                'B' => highportRoll >= 8,
                'C' => highportRoll >= 10,
                'D' => highportRoll >= 12,
                _   => false
            };

            // ── Berthing Fees ─────────────────────────────────────────────────
            mw.BerthingFees = sp switch
            {
                'A' => $"Cr {Starhelper.diceRoll(6, 1, dice) * 1000:N0}",
                'B' => $"Cr {Starhelper.diceRoll(6, 1, dice) * 500:N0}",
                'C' => $"Cr {Starhelper.diceRoll(6, 1, dice) * 100:N0}",
                'D' => $"Cr {Starhelper.diceRoll(6, 1, dice) * 10:N0}",
                _   => "None"
            };

            // Starports E and X have no docking capacity or build capacity
            if (sp is 'E' or 'X')
            {
                mw.ExpectedWeeklyTraffic = 0;
                mw.HighportTotalDocking  = 0;
                mw.DownportTotalDocking  = 0;
                mw.StarportBuildCapacity = 0;
                mw.AnnualShipyardOutput  = 0;
                return;
            }

            int    imp   = mw.Economics.Importance;
            int    wtn   = mw.Economics.WorldTradeNumber;
            int    ef    = mw.Economics.EfficiencyFactor;
            int    inf   = mw.Economics.InfrastructureFactor;
            double gwp   = mw.Economics.TotalGWPMCr;
            bool   hasHP = mw.HasHighport;

            // ── Traffic ───────────────────────────────────────────────────────
            int impTraffic = imp;
            if (wtn >= 10) impTraffic++;
            if (wtn <= 4)  impTraffic--;
            impTraffic = Math.Clamp(impTraffic, -3, 6);
            mw.ExpectedWeeklyTraffic = GetWeeklyTraffic(impTraffic);

            // ── Docking Capacity ──────────────────────────────────────────────
            int impCap = Math.Max(1, imp);
            int weekly  = mw.ExpectedWeeklyTraffic;

            double AdditiveCapacity(int multiplier)
            {
                double factor = (1.0 + Starhelper.diceRoll(6, 1, dice) + ef) / 5.0;
                return Math.Max(0.0, impCap * weekly * multiplier * pop * factor);
            }

            if (hasHP)
            {
                double hpBase = sp switch { 'A' => 100_000, 'B' => 50_000, 'C' => 20_000, 'D' => 500, _ => 0 };
                int    hpMult = sp switch { 'A' => 500,     'B' => 500,    'C' => 200,    'D' => 100, _ => 0 };
                mw.HighportTotalDocking = RoundTo100(hpBase + AdditiveCapacity(hpMult));
                mw.DownportTotalDocking = RoundTo100(mw.HighportTotalDocking * 0.10 * Starhelper.diceRoll(6, 1, dice));
            }
            else
            {
                double dpBase = sp switch { 'A' => 100_000, 'B' => 50_000, 'C' => 20_000, 'D' => 500, _ => 0 };
                int    dpMult = sp switch { 'A' => 500,     'B' => 500,    'C' => 200,    'D' => 100, _ => 0 };
                mw.DownportTotalDocking = RoundTo100(dpBase + AdditiveCapacity(dpMult));
            }

            int minCap = sp switch { 'A' => 100_000, 'B' => 50_000, 'C' => 20_000, 'D' => 400, _ => 0 };
            int total  = mw.HighportTotalDocking + mw.DownportTotalDocking;
            if (total < minCap)
                mw.DownportTotalDocking += minCap - total;

            // ── Build Capacity ────────────────────────────────────────────────
            if (sp is not ('A' or 'B' or 'C'))
            {
                mw.StarportBuildCapacity = 0;
            }
            else
            {
                int buildDM = 0;
                if (tl <= 8)               buildDM -= 4;
                if (tl >= 12 && tl <= 14)  buildDM += 2;
                if (tl >= 15)              buildDM += 4;
                if (mw.TradeCodes.Any(tc => tc.Code == "Ni")) buildDM -= 2;
                if (mw.TradeCodes.Any(tc => tc.Code == "In")) buildDM += 2;

                int    roll     = Starhelper.diceRoll(6, 1, dice);
                double buildCap;

                if (sp == 'A')
                {
                    buildCap = Math.Max(0, (ef + inf + roll + buildDM) * (gwp / 20_000.0));
                    buildCap = RoundTo100(buildCap);
                    if (buildCap < 9_000)
                        buildCap = 9_000 + Starhelper.diceRoll(6, 1, dice) * 500;
                }
                else if (sp == 'B')
                {
                    buildCap = Math.Max(0, (ef + inf + roll + buildDM) * (gwp / 100_000.0));
                    buildCap = RoundTo100(buildCap);
                    if (buildCap < 5_000)
                        buildCap = 4_000 + Starhelper.diceRoll(6, 2, dice) * 100;
                }
                else // 'C'
                {
                    buildCap = Math.Max(0, (ef + inf + roll - 3 + buildDM) * (gwp / 15_000.0));
                    buildCap = RoundTo100(buildCap);
                }
                mw.StarportBuildCapacity = (int)buildCap;
            }

            // ── Annual Shipyard Output ────────────────────────────────────────
            if (mw.StarportBuildCapacity > 0)
            {
                double annual;
                if (sp is 'A' or 'B')
                    annual = imp >= 1
                        ? mw.StarportBuildCapacity / (double)imp
                        : mw.StarportBuildCapacity * (1.0 - imp);
                else
                    annual = 10.0 * mw.StarportBuildCapacity;
                mw.AnnualShipyardOutput = RoundTo100(annual);
            }
            else
            {
                mw.AnnualShipyardOutput = 0;
            }
        }

        private static int GetWeeklyTraffic(int imp) => imp switch
        {
            >= 6 => 2000,
               5 => 1000,
               4 => 150,
               3 => 30,
               2 => 20,
               1 => 10,
               0 => 5,
              -1 => 5,
              -2 => 2,
               _  => 1
        };

        private static int RoundTo100(double value) =>
            (int)(Math.Round(value / 100.0) * 100);

        // ── Remarks / Trade codes ─────────────────────────────────────

        // All recognised T5SS trade codes → display name
        private static readonly Dictionary<string, string> AllTradeCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Ag"] = "Agricultural",      ["As"] = "Asteroid",         ["Ba"] = "Barren",
            ["De"] = "Desert",            ["Fl"] = "Fluid Oceans",     ["Ga"] = "Garden World",
            ["Hi"] = "High Population",   ["Ht"] = "High Technology",  ["Ic"] = "Ice-Capped",
            ["In"] = "Industrial",        ["Lo"] = "Low Population",   ["Lt"] = "Low Technology",
            ["Na"] = "Non-Agricultural",  ["Ni"] = "Non-Industrial",   ["Po"] = "Poor",
            ["Ri"] = "Rich",              ["Va"] = "Vacuum",           ["Wa"] = "Waterworld",
            ["Pa"] = "Pre-Agricultural",  ["Fr"] = "Frozen",           ["Ho"] = "Hot",
            ["Tr"] = "Tropical",          ["He"] = "Hellworld",        ["Oc"] = "Ocean World",
            ["Cx"] = "Sector Capital",    ["Cp"] = "Subsector Capital",
            ["Cy"] = "Colony",            ["Fa"] = "Farming",          ["Fp"] = "Freeport",
            ["Mb"] = "Military Base",     ["Mi"] = "Mining Facility",  ["Pe"] = "Penal Colony",
            ["Rb"] = "Research Base",     ["Di"] = "Dieback",          ["Ph"] = "Pre-High Population",
            ["An"] = "Ancient Site",      ["Pz"] = "Puzzle",           ["Fo"] = "Forbidden",
            ["Re"] = "Reserve",           ["Sa"] = "Satellite",        ["Pi"] = "Pre-Industrial",
            ["Px"] = "Prison/Exile Camp",
        };

        private static void ApplyRemarks(SystemSnapshot snap, string remarks)
        {
            if (snap.Mainworld == null) return;

            // Replace generated trade codes with canonical T5SS list
            snap.Mainworld.TradeCodes.Clear();

            if (string.IsNullOrWhiteSpace(remarks)) return;

            var tokens = remarks.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string? ownershipNote = null;

            foreach (string tok in tokens)
            {
                // Ownership code: O:XXYY
                if (tok.StartsWith("O:", StringComparison.OrdinalIgnoreCase))
                {
                    ownershipNote = $"Ownership: {tok}";
                    continue;
                }

                // Skip sophont/alien presence codes in parens like "(Amindii)2", "Varg0"
                if (tok.StartsWith("(") || (tok.Length >= 2 && char.IsUpper(tok[0]) && char.IsLower(tok[tok.Length - 1]) && char.IsDigit(tok[tok.Length - 1])))
                    continue;

                if (AllTradeCodes.TryGetValue(tok, out var name))
                {
                    snap.Mainworld.TradeCodes.Add(new TradeCode
                    {
                        Code = tok,
                        Name = name
                    });
                }
            }

            // Append ownership to the Nobility/notes field
            if (ownershipNote != null)
            {
                snap.Nobility = string.IsNullOrWhiteSpace(snap.Nobility)
                    ? ownershipNote
                    : snap.Nobility + "; " + ownershipNote;
            }
        }
    }
}
