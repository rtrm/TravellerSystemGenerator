using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
//using System.Deployment.Internal;
using System.Linq;
using System.Reflection;
//using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TravellerSystemGenerator
{
    // Helper class for displaying star data in table format
    internal class StarDisplayData
    {
        public string Component { get; set; } = "";
        public string? ParentDesignation { get; set; } // For "Aab (A)" notation
        public string Class { get; set; } = "";
        public float Mass { get; set; }
        public float Temp { get; set; }
        public float Diameter { get; set; }
        public float Luminosity { get; set; }
        public float? Orbit { get; set; } // nullable for primary
        public float? AU { get; set; }
        public float? Ecc { get; set; }
        public string? Period { get; set; }
        public float MAO { get; set; }
        public float HZCO { get; set; }
        public bool IsCombined { get; set; }
        public int SortOrder { get; set; } // For proper ordering
    }

    // Helper class for displaying world data in table format
    internal class WorldDisplayData
    {
        public string Primary { get; set; } = "";
        public string Object { get; set; } = "";
        public string Size { get; set; } = ""; // Size code for terrestrial planets
        public float Orbit { get; set; }
        public float AU { get; set; }
        public float Ecc { get; set; }
        public string Period { get; set; } = "";
        public string Sub { get; set; } = "";  // Significant moons count minus rings
        public string Notes { get; set; } = "";
    }

    internal class StarSystem
    {
        private Random dice;
        internal int Seed { get; private set; }

        internal StarSystem(int? seed = null)
        {
            DebugLogger.LogSection("STAR SYSTEM GENERATION");

            // Generate or use provided seed
            if (seed.HasValue)
            {
                Seed = seed.Value;
                DebugLogger.Log($"Using provided seed: {Seed}");
            }
            else
            {
                Seed = Environment.TickCount;
                DebugLogger.Log($"Generated seed: {Seed}");
            }

            dice = new Random(Seed);
            DebugLogger.Log("Random number generator initialized");

            DebugLogger.Log("");
            DebugLogger.Log("Creating primary celestial object...");
            primaryObject = new CelestrialObject();

            DebugLogger.Log("Generating primary star...");
            primaryObject.celestrialObject = new Star(dice);

            DebugLogger.Log("Loading orbital values...");
            Starhelper.LoadOrbitalValues();

            DebugLogger.Log("");
            DebugLogger.Log("Checking for additional companion stars...");
            GenerateAdditionalStars(primaryObject, dice);

            // Assign star designations
            AssignStarDesignations();

            // Calculate orbital periods for all companion stars
            CalculateAllOrbitalPeriods();

            // Calculate orbital availability (min/max allowable orbits and unavailable ranges)
            CalculateOrbitalAvailability();

            // Calculate habitable zone center orbits for all non-Companion stars
            CalculateAllHZCO();

            // Determine non-stellar objects
            // D primary systems must first check if they have a planetary system at all
            bool hasPlanetarySystem = true;
            if (primaryObject.celestrialObject is Star pStar && pStar.type == "D")
            {
                hasPlanetarySystem = DetermineDPlanetarySystem(dice);
            }

            if (hasPlanetarySystem)
            {
                GasGiantCount = DetermineGasGiants(dice);
                PlanetoidBeltCount = DeterminePlanetoidBelts(dice, GasGiantCount);
                TerrestrialPlanetCount = DetermineTerrestrialPlanets(dice);
            }
            else
            {
                GasGiantCount = 0;
                PlanetoidBeltCount = 0;
                TerrestrialPlanetCount = 0;
            }

            // Calculate total available orbits and assign worlds to stars
            CalculateOrbitsAndWorlds();

            // Calculate System Baseline Numbers for primary star
            CalculateAllSystemBaselineNumbers();

            // Calculate Baseline Orbits for primary star
            CalculateAllBaselineOrbits();

            // Calculate empty orbits and distribute to stars
            CalculateEmptyOrbits(dice);

            // Calculate system spread for orbit placement
            CalculateSystemSpread();

            // Place orbits around primary star
            PlacePrimaryStarOrbits(dice);

            // Place orbits around companion stars
            PlaceCompanionStarOrbits(dice);

            // Generate anomalous orbits
            GenerateAnomalousOrbits(dice);

            // Place worlds in orbits
            PlaceWorlds(dice);

            // Generate moons for worlds
            GenerateMoons(dice);

            // Assign world designations
            AssignWorldDesignations();

            // Collect data for table-based output
            List<StarDisplayData> starData = CollectAllStarData();
            List<WorldDisplayData> worldData = CollectAllWorldData();

            // Print console output header
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("              TRAVELLER STAR SYSTEM GENERATION");
            Console.WriteLine($"                        Version {Version.VersionString}");
            Console.WriteLine($"                          Seed: {Seed}");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine();

            DebugLogger.LogSection("SYSTEM SUMMARY");

            // Print STELLAR summary
            PrintStellarSummary();

            // Print STARS table
            PrintStarsTable(starData);

            // Print OBJECTS table
            PrintObjectsTable(worldData);

            // Print console output footer
            Console.WriteLine("═══════════════════════════════════════════════════════════════");

            DebugLogger.Log("");
            DebugLogger.Log("NON-STELLAR OBJECTS SUMMARY:");
            DebugLogger.LogFormat("  Gas Giants: {0}", GasGiantCount);
            DebugLogger.LogFormat("  Planetoid Belts: {0}", PlanetoidBeltCount);
            DebugLogger.LogFormat("  Terrestrial Planets: {0}", TerrestrialPlanetCount);
            DebugLogger.LogFormat("  System Total Worlds: {0}", SystemTotalWorlds);
            DebugLogger.LogFormat("  System Total Available Orbits: {0:F2}", SystemTotalAvailableOrbits);


            //Console.WriteLine("Primary = " + GetProperty(primaryObject.celestrialObject, "type") + GetProperty(primaryObject.celestrialObject, "subType") + " " + GetProperty(primaryObject.celestrialObject, "starclass"));
            //if (primary.type != "BD" && primary.type != "D")

                //Console.WriteLine("Primary = " + primary.type + primary.subType + " " + primary.starclass);
            //if (GetProperty(primaryObject.celestrialObject, "type") != "BD" && GetProperty(primaryObject.celestrialObject, "type") != "D")
                //Console.WriteLine("Colour = " + GetProperty(primaryObject.celestrialObject, "colour"));
            //Console.WriteLine("Mass = " + GetProperty(primaryObject.celestrialObject, "mass"));
            //Console.WriteLine("Temperture = " + GetProperty(primaryObject.celestrialObject, "temperture"));
            //Console.WriteLine("Diameter = " + GetProperty(primaryObject.celestrialObject, "diameter"));
            //Console.WriteLine("Luminosity = " + GetProperty(primaryObject.celestrialObject, "luminosity"));
            //Console.WriteLine("Age = " + GetProperty(primaryObject.celestrialObject, "age"));

            //if(primaryObject.celestrialObjectOrbits.Count > 0)
            //{
            //    Console.WriteLine();

            //    Type sec = primaryObject.celestrialObjectOrbits[0].celestrialObject.GetType();
            //    Type secCO = primaryObject.celestrialObjectOrbits[0].GetType();
            //    PropertyInfo secType = sec.GetProperty("type");
            //    PropertyInfo secSubType = sec.GetProperty("subType");
            //    PropertyInfo secStarClass = sec.GetProperty("starclass");
            //    PropertyInfo secOrbit = secCO.GetProperty("orbit");
            //    Console.WriteLine("Close orbit star = " + secType.GetValue(primaryObject.celestrialObjectOrbits[0].celestrialObject, null).ToString() +
            //        secSubType.GetValue(primaryObject.celestrialObjectOrbits[0].celestrialObject, null).ToString() + " " +
            //        secStarClass.GetValue(primaryObject.celestrialObjectOrbits[0].celestrialObject, null).ToString());
            //    Console.WriteLine("Close orbit star orbit = " + secOrbit.GetValue(primaryObject.celestrialObjectOrbits[0], null).ToString());
            //}


        }

        private void GenerateAnomalousOrbits(Random dice)
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("GENERATING ANOMALOUS ORBITS");

            // Roll 2d6 for number of anomalous orbits
            int anomalousRoll = Starhelper.diceRoll(6, 2, dice);
            DebugLogger.LogDiceRoll(2, anomalousRoll, "Number of anomalous orbits");

            int anomalousCount = 0;
            if (anomalousRoll <= 9)
            {
                anomalousCount = 0;
            }
            else if (anomalousRoll == 10)
            {
                anomalousCount = 1;
            }
            else if (anomalousRoll == 11)
            {
                anomalousCount = 2;
            }
            else // 12+
            {
                anomalousCount = 3;
            }

            DebugLogger.LogFormat("Anomalous orbits to generate: {0}", anomalousCount);

            if (anomalousCount == 0)
            {
                DebugLogger.Log("No anomalous orbits for this system");
                return;
            }

            // Get all non-Companion orbit stars for orbit placement
            List<(Star star, CelestrialObject cobj)> availableStars = new List<(Star, CelestrialObject)>();

            // Add primary
            if (primaryObject.celestrialObject is Star primaryStar &&
                primaryStar.starOrbitType != Starhelper.starOrbitType.Companion)
            {
                availableStars.Add((primaryStar, primaryObject));
            }

            // Add Close/Near/Far companions
            foreach (var companionCobj in primaryObject.celestrialObjectOrbits)
            {
                if (companionCobj.celestrialObject is Star companionStar &&
                    companionStar.starOrbitType != Starhelper.starOrbitType.Companion)
                {
                    availableStars.Add((companionStar, companionCobj));
                }
            }

            if (availableStars.Count == 0)
            {
                DebugLogger.Log("No eligible stars for anomalous orbit placement");
                return;
            }

            DebugLogger.LogFormat("Found {0} eligible star(s) for anomalous orbits", availableStars.Count);

            // Generate each anomalous orbit
            for (int i = 0; i < anomalousCount; i++)
            {
                DebugLogger.Log("");
                DebugLogger.LogFormat("--- Anomalous Orbit {0}/{1} ---", i + 1, anomalousCount);

                // Roll for anomalous orbit type
                int typeRoll = Starhelper.diceRoll(6, 2, dice);
                DebugLogger.LogDiceRoll(2, typeRoll, "Anomalous orbit type");

                CelestialBodyType anomalousType;
                bool isTrojan = false;

                if (typeRoll <= 7)
                {
                    anomalousType = CelestialBodyType.Random;
                    DebugLogger.Log("Type: Random");
                }
                else if (typeRoll == 8)
                {
                    anomalousType = CelestialBodyType.Eccentric;
                    DebugLogger.Log("Type: Eccentric");
                }
                else if (typeRoll == 9)
                {
                    anomalousType = CelestialBodyType.Inclined;
                    DebugLogger.Log("Type: Inclined");
                }
                else if (typeRoll >= 10 && typeRoll <= 11)
                {
                    anomalousType = CelestialBodyType.Retrograde;
                    DebugLogger.Log("Type: Retrograde");
                }
                else // 12
                {
                    anomalousType = CelestialBodyType.Trojan;
                    isTrojan = true;
                    DebugLogger.Log("Type: Trojan");
                }

                // Handle Trojan type
                if (isTrojan)
                {
                    const int MAX_TROJAN_REROLLS = 5;
                    int trojanRerollCount = 0;
                    bool trojanPlaced = false;

                    while (!trojanPlaced && trojanRerollCount <= MAX_TROJAN_REROLLS)
                    {
                        // Get all celestial body orbits from all eligible stars
                        List<(CelestrialObject cobj, Star star)> allCelestialBodies = new List<(CelestrialObject, Star)>();

                        foreach (var (star, starCobj) in availableStars)
                        {
                            var celestialBodies = starCobj.celestrialObjectOrbits
                                .Where(o => o.celestrialObject is CelestialBody)
                                .ToList();

                            foreach (var body in celestialBodies)
                            {
                                allCelestialBodies.Add((body, star));
                            }
                        }

                        DebugLogger.LogFormat("  Found {0} total celestial bodies for potential Trojan placement",
                            allCelestialBodies.Count);

                        // Check if we have gas giants or terrestrial planets
                        if ((GasGiantCount + TerrestrialPlanetCount) > 0 && allCelestialBodies.Count > 0)
                        {
                            // Randomly select a celestial body
                            int selectedIndex = dice.Next(allCelestialBodies.Count);
                            var (selectedCobj, selectedStar) = allCelestialBodies[selectedIndex];

                            // Change its type to Trojan
                            if (selectedCobj.celestrialObject is CelestialBody celestialBody)
                            {
                                celestialBody.Type = CelestialBodyType.Trojan;
                                DebugLogger.LogFormat("  ✓ Trojan placed at orbit {0:F4} around {1} star",
                                    selectedCobj.orbit, selectedStar.starOrbitType);
                                trojanPlaced = true;

                                // Increment world counts for anomalous orbit
                                if (TerrestrialPlanetCount < 13)
                                {
                                    TerrestrialPlanetCount++;
                                    DebugLogger.LogFormat("  Terrestrial Planets increased to {0}", TerrestrialPlanetCount);
                                }
                                else
                                {
                                    PlanetoidBeltCount++;
                                    DebugLogger.LogFormat("  Terrestrial Planets at cap (13), Planetoid Belts increased to {0}", PlanetoidBeltCount);
                                }
                            }
                        }
                        else
                        {
                            // No planets available - re-roll type
                            trojanRerollCount++;

                            if (trojanRerollCount > MAX_TROJAN_REROLLS)
                            {
                                DebugLogger.LogFormat("  ✗ No Gas Giants or Terrestrial Planets available for Trojan after {0} re-rolls",
                                    MAX_TROJAN_REROLLS);
                                DebugLogger.Log("  Skipping this anomalous orbit");
                                break;
                            }

                            DebugLogger.LogFormat("  No Gas Giants or Terrestrial Planets available for Trojan (re-roll {0}/{1})",
                                trojanRerollCount, MAX_TROJAN_REROLLS);

                            // Re-roll type
                            typeRoll = Starhelper.diceRoll(6, 2, dice);
                            DebugLogger.LogDiceRoll(2, typeRoll, "Re-rolled anomalous orbit type");

                            if (typeRoll <= 7)
                            {
                                anomalousType = CelestialBodyType.Random;
                                DebugLogger.Log("  New type: Random");
                                isTrojan = false;
                            }
                            else if (typeRoll == 8)
                            {
                                anomalousType = CelestialBodyType.Eccentric;
                                DebugLogger.Log("  New type: Eccentric");
                                isTrojan = false;
                            }
                            else if (typeRoll == 9)
                            {
                                anomalousType = CelestialBodyType.Inclined;
                                DebugLogger.Log("  New type: Inclined");
                                isTrojan = false;
                            }
                            else if (typeRoll >= 10 && typeRoll <= 11)
                            {
                                anomalousType = CelestialBodyType.Retrograde;
                                DebugLogger.Log("  New type: Retrograde");
                                isTrojan = false;
                            }
                            else // 12 - rolled Trojan again
                            {
                                DebugLogger.Log("  Rolled Trojan again");
                                // Continue loop to re-roll again
                            }
                        }
                    }

                    // If we successfully placed Trojan, continue to next anomalous orbit
                    if (trojanPlaced)
                    {
                        continue;
                    }

                    // If we exhausted re-rolls and still have Trojan, skip this anomalous orbit entirely
                    if (isTrojan && trojanRerollCount > MAX_TROJAN_REROLLS)
                    {
                        continue;
                    }
                }

                // Handle Random, Eccentric, Inclined, Retrograde types
                // These all use the random allocation function

                // Randomly select a star
                int starIndex = dice.Next(availableStars.Count);
                var (targetStar, targetStarCobj) = availableStars[starIndex];

                DebugLogger.LogFormat("  Selected {0} star for placement", targetStar.starOrbitType);

                // Select a random valid orbit
                float selectedOrbit = SelectRandomAvailableOrbit(targetStar, targetStarCobj, dice);

                if (selectedOrbit < 0)
                {
                    DebugLogger.LogFormat("  ✗ Could not find valid orbit for {0} anomalous orbit - skipping", anomalousType);
                    continue;
                }

                // Add celestial body with anomalous type
                targetStarCobj.AddCelestialBody(selectedOrbit, dice);

                // Get the just-added celestial body and set its type
                var lastAddedCobj = targetStarCobj.celestrialObjectOrbits[targetStarCobj.celestrialObjectOrbits.Count - 1];
                if (lastAddedCobj.celestrialObject is CelestialBody newBody)
                {
                    newBody.Type = anomalousType;
                    float orbitAU = lastAddedCobj.orbitAU;
                    DebugLogger.LogFormat("  ✓ {0} anomalous orbit placed at {1:F4} ({2:F2} AU) around {3} star",
                        anomalousType, selectedOrbit, orbitAU, targetStar.starOrbitType);

                    // Increment world counts for anomalous orbit
                    if (TerrestrialPlanetCount < 13)
                    {
                        TerrestrialPlanetCount++;
                        DebugLogger.LogFormat("  Terrestrial Planets increased to {0}", TerrestrialPlanetCount);
                    }
                    else
                    {
                        PlanetoidBeltCount++;
                        DebugLogger.LogFormat("  Terrestrial Planets at cap (13), Planetoid Belts increased to {0}", PlanetoidBeltCount);
                    }
                }
            }

            DebugLogger.Log("");
            DebugLogger.Log("Anomalous orbit generation complete");
        }

        private void PlaceWorlds(Random dice)
        {
            DebugLogger.Log("");
            DebugLogger.Log("═══════════════════════════════════════════════════════════════");
            DebugLogger.Log("Starting world placement...");
            DebugLogger.Log("═══════════════════════════════════════════════════════════════");

            // Step 1: Place Empty orbits
            PlaceEmptyOrbits(dice);

            // Step 2: Place Gas Giants
            PlaceGasGiants(dice);

            // Step 3: Place Planetoid Belts
            PlacePlanetoidBelts(dice);

            // Step 4: Handle Trojan orbits
            HandleTrojanOrbits(dice);

            // Step 5: Place Terrestrial Planets
            PlaceTerrestrialPlanets(dice);

            DebugLogger.Log("");
            DebugLogger.Log("World placement complete");
        }

        private void GenerateMoons(Random dice)
        {
            DebugLogger.Log("");
            DebugLogger.Log("═══════════════════════════════════════════════════════════════");
            DebugLogger.Log("Generating significant moons...");
            DebugLogger.Log("═══════════════════════════════════════════════════════════════");

            // Generate moons for primary star's worlds
            if (primaryObject.celestrialObject is Star primaryStar)
            {
                foreach (var bodyObj in primaryObject.celestrialObjectOrbits)
                {
                    if (bodyObj.celestrialObject is CelestialBody body)
                    {
                        if (body is TerrestrialPlanet || body is GasGiant)
                        {
                            GenerateWorldMoons(body, bodyObj, primaryStar, dice);
                        }
                    }
                }
            }

            // Generate moons for companion stars' worlds
            foreach (var companionObj in primaryObject.celestrialObjectOrbits)
            {
                if (companionObj.celestrialObject is Star companionStar)
                {
                    foreach (var bodyObj in companionObj.celestrialObjectOrbits)
                    {
                        if (bodyObj.celestrialObject is CelestialBody body)
                        {
                            if (body is TerrestrialPlanet || body is GasGiant)
                            {
                                GenerateWorldMoons(body, bodyObj, companionStar, dice);
                            }
                        }
                    }
                }
            }

            DebugLogger.Log("");
            DebugLogger.Log("Moon generation complete");
        }

        private void GenerateWorldMoons(CelestialBody body, CelestrialObject bodyObj, Star parentStar, Random dice)
        {
            int moonCount = DetermineMoonCount(body, bodyObj, parentStar, dice);

            if (moonCount <= 0)
            {
                DebugLogger.LogFormat("  {0} has no significant moons", body.Designation);
                return;
            }

            DebugLogger.LogFormat("  {0} has {1} significant moon(s)", body.Designation, moonCount);

            // Generate moons
            for (int i = 0; i < moonCount; i++)
            {
                Moon moon = new Moon();
                moon.Designation = ((char)('a' + i)).ToString();
                moon.Size = DetermineMoonSize(body, dice);

                if (body is TerrestrialPlanet tp)
                {
                    tp.Moons.Add(moon);
                }
                else if (body is GasGiant gg)
                {
                    gg.Moons.Add(moon);
                }

                DebugLogger.LogFormat("    Moon {0}: Size {1}", moon.Designation, moon.Size);
            }
        }

        private int DetermineMoonCount(CelestialBody body, CelestrialObject bodyObj, Star parentStar, Random dice)
        {
            // Get world size for calculation
            string worldSize = "";
            if (body is TerrestrialPlanet tp)
                worldSize = tp.Size;
            else if (body is GasGiant gg)
                worldSize = gg.Size;

            if (string.IsNullOrEmpty(worldSize))
                return 0;

            int numDice = 0;
            int modifier = 0;

            // Determine base dice and modifier based on world size
            if (body is TerrestrialPlanet)
            {
                int size = FromEhex(worldSize);
                if (size >= 1 && size <= 2)
                {
                    numDice = 1;
                    modifier = -5;
                }
                else if (size >= 3 && size <= 9)
                {
                    numDice = 2;
                    modifier = -8;
                }
                else if (size >= 10) // A-F
                {
                    numDice = 2;
                    modifier = -6;
                }
                else // 0, S, R
                {
                    return 0;
                }
            }
            else if (body is GasGiant gg)
            {
                if (gg.Size == "GS")
                {
                    numDice = 3;
                    modifier = -7;
                }
                else // GM or GL
                {
                    numDice = 4;
                    modifier = -6;
                }
            }

            // Apply modifiers for special conditions
            int perDieModifier = 0;

            // -1 per die if orbit < 1.0
            if (bodyObj.orbit < 1.0f)
                perDieModifier -= 1;

            // -1 per die if planet is next to a companion star
            if (IsNextToCompanionStar(bodyObj, parentStar))
                perDieModifier -= 1;

            // -1 per die if orbit within spread of unavailable range
            if (IsWithinUnavailableSpread(bodyObj, parentStar))
                perDieModifier -= 1;

            // -1 per die if orbiting Close/Near/Far star and within spread of MAO
            if (parentStar.starOrbitType != Starhelper.starOrbitType.Primary)
            {
                if (IsWithinMAOSpread(bodyObj, parentStar))
                    perDieModifier -= 1;
            }

            // Apply per-die modifier to total modifier
            modifier += perDieModifier * numDice;

            int roll = Starhelper.diceRoll(6, numDice, dice);
            int moonCount = Math.Max(0, roll + modifier);

            DebugLogger.LogFormat("  {0} moon count: {1}d6{2:+#;-#;+0} = {3} + {4} = {5}",
                body.Designation, numDice, modifier, numDice, roll, modifier, moonCount);

            return moonCount;
        }

        private bool IsNextToCompanionStar(CelestrialObject bodyObj, Star parentStar)
        {
            // Check if the world's orbit is adjacent to a companion star's orbit
            // Get all companion stars
            var companionStars = primaryObject.celestrialObjectOrbits
                .Where(obj => obj.celestrialObject is Star && obj.celestrialObject != parentStar)
                .ToList();

            foreach (var companionObj in companionStars)
            {
                // Check if this world's orbit is the next occupied orbit above or below the companion
                float orbitDiff = Math.Abs(bodyObj.orbit - companionObj.orbit);
                if (orbitDiff <= 1.0f) // Within one orbit step
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsWithinUnavailableSpread(CelestrialObject bodyObj, Star parentStar)
        {
            float spread = parentStar.SystemSpread;

            foreach (var unavailableRange in parentStar.UnavailableOrbitRanges)
            {
                float rangeStart = unavailableRange.min;
                float rangeEnd = unavailableRange.max;

                // Check if world orbit is within spread distance of the unavailable range
                if (bodyObj.orbit >= rangeStart - spread && bodyObj.orbit <= rangeEnd + spread)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsWithinMAOSpread(CelestrialObject bodyObj, Star parentStar)
        {
            float spread = parentStar.SystemSpread;
            float mao = parentStar.MaxAllowableOrbit;

            // Check if world orbit is within spread of MAO
            return bodyObj.orbit >= mao - spread && bodyObj.orbit <= mao + spread;
        }

        private string DetermineMoonSize(CelestialBody parentBody, Random dice)
        {
            int roll = Starhelper.diceRoll(6, 1, dice);

            if (roll >= 1 && roll <= 3)
            {
                return "S";
            }
            else if (roll >= 4 && roll <= 5)
            {
                int sizeRoll = Starhelper.diceRoll(3, 1, dice) - 1;
                return sizeRoll == 0 ? "R" : ToEhex(sizeRoll);
            }
            else // roll == 6
            {
                if (parentBody is TerrestrialPlanet tp)
                {
                    return DetermineTerrestrialMoonSize(tp, dice);
                }
                else if (parentBody is GasGiant gg)
                {
                    return DetermineGasGiantMoonSize(gg, dice);
                }
            }

            return "S";
        }

        private string DetermineTerrestrialMoonSize(TerrestrialPlanet parent, Random dice)
        {
            int parentSize = FromEhex(parent.Size);

            if (parentSize == 1)
                return "S";

            int moonSize = parentSize - 1 - Starhelper.diceRoll(6, 1, dice);

            if (moonSize <= 0)
                return "R";

            // Check for special case where moon size is >= parent size - 2
            if (moonSize >= parentSize - 2)
            {
                int specialRoll = Starhelper.diceRoll(6, 2, dice);
                if (specialRoll == 2)
                    moonSize = parentSize - 1;
                else if (specialRoll == 12)
                    moonSize = parentSize;
            }

            return ToEhex(moonSize);
        }

        private string DetermineGasGiantMoonSize(GasGiant parent, Random dice)
        {
            int roll = Starhelper.diceRoll(6, 1, dice);

            if (roll >= 1 && roll <= 3)
            {
                return ToEhex(Starhelper.diceRoll(6, 1, dice));
            }
            else if (roll >= 4 && roll <= 5)
            {
                return ToEhex(Starhelper.diceRoll(6, 2, dice) - 2);
            }
            else // roll == 6
            {
                int size = Starhelper.diceRoll(6, 2, dice) + 4;

                if (size == 16)
                {
                    return "GS";
                }
                else if (parent.Size == "GL" && size >= 16)
                {
                    int gmRoll = Starhelper.diceRoll(6, 2, dice);
                    if (gmRoll == 12)
                        return "GM";
                }

                return ToEhex(size);
            }
        }

        private int FromEhex(string ehex)
        {
            if (string.IsNullOrEmpty(ehex))
                return 0;

            if (int.TryParse(ehex, out int number))
                return number;

            char c = ehex.ToUpper()[0];
            if (c >= 'A' && c <= 'Z')
                return 10 + (c - 'A');

            return 0;
        }

        private void PlaceEmptyOrbits(Random dice)
        {
            DebugLogger.Log("");
            DebugLogger.Log("Placing Empty orbits...");

            // Get all stars with EmptyOrbits > 0
            List<Star> starsNeedingEmpty = new List<Star>();

            if (primaryObject.celestrialObject is Star primaryStar && primaryStar.EmptyOrbits > 0)
            {
                starsNeedingEmpty.Add(primaryStar);
            }

            foreach (var companionObj in primaryObject.celestrialObjectOrbits)
            {
                if (companionObj.celestrialObject is Star companionStar && companionStar.EmptyOrbits > 0)
                {
                    starsNeedingEmpty.Add(companionStar);

                    // Check sub-companions
                    foreach (var subCompanionObj in companionObj.celestrialObjectOrbits)
                    {
                        if (subCompanionObj.celestrialObject is Star subStar && subStar.EmptyOrbits > 0)
                        {
                            starsNeedingEmpty.Add(subStar);
                        }
                    }
                }
            }

            // For each star, select one orbit and make it Empty
            foreach (var star in starsNeedingEmpty)
            {
                DebugLogger.LogFormat("  Processing star with {0} Empty orbits needed", star.EmptyOrbits);

                // Get all Filled celestial bodies for this star
                List<CelestrialObject> filledOrbits = new List<CelestrialObject>();

                CelestrialObject? starCobj = FindCelestrialObjectForStar(star);
                if (starCobj != null)
                {
                    foreach (var cobj in starCobj.celestrialObjectOrbits)
                    {
                        if (cobj.celestrialObject is CelestialBody cb && cb.Type == CelestialBodyType.Filled)
                        {
                            filledOrbits.Add(cobj);
                        }
                    }
                }

                if (filledOrbits.Count == 0)
                {
                    DebugLogger.Log("    WARNING: No Filled orbits available for Empty orbit placement");
                    continue;
                }

                // Filter out anomalous orbits if possible
                List<CelestrialObject> candidateOrbits = filledOrbits.Where(c =>
                {
                    if (c.celestrialObject is CelestialBody cb)
                    {
                        return cb.Type == CelestialBodyType.Filled; // Only non-anomalous
                    }
                    return false;
                }).ToList();

                // If filtering removed all candidates, use all filled orbits
                if (candidateOrbits.Count == 0)
                {
                    candidateOrbits = filledOrbits;
                }

                // Filter out innermost and outermost if at least 3 orbits exist
                if (candidateOrbits.Count >= 3)
                {
                    float minOrbit = candidateOrbits.Min(c => c.orbit);
                    float maxOrbit = candidateOrbits.Max(c => c.orbit);
                    candidateOrbits = candidateOrbits.Where(c => c.orbit != minOrbit && c.orbit != maxOrbit).ToList();
                }

                // If we still have no candidates, use all filled orbits
                if (candidateOrbits.Count == 0)
                {
                    candidateOrbits = filledOrbits;
                }

                // Randomly select one orbit
                int selectedIndex = Starhelper.diceRoll(candidateOrbits.Count, 1, dice) - 1;
                CelestrialObject selectedOrbit = candidateOrbits[selectedIndex];

                // Replace with EmptyOrbit
                selectedOrbit.celestrialObject = new EmptyOrbit();
                DebugLogger.LogFormat("    Placed Empty orbit at {0:F3}", selectedOrbit.orbit);
            }

            DebugLogger.Log("  Empty orbit placement complete");
        }

        private CelestrialObject? FindCelestrialObjectForStar(Star star)
        {
            if (primaryObject.celestrialObject == star)
            {
                return primaryObject;
            }

            foreach (var companionObj in primaryObject.celestrialObjectOrbits)
            {
                if (companionObj.celestrialObject == star)
                {
                    return companionObj;
                }

                foreach (var subCompanionObj in companionObj.celestrialObjectOrbits)
                {
                    if (subCompanionObj.celestrialObject == star)
                    {
                        return subCompanionObj;
                    }
                }
            }

            return null;
        }

        private void PlaceGasGiants(Random dice)
        {
            DebugLogger.Log("");
            DebugLogger.LogFormat("Placing {0} Gas Giants...", GasGiantCount);

            // Get all Filled bodies (available for placement)
            var emptyOrbits = GetAllCelestialBodiesOfType(CelestialBodyType.Filled);

            if (emptyOrbits.Count < GasGiantCount)
            {
                DebugLogger.LogFormat("  WARNING: Not enough Empty orbits ({0}) for Gas Giants ({1})", emptyOrbits.Count, GasGiantCount);
            }

            // Randomly select GasGiantCount orbits
            int planetsToPlace = Math.Min(GasGiantCount, emptyOrbits.Count);
            for (int i = 0; i < planetsToPlace; i++)
            {
                int selectedIndex = Starhelper.diceRoll(emptyOrbits.Count, 1, dice) - 1;
                var (cobj, parentStar) = emptyOrbits[selectedIndex];
                emptyOrbits.RemoveAt(selectedIndex);

                // Create Gas Giant
                GasGiant gasGiant = new GasGiant();

                // Check for anomalous orbit type
                if (cobj.celestrialObject is CelestialBody cb)
                {
                    if (cb.Type == CelestialBodyType.Random || cb.Type == CelestialBodyType.Eccentric ||
                        cb.Type == CelestialBodyType.Inclined || cb.Type == CelestialBodyType.Retrograde ||
                        cb.Type == CelestialBodyType.Trojan)
                    {
                        gasGiant.Type = cb.Type; // Preserve anomalous type
                    }
                }

                cobj.celestrialObject = gasGiant;

                // Calculate eccentricity with modifiers
                int modifier = 0;
                if (gasGiant.Type == CelestialBodyType.Random || gasGiant.Type == CelestialBodyType.Eccentric)
                {
                    modifier = 2;
                }

                // Calculate inclination for Eccentric orbits
                if (gasGiant.Type == CelestialBodyType.Eccentric)
                {
                    int inclinationRoll = Starhelper.diceRoll(6, 1, dice);
                    gasGiant.Inclination = ((inclinationRoll + 2) * 10) + 10;
                    DebugLogger.LogFormat("    Eccentric orbit inclination: {0:F0}°", gasGiant.Inclination);
                }

                int starsOrbited = CountStarsOrbitedByPlanet(parentStar);
                cobj.OrbitEccentricity(starsOrbited, dice, belt: false, modifier: modifier);

                // Calculate orbital period
                CalculateOrbitalPeriod(cobj);

                // Determine gas giant size
                DetermineGasGiantSize(gasGiant, dice);

                DebugLogger.LogFormat("  Placed Gas Giant at orbit {0:F3} (Size:{1}-{2}, Mass:{3}, e:{4:F3}, P:{5})",
                    cobj.orbit, gasGiant.Size, ToEhex(gasGiant.Diameter), gasGiant.GasGiantMass,
                    cobj.orbitEccentricity, FormatOrbitalPeriod(cobj.OrbitalPeriodYears));
            }

            DebugLogger.Log("  Gas Giant placement complete");
        }

        private void DetermineGasGiantSize(GasGiant gasGiant, Random dice)
        {
            // Get primary star for modifiers
            Star? primaryStar = primaryObject.celestrialObject as Star;
            if (primaryStar == null) return;

            // Roll 1d6
            int roll = Starhelper.diceRoll(6, 1, dice);
            int modifier = 0;

            DebugLogger.LogFormat("    Gas Giant size roll: {0}", roll);

            // Modifier: -1 if primary is BD, M-Type Class V, or any Class VI
            if (primaryStar.type == "BD" ||
                (primaryStar.type == "M" && primaryStar.starclass == "V") ||
                primaryStar.starclass == "VI")
            {
                modifier -= 1;
                DebugLogger.LogFormat("    Primary is {0}{1} {2} - applying -1 modifier",
                    primaryStar.type, primaryStar.subType, primaryStar.starclass);
            }

            // Modifier: -1 if Spread < 0.1
            if (primaryStar.SystemSpread < 0.1f)
            {
                modifier -= 1;
                DebugLogger.LogFormat("    System Spread {0:F4} < 0.1 - applying -1 modifier", primaryStar.SystemSpread);
            }

            int finalRoll = roll + modifier;
            DebugLogger.LogFormat("    Final roll: {0} + {1} = {2}", roll, modifier, finalRoll);

            // Determine size based on final roll
            if (finalRoll <= 2)
            {
                // Small: GS
                gasGiant.Size = "GS";
                int diam1 = Starhelper.diceRoll(3, 1, dice);
                int diam2 = Starhelper.diceRoll(3, 1, dice);
                gasGiant.Diameter = diam1 + diam2; // 2-6
                int massRoll = Starhelper.diceRoll(6, 1, dice);
                gasGiant.GasGiantMass = 5 * (massRoll + 1); // 10-35
                DebugLogger.LogFormat("    Size: GS (Small), Diameter: {0}, Mass: {1}", gasGiant.Diameter, gasGiant.GasGiantMass);
            }
            else if (finalRoll >= 3 && finalRoll <= 4)
            {
                // Medium: GM
                gasGiant.Size = "GM";
                int diamRoll = Starhelper.diceRoll(6, 1, dice);
                gasGiant.Diameter = diamRoll + 6; // 7-12
                int massRoll = Starhelper.diceRoll(6, 3, dice);
                gasGiant.GasGiantMass = 10 * (massRoll - 1); // 20-170
                DebugLogger.LogFormat("    Size: GM (Medium), Diameter: {0}, Mass: {1}", gasGiant.Diameter, gasGiant.GasGiantMass);
            }
            else // >= 5
            {
                // Large: GL
                gasGiant.Size = "GL";
                int diamRoll = Starhelper.diceRoll(6, 2, dice);
                gasGiant.Diameter = diamRoll + 6; // 8-18
                int massMultiplier = Starhelper.diceRoll(3, 1, dice);
                int massRoll = Starhelper.diceRoll(6, 3, dice);
                gasGiant.GasGiantMass = massMultiplier * 50 * (massRoll + 4); // 350-1650

                // Check if mass >= 3000, then recalculate
                if (gasGiant.GasGiantMass >= 3000)
                {
                    int rerollDice = Starhelper.diceRoll(6, 2, dice);
                    gasGiant.GasGiantMass = 4000 - ((rerollDice - 2) * 200); // 2000-3800
                    DebugLogger.LogFormat("    Mass >= 3000, recalculated to: {0}", gasGiant.GasGiantMass);
                }
                DebugLogger.LogFormat("    Size: GL (Large), Diameter: {0}, Mass: {1}", gasGiant.Diameter, gasGiant.GasGiantMass);
            }
        }

        private void PlacePlanetoidBelts(Random dice)
        {
            DebugLogger.Log("");
            DebugLogger.LogFormat("Placing {0} Planetoid Belts...", PlanetoidBeltCount);

            // Get all remaining Filled bodies, filter out Retrograde and Trojan
            var emptyOrbits = GetAllCelestialBodiesOfType(CelestialBodyType.Filled)
                .Where(item =>
                {
                    if (item.cobj.celestrialObject is CelestialBody cb)
                    {
                        return cb.Type != CelestialBodyType.Retrograde && cb.Type != CelestialBodyType.Trojan;
                    }
                    return true;
                }).ToList();

            if (emptyOrbits.Count < PlanetoidBeltCount)
            {
                DebugLogger.LogFormat("  WARNING: Not enough Empty orbits ({0}) for Planetoid Belts ({1})", emptyOrbits.Count, PlanetoidBeltCount);
            }

            // Randomly select PlanetoidBeltCount orbits
            int beltsToPlace = Math.Min(PlanetoidBeltCount, emptyOrbits.Count);
            for (int i = 0; i < beltsToPlace; i++)
            {
                int selectedIndex = Starhelper.diceRoll(emptyOrbits.Count, 1, dice) - 1;
                var (cobj, parentStar) = emptyOrbits[selectedIndex];
                emptyOrbits.RemoveAt(selectedIndex);

                // Create Planetoid Belt
                PlanetoidBelt belt = new PlanetoidBelt();

                // Check for anomalous orbit type (but not Retrograde or Trojan as filtered above)
                if (cobj.celestrialObject is CelestialBody cb)
                {
                    if (cb.Type == CelestialBodyType.Random || cb.Type == CelestialBodyType.Eccentric ||
                        cb.Type == CelestialBodyType.Inclined)
                    {
                        belt.Type = cb.Type; // Preserve anomalous type
                    }
                }

                cobj.celestrialObject = belt;

                // Calculate eccentricity with belt=true and modifiers
                int modifier = 0;
                if (belt.Type == CelestialBodyType.Random || belt.Type == CelestialBodyType.Eccentric)
                {
                    modifier = 2;
                }

                int starsOrbited = CountStarsOrbitedByPlanet(parentStar);
                cobj.OrbitEccentricity(starsOrbited, dice, belt: true, modifier: modifier);

                // Calculate orbital period
                CalculateOrbitalPeriod(cobj);

                DebugLogger.LogFormat("  Placed Planetoid Belt at orbit {0:F3} (e:{1:F3}, P:{2})",
                    cobj.orbit, cobj.orbitEccentricity, FormatOrbitalPeriod(cobj.OrbitalPeriodYears));
            }

            DebugLogger.Log("  Planetoid Belt placement complete");
        }

        private void HandleTrojanOrbits(Random dice)
        {
            DebugLogger.Log("");
            DebugLogger.Log("Handling Trojan orbits...");

            // Get all orbits with Trojan anomalous type
            var trojanOrbits = GetAllCelestialBodiesOfType(CelestialBodyType.Trojan);

            foreach (var (cobj, parentStar) in trojanOrbits)
            {
                DebugLogger.LogFormat("  Processing Trojan orbit at {0:F3}", cobj.orbit);

                // Determine primary body type (Gas Giant or Terrestrial Planet)
                CelestialBody? primaryBody = null;
                CelestialBody? trojanBody = null;

                // If already placed (Gas Giant), use it as primary
                if (cobj.celestrialObject is GasGiant)
                {
                    primaryBody = (CelestialBody)cobj.celestrialObject;
                    trojanBody = new TerrestrialPlanet();
                }
                else
                {
                    // Randomly determine: 50/50 Gas Giant or Terrestrial Planet
                    bool primaryIsGasGiant = Starhelper.diceRoll(2, 1, dice) == 1;
                    bool trojanIsGasGiant = Starhelper.diceRoll(2, 1, dice) == 1;

                    // If at least one is Gas Giant, make it the primary
                    if (primaryIsGasGiant && !trojanIsGasGiant)
                    {
                        primaryBody = new GasGiant();
                        trojanBody = new TerrestrialPlanet();
                    }
                    else if (!primaryIsGasGiant && trojanIsGasGiant)
                    {
                        primaryBody = new TerrestrialPlanet();
                        trojanBody = new GasGiant();
                    }
                    else if (primaryIsGasGiant && trojanIsGasGiant)
                    {
                        primaryBody = new GasGiant();
                        trojanBody = new GasGiant();
                    }
                    else
                    {
                        primaryBody = new TerrestrialPlanet();
                        trojanBody = new TerrestrialPlanet();
                    }

                    // Set the primary body
                    cobj.celestrialObject = primaryBody;
                }

                // Set Trojan position (L4 or L5)
                string trojanPosition = Starhelper.diceRoll(2, 1, dice) == 1 ? "L4" : "L5";
                if (trojanBody is GasGiant gg)
                {
                    gg.TrojanPosition = trojanPosition;
                    DetermineGasGiantSize(gg, dice);
                }
                else if (trojanBody is TerrestrialPlanet tp)
                {
                    tp.TrojanPosition = trojanPosition;
                    tp.Size = DetermineTerrestrialSize(dice);
                }

                // Determine size for primary based on type
                if (primaryBody is TerrestrialPlanet primaryTp)
                {
                    primaryTp.Size = DetermineTerrestrialSize(dice);
                }
                else if (primaryBody is GasGiant primaryGg)
                {
                    DetermineGasGiantSize(primaryGg, dice);
                }

                // Calculate eccentricity for primary (if not already calculated)
                if (cobj.orbitEccentricity == 0)
                {
                    int starsOrbited = CountStarsOrbitedByPlanet(parentStar);
                    cobj.OrbitEccentricity(starsOrbited, dice, belt: false, modifier: 0);
                    CalculateOrbitalPeriod(cobj);
                }

                // Create second CelestrialObject for Trojan companion
                CelestrialObject trojanCobj = new CelestrialObject();
                trojanCobj.orbit = cobj.orbit;
                trojanCobj.orbitAU = cobj.orbitAU;
                trojanCobj.orbitEccentricity = cobj.orbitEccentricity;
                trojanCobj.OrbitalPeriodYears = cobj.OrbitalPeriodYears;
                trojanCobj.celestrialObject = trojanBody;

                // Add to parent star's orbits
                CelestrialObject? parentCobj = FindCelestrialObjectForStar(parentStar);
                if (parentCobj != null)
                {
                    parentCobj.celestrialObjectOrbits.Add(trojanCobj);

                    // Re-sort the orbits
                    parentCobj.celestrialObjectOrbits = parentCobj.celestrialObjectOrbits.OrderBy(o => o.orbit).ToList();
                }

                DebugLogger.LogFormat("    Created Trojan companion at {0} position", trojanPosition);
            }

            DebugLogger.Log("  Trojan orbit handling complete");
        }

        private void PlaceTerrestrialPlanets(Random dice)
        {
            DebugLogger.Log("");
            DebugLogger.Log("Placing Terrestrial Planets in remaining orbits...");

            // Get all remaining Filled bodies
            var emptyOrbits = GetAllCelestialBodiesOfType(CelestialBodyType.Filled);

            DebugLogger.LogFormat("  Found {0} Empty orbits to fill with Terrestrial Planets", emptyOrbits.Count);

            foreach (var (cobj, parentStar) in emptyOrbits)
            {
                // Create Terrestrial Planet
                TerrestrialPlanet planet = new TerrestrialPlanet();

                // Check for anomalous orbit type
                if (cobj.celestrialObject is CelestialBody cb)
                {
                    if (cb.Type == CelestialBodyType.Random || cb.Type == CelestialBodyType.Eccentric ||
                        cb.Type == CelestialBodyType.Inclined || cb.Type == CelestialBodyType.Retrograde)
                    {
                        planet.Type = cb.Type; // Preserve anomalous type
                    }
                }

                cobj.celestrialObject = planet;

                // Calculate eccentricity with modifiers
                int modifier = 0;
                if (planet.Type == CelestialBodyType.Random || planet.Type == CelestialBodyType.Eccentric)
                {
                    modifier = 2;
                }

                // Calculate inclination for Eccentric orbits
                if (planet.Type == CelestialBodyType.Eccentric)
                {
                    int inclinationRoll = Starhelper.diceRoll(6, 1, dice);
                    planet.Inclination = ((inclinationRoll + 2) * 10) + 10;
                }

                int starsOrbited = CountStarsOrbitedByPlanet(parentStar);
                cobj.OrbitEccentricity(starsOrbited, dice, belt: false, modifier: modifier);

                // Calculate orbital period
                CalculateOrbitalPeriod(cobj);

                // Determine planet size
                planet.Size = DetermineTerrestrialSize(dice);

                DebugLogger.LogFormat("  Placed Terrestrial Planet at orbit {0:F3} (Size:{1}, e:{2:F3}, P:{3})",
                    cobj.orbit, planet.Size, cobj.orbitEccentricity, FormatOrbitalPeriod(cobj.OrbitalPeriodYears));
            }

            DebugLogger.Log("  Terrestrial Planet placement complete");
        }

        private string DetermineTerrestrialSize(Random dice)
        {
            // First roll: 1d6
            int firstRoll = Starhelper.diceRoll(6, 1, dice);
            int size = 0;

            DebugLogger.LogFormat("    First roll: {0}", firstRoll);

            // Determine size based on first roll
            if (firstRoll >= 1 && firstRoll <= 2)
            {
                // Roll 1d6, size range 1-6
                size = Starhelper.diceRoll(6, 1, dice);
                DebugLogger.LogFormat("    Second roll (1d6): {0}", size);
            }
            else if (firstRoll >= 3 && firstRoll <= 4)
            {
                // Roll 2d6, size range 2-12 (C)
                size = Starhelper.diceRoll(6, 2, dice);
                DebugLogger.LogFormat("    Second roll (2d6): {0}", size);
            }
            else // 5-6
            {
                // Roll 2d6+3, size range 5-15 (F)
                size = Starhelper.diceRoll(6, 2, dice) + 3;
                DebugLogger.LogFormat("    Second roll (2d6+3): {0}", size);
            }

            // Convert size to code (0, S, 1-9, A-F)
            string sizeCode;
            if (size == 0)
                sizeCode = "0";
            else if (size >= 1 && size <= 9)
                sizeCode = size.ToString();
            else if (size == 10)
                sizeCode = "A";
            else if (size == 11)
                sizeCode = "B";
            else if (size == 12)
                sizeCode = "C";
            else if (size == 13)
                sizeCode = "D";
            else if (size == 14)
                sizeCode = "E";
            else if (size == 15)
                sizeCode = "F";
            else
                sizeCode = "0"; // Fallback

            DebugLogger.LogFormat("    Size code: {0}", sizeCode);
            return sizeCode;
        }

        private void PrintStar (Star star, float orbit, CelestrialObject Cobj, Random dice)
        {
            if (star.starOrbitType == Starhelper.starOrbitType.Primary)
            {
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine($"PRIMARY STAR ({star.Designation})");
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                DebugLogger.Log("");
                DebugLogger.LogFormat("{0} STAR ({1}):", star.starOrbitType.ToString().ToUpper(), star.Designation);
            }
            else
            {
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine($"{star.starOrbitType.ToString().ToUpper()} COMPANION STAR ({star.Designation})");
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine($"Orbital Position:    {orbit:F2} ({Cobj.orbitAU:F2} AU)");
                Console.WriteLine($"Eccentricity:        {Cobj.orbitEccentricity:F3}");

                DebugLogger.Log("");
                DebugLogger.LogFormat("{0} STAR ({1}):", star.starOrbitType.ToString().ToUpper(), star.Designation);
                DebugLogger.LogFormat("  Orbit: {0:F2} ({1:F2} AU)", orbit, Cobj.orbitAU);
                DebugLogger.LogFormat("  Eccentricity: {0:F3}", Cobj.orbitEccentricity);

                if (Cobj.orbitEccentricity > 0)
                {
                    Console.WriteLine($"Max Separation:      {Cobj.orbitMaxSep:F2} AU");
                    Console.WriteLine($"Min Separation:      {Cobj.orbitMinSep:F2} AU");
                    DebugLogger.LogFormat("  Max Separation: {0:F2} AU", Cobj.orbitMaxSep);
                    DebugLogger.LogFormat("  Min Separation: {0:F2} AU", Cobj.orbitMinSep);
                }

                // Display orbital period
                if (Cobj.OrbitalPeriodYears > 0)
                {
                    string periodDisplay = FormatOrbitalPeriod(Cobj.OrbitalPeriodYears);
                    Console.WriteLine($"Orbital Period:      {periodDisplay}");
                    DebugLogger.LogFormat("  Orbital Period: {0}", periodDisplay);
                }

                Console.WriteLine();
            }

            // Star classification
            if (star.type != "BD" && star.type != "D")
            {
                Console.WriteLine($"Classification:      {star.type}{star.subType} {star.starclass}");
                DebugLogger.LogFormat("  Classification: {0}{1} {2}", star.type, star.subType, star.starclass);
            }
            else
            {
                string typeName = star.type == "BD" ? "Brown Dwarf" : "White Dwarf";
                Console.WriteLine($"Classification:      {star.type} ({typeName})");
                DebugLogger.LogFormat("  Classification: {0} ({1})", star.type, typeName);
            }

            Console.WriteLine($"Colour:              {star.colour}");
            DebugLogger.LogFormat("  Colour: {0}", star.colour);

            // Physical properties
            Console.WriteLine($"Mass:                {star.mass:F3} solar masses");
            Console.WriteLine($"Temperature:         {star.temperture:N0} K");
            Console.WriteLine($"Diameter:            {star.diameter:F4} solar diameters");
            Console.WriteLine($"Luminosity:          {star.luminosity:F6}");
            Console.WriteLine($"Age:                 {star.age:F2} billion years");

            // Minimum allowable orbit (except for Companion orbit stars)
            if (star.starOrbitType != Starhelper.starOrbitType.Companion && star.MinAllowableOrbit > 0)
            {
                Console.WriteLine($"Min Allowable Orbit: {star.MinAllowableOrbit:F3}");
            }

            // Maximum allowable orbit (except for Companion orbit stars)
            if (star.starOrbitType != Starhelper.starOrbitType.Companion && star.MaxAllowableOrbit > 0)
            {
                Console.WriteLine($"Max Allowable Orbit: {star.MaxAllowableOrbit:F3}");
            }

            // Unavailable orbit ranges
            if (star.UnavailableOrbitRanges.Count > 0)
            {
                Console.WriteLine("Unavailable Orbits:");
                foreach (var range in star.UnavailableOrbitRanges)
                {
                    Console.WriteLine($"  {range.min:F2} to {range.max:F2}");
                }
            }

            // Total available orbits (except for Companion orbit stars)
            if (star.starOrbitType != Starhelper.starOrbitType.Companion && star.TotalAvailableOrbits > 0)
            {
                Console.WriteLine($"Total Available Orbits: {star.TotalAvailableOrbits:F2}");
            }

            // Worlds assigned (except for Companion orbit stars)
            if (star.starOrbitType != Starhelper.starOrbitType.Companion)
            {
                Console.WriteLine($"Worlds Assigned:     {star.WorldsAssigned}");
            }

            // System Baseline Number and zone allocations (Primary star only)
            if (star.starOrbitType == Starhelper.starOrbitType.Primary && star.WorldsAssigned > 0)
            {
                Console.WriteLine($"System Baseline #:   {star.SystemBaselineNumber}");

                // Show inner/outer zone breakdown if applicable (Scenario A)
                if (star.InnerZoneWorldCount > 0 || star.OuterZoneWorldCount > 0)
                {
                    Console.WriteLine($"  Inner Zone Worlds: {star.InnerZoneWorldCount}");
                    Console.WriteLine($"  Outer Zone Worlds: {star.OuterZoneWorldCount}");
                }

                // Baseline orbit information
                if (star.BaselineOrbit > 0)
                {
                    Console.WriteLine($"Baseline Orbit:      {star.BaselineOrbit:F4}");
                    Console.WriteLine($"  Inside Baseline:   {star.InsideBaseline}");
                    Console.WriteLine($"  Outside Baseline:  {star.OutsideBaseline}");
                }

                // Empty orbits (if any)
                if (star.starOrbitType != Starhelper.starOrbitType.Companion && star.EmptyOrbits > 0)
                {
                    Console.WriteLine($"Empty Orbits:        {star.EmptyOrbits}");
                }

                // System spread (Primary only)
                if (star.starOrbitType == Starhelper.starOrbitType.Primary && star.SystemSpread > 0)
                {
                    Console.WriteLine($"System Spread:       {star.SystemSpread:F4}");
                }
            }

            // Habitable zone (except for Companion orbit stars)
            if (star.starOrbitType != Starhelper.starOrbitType.Companion)
            {
                float hzMin = Math.Max(0, star.HZCO - 1);  // Clamp to 0
                float hzMax = star.HZCO + 1;
                Console.WriteLine($"Habitable Zone Center: {star.HZCO:F3}");
                Console.WriteLine($"Habitable Zone:      {hzMin:F3} to {hzMax:F3}");
            }

            // Orbits list (if any objects placed) - for all stars
            var celestialBodies = Cobj.celestrialObjectOrbits
                .Where(o => o.celestrialObject is CelestialBody)
                .OrderBy(o => o.orbit)
                .ToList();

            if (celestialBodies.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Orbits:");
                for (int i = 0; i < celestialBodies.Count; i++)
                {
                    var orbitObj = celestialBodies[i];
                    CelestrialObject? nextOrbit = i < celestialBodies.Count - 1 ? celestialBodies[i + 1] : null;

                    float mkm = orbitObj.orbitAU * 149.597870700f;
                    string mkmFormat;

                    // Dynamic precision based on AU distance
                    if (orbitObj.orbitAU < 0.1f)
                    {
                        mkmFormat = $"{mkm:F2}"; // 2 decimal places for very small orbits
                    }
                    else if (orbitObj.orbitAU < 1.0f)
                    {
                        mkmFormat = $"{mkm:F1}"; // 1 decimal place for small orbits
                    }
                    else
                    {
                        mkmFormat = $"{mkm:F0}"; // Whole numbers for larger orbits
                    }

                    if (orbitObj.celestrialObject is CelestialBody cb)
                    {
                        // Skip Empty Orbits in output
                        if (cb is EmptyOrbit)
                            continue;

                        // Build world type label
                        string worldLabel = GetWorldTypeLabel(cb);

                        // Build orbital properties string
                        string orbitalProps = BuildOrbitalPropertiesString(orbitObj);

                        // Build designation prefix
                        string designationPrefix = string.IsNullOrEmpty(cb.Designation) ? "" : $"{cb.Designation}\t";

                        // Handle Trojan special formatting
                        if (IsTrojanPrimary(orbitObj, nextOrbit))
                        {
                            Console.WriteLine($"  {designationPrefix}{orbitObj.orbit:F3} ({orbitObj.orbitAU:F3} AU / {mkmFormat} Mkm) {worldLabel}{orbitalProps}");
                            if (nextOrbit != null)
                            {
                                string trojanLabel = GetTrojanCompanionLabel(nextOrbit);
                                string trojanPos = GetTrojanPosition(nextOrbit);

                                // Get trojan designation if it has one
                                if (nextOrbit.celestrialObject is CelestialBody trojanCb && !string.IsNullOrEmpty(trojanCb.Designation))
                                {
                                    Console.WriteLine($"  {trojanCb.Designation}\t                      {trojanLabel} (Trojan {trojanPos})");
                                }
                                else
                                {
                                    Console.WriteLine($"                              {trojanLabel} (Trojan {trojanPos})");
                                }
                            }
                        }
                        else if (!IsTrojanCompanion(orbitObj))
                        {
                            Console.WriteLine($"  {designationPrefix}{orbitObj.orbit:F3} ({orbitObj.orbitAU:F3} AU / {mkmFormat} Mkm) {worldLabel}{orbitalProps}");
                        }
                    }
                }
            }

            DebugLogger.LogFormat("  Mass: {0:F2} solar masses", star.mass);
            DebugLogger.LogFormat("  Temperature: {0} K", star.temperture);
            DebugLogger.LogFormat("  Diameter: {0:F4} solar diameters", star.diameter);
            DebugLogger.LogFormat("  Luminosity: {0:F6}", star.luminosity);
            DebugLogger.LogFormat("  Age: {0:F2} billion years", star.age);

            // Minimum allowable orbit (except for Companion orbit stars)
            if (star.starOrbitType != Starhelper.starOrbitType.Companion && star.MinAllowableOrbit > 0)
            {
                DebugLogger.LogFormat("  Min Allowable Orbit: {0:F3} (orbit number)", star.MinAllowableOrbit);
            }
        }

        private string GetWorldTypeLabel(CelestialBody? celestialObj)
        {
            if (celestialObj == null)
                return "Unknown";

            return celestialObj switch
            {
                GasGiant gg => gg.Type == CelestialBodyType.GasGiant ? "Gas Giant" : $"Gas Giant [{gg.Type}]",
                TerrestrialPlanet tp => tp.Type == CelestialBodyType.TerrestrialPlanet ? "Terrestrial Planet" : $"Terrestrial Planet [{tp.Type}]",
                PlanetoidBelt pb => pb.Type == CelestialBodyType.PlanetoidBelt ? "Planetoid Belt" : $"Planetoid Belt [{pb.Type}]",
                EmptyOrbit _ => "Empty Orbit",
                CelestialBody cb when cb.Type == CelestialBodyType.Filled => "Filled",
                CelestialBody cb => $"Filled [{cb.Type}]",
                _ => "Unknown"
            };
        }

        private string BuildOrbitalPropertiesString(CelestrialObject cObj)
        {
            List<string> props = new List<string>();

            if (cObj.orbitEccentricity > 0)
                props.Add($"e:{cObj.orbitEccentricity:F3}");

            if (cObj.celestrialObject is GasGiant gg && gg.Inclination.HasValue)
                props.Add($"i:{gg.Inclination:F0}°");
            else if (cObj.celestrialObject is TerrestrialPlanet tp && tp.Inclination.HasValue)
                props.Add($"i:{tp.Inclination:F0}°");

            if (cObj.OrbitalPeriodYears > 0)
                props.Add($"P:{FormatOrbitalPeriod(cObj.OrbitalPeriodYears)}");

            return props.Count > 0 ? $" [{string.Join(", ", props)}]" : "";
        }

        private bool IsTrojanPrimary(CelestrialObject cObj, CelestrialObject? nextObj)
        {
            if (nextObj == null)
                return false;

            // Check if next orbit is at same position and has Trojan position set
            if (Math.Abs(cObj.orbit - nextObj.orbit) < 0.001f)
            {
                if (nextObj.celestrialObject is GasGiant gg && !string.IsNullOrEmpty(gg.TrojanPosition))
                    return true;
                if (nextObj.celestrialObject is TerrestrialPlanet tp && !string.IsNullOrEmpty(tp.TrojanPosition))
                    return true;
            }

            return false;
        }

        private bool IsTrojanCompanion(CelestrialObject cObj)
        {
            if (cObj.celestrialObject is GasGiant gg && !string.IsNullOrEmpty(gg.TrojanPosition))
                return true;
            if (cObj.celestrialObject is TerrestrialPlanet tp && !string.IsNullOrEmpty(tp.TrojanPosition))
                return true;

            return false;
        }

        private string GetTrojanCompanionLabel(CelestrialObject cObj)
        {
            return GetWorldTypeLabel(cObj.celestrialObject as CelestialBody);
        }

        private string GetTrojanPosition(CelestrialObject cObj)
        {
            if (cObj.celestrialObject is GasGiant gg && !string.IsNullOrEmpty(gg.TrojanPosition))
                return gg.TrojanPosition;
            if (cObj.celestrialObject is TerrestrialPlanet tp && !string.IsNullOrEmpty(tp.TrojanPosition))
                return tp.TrojanPosition;

            return "??";
        }

        private string GetProperty(Object? obj, string prop)
        {
            if (obj == null)
                return "";

            Type type = obj.GetType();
            string rtn = "";
            PropertyInfo? property = type.GetProperty(prop);
            //if (property != null)
            //return property.GetValue(obj, null).ToString();
            //else return "";
            try
            {
                rtn = property?.GetValue(obj, null)?.ToString() ?? "";
            }
            catch (NullReferenceException)
            {
                //Console.WriteLine("NullReferenceException");
                rtn = "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return rtn;
        }

        private int CheckCompanionTypePresent (Starhelper.starOrbitType starOrbit, Random dice)
        {
            int starPresent = Starhelper.diceRoll(6, 2, dice);
            DebugLogger.LogFormat("  Checking for {0} companion star - Base roll: {1}", starOrbit, starPresent);

            if (starOrbit == Starhelper.starOrbitType.Close)
            {
                if (GetProperty(primaryObject.celestrialObject, "starclass") == "Ia" ||
                    GetProperty(primaryObject.celestrialObject, "starclass") == "Ib" ||
                    GetProperty(primaryObject.celestrialObject, "starclass") == "II" ||
                    GetProperty(primaryObject.celestrialObject, "starclass") == "III")
                {
                    starPresent = 0;
                    DebugLogger.Log("    Giant/Supergiant stars cannot have close companions - setting to 0");
                }
            }
            else
            {
                if (GetProperty(primaryObject.celestrialObject, "starclass") == "Ia" ||
                    GetProperty(primaryObject.celestrialObject, "starclass") == "Ib" ||
                    GetProperty(primaryObject.celestrialObject, "starclass") == "II" ||
                    GetProperty(primaryObject.celestrialObject, "starclass") == "III" ||
                    GetProperty(primaryObject.celestrialObject, "starclass") == "IV" )
                {
                    starPresent++;
                }
                if ((GetProperty(primaryObject.celestrialObject, "starclass") == "V" ||
                    GetProperty(primaryObject.celestrialObject, "starclass") == "VI" ) &&
                    (GetProperty(primaryObject.celestrialObject, "type") == "O" ||
                    GetProperty(primaryObject.celestrialObject, "type") == "B" ||
                    GetProperty(primaryObject.celestrialObject, "type") == "A" ||
                    GetProperty(primaryObject.celestrialObject, "type") == "F" ))
                {
                    starPresent++;
                }
                if (GetProperty(primaryObject.celestrialObject, "starclass") == "V" ||
                    GetProperty(primaryObject.celestrialObject, "starclass") == "VI" ||
                    GetProperty(primaryObject.celestrialObject, "starclass") == "M" )
                {
                    starPresent--;
                }
                if (GetProperty(primaryObject.celestrialObject, "starclass") == "D" ||
                    GetProperty(primaryObject.celestrialObject, "starclass") == "BD" )
                {
                    starPresent--;
                }
            }

            DebugLogger.LogFormat("  Final roll for {0} companion: {1} (need 10+)", starOrbit, starPresent);
            return starPresent;
        }

        private int CheckCompanionTypePresentForStar(Star star, Random dice)
        {
            int starPresent = Starhelper.diceRoll(6, 2, dice);
            DebugLogger.LogFormat("    Checking for Companion orbit companion - Base roll: {0}", starPresent);

            // Apply modifiers based on the companion star's properties
            // For Companion orbit companions (only checking for very close companions)
            if (star.starclass == "Ia" ||
                star.starclass == "Ib" ||
                star.starclass == "II" ||
                star.starclass == "III" ||
                star.starclass == "IV")
            {
                starPresent++;
                DebugLogger.Log("      Star is Class Ia/Ib/II/III/IV - adding +1");
            }
            if ((star.starclass == "V" || star.starclass == "VI") &&
                (star.type == "O" || star.type == "B" || star.type == "A" || star.type == "F"))
            {
                starPresent++;
                DebugLogger.Log("      Star is Class V/VI and type O/B/A/F - adding +1");
            }
            if (star.starclass == "V" || star.starclass == "VI" || star.type == "M")
            {
                starPresent--;
                DebugLogger.Log("      Star is Class V/VI or M-type - subtracting 1");
            }
            if (star.type == "D" || star.type == "BD")
            {
                starPresent--;
                DebugLogger.Log("      Star is D or BD - subtracting 1");
            }

            DebugLogger.LogFormat("    Final roll for Companion orbit companion: {0} (need 10+)", starPresent);
            return starPresent;
        }

        private void GenerateAdditionalStars(CelestrialObject cObj, Random dice)
        {

            int closeStarPresent = CheckCompanionTypePresent(Starhelper.starOrbitType.Close, dice);
            int nearStarPresent = CheckCompanionTypePresent(Starhelper.starOrbitType.Near, dice);
            int farStarPresent = CheckCompanionTypePresent(Starhelper.starOrbitType.Far, dice);
            int companionStarPresent = CheckCompanionTypePresent(Starhelper.starOrbitType.Companion, dice);

            // Track companions that can have their own Companion orbit companions
            List<CelestrialObject> companionsToCheck = new List<CelestrialObject>();

            if (closeStarPresent >= 10)
                {
                Console.WriteLine("Close Star present");
                DebugLogger.Log("CLOSE STAR DETECTED - Generating orbital position");
                int baseOrb = Starhelper.diceRoll(6, 1, dice) - 1;
                DebugLogger.LogFormat("  Base orbit: {0}", baseOrb);
                float fractionalOrbit = FractionalOrbit(baseOrb, dice, Starhelper.starOrbitType.Close);
                DebugLogger.LogFormat("  Fractional orbit: {0:F2}", fractionalOrbit);
                cObj.AddStar(fractionalOrbit, Starhelper.starOrbitType.Close, dice);
                // Add to list to check for Companion orbit companions
                companionsToCheck.Add(cObj.celestrialObjectOrbits[cObj.celestrialObjectOrbits.Count - 1]);
                }
            if (nearStarPresent >= 10)
            {
                Console.WriteLine("Near Star present");
                DebugLogger.Log("NEAR STAR DETECTED - Generating orbital position");
                int baseOrb = Starhelper.diceRoll(6, 1, dice) + 5;
                DebugLogger.LogFormat("  Base orbit: {0}", baseOrb);
                float fractionalOrbit = FractionalOrbit(baseOrb, dice, Starhelper.starOrbitType.Near);
                DebugLogger.LogFormat("  Fractional orbit: {0:F2}", fractionalOrbit);
                cObj.AddStar(fractionalOrbit, Starhelper.starOrbitType.Near, dice);
                // Add to list to check for Companion orbit companions
                companionsToCheck.Add(cObj.celestrialObjectOrbits[cObj.celestrialObjectOrbits.Count - 1]);
            }
            if (farStarPresent >= 10)
            {
                Console.WriteLine("Far Star present");
                DebugLogger.Log("FAR STAR DETECTED - Generating orbital position");
                int baseOrb = Starhelper.diceRoll(6, 1, dice) + 11;
                DebugLogger.LogFormat("  Base orbit: {0}", baseOrb);
                float fractionalOrbit = FractionalOrbit(baseOrb, dice, Starhelper.starOrbitType.Far);
                DebugLogger.LogFormat("  Fractional orbit: {0:F2}", fractionalOrbit);
                cObj.AddStar(fractionalOrbit, Starhelper.starOrbitType.Far, dice);
                // Add to list to check for Companion orbit companions
                companionsToCheck.Add(cObj.celestrialObjectOrbits[cObj.celestrialObjectOrbits.Count - 1]);
            }
            if(companionStarPresent >= 10)
            {
                Console.WriteLine("Companion Star present");
                DebugLogger.Log("COMPANION STAR DETECTED - Generating orbital position");
                int baseOrb = Starhelper.diceRoll(6, 1, dice) / 10 + (Starhelper.diceRoll(6, 2, dice) - 7) / 100;
                DebugLogger.LogFormat("  Base orbit: {0}", baseOrb);
                float fractionalOrbit = FractionalOrbit(baseOrb, dice, Starhelper.starOrbitType.Companion);
                DebugLogger.LogFormat("  Fractional orbit: {0:F2}", fractionalOrbit);
                cObj.AddStar(fractionalOrbit, Starhelper.starOrbitType.Companion, dice);
            }

            // Check each Close/Near/Far companion for Companion orbit companions
            foreach (CelestrialObject companion in companionsToCheck)
            {
                if (companion.celestrialObject is Star)
                {
                    Star companionStar = (Star)companion.celestrialObject;
                    DebugLogger.Log("");
                    DebugLogger.LogFormat("Checking if {0} companion has its own Companion orbit companion...", companionStar.starOrbitType);

                    // Check for Companion orbit companion of this companion
                    int subCompanionPresent = CheckCompanionTypePresentForStar(companionStar, dice);

                    if (subCompanionPresent >= 10)
                    {
                        Console.WriteLine($"  {companionStar.starOrbitType} star has Companion orbit companion");
                        DebugLogger.LogFormat("  {0} companion will have a Companion orbit companion", companionStar.starOrbitType);
                        int baseOrb = Starhelper.diceRoll(6, 1, dice) / 10 + (Starhelper.diceRoll(6, 2, dice) - 7) / 100;
                        DebugLogger.LogFormat("    Base orbit: {0}", baseOrb);
                        float fractionalOrbit = FractionalOrbit(baseOrb, dice, Starhelper.starOrbitType.Companion);
                        DebugLogger.LogFormat("    Fractional orbit: {0:F2}", fractionalOrbit);
                        companion.AddStar(fractionalOrbit, Starhelper.starOrbitType.Companion, dice);
                    }
                    else
                    {
                        DebugLogger.LogFormat("  No Companion orbit companion for {0} companion", companionStar.starOrbitType);
                    }
                }
            }

        }

        private float FractionalOrbit (float orbitNum, Random dice, Starhelper.starOrbitType orbitType)
        {
            float fractionalOrbit = 0;

            // Close companions: if 1d6-1 = 0, orbit is exactly 0.5
            if (orbitType == Starhelper.starOrbitType.Close && orbitNum <= 0)
            {
                return 0.5F;
            }

            int roll = Starhelper.diceRoll(10, 1, dice);
            roll++; //so roll is 1-10

            if (orbitNum != 0)
            {
                fractionalOrbit = orbitNum - 1 + 0.5F + (roll / 10);
            }
            else
            {
                fractionalOrbit = roll / 20 + ((Starhelper.diceRoll(10, 1, dice)) / 100);
            }

            return fractionalOrbit;
        }

        private float FractionalOrbit(float orbitNum, Random dice)
        {
            float fractionalOrbit = 0;
            int roll = Starhelper.diceRoll(10, 1, dice);
            roll++; //so roll is 1-10

            if (orbitNum != 0)
            {
                fractionalOrbit = orbitNum - 1 + 0.5F + (roll / 10);
            }
            else
            {
                fractionalOrbit = roll / 20 + ((Starhelper.diceRoll(10, 1, dice)) / 100);
            }

            return fractionalOrbit;
        }

        private float CalculateTotalOrbitedMass(CelestrialObject orbitingObj, Star orbitingStar)
        {
            // Calculate M = total mass of stars being orbited
            float totalMass = 0;

            if (orbitingStar.starOrbitType == Starhelper.starOrbitType.Companion)
            {
                // Companion orbits just its parent star
                CelestrialObject? parent = FindParentObject(orbitingObj);
                if (parent != null && parent.celestrialObject is Star)
                {
                    totalMass = ((Star)parent.celestrialObject).mass;
                    DebugLogger.LogFormat("    Companion orbit - orbiting parent star with mass {0:F3}", totalMass);
                }
            }
            else if (orbitingStar.starOrbitType == Starhelper.starOrbitType.Close)
            {
                // Close orbits: primary + any companion of the primary
                if (primaryObject.celestrialObject is Star)
                {
                    totalMass = ((Star)primaryObject.celestrialObject).mass;
                    DebugLogger.LogFormat("    Close orbit - primary mass: {0:F3}", totalMass);

                    // Add any Companion orbit companion of the primary
                    foreach (var obj in primaryObject.celestrialObjectOrbits)
                    {
                        if (obj.celestrialObject is Star)
                        {
                            Star s = (Star)obj.celestrialObject;
                            if (s.starOrbitType == Starhelper.starOrbitType.Companion)
                            {
                                totalMass += s.mass;
                                DebugLogger.LogFormat("    Adding primary's companion mass: {0:F3}", s.mass);
                            }
                        }
                    }
                }
            }
            else if (orbitingStar.starOrbitType == Starhelper.starOrbitType.Near)
            {
                // Near orbits: primary + any close companion + companion companions of either
                if (primaryObject.celestrialObject is Star)
                {
                    totalMass = ((Star)primaryObject.celestrialObject).mass;
                    DebugLogger.LogFormat("    Near orbit - primary mass: {0:F3}", totalMass);

                    foreach (var obj in primaryObject.celestrialObjectOrbits)
                    {
                        if (obj.celestrialObject is Star)
                        {
                            Star s = (Star)obj.celestrialObject;
                            if (s.starOrbitType == Starhelper.starOrbitType.Close || s.starOrbitType == Starhelper.starOrbitType.Companion)
                            {
                                totalMass += s.mass;
                                DebugLogger.LogFormat("    Adding {0} companion mass: {1:F3}", s.starOrbitType, s.mass);

                                // Add any Companion orbit companions
                                foreach (var subObj in obj.celestrialObjectOrbits)
                                {
                                    if (subObj.celestrialObject is Star)
                                    {
                                        Star subStar = (Star)subObj.celestrialObject;
                                        totalMass += subStar.mass;
                                        DebugLogger.LogFormat("    Adding sub-companion mass: {0:F3}", subStar.mass);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (orbitingStar.starOrbitType == Starhelper.starOrbitType.Far)
            {
                // Far orbits: primary + close + near + all companion companions
                if (primaryObject.celestrialObject is Star)
                {
                    totalMass = ((Star)primaryObject.celestrialObject).mass;
                    DebugLogger.LogFormat("    Far orbit - primary mass: {0:F3}", totalMass);

                    foreach (var obj in primaryObject.celestrialObjectOrbits)
                    {
                        if (obj.celestrialObject is Star)
                        {
                            Star s = (Star)obj.celestrialObject;
                            if (s.starOrbitType == Starhelper.starOrbitType.Close ||
                                s.starOrbitType == Starhelper.starOrbitType.Near ||
                                s.starOrbitType == Starhelper.starOrbitType.Companion)
                            {
                                totalMass += s.mass;
                                DebugLogger.LogFormat("    Adding {0} companion mass: {1:F3}", s.starOrbitType, s.mass);

                                // Add any Companion orbit companions
                                foreach (var subObj in obj.celestrialObjectOrbits)
                                {
                                    if (subObj.celestrialObject is Star)
                                    {
                                        Star subStar = (Star)subObj.celestrialObject;
                                        totalMass += subStar.mass;
                                        DebugLogger.LogFormat("    Adding sub-companion mass: {0:F3}", subStar.mass);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return totalMass;
        }

        private CelestrialObject? FindParentObject(CelestrialObject childObj)
        {
            // Search through all celestial objects to find the parent of childObj
            foreach (var obj in primaryObject.celestrialObjectOrbits)
            {
                if (obj.celestrialObjectOrbits.Contains(childObj))
                {
                    return obj;
                }
            }
            return null;
        }

        private void CalculateOrbitalPeriod(CelestrialObject cObj)
        {
            if (cObj.celestrialObject is Star)
            {
                Star star = (Star)cObj.celestrialObject;

                // Primary stars don't have orbital periods
                if (star.starOrbitType == Starhelper.starOrbitType.Primary)
                    return;

                DebugLogger.LogFormat("  Calculating orbital period for {0} companion...", star.starOrbitType);

                // Calculate M (total orbited mass)
                float M = CalculateTotalOrbitedMass(cObj, star);

                // m = mass of the orbiting star
                float m = star.mass;
                DebugLogger.LogFormat("    Orbiting star mass (m): {0:F3}", m);

                // orbital period = sqrt(orbit³ / M) - Kepler's 3rd law
                float orbitAU = cObj.orbitAU;
                float period = (float)Math.Sqrt((orbitAU * orbitAU * orbitAU) / M);

                cObj.OrbitalPeriodYears = period;
                DebugLogger.LogFormat("    Orbital period: {0:F6} years", period);
            }
            else if (cObj.celestrialObject is CelestialBody)
            {
                DebugLogger.Log("  Calculating orbital period for celestial body...");

                // Calculate M (total mass of stars being orbited)
                float M = CalculateTotalOrbitedMassForPlanet(cObj);

                // orbital period = sqrt(orbit³ / M) - Kepler's 3rd law
                float orbitAU = cObj.orbitAU;
                float period = (float)Math.Sqrt((orbitAU * orbitAU * orbitAU) / M);

                cObj.OrbitalPeriodYears = period;
                DebugLogger.LogFormat("    Orbital period: {0:F6} years", period);
            }
        }

        private float CalculateTotalOrbitedMassForPlanet(CelestrialObject planetCobj)
        {
            // Find which star this planet belongs to
            Star? parentStar = FindStarForOrbit(planetCobj);

            if (parentStar == null)
            {
                DebugLogger.Log("    WARNING: Could not find parent star for planet");
                return 1.0f; // Default to 1 solar mass
            }

            DebugLogger.LogFormat("    Planet orbits {0} star", parentStar.starOrbitType);

            // Calculate total mass based on parent star type
            float totalMass = 0;

            if (parentStar.starOrbitType == Starhelper.starOrbitType.Primary)
            {
                // Primary star - just its mass
                totalMass = parentStar.mass;
                DebugLogger.LogFormat("    Primary star mass: {0:F3}", totalMass);
            }
            else if (parentStar.starOrbitType == Starhelper.starOrbitType.Companion)
            {
                // Companion orbits another star - planet orbits just the companion
                totalMass = parentStar.mass;
                DebugLogger.LogFormat("    Companion star mass: {0:F3}", totalMass);
            }
            else if (parentStar.starOrbitType == Starhelper.starOrbitType.Close)
            {
                // Close orbit - planet may orbit primary + close companion together
                totalMass = parentStar.mass;
                if (primaryObject.celestrialObject is Star)
                {
                    totalMass += ((Star)primaryObject.celestrialObject).mass;
                }
                DebugLogger.LogFormat("    Close orbit total mass: {0:F3}", totalMass);
            }
            else
            {
                // Near/Far orbits - just the star's own mass for now (simplified)
                totalMass = parentStar.mass;
                DebugLogger.LogFormat("    {0} star mass: {1:F3}", parentStar.starOrbitType, totalMass);
            }

            return totalMass;
        }

        private Star? FindStarForOrbit(CelestrialObject planetCobj)
        {
            // Check primary star's orbits
            if (primaryObject.celestrialObjectOrbits.Contains(planetCobj))
            {
                return primaryObject.celestrialObject as Star;
            }

            // Check companion stars' orbits
            foreach (var companionObj in primaryObject.celestrialObjectOrbits)
            {
                if (companionObj.celestrialObject is Star)
                {
                    if (companionObj.celestrialObjectOrbits.Contains(planetCobj))
                    {
                        return companionObj.celestrialObject as Star;
                    }

                    // Check sub-companions
                    foreach (var subCompanionObj in companionObj.celestrialObjectOrbits)
                    {
                        if (subCompanionObj.celestrialObject is Star)
                        {
                            if (subCompanionObj.celestrialObjectOrbits.Contains(planetCobj))
                            {
                                return subCompanionObj.celestrialObject as Star;
                            }
                        }
                    }
                }
            }

            // Default to primary if not found
            return primaryObject.celestrialObject as Star;
        }

        private int CountStarsOrbitedByPlanet(Star parentStar)
        {
            // For most cases, planets orbit just their local star
            if (parentStar.starOrbitType == Starhelper.starOrbitType.Primary ||
                parentStar.starOrbitType == Starhelper.starOrbitType.Companion ||
                parentStar.starOrbitType == Starhelper.starOrbitType.Near ||
                parentStar.starOrbitType == Starhelper.starOrbitType.Far)
            {
                return 1;
            }
            else if (parentStar.starOrbitType == Starhelper.starOrbitType.Close)
            {
                // Close orbit - may orbit primary + close companion
                return 2;
            }

            return 1;
        }

        private List<(CelestrialObject cobj, Star parentStar)> GetAllCelestialBodiesOfType(CelestialBodyType targetType)
        {
            List<(CelestrialObject, Star)> results = new List<(CelestrialObject, Star)>();

            // Check primary star's orbits
            if (primaryObject.celestrialObject is Star primaryStar)
            {
                foreach (var cobj in primaryObject.celestrialObjectOrbits)
                {
                    if (cobj.celestrialObject is CelestialBody cb && MatchesCelestialBodyType(cb, targetType))
                    {
                        results.Add((cobj, primaryStar));
                    }
                }
            }

            // Check companion stars' orbits
            foreach (var companionObj in primaryObject.celestrialObjectOrbits)
            {
                if (companionObj.celestrialObject is Star companionStar)
                {
                    foreach (var cobj in companionObj.celestrialObjectOrbits)
                    {
                        if (cobj.celestrialObject is CelestialBody cb && MatchesCelestialBodyType(cb, targetType))
                        {
                            results.Add((cobj, companionStar));
                        }
                    }

                    // Check sub-companions
                    foreach (var subCompanionObj in companionObj.celestrialObjectOrbits)
                    {
                        if (subCompanionObj.celestrialObject is Star subCompanionStar)
                        {
                            foreach (var cobj in subCompanionObj.celestrialObjectOrbits)
                            {
                                if (cobj.celestrialObject is CelestialBody cb && MatchesCelestialBodyType(cb, targetType))
                                {
                                    results.Add((cobj, subCompanionStar));
                                }
                            }
                        }
                    }
                }
            }

            return results;
        }

        private bool MatchesCelestialBodyType(CelestialBody cb, CelestialBodyType targetType)
        {
            // When looking for Filled, include all anomalous orbit types (they're still placeholders)
            if (targetType == CelestialBodyType.Filled)
            {
                return cb.Type == CelestialBodyType.Filled ||
                       cb.Type == CelestialBodyType.Random ||
                       cb.Type == CelestialBodyType.Eccentric ||
                       cb.Type == CelestialBodyType.Inclined ||
                       cb.Type == CelestialBodyType.Retrograde ||
                       cb.Type == CelestialBodyType.Trojan;
            }

            // For other types, exact match
            return cb.Type == targetType;
        }

        private void AssignStarDesignations()
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("ASSIGNING STAR DESIGNATIONS");

            // Primary star is always A (or Aa if it has a companion)
            if (primaryObject.celestrialObject is Star primaryStar)
            {
                // Check if primary has a companion
                bool hasCompanion = primaryObject.celestrialObjectOrbits
                    .Any(obj => obj.celestrialObject is Star s && s.starOrbitType == Starhelper.starOrbitType.Companion);

                if (hasCompanion)
                {
                    primaryStar.Designation = "Aa";
                    DebugLogger.Log($"Primary star: Aa (has companion)");
                }
                else
                {
                    primaryStar.Designation = "A";
                    DebugLogger.Log($"Primary star: A");
                }
            }

            // Assign designations to Close, Near, Far stars (B, C, D, etc.)
            char currentLetter = 'B';
            foreach (var companionObj in primaryObject.celestrialObjectOrbits)
            {
                if (companionObj.celestrialObject is Star companionStar)
                {
                    if (companionStar.starOrbitType == Starhelper.starOrbitType.Close ||
                        companionStar.starOrbitType == Starhelper.starOrbitType.Near ||
                        companionStar.starOrbitType == Starhelper.starOrbitType.Far)
                    {
                        // Check if this star has a companion
                        bool hasSubCompanion = companionObj.celestrialObjectOrbits
                            .Any(obj => obj.celestrialObject is Star s && s.starOrbitType == Starhelper.starOrbitType.Companion);

                        if (hasSubCompanion)
                        {
                            companionStar.Designation = $"{currentLetter}a";
                            DebugLogger.Log($"{companionStar.starOrbitType} star: {currentLetter}a (has companion)");
                        }
                        else
                        {
                            companionStar.Designation = $"{currentLetter}";
                            DebugLogger.Log($"{companionStar.starOrbitType} star: {currentLetter}");
                        }

                        currentLetter++;
                    }
                    else if (companionStar.starOrbitType == Starhelper.starOrbitType.Companion)
                    {
                        // Primary's companion is Ab
                        companionStar.Designation = "Ab";
                        DebugLogger.Log($"Primary's companion: Ab");
                    }
                }
            }

            // Assign designations to sub-companions (companions of Close/Near/Far stars)
            currentLetter = 'B';
            foreach (var companionObj in primaryObject.celestrialObjectOrbits)
            {
                if (companionObj.celestrialObject is Star companionStar)
                {
                    if (companionStar.starOrbitType == Starhelper.starOrbitType.Close ||
                        companionStar.starOrbitType == Starhelper.starOrbitType.Near ||
                        companionStar.starOrbitType == Starhelper.starOrbitType.Far)
                    {
                        // Check for sub-companions
                        foreach (var subCompanionObj in companionObj.celestrialObjectOrbits)
                        {
                            if (subCompanionObj.celestrialObject is Star subCompanionStar &&
                                subCompanionStar.starOrbitType == Starhelper.starOrbitType.Companion)
                            {
                                subCompanionStar.Designation = $"{currentLetter}b";
                                DebugLogger.Log($"{companionStar.Designation}'s companion: {currentLetter}b");
                            }
                        }

                        currentLetter++;
                    }
                }
            }

            DebugLogger.Log("Star designation assignment complete");
        }

        private void AssignWorldDesignations()
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("ASSIGNING WORLD DESIGNATIONS");

            // Check if single star system
            bool isSingleStar = !primaryObject.celestrialObjectOrbits.Any(obj => obj.celestrialObject is Star);

            // Get all companion stars sorted by orbit
            var companionStars = primaryObject.celestrialObjectOrbits
                .Where(obj => obj.celestrialObject is Star)
                .OrderBy(obj => obj.orbit)
                .ToList();

            // Assign designations to primary's worlds
            AssignWorldDesignationsForStar(primaryObject, isSingleStar, companionStars);

            // Assign designations to each companion star's worlds
            foreach (var companionObj in companionStars)
            {
                if (companionObj.celestrialObject is Star companionStar)
                {
                    AssignWorldDesignationsForStar(companionObj, false, new List<CelestrialObject>());
                }
            }

            DebugLogger.Log("World designation assignment complete");
        }

        private void AssignWorldDesignationsForStar(CelestrialObject starCobj, bool isSingleStar, List<CelestrialObject> companionStars)
        {
            if (!(starCobj.celestrialObject is Star parentStar))
                return;

            DebugLogger.LogFormat("Assigning designations for {0} star's worlds", parentStar.Designation);

            // Get all celestial bodies (not stars) sorted by orbit
            var celestialBodies = starCobj.celestrialObjectOrbits
                .Where(obj => obj.celestrialObject is CelestialBody)
                .OrderBy(obj => obj.orbit)
                .ToList();

            string currentStarDesignation = "";
            int planetCounter = 1;
            int beltCounter = 1;

            foreach (var bodyObj in celestialBodies)
            {
                if (!(bodyObj.celestrialObject is CelestialBody body))
                    continue;

                // Skip Empty Orbits
                if (body is EmptyOrbit)
                    continue;

                // Determine which stars this world is orbiting
                string starDesignation = DetermineStarDesignation(bodyObj, parentStar, companionStars, isSingleStar);

                // Reset counters if star designation changed
                if (starDesignation != currentStarDesignation)
                {
                    currentStarDesignation = starDesignation;
                    planetCounter = 1;
                    beltCounter = 1;
                    DebugLogger.LogFormat("  Star designation changed to: {0}", starDesignation);
                }

                // Assign designation based on body type
                if (body is PlanetoidBelt)
                {
                    string romanNumeral = ToRomanNumeral(beltCounter);
                    body.Designation = $"{starDesignation} P{romanNumeral}";
                    DebugLogger.LogFormat("    Belt at orbit {0:F3}: {1}", bodyObj.orbit, body.Designation);
                    beltCounter++;
                }
                else // Gas Giant or Terrestrial Planet
                {
                    string romanNumeral = ToRomanNumeral(planetCounter);
                    body.Designation = $"{starDesignation} {romanNumeral}";
                    DebugLogger.LogFormat("    World at orbit {0:F3}: {1}", bodyObj.orbit, body.Designation);
                    planetCounter++;
                }
            }
        }

        private string DeterminePrimaryDesignation(CelestrialObject worldObj, Star parentStar, List<CelestrialObject> companionStars)
        {
            // If this world belongs to a secondary star (not primary), return just that star's designation
            if (parentStar.starOrbitType != Starhelper.starOrbitType.Primary)
            {
                // Check if parent has a companion
                if (parentStar.Designation.EndsWith("a"))
                {
                    // Parent is part of a companion pair, return combined (e.g., "Bab", "Cab")
                    string baseLetter = parentStar.Designation.TrimEnd('a');
                    return $"{baseLetter}ab";
                }
                return parentStar.Designation; // Just "B", "C", etc.
            }

            // World belongs to primary - determine combined designation
            // Use existing DetermineStarDesignation logic
            return DetermineStarDesignation(worldObj, parentStar, companionStars, false);
        }

        private string BuildNotesString(CelestrialObject worldObj, CelestialBody body, Star parentStar)
        {
            List<string> notes = new List<string>();

            // Anomalous orbit types
            if (body.Type == CelestialBodyType.Retrograde)
                notes.Add("Retrograde orbit");
            else if (body.Type == CelestialBodyType.Random)
                notes.Add("R01");
            else if (body.Type == CelestialBodyType.Eccentric)
            {
                float inclination = 0;
                if (body is GasGiant gg)
                    inclination = gg.Inclination ?? 0;
                else if (body is TerrestrialPlanet tp)
                    inclination = tp.Inclination ?? 0;

                if (inclination > 0)
                    notes.Add($"R02, i:{inclination:F0}°");
                else
                    notes.Add("R02");
            }
            else if (body.Type == CelestialBodyType.Inclined)
                notes.Add("Inclined orbit");

            // Trojan position
            string? trojanPos = null;
            if (body is GasGiant gasGiant)
                trojanPos = gasGiant.TrojanPosition;
            else if (body is TerrestrialPlanet terrestrial)
                trojanPos = terrestrial.TrojanPosition;

            if (!string.IsNullOrEmpty(trojanPos))
                notes.Add($"Trojan {trojanPos}");

            // Habitable zone check
            // Orbits below 1.0 are treated as only 10% as large
            if (parentStar.HZCO > 0)
            {
                float hzMin, hzMax;

                // Calculate lower bound
                if (parentStar.HZCO >= 2.0f)
                {
                    // Range doesn't cross below 1.0
                    hzMin = parentStar.HZCO - 1.0f;
                }
                else if (parentStar.HZCO >= 1.0f)
                {
                    // Range crosses below 1.0
                    // Distance from HZCO down to 1.0: (HZCO - 1.0) effective orbits
                    // Remaining to cover below 1.0: 2.0 - HZCO effective orbits
                    // Since orbits below 1.0 are 10% as large, we move (2.0 - HZCO) * 0.1 orbit numbers
                    hzMin = 1.0f - (2.0f - parentStar.HZCO) * 0.1f;
                }
                else
                {
                    // HZCO is below 1.0, go down 0.1 orbit numbers (= 1.0 effective)
                    hzMin = Math.Max(0, parentStar.HZCO - 0.1f);
                }

                // Calculate upper bound
                if (parentStar.HZCO >= 1.0f)
                {
                    // Standard calculation
                    hzMax = parentStar.HZCO + 1.0f;
                }
                else
                {
                    // HZCO is below 1.0
                    // Distance to 1.0 in orbit numbers: (1.0 - HZCO)
                    // Effective distance to 1.0: (1.0 - HZCO) / 0.1 = (1.0 - HZCO) * 10
                    float effectiveDistToOne = (1.0f - parentStar.HZCO) * 10.0f;
                    if (effectiveDistToOne >= 1.0f)
                    {
                        // We've used up all our 1.0 effective distance just getting to orbit 1.0
                        hzMax = 1.0f;
                    }
                    else
                    {
                        // Remaining effective distance after reaching orbit 1.0
                        float remaining = 1.0f - effectiveDistToOne;
                        hzMax = 1.0f + remaining;
                    }
                }

                if (worldObj.orbit >= hzMin && worldObj.orbit <= hzMax)
                {
                    // Add HZ first if it hasn't been added yet
                    if (!notes.Contains("HZ"))
                        notes.Insert(0, "HZ");
                }
            }

            return string.Join(", ", notes);
        }

        private string DetermineStarDesignation(CelestrialObject worldObj, Star parentStar, List<CelestrialObject> companionStars, bool isSingleStar)
        {
            // If this world belongs to a secondary star (not primary), just return the star's designation
            if (parentStar.starOrbitType != Starhelper.starOrbitType.Primary)
            {
                return parentStar.Designation;
            }

            // For primary star worlds, determine which stars are encompassed
            List<string> orbitedStars = new List<string>();

            // Check if primary has a companion (Aa/Ab case)
            bool primaryHasCompanion = false;
            if (primaryObject.celestrialObject is Star primaryStarCheck)
            {
                primaryHasCompanion = primaryStarCheck.Designation.EndsWith("a");
            }

            if (primaryHasCompanion)
            {
                orbitedStars.Add("Aab");
            }
            else
            {
                orbitedStars.Add("A");
            }

            // Check which companion stars have orbits less than this world's orbit
            foreach (var companionObj in companionStars)
            {
                if (companionObj.celestrialObject is Star companionStar)
                {
                    if (companionObj.orbit < worldObj.orbit)
                    {
                        // This world orbits beyond this companion star
                        if (companionStar.Designation.EndsWith("a"))
                        {
                            // Companion has its own companion, use "Xab" notation
                            string baseLetter = companionStar.Designation.TrimEnd('a');
                            orbitedStars.Add($"{baseLetter}ab");
                        }
                        else
                        {
                            orbitedStars.Add(companionStar.Designation);
                        }
                    }
                }
            }

            // Collapse the designation (e.g., Aab + B = AB, Aab + Bab = AB, etc.)
            return CollapseStarDesignation(orbitedStars);
        }

        private string CollapseStarDesignation(List<string> orbitedStars)
        {
            if (orbitedStars.Count == 0)
                return "";

            if (orbitedStars.Count == 1)
                return orbitedStars[0];

            // Extract base letters and combine them
            // "Aab" -> "A", "Bab" -> "B", etc.
            List<char> baseLetters = new List<char>();
            foreach (var designation in orbitedStars)
            {
                char baseLetter = designation[0]; // First character is always the base letter
                if (!baseLetters.Contains(baseLetter))
                {
                    baseLetters.Add(baseLetter);
                }
            }

            return string.Join("", baseLetters);
        }

        private string ToRomanNumeral(int number)
        {
            if (number < 1) return "";
            if (number >= 4000) return number.ToString();

            string[] thousands = { "", "M", "MM", "MMM" };
            string[] hundreds = { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
            string[] tens = { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
            string[] ones = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };

            return thousands[number / 1000] +
                   hundreds[(number % 1000) / 100] +
                   tens[(number % 100) / 10] +
                   ones[number % 10];
        }

        private string ToEhex(int number)
        {
            if (number < 0) return "0";
            if (number >= 0 && number <= 9) return number.ToString();
            if (number == 10) return "A";
            if (number == 11) return "B";
            if (number == 12) return "C";
            if (number == 13) return "D";
            if (number == 14) return "E";
            if (number == 15) return "F";
            if (number == 16) return "G";
            if (number == 17) return "H";
            if (number == 18) return "J";
            return number.ToString(); // Fallback for larger numbers
        }

        private void CalculateAllOrbitalPeriods()
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("CALCULATING ORBITAL PERIODS");

            // Calculate for primary's companions
            foreach (var obj in primaryObject.celestrialObjectOrbits)
            {
                CalculateOrbitalPeriod(obj);

                // Calculate for sub-companions
                foreach (var subObj in obj.celestrialObjectOrbits)
                {
                    CalculateOrbitalPeriod(subObj);
                }
            }
        }

        private string FormatOrbitalPeriod(float years)
        {
            if (years < (1.0f / 8766))  // Less than 1 hour
            {
                float hours = years * 8766;
                return $"{hours:F2}h";
            }
            else if (years < (1.0f / 365.25))  // Less than 1 day
            {
                float hours = years * 8766;
                return $"{hours:F2}h";
            }
            else if (years < (5.0f / 365.25))  // Less than 5 days
            {
                float days = years * 365.25f;
                float remainderHours = (days - (int)days) * 24;
                return $"{(int)days}d {remainderHours:F1}h";
            }
            else if (years < 1.0f)  // Less than 1 year
            {
                float days = years * 365.25f;
                return $"{days:F1}d";
            }
            else if (years < 5.0f)  // Less than 5 years
            {
                float remainderDays = (years - (int)years) * 365.25f;
                return $"{(int)years}y {remainderDays:F0}d";
            }
            else  // 5+ years
            {
                return $"{years:F2}y";
            }
        }

        private int CountStarsInSystem()
        {
            int count = 1; // Primary star
            count += primaryObject.celestrialObjectOrbits.Count;

            // Count sub-companions
            foreach (var obj in primaryObject.celestrialObjectOrbits)
            {
                count += obj.celestrialObjectOrbits.Count;
            }

            return count;
        }

        private int CountAllStars()
        {
            int count = 1; // Primary star

            // Count companion stars
            foreach (var obj in primaryObject.celestrialObjectOrbits)
            {
                if (obj.celestrialObject is Star)
                {
                    count++;

                    // Count sub-companions
                    foreach (var subObj in obj.celestrialObjectOrbits)
                    {
                        if (subObj.celestrialObject is Star)
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        private List<StarDisplayData> CollectAllStarData()
        {
            List<StarDisplayData> starData = new List<StarDisplayData>();
            int sortOrder = 0;

            // Get primary star
            Star? primaryStar = primaryObject.celestrialObject as Star;
            if (primaryStar == null) return starData;

            // Check if primary has a companion (Aa/Ab case)
            bool primaryHasCompanion = primaryStar.Designation.EndsWith("a");
            Star? primaryCompanion = null;
            CelestrialObject? primaryCompanionObj = null;

            if (primaryHasCompanion)
            {
                // Find Ab companion
                foreach (var obj in primaryObject.celestrialObjectOrbits)
                {
                    if (obj.celestrialObject is Star star && star.Designation == "Ab")
                    {
                        primaryCompanion = star;
                        primaryCompanionObj = obj;
                        break;
                    }
                }
            }

            // Add primary star (Aa or A)
            starData.Add(new StarDisplayData
            {
                Component = primaryStar.Designation,
                ParentDesignation = null,
                Class = $"{primaryStar.type}{primaryStar.subType} {primaryStar.starclass}",
                Mass = primaryStar.mass,
                Temp = primaryStar.temperture,
                Diameter = primaryStar.diameter,
                Luminosity = primaryStar.luminosity,
                Orbit = null,
                AU = null,
                Ecc = null,
                Period = null,
                MAO = primaryStar.MinAllowableOrbit,
                HZCO = primaryStar.HZCO,
                IsCombined = false,
                SortOrder = sortOrder++
            });

            // Add primary companion (Ab) if exists
            if (primaryCompanion != null && primaryCompanionObj != null)
            {
                starData.Add(new StarDisplayData
                {
                    Component = primaryCompanion.Designation,
                    ParentDesignation = null,
                    Class = $"{primaryCompanion.type}{primaryCompanion.subType} {primaryCompanion.starclass}",
                    Mass = primaryCompanion.mass,
                    Temp = primaryCompanion.temperture,
                    Diameter = primaryCompanion.diameter,
                    Luminosity = primaryCompanion.luminosity,
                    Orbit = null,
                    AU = null,
                    Ecc = null,
                    Period = null,
                    MAO = primaryCompanion.MinAllowableOrbit,
                    HZCO = primaryCompanion.HZCO,
                    IsCombined = false,
                    SortOrder = sortOrder++
                });

                // Add combined Aab
                starData.Add(new StarDisplayData
                {
                    Component = "Aab",
                    ParentDesignation = "A",
                    Class = "—",
                    Mass = primaryStar.mass + primaryCompanion.mass,
                    Temp = 0,
                    Diameter = 0,
                    Luminosity = primaryStar.luminosity + primaryCompanion.luminosity,
                    Orbit = primaryCompanionObj.orbit,
                    AU = primaryCompanionObj.orbitAU,
                    Ecc = primaryCompanionObj.orbitEccentricity,
                    Period = FormatOrbitalPeriod(primaryCompanionObj.OrbitalPeriodYears),
                    MAO = primaryStar.MinAllowableOrbit,
                    HZCO = primaryStar.HZCO,
                    IsCombined = true,
                    SortOrder = sortOrder++
                });
            }

            // Get all companion stars (B, C, etc.)
            var companionStars = primaryObject.celestrialObjectOrbits
                .Where(obj => obj.celestrialObject is Star)
                .OrderBy(obj => obj.orbit)
                .ToList();

            // Add secondary stars and their companions
            foreach (var companionObj in companionStars)
            {
                if (!(companionObj.celestrialObject is Star companionStar)) continue;

                // Skip Ab as we already added it
                if (companionStar.Designation == "Ab") continue;

                // Check if this companion has its own companion
                bool companionHasCompanion = companionStar.Designation.EndsWith("a");
                Star? subCompanion = null;
                CelestrialObject? subCompanionObj = null;

                if (companionHasCompanion)
                {
                    // Find the b companion
                    foreach (var subObj in companionObj.celestrialObjectOrbits)
                    {
                        if (subObj.celestrialObject is Star star && star.Designation.EndsWith("b"))
                        {
                            subCompanion = star;
                            subCompanionObj = subObj;
                            break;
                        }
                    }
                }

                // Add main companion star (Ba, Ca, etc. or B, C, etc.)
                starData.Add(new StarDisplayData
                {
                    Component = companionStar.Designation,
                    ParentDesignation = null,
                    Class = $"{companionStar.type}{companionStar.subType} {companionStar.starclass}",
                    Mass = companionStar.mass,
                    Temp = companionStar.temperture,
                    Diameter = companionStar.diameter,
                    Luminosity = companionStar.luminosity,
                    Orbit = companionHasCompanion ? null : (float?)companionObj.orbit,
                    AU = companionHasCompanion ? null : (float?)companionObj.orbitAU,
                    Ecc = companionHasCompanion ? null : (float?)companionObj.orbitEccentricity,
                    Period = companionHasCompanion ? null : FormatOrbitalPeriod(companionObj.OrbitalPeriodYears),
                    MAO = companionStar.MinAllowableOrbit,
                    HZCO = companionStar.HZCO,
                    IsCombined = false,
                    SortOrder = sortOrder++
                });

                // Add sub-companion if exists
                if (subCompanion != null && subCompanionObj != null)
                {
                    starData.Add(new StarDisplayData
                    {
                        Component = subCompanion.Designation,
                        ParentDesignation = null,
                        Class = $"{subCompanion.type}{subCompanion.subType} {subCompanion.starclass}",
                        Mass = subCompanion.mass,
                        Temp = subCompanion.temperture,
                        Diameter = subCompanion.diameter,
                        Luminosity = subCompanion.luminosity,
                        Orbit = null,
                        AU = null,
                        Ecc = null,
                        Period = null,
                        MAO = subCompanion.MinAllowableOrbit,
                        HZCO = subCompanion.HZCO,
                        IsCombined = false,
                        SortOrder = sortOrder++
                    });

                    // Add combined (e.g., Bab, Cab)
                    string baseLetter = companionStar.Designation.TrimEnd('a');
                    starData.Add(new StarDisplayData
                    {
                        Component = $"{baseLetter}ab",
                        ParentDesignation = baseLetter,
                        Class = "—",
                        Mass = companionStar.mass + subCompanion.mass,
                        Temp = 0,
                        Diameter = 0,
                        Luminosity = companionStar.luminosity + subCompanion.luminosity,
                        Orbit = companionObj.orbit,
                        AU = companionObj.orbitAU,
                        Ecc = companionObj.orbitEccentricity,
                        Period = FormatOrbitalPeriod(companionObj.OrbitalPeriodYears),
                        MAO = companionStar.MinAllowableOrbit,
                        HZCO = companionStar.HZCO,
                        IsCombined = true,
                        SortOrder = sortOrder++
                    });
                }
            }

            // Add multi-star combinations (AB, ABC, etc.) - only if needed
            // For now, we'll add them based on what worlds exist in CollectAllWorldData
            // This is a simplified version - full implementation would check which combinations have worlds
            if (companionStars.Count > 0)
            {
                List<string> starLetters = new List<string>();

                // Add primary
                if (primaryHasCompanion)
                    starLetters.Add("A"); // Use base letter for combined
                else
                    starLetters.Add(primaryStar.Designation);

                // Add companions
                foreach (var companionObj in companionStars)
                {
                    if (companionObj.celestrialObject is Star star && star.Designation != "Ab")
                    {
                        string baseLetter = star.Designation.TrimEnd('a');
                        if (!starLetters.Contains(baseLetter))
                            starLetters.Add(baseLetter);
                    }
                }

                // Add AB, ABC combinations if multiple stars
                if (starLetters.Count >= 2)
                {
                    string abDesignation = string.Join("", starLetters.Take(2));
                    float totalMass = 0;
                    float totalLuminosity = 0;

                    // Calculate combined properties for first two stars
                    if (primaryHasCompanion && primaryCompanion != null)
                    {
                        totalMass += primaryStar.mass + primaryCompanion.mass;
                        totalLuminosity += primaryStar.luminosity + primaryCompanion.luminosity;
                    }
                    else
                    {
                        totalMass += primaryStar.mass;
                        totalLuminosity += primaryStar.luminosity;
                    }

                    // Add first companion's mass/luminosity
                    var firstCompanionObj = companionStars[0];
                    if (firstCompanionObj.celestrialObject is Star firstCompanion)
                    {
                        bool firstHasCompanion = firstCompanion.Designation.EndsWith("a");
                        totalMass += firstCompanion.mass;
                        totalLuminosity += firstCompanion.luminosity;

                        if (firstHasCompanion)
                        {
                            // Add sub-companion mass/luminosity
                            foreach (var subObj in firstCompanionObj.celestrialObjectOrbits)
                            {
                                if (subObj.celestrialObject is Star subStar)
                                {
                                    totalMass += subStar.mass;
                                    totalLuminosity += subStar.luminosity;
                                    break;
                                }
                            }
                        }

                        starData.Add(new StarDisplayData
                        {
                            Component = abDesignation,
                            ParentDesignation = null,
                            Class = "—",
                            Mass = totalMass,
                            Temp = 0,
                            Diameter = 0,
                            Luminosity = totalLuminosity,
                            Orbit = firstCompanionObj.orbit,
                            AU = firstCompanionObj.orbitAU,
                            Ecc = firstCompanionObj.orbitEccentricity,
                            Period = FormatOrbitalPeriod(firstCompanionObj.OrbitalPeriodYears),
                            MAO = 0, // Combined stars don't have meaningful MAO
                            HZCO = 0,
                            IsCombined = true,
                            SortOrder = sortOrder++
                        });
                    }
                }

                // Add ABC if three or more stars
                if (starLetters.Count >= 3)
                {
                    string abcDesignation = string.Join("", starLetters.Take(3));
                    float totalMass = 0;
                    float totalLuminosity = 0;

                    // Calculate combined properties for all three stars
                    if (primaryHasCompanion && primaryCompanion != null)
                    {
                        totalMass += primaryStar.mass + primaryCompanion.mass;
                        totalLuminosity += primaryStar.luminosity + primaryCompanion.luminosity;
                    }
                    else
                    {
                        totalMass += primaryStar.mass;
                        totalLuminosity += primaryStar.luminosity;
                    }

                    // Add first two companions
                    for (int i = 0; i < Math.Min(2, companionStars.Count); i++)
                    {
                        var companionObj = companionStars[i];
                        if (companionObj.celestrialObject is Star companion)
                        {
                            bool companionHasCompanionInner = companion.Designation.EndsWith("a");
                            totalMass += companion.mass;
                            totalLuminosity += companion.luminosity;

                            if (companionHasCompanionInner)
                            {
                                foreach (var subObj in companionObj.celestrialObjectOrbits)
                                {
                                    if (subObj.celestrialObject is Star subStar)
                                    {
                                        totalMass += subStar.mass;
                                        totalLuminosity += subStar.luminosity;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    var secondCompanionObj = companionStars[1];
                    starData.Add(new StarDisplayData
                    {
                        Component = abcDesignation,
                        ParentDesignation = null,
                        Class = "—",
                        Mass = totalMass,
                        Temp = 0,
                        Diameter = 0,
                        Luminosity = totalLuminosity,
                        Orbit = secondCompanionObj.orbit,
                        AU = secondCompanionObj.orbitAU,
                        Ecc = secondCompanionObj.orbitEccentricity,
                        Period = FormatOrbitalPeriod(secondCompanionObj.OrbitalPeriodYears),
                        MAO = 0,
                        HZCO = 0,
                        IsCombined = true,
                        SortOrder = sortOrder++
                    });
                }
            }

            return starData.OrderBy(s => s.SortOrder).ToList();
        }

        private List<WorldDisplayData> CollectAllWorldData()
        {
            List<WorldDisplayData> worldData = new List<WorldDisplayData>();

            // Get all companion stars sorted by orbit
            var companionStars = primaryObject.celestrialObjectOrbits
                .Where(obj => obj.celestrialObject is Star)
                .OrderBy(obj => obj.orbit)
                .ToList();

            // Collect worlds from primary star
            if (primaryObject.celestrialObject is Star primaryStar)
            {
                foreach (var bodyObj in primaryObject.celestrialObjectOrbits)
                {
                    if (bodyObj.celestrialObject is CelestialBody body && !(body is EmptyOrbit))
                    {
                        string primaryDesignation = DeterminePrimaryDesignation(bodyObj, primaryStar, companionStars);
                        string notes = BuildNotesString(bodyObj, body, primaryStar);

                        // Determine size based on body type
                        string size = "";
                        string sub = "";
                        int ringCount = 0;
                        List<Moon> moons = new List<Moon>();

                        if (body is TerrestrialPlanet tp)
                        {
                            size = tp.Size + "??";
                            ringCount = tp.RingCount;
                            moons = tp.Moons;
                            sub = (moons.Count - ringCount).ToString();
                        }
                        else if (body is GasGiant gg)
                        {
                            size = $"{gg.Size}{ToEhex(gg.Diameter)}";
                            ringCount = gg.RingCount;
                            moons = gg.Moons;
                            sub = (moons.Count - ringCount).ToString();

                            // Add mass to notes with ME suffix, but after HZ if present
                            if (!string.IsNullOrEmpty(notes))
                            {
                                if (notes.StartsWith("HZ"))
                                    notes = $"HZ, {gg.GasGiantMass}ME" + (notes.Length > 2 ? ", " + notes.Substring(2).TrimStart(',', ' ') : "");
                                else
                                    notes = $"{gg.GasGiantMass}ME, {notes}";
                            }
                            else
                                notes = $"{gg.GasGiantMass}ME";
                        }
                        else if (body is PlanetoidBelt)
                        {
                            sub = "?";
                        }

                        // Add ring count and moon sizes to notes
                        if (ringCount > 0 || moons.Count > 0)
                        {
                            List<string> moonInfo = new List<string>();
                            if (ringCount > 0)
                                moonInfo.Add($"R0{ringCount}");

                            foreach (var moon in moons)
                            {
                                moonInfo.Add(moon.Size);
                            }

                            string moonString = string.Join(", ", moonInfo);
                            if (!string.IsNullOrEmpty(notes))
                                notes = $"{notes}, {moonString}";
                            else
                                notes = moonString;
                        }

                        worldData.Add(new WorldDisplayData
                        {
                            Primary = primaryDesignation,
                            Object = body.Designation,
                            Size = size,
                            Orbit = bodyObj.orbit,
                            AU = bodyObj.orbitAU,
                            Ecc = bodyObj.orbitEccentricity,
                            Period = FormatOrbitalPeriod(bodyObj.OrbitalPeriodYears),
                            Sub = sub,
                            Notes = notes
                        });
                    }
                }
            }

            // Collect worlds from companion stars
            foreach (var companionObj in companionStars)
            {
                if (companionObj.celestrialObject is Star companionStar)
                {
                    foreach (var bodyObj in companionObj.celestrialObjectOrbits)
                    {
                        if (bodyObj.celestrialObject is CelestialBody body && !(body is EmptyOrbit))
                        {
                            string primaryDesignation = DeterminePrimaryDesignation(bodyObj, companionStar, new List<CelestrialObject>());
                            string notes = BuildNotesString(bodyObj, body, companionStar);

                            // Determine size based on body type
                            string size = "";
                            string sub = "";
                            int ringCount = 0;
                            List<Moon> moons = new List<Moon>();

                            if (body is TerrestrialPlanet tp)
                            {
                                size = tp.Size + "??";
                                ringCount = tp.RingCount;
                                moons = tp.Moons;
                                sub = (moons.Count - ringCount).ToString();
                            }
                            else if (body is GasGiant gg)
                            {
                                size = $"{gg.Size}{ToEhex(gg.Diameter)}";
                                ringCount = gg.RingCount;
                                moons = gg.Moons;
                                sub = (moons.Count - ringCount).ToString();

                                // Add mass to notes with ME suffix, but after HZ if present
                                if (!string.IsNullOrEmpty(notes))
                                {
                                    if (notes.StartsWith("HZ"))
                                        notes = $"HZ, {gg.GasGiantMass}ME" + (notes.Length > 2 ? ", " + notes.Substring(2).TrimStart(',', ' ') : "");
                                    else
                                        notes = $"{gg.GasGiantMass}ME, {notes}";
                                }
                                else
                                    notes = $"{gg.GasGiantMass}ME";
                            }
                            else if (body is PlanetoidBelt)
                            {
                                sub = "?";
                            }

                            // Add ring count and moon sizes to notes
                            if (ringCount > 0 || moons.Count > 0)
                            {
                                List<string> moonInfo = new List<string>();
                                if (ringCount > 0)
                                    moonInfo.Add($"R0{ringCount}");

                                foreach (var moon in moons)
                                {
                                    moonInfo.Add(moon.Size);
                                }

                                string moonString = string.Join(", ", moonInfo);
                                if (!string.IsNullOrEmpty(notes))
                                    notes = $"{notes}, {moonString}";
                                else
                                    notes = moonString;
                            }

                            worldData.Add(new WorldDisplayData
                            {
                                Primary = primaryDesignation,
                                Object = body.Designation,
                                Size = size,
                                Orbit = bodyObj.orbit,
                                AU = bodyObj.orbitAU,
                                Ecc = bodyObj.orbitEccentricity,
                                Period = FormatOrbitalPeriod(bodyObj.OrbitalPeriodYears),
                                Sub = sub,
                                Notes = notes
                            });
                        }
                    }
                }
            }

            return worldData;
        }

        private void PrintStellarSummary()
        {
            int starCount = CountAllStars();

            Console.WriteLine("STELLAR");
            Console.WriteLine($"  Stars: {starCount}");
            Console.WriteLine($"  Gas Giants: {GasGiantCount}");
            Console.WriteLine($"  Planetoid Belts: {PlanetoidBeltCount}");
            Console.WriteLine($"  Terrestrials: {TerrestrialPlanetCount}");
            Console.WriteLine();
        }

        private void PrintStarsTable(List<StarDisplayData> starData)
        {
            if (starData.Count == 0) return;

            Console.WriteLine("STARS");

            // Calculate column widths dynamically based on data
            int compWidth = Math.Max("Component".Length, starData.Max(s => s.Component.Length + (s.ParentDesignation != null ? 4 : 0)));
            int classWidth = Math.Max("Class".Length, starData.Max(s => s.Class.Length));
            int massWidth = Math.Max("Mass".Length, starData.Max(s => s.Mass.ToString("F3").Length));
            int tempWidth = Math.Max("Temp".Length, starData.Max(s => s.Temp > 0 ? s.Temp.ToString("F0").Length : 1));
            int diamWidth = Math.Max("Diam".Length, starData.Max(s => s.Diameter > 0 ? s.Diameter.ToString("F3").Length : 1));
            int luminWidth = Math.Max("Lumin".Length, starData.Max(s => s.Luminosity.ToString("F4").Length));
            int orbitWidth = Math.Max("Orbit#".Length, starData.Max(s => s.Orbit.HasValue ? s.Orbit.Value.ToString("F2").Length : 1));
            int auWidth = Math.Max("AU".Length, starData.Max(s => s.AU.HasValue ? s.AU.Value.ToString("F2").Length : 1));
            int eccWidth = Math.Max("Ecc".Length, starData.Max(s => s.Ecc.HasValue ? s.Ecc.Value.ToString("F2").Length : 1));
            int periodWidth = Math.Max("Period".Length, starData.Max(s => s.Period != null ? s.Period.Length : 1));
            int maoWidth = Math.Max("MAO".Length, starData.Max(s => s.MAO > 0 ? s.MAO.ToString("F2").Length : 1));
            int hzcoWidth = Math.Max("HZCO".Length, starData.Max(s => s.HZCO > 0 ? s.HZCO.ToString("F2").Length : 1));

            // Print header
            Console.WriteLine($"{"Component".PadRight(compWidth)} {"Class".PadRight(classWidth)} {"Mass".PadLeft(massWidth)} {"Temp".PadLeft(tempWidth)} {"Diam".PadLeft(diamWidth)} {"Lumin".PadLeft(luminWidth)} {"Orbit#".PadLeft(orbitWidth)} {"AU".PadLeft(auWidth)} {"Ecc".PadLeft(eccWidth)} {"Period".PadRight(periodWidth)} {"MAO".PadLeft(maoWidth)} {"HZCO".PadLeft(hzcoWidth)}");

            // Print each star
            foreach (var star in starData)
            {
                string component = star.Component;
                if (star.ParentDesignation != null)
                    component += $" ({star.ParentDesignation})";

                string mass = star.Mass.ToString("F3");
                string temp = star.Temp > 0 ? star.Temp.ToString("F0") : "—";
                string diam = star.Diameter > 0 ? star.Diameter.ToString("F3") : "—";
                string lumin = star.Luminosity.ToString("F4");
                string orbit = star.Orbit.HasValue ? star.Orbit.Value.ToString("F2") : "—";
                string au = star.AU.HasValue ? star.AU.Value.ToString("F2") : "—";
                string ecc = star.Ecc.HasValue ? star.Ecc.Value.ToString("F2") : "—";
                string period = star.Period ?? "—";
                string mao = star.MAO > 0 ? star.MAO.ToString("F2") : "—";
                string hzco = star.HZCO > 0 ? star.HZCO.ToString("F2") : "—";

                Console.WriteLine($"{component.PadRight(compWidth)} {star.Class.PadRight(classWidth)} {mass.PadLeft(massWidth)} {temp.PadLeft(tempWidth)} {diam.PadLeft(diamWidth)} {lumin.PadLeft(luminWidth)} {orbit.PadLeft(orbitWidth)} {au.PadLeft(auWidth)} {ecc.PadLeft(eccWidth)} {period.PadRight(periodWidth)} {mao.PadLeft(maoWidth)} {hzco.PadLeft(hzcoWidth)}");
            }

            Console.WriteLine();
        }

        private void PrintObjectsTable(List<WorldDisplayData> worldData)
        {
            if (worldData.Count == 0)
            {
                Console.WriteLine("OBJECTS");
                Console.WriteLine("  No worlds generated in this system");
                Console.WriteLine();
                return;
            }

            Console.WriteLine("OBJECTS");

            // Calculate column widths dynamically based on data
            int primaryWidth = Math.Max("Primary".Length, worldData.Max(w => w.Primary.Length));
            int objectWidth = Math.Max("Object".Length, worldData.Max(w => w.Object.Length));
            int sizeWidth = worldData.Any(w => w.Size.Length > 0) ? Math.Max("SAH/UWP".Length, worldData.Max(w => w.Size.Length)) : "SAH/UWP".Length;
            int orbitWidth = Math.Max("Orbit#".Length, worldData.Max(w => w.Orbit.ToString("F2").Length));
            int auWidth = Math.Max("AU".Length, worldData.Max(w => w.AU.ToString("F2").Length));
            int eccWidth = Math.Max("Ecc".Length, worldData.Max(w => w.Ecc.ToString("F3").Length));
            int periodWidth = Math.Max("Period".Length, worldData.Max(w => w.Period.Length));
            int subWidth = Math.Max("Sub".Length, worldData.Any(w => w.Sub.Length > 0) ? worldData.Max(w => w.Sub.Length) : 1);
            int notesWidth = worldData.Any(w => w.Notes.Length > 0) ? Math.Max("Notes".Length, worldData.Max(w => w.Notes.Length)) : "Notes".Length;

            // Print header - moved Size after Period, changed heading to SAH/UWP, added Sub column, added extra spacing
            Console.WriteLine($"{"Primary".PadRight(primaryWidth)} {"Object".PadRight(objectWidth)}  {"Orbit#".PadLeft(orbitWidth)}  {"AU".PadLeft(auWidth)}  {"Ecc".PadLeft(eccWidth)}  {"Period".PadRight(periodWidth)} {"SAH/UWP".PadRight(sizeWidth)} {"Sub".PadLeft(subWidth)} {"Notes".PadRight(notesWidth)}");

            // Group by primary and print
            var groupedWorlds = worldData.GroupBy(w => w.Primary).OrderBy(g => g.Key);
            foreach (var group in groupedWorlds)
            {
                foreach (var world in group.OrderBy(w => w.Orbit))
                {
                    string orbit = world.Orbit.ToString("F2");
                    string au = world.AU.ToString("F2");
                    string ecc = world.Ecc.ToString("F3");

                    Console.WriteLine($"{world.Primary.PadRight(primaryWidth)} {world.Object.PadRight(objectWidth)}  {orbit.PadLeft(orbitWidth)}  {au.PadLeft(auWidth)}  {ecc.PadLeft(eccWidth)}  {world.Period.PadRight(periodWidth)} {world.Size.PadRight(sizeWidth)} {world.Sub.PadLeft(subWidth)} {world.Notes}");
                }
            }

            Console.WriteLine();
        }

        private int CountDStarsInSystem()
        {
            int count = 0;

            // Check primary
            if (primaryObject.celestrialObject is Star primaryStar && primaryStar.type == "D")
            {
                count++;
            }

            // Check companions
            foreach (var obj in primaryObject.celestrialObjectOrbits)
            {
                if (obj.celestrialObject is Star star && star.type == "D")
                {
                    count++;
                }

                // Check sub-companions
                foreach (var subObj in obj.celestrialObjectOrbits)
                {
                    if (subObj.celestrialObject is Star subStar && subStar.type == "D")
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private bool IsSingleClassVStar()
        {
            // Must have exactly 1 star (no companions)
            if (primaryObject.celestrialObjectOrbits.Count > 0)
                return false;

            // Primary must be Class V
            if (primaryObject.celestrialObject is Star primaryStar)
            {
                return primaryStar.starclass == "V";
            }

            return false;
        }

        private bool IsPrimaryBDOrD()
        {
            if (primaryObject.celestrialObject is Star primaryStar)
            {
                return primaryStar.type == "BD" || primaryStar.type == "D";
            }
            return false;
        }

        private CelestrialObject? FindCompanionByOrbitType(Starhelper.starOrbitType orbitType)
        {
            // Search primaryObject.celestrialObjectOrbits for companion with matching orbit type
            // Used in MaxAllowableOrbit calculations
            foreach (var obj in primaryObject.celestrialObjectOrbits)
            {
                if (obj.celestrialObject is Star star && star.starOrbitType == orbitType)
                {
                    return obj;
                }
            }
            return null;
        }

        private bool HasCompanionOrbitCompanion(CelestrialObject companionObj)
        {
            // Check if this companion has any Companion orbit sub-companions
            foreach (var subObj in companionObj.celestrialObjectOrbits)
            {
                if (subObj.celestrialObject is Star subStar &&
                    subStar.starOrbitType == Starhelper.starOrbitType.Companion)
                {
                    return true;
                }
            }
            return false;
        }

        private CelestrialObject? GetCompanionOrbitCompanion(CelestrialObject companionObj)
        {
            // Get the first Companion orbit sub-companion of this companion
            foreach (var subObj in companionObj.celestrialObjectOrbits)
            {
                if (subObj.celestrialObject is Star subStar &&
                    subStar.starOrbitType == Starhelper.starOrbitType.Companion)
                {
                    return subObj;
                }
            }
            return null;
        }

        private void AdjustMinAllowableOrbitsForCompanions()
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("ADJUSTING MIN ALLOWABLE ORBITS FOR COMPANION COMPANIONS");

            bool anyAdjustments = false;

            // Check if primary star has a Companion orbit companion
            if (primaryObject.celestrialObject is Star primaryStar)
            {
                if (HasCompanionOrbitCompanion(primaryObject))
                {
                    CelestrialObject? subCompanion = GetCompanionOrbitCompanion(primaryObject);
                    if (subCompanion != null)
                    {
                        anyAdjustments = true;
                        float oldMin = primaryStar.MinAllowableOrbit;
                        float companionEcc = subCompanion.orbitEccentricity;

                        DebugLogger.LogFormat("Primary star has Companion orbit companion (eccentricity {0:F3})", companionEcc);
                        DebugLogger.LogFormat("  Original MinAllowableOrbit: {0:F3}", oldMin);

                        if (oldMin <= 0.2f)
                        {
                            primaryStar.MinAllowableOrbit = 0.5f + companionEcc;
                            DebugLogger.LogFormat("  MinAllowableOrbit <= 0.2, setting to: 0.5 + {0:F3} = {1:F3}",
                                companionEcc, primaryStar.MinAllowableOrbit);
                        }
                        else
                        {
                            primaryStar.MinAllowableOrbit = oldMin + 0.5f + companionEcc;
                            DebugLogger.LogFormat("  MinAllowableOrbit > 0.2, setting to: {0:F3} + 0.5 + {1:F3} = {2:F3}",
                                oldMin, companionEcc, primaryStar.MinAllowableOrbit);
                        }
                    }
                }
            }

            // Check each Close/Near/Far companion
            foreach (var companion in primaryObject.celestrialObjectOrbits)
            {
                if (companion.celestrialObject is Star companionStar)
                {
                    // Check if this companion has a Companion orbit companion
                    if (HasCompanionOrbitCompanion(companion))
                    {
                        CelestrialObject? subCompanion = GetCompanionOrbitCompanion(companion);
                        if (subCompanion != null)
                        {
                            anyAdjustments = true;
                            float oldMin = companionStar.MinAllowableOrbit;
                            float companionEcc = subCompanion.orbitEccentricity;

                            DebugLogger.LogFormat("{0} companion has Companion orbit companion (eccentricity {1:F3})",
                                companionStar.starOrbitType, companionEcc);
                            DebugLogger.LogFormat("  Original MinAllowableOrbit: {0:F3}", oldMin);

                            if (oldMin <= 0.2f)
                            {
                                companionStar.MinAllowableOrbit = 0.5f + companionEcc;
                                DebugLogger.LogFormat("  MinAllowableOrbit <= 0.2, setting to: 0.5 + {0:F3} = {1:F3}",
                                    companionEcc, companionStar.MinAllowableOrbit);
                            }
                            else
                            {
                                companionStar.MinAllowableOrbit = oldMin + 0.5f + companionEcc;
                                DebugLogger.LogFormat("  MinAllowableOrbit > 0.2, setting to: {0:F3} + 0.5 + {1:F3} = {2:F3}",
                                    oldMin, companionEcc, companionStar.MinAllowableOrbit);
                            }
                        }
                    }
                }
            }

            if (!anyAdjustments)
            {
                DebugLogger.Log("No stars with Companion orbit companions found - no adjustments needed");
            }
        }

        private void CalculateAllMaxAllowableOrbits()
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("CALCULATING MAX ALLOWABLE ORBITS");

            // Primary star
            if (primaryObject.celestrialObject is Star primaryStar)
            {
                primaryStar.MaxAllowableOrbit = 20f;
                DebugLogger.Log("Primary star MaxAllowableOrbit: 20.000");
            }

            // Companions
            foreach (var companion in primaryObject.celestrialObjectOrbits)
            {
                if (companion.celestrialObject is Star companionStar)
                {
                    CalculateMaxAllowableOrbitForStar(companionStar, companion);

                    // Sub-companions (Companion orbit companions)
                    foreach (var subCompanion in companion.celestrialObjectOrbits)
                    {
                        if (subCompanion.celestrialObject is Star subStar)
                        {
                            CalculateMaxAllowableOrbitForStar(subStar, subCompanion);
                        }
                    }
                }
            }
        }

        private void CalculateMaxAllowableOrbitForStar(Star star, CelestrialObject starObj)
        {
            DebugLogger.Log("");
            DebugLogger.LogFormat("Calculating MaxAllowableOrbit for {0} star...", star.starOrbitType);

            // Companion orbit companions don't have MaxAllowableOrbit
            if (star.starOrbitType == Starhelper.starOrbitType.Companion)
            {
                star.MaxAllowableOrbit = 0;
                DebugLogger.Log("  Companion orbit star - no MaxAllowableOrbit");
                return;
            }

            // Base calculation: orbit - 3
            float baseMax = starObj.orbit - 3;
            DebugLogger.LogFormat("  Base calculation: {0:F3} - 3 = {1:F3}", starObj.orbit, baseMax);

            int reductions = 0;

            // Get all Close/Near/Far companions
            CelestrialObject? closeCompanion = FindCompanionByOrbitType(Starhelper.starOrbitType.Close);
            CelestrialObject? nearCompanion = FindCompanionByOrbitType(Starhelper.starOrbitType.Near);
            CelestrialObject? farCompanion = FindCompanionByOrbitType(Starhelper.starOrbitType.Far);

            // Apply reductions based on orbit type and other companions
            if (star.starOrbitType == Starhelper.starOrbitType.Close)
            {
                // Reduction: Near companion exists
                if (nearCompanion != null)
                {
                    reductions += 1;
                    DebugLogger.Log("  -1 (Near companion exists)");
                }

                // Reductions: Eccentricity > 0.2
                if (closeCompanion != null && closeCompanion.orbitEccentricity > 0.2f)
                {
                    reductions += 1;
                    DebugLogger.LogFormat("  -1 (Close eccentricity {0:F3} > 0.2)", closeCompanion.orbitEccentricity);
                }

                if (nearCompanion != null && nearCompanion.orbitEccentricity > 0.2f)
                {
                    reductions += 1;
                    DebugLogger.LogFormat("  -1 (Near eccentricity {0:F3} > 0.2)", nearCompanion.orbitEccentricity);
                }

                // Reductions: Eccentricity > 0.5
                if (closeCompanion != null && closeCompanion.orbitEccentricity > 0.5f)
                {
                    reductions += 1;
                    DebugLogger.LogFormat("  -1 (Close eccentricity {0:F3} > 0.5)", closeCompanion.orbitEccentricity);
                }

                if (nearCompanion != null && nearCompanion.orbitEccentricity > 0.5f)
                {
                    reductions += 1;
                    DebugLogger.LogFormat("  -1 (Near eccentricity {0:F3} > 0.5)", nearCompanion.orbitEccentricity);
                }
            }
            else if (star.starOrbitType == Starhelper.starOrbitType.Near)
            {
                // Reduction: Close or Far companion exists
                if (closeCompanion != null || farCompanion != null)
                {
                    reductions += 1;
                    DebugLogger.Log("  -1 (Close or Far companion exists)");
                }

                // Reductions: Eccentricity > 0.2
                if (closeCompanion != null && closeCompanion.orbitEccentricity > 0.2f)
                {
                    reductions += 1;
                    DebugLogger.LogFormat("  -1 (Close eccentricity {0:F3} > 0.2)", closeCompanion.orbitEccentricity);
                }

                if (nearCompanion != null && nearCompanion.orbitEccentricity > 0.2f)
                {
                    reductions += 1;
                    DebugLogger.LogFormat("  -1 (Near eccentricity {0:F3} > 0.2)", nearCompanion.orbitEccentricity);
                }

                if (farCompanion != null && farCompanion.orbitEccentricity > 0.2f)
                {
                    reductions += 1;
                    DebugLogger.LogFormat("  -1 (Far eccentricity {0:F3} > 0.2)", farCompanion.orbitEccentricity);
                }

                // Reductions: Eccentricity > 0.5
                if (closeCompanion != null && closeCompanion.orbitEccentricity > 0.5f)
                {
                    reductions += 1;
                    DebugLogger.LogFormat("  -1 (Close eccentricity {0:F3} > 0.5)", closeCompanion.orbitEccentricity);
                }

                if (nearCompanion != null && nearCompanion.orbitEccentricity > 0.5f)
                {
                    reductions += 1;
                    DebugLogger.LogFormat("  -1 (Near eccentricity {0:F3} > 0.5)", nearCompanion.orbitEccentricity);
                }

                if (farCompanion != null && farCompanion.orbitEccentricity > 0.5f)
                {
                    reductions += 1;
                    DebugLogger.LogFormat("  -1 (Far eccentricity {0:F3} > 0.5)", farCompanion.orbitEccentricity);
                }
            }
            else if (star.starOrbitType == Starhelper.starOrbitType.Far)
            {
                // Reduction: Near companion exists
                if (nearCompanion != null)
                {
                    reductions += 1;
                    DebugLogger.Log("  -1 (Near companion exists)");
                }

                // Reductions: Eccentricity > 0.2
                if (nearCompanion != null && nearCompanion.orbitEccentricity > 0.2f)
                {
                    reductions += 1;
                    DebugLogger.LogFormat("  -1 (Near eccentricity {0:F3} > 0.2)", nearCompanion.orbitEccentricity);
                }

                if (farCompanion != null && farCompanion.orbitEccentricity > 0.2f)
                {
                    reductions += 1;
                    DebugLogger.LogFormat("  -1 (Far eccentricity {0:F3} > 0.2)", farCompanion.orbitEccentricity);
                }

                // Reductions: Eccentricity > 0.5
                if (nearCompanion != null && nearCompanion.orbitEccentricity > 0.5f)
                {
                    reductions += 1;
                    DebugLogger.LogFormat("  -1 (Near eccentricity {0:F3} > 0.5)", nearCompanion.orbitEccentricity);
                }

                if (farCompanion != null && farCompanion.orbitEccentricity > 0.5f)
                {
                    reductions += 1;
                    DebugLogger.LogFormat("  -1 (Far eccentricity {0:F3} > 0.5)", farCompanion.orbitEccentricity);
                }
            }

            // Calculate final MaxAllowableOrbit
            float finalMax = baseMax - reductions;

            // Clamp to MinAllowableOrbit if negative (can happen for companions at orbit < 3)
            if (finalMax < star.MinAllowableOrbit)
            {
                DebugLogger.LogFormat("  Calculated MaxAllowableOrbit ({0:F3}) < MinAllowableOrbit ({1:F3})",
                    finalMax, star.MinAllowableOrbit);
                finalMax = star.MinAllowableOrbit;
                DebugLogger.LogFormat("  Clamping MaxAllowableOrbit to MinAllowableOrbit: {0:F3}", finalMax);
            }

            star.MaxAllowableOrbit = finalMax;

            if (reductions > 0)
            {
                DebugLogger.LogFormat("  Final MaxAllowableOrbit: {0:F3} - {1} = {2:F3}", baseMax, reductions, finalMax);
            }
            else
            {
                DebugLogger.LogFormat("  Final MaxAllowableOrbit: {0:F3}", finalMax);
            }
        }

        private void CalculateAllUnavailableOrbits()
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("CALCULATING UNAVAILABLE ORBITS");

            // Primary star
            if (primaryObject.celestrialObject is Star primaryStar)
            {
                CalculateUnavailableOrbitsForStar(primaryStar, primaryObject);
            }

            // Companions (but NOT Companion orbit companions)
            foreach (var companion in primaryObject.celestrialObjectOrbits)
            {
                if (companion.celestrialObject is Star companionStar)
                {
                    if (companionStar.starOrbitType != Starhelper.starOrbitType.Companion)
                    {
                        CalculateUnavailableOrbitsForStar(companionStar, companion);
                    }
                }
            }
        }

        private void CalculateUnavailableOrbitsForStar(Star star, CelestrialObject starObj)
        {
            DebugLogger.Log("");
            DebugLogger.LogFormat("Calculating unavailable orbits for {0} star...", star.starOrbitType);

            // Companion orbit stars don't have unavailable orbits
            if (star.starOrbitType == Starhelper.starOrbitType.Companion)
            {
                DebugLogger.Log("  Companion orbit star - no unavailable orbits");
                return;
            }

            star.UnavailableOrbitRanges.Clear();

            // Get all Close/Near/Far companions from primary
            List<CelestrialObject> companionsToCheck = new List<CelestrialObject>();
            foreach (var obj in primaryObject.celestrialObjectOrbits)
            {
                if (obj.celestrialObject is Star s &&
                    s.starOrbitType != Starhelper.starOrbitType.Companion)
                {
                    companionsToCheck.Add(obj);
                }
            }

            // Check each other companion
            foreach (var otherCompanion in companionsToCheck)
            {
                // Don't compare with self
                if (otherCompanion == starObj)
                {
                    continue;
                }

                if (otherCompanion.celestrialObject is Star otherStar)
                {
                    float companionOrbit = otherCompanion.orbit;
                    float companionEccentricity = otherCompanion.orbitEccentricity;

                    // Base range: ±1
                    float rangeOffset = 1f;

                    // If eccentricity > 0.2: expand to ±2
                    if (companionEccentricity > 0.2f)
                    {
                        rangeOffset = 2f;
                        DebugLogger.LogFormat("  {0} companion (ecc {1:F3} > 0.2): expanding range to ±2",
                            otherStar.starOrbitType, companionEccentricity);
                    }

                    // If Close or Near companion with eccentricity > 0.5: expand to ±3
                    if ((otherStar.starOrbitType == Starhelper.starOrbitType.Close ||
                         otherStar.starOrbitType == Starhelper.starOrbitType.Near) &&
                        companionEccentricity > 0.5f)
                    {
                        rangeOffset = 3f;
                        DebugLogger.LogFormat("  {0} companion (ecc {1:F3} > 0.5): expanding range to ±3",
                            otherStar.starOrbitType, companionEccentricity);
                    }

                    float minUnavailable = companionOrbit - rangeOffset;
                    float maxUnavailable = companionOrbit + rangeOffset;

                    // Clamp minimum to 0 (orbits can't be negative)
                    if (minUnavailable < 0)
                    {
                        minUnavailable = 0;
                    }

                    star.UnavailableOrbitRanges.Add((minUnavailable, maxUnavailable));

                    DebugLogger.LogFormat("  {0} companion at orbit {1:F2} (ecc {2:F3})",
                        otherStar.starOrbitType, companionOrbit, companionEccentricity);
                    DebugLogger.LogFormat("    Makes orbits {0:F2} to {1:F2} unavailable",
                        minUnavailable, maxUnavailable);
                }
            }

            if (star.UnavailableOrbitRanges.Count == 0)
            {
                DebugLogger.Log("  No unavailable orbits for this star");
            }
        }

        private void CalculateOrbitalAvailability()
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("ORBITAL AVAILABILITY CALCULATIONS");

            // Step 1: Adjust MinAllowableOrbit for stars with Companion companions
            AdjustMinAllowableOrbitsForCompanions();

            // Step 2: Calculate MaxAllowableOrbit for all stars
            CalculateAllMaxAllowableOrbits();

            // Step 3: Calculate unavailable orbits for all stars (except Companion orbit stars)
            CalculateAllUnavailableOrbits();

            DebugLogger.Log("");
            DebugLogger.Log("Orbital availability calculations complete");
        }

        private float ConvertAUToOrbitNumber(float au)
        {
            // Find the orbit number that corresponds to this AU value
            // Use Starhelper.orbitValues dictionary to interpolate

            for (int i = 0; i < 20; i++)
            {
                float lowerAU = Starhelper.orbitValues[i];
                float upperAU = Starhelper.orbitValues[i + 1];

                if (au >= lowerAU && au <= upperAU)
                {
                    // Interpolate between orbit numbers
                    float fraction = (au - lowerAU) / (upperAU - lowerAU);
                    float orbitNumber = i + fraction;
                    return orbitNumber;
                }
            }

            // If AU is beyond orbit 20, return 20
            if (au > Starhelper.orbitValues[20])
                return 20f;

            // If AU is less than orbit 0, return 0
            return 0f;
        }

        private void CalculateAllHZCO()
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("CALCULATING HABITABLE ZONE CENTER ORBITS");

            // Primary star
            if (primaryObject.celestrialObject is Star primaryStar)
            {
                CalculateHZCOForStar(primaryStar, primaryObject);
            }

            // Close/Near/Far companions (but NOT Companion orbit companions)
            foreach (var companion in primaryObject.celestrialObjectOrbits)
            {
                if (companion.celestrialObject is Star companionStar)
                {
                    if (companionStar.starOrbitType != Starhelper.starOrbitType.Companion)
                    {
                        CalculateHZCOForStar(companionStar, companion);
                    }
                }
            }
        }

        private void CalculateHZCOForStar(Star star, CelestrialObject starObj)
        {
            DebugLogger.Log("");
            DebugLogger.LogFormat("Calculating HZCO for {0} star...", star.starOrbitType);

            // Companion orbit stars don't have HZCO
            if (star.starOrbitType == Starhelper.starOrbitType.Companion)
            {
                star.HZCO = 0;
                DebugLogger.Log("  Companion orbit star - no HZCO");
                return;
            }

            // Step 1: Calculate initial HZCO in AU
            float totalLuminosity = star.luminosity;
            DebugLogger.LogFormat("  Star luminosity: {0:F6}", star.luminosity);

            // Add luminosity of any Companion companion
            foreach (var subObj in starObj.celestrialObjectOrbits)
            {
                if (subObj.celestrialObject is Star subStar &&
                    subStar.starOrbitType == Starhelper.starOrbitType.Companion)
                {
                    totalLuminosity += subStar.luminosity;
                    DebugLogger.LogFormat("  + Companion companion luminosity: {0:F6}", subStar.luminosity);
                }
            }

            float hzcoAU = (float)Math.Sqrt(totalLuminosity);
            DebugLogger.LogFormat("  Initial HZCO (AU): sqrt({0:F6}) = {1:F6}", totalLuminosity, hzcoAU);

            // Step 2: Convert to orbit number
            float initialHZCO = ConvertAUToOrbitNumber(hzcoAU);
            DebugLogger.LogFormat("  Initial HZCO (orbit): {0:F3}", initialHZCO);

            // Step 3: For PRIMARY only, check if we need to recalculate
            if (star.starOrbitType == Starhelper.starOrbitType.Primary)
            {
                // Check if any Close/Near/Far companions have orbits < initial HZCO
                bool needsRecalculation = false;
                List<CelestrialObject> starsToInclude = new List<CelestrialObject>();

                foreach (var companion in primaryObject.celestrialObjectOrbits)
                {
                    if (companion.celestrialObject is Star companionStar &&
                        companionStar.starOrbitType != Starhelper.starOrbitType.Companion)
                    {
                        if (companion.orbit < initialHZCO)
                        {
                            needsRecalculation = true;
                            starsToInclude.Add(companion);
                            DebugLogger.LogFormat("  {0} companion at orbit {1:F2} < initial HZCO {2:F3}",
                                companionStar.starOrbitType, companion.orbit, initialHZCO);
                        }
                    }
                }

                if (needsRecalculation)
                {
                    DebugLogger.Log("  Recalculating HZCO to include inner companions...");

                    // Recalculate with all stars in orbits < initial HZCO
                    totalLuminosity = star.luminosity;
                    DebugLogger.LogFormat("  Primary luminosity: {0:F6}", star.luminosity);

                    // Add primary's Companion companion
                    foreach (var subObj in starObj.celestrialObjectOrbits)
                    {
                        if (subObj.celestrialObject is Star subStar &&
                            subStar.starOrbitType == Starhelper.starOrbitType.Companion)
                        {
                            totalLuminosity += subStar.luminosity;
                            DebugLogger.LogFormat("  + Primary's Companion companion luminosity: {0:F6}", subStar.luminosity);
                        }
                    }

                    // Add inner companions and their Companion companions
                    foreach (var innerComp in starsToInclude)
                    {
                        if (innerComp.celestrialObject is Star innerStar)
                        {
                            totalLuminosity += innerStar.luminosity;
                            DebugLogger.LogFormat("  + {0} companion luminosity: {1:F6}",
                                innerStar.starOrbitType, innerStar.luminosity);

                            // Add any Companion companions of this inner companion
                            foreach (var subObj in innerComp.celestrialObjectOrbits)
                            {
                                if (subObj.celestrialObject is Star subStar &&
                                    subStar.starOrbitType == Starhelper.starOrbitType.Companion)
                                {
                                    totalLuminosity += subStar.luminosity;
                                    DebugLogger.LogFormat("  + {0} companion's Companion companion luminosity: {1:F6}",
                                        innerStar.starOrbitType, subStar.luminosity);
                                }
                            }
                        }
                    }

                    hzcoAU = (float)Math.Sqrt(totalLuminosity);
                    DebugLogger.LogFormat("  Recalculated HZCO (AU): sqrt({0:F6}) = {1:F6}", totalLuminosity, hzcoAU);

                    star.HZCO = ConvertAUToOrbitNumber(hzcoAU);
                    DebugLogger.LogFormat("  Final HZCO (orbit): {0:F3}", star.HZCO);
                }
                else
                {
                    star.HZCO = initialHZCO;
                    DebugLogger.Log("  No companions in inner orbits - using initial HZCO");
                }
            }
            else
            {
                // Not primary - use initial HZCO
                star.HZCO = initialHZCO;
                DebugLogger.LogFormat("  Final HZCO (orbit): {0:F3}", star.HZCO);
            }
        }

        private void CalculateAllTotalAvailableOrbits()
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("CALCULATING TOTAL AVAILABLE ORBITS");

            SystemTotalAvailableOrbits = 0;

            // Primary star
            if (primaryObject.celestrialObject is Star primaryStar)
            {
                CalculateTotalAvailableOrbitsForStar(primaryStar, primaryObject);
                SystemTotalAvailableOrbits += primaryStar.TotalAvailableOrbits;
            }

            // Close/Near/Far companions (but NOT Companion orbit companions)
            foreach (var companion in primaryObject.celestrialObjectOrbits)
            {
                if (companion.celestrialObject is Star companionStar)
                {
                    if (companionStar.starOrbitType != Starhelper.starOrbitType.Companion)
                    {
                        CalculateTotalAvailableOrbitsForStar(companionStar, companion);
                        SystemTotalAvailableOrbits += companionStar.TotalAvailableOrbits;
                    }
                }
            }

            DebugLogger.Log("");
            DebugLogger.LogFormat("System Total Available Orbits: {0:F2}", SystemTotalAvailableOrbits);
        }

        private void CalculateTotalAvailableOrbitsForStar(Star star, CelestrialObject starObj)
        {
            DebugLogger.Log("");
            DebugLogger.LogFormat("Calculating Total Available Orbits for {0} star...", star.starOrbitType);

            // Companion orbit stars don't have Total Available Orbits
            if (star.starOrbitType == Starhelper.starOrbitType.Companion)
            {
                star.TotalAvailableOrbits = 0;
                DebugLogger.Log("  Companion orbit star - no Total Available Orbits");
                return;
            }

            // Start with Max - Min
            float total = star.MaxAllowableOrbit - star.MinAllowableOrbit;
            DebugLogger.LogFormat("  Base calculation: {0:F3} - {1:F3} = {2:F3}",
                star.MaxAllowableOrbit, star.MinAllowableOrbit, total);

            // Subtract unavailable orbit ranges (only the portions that overlap with available range)
            float totalUnavailable = 0;
            foreach (var range in star.UnavailableOrbitRanges)
            {
                // Calculate overlap between unavailable range and [MinAllowableOrbit, MaxAllowableOrbit]
                float overlapMin = Math.Max(range.min, star.MinAllowableOrbit);
                float overlapMax = Math.Min(range.max, star.MaxAllowableOrbit);

                if (overlapMax > overlapMin)
                {
                    // There is an overlap
                    float overlapSize = overlapMax - overlapMin;
                    totalUnavailable += overlapSize;
                    DebugLogger.LogFormat("  Unavailable range {0:F2} to {1:F2} overlaps {2:F2} to {3:F2}: -{4:F2}",
                        range.min, range.max, overlapMin, overlapMax, overlapSize);
                }
                else
                {
                    // No overlap - range is outside Min/Max allowable orbits
                    DebugLogger.LogFormat("  Unavailable range {0:F2} to {1:F2} is outside allowable range ({2:F3} to {3:F3}): no effect",
                        range.min, range.max, star.MinAllowableOrbit, star.MaxAllowableOrbit);
                }
            }

            if (totalUnavailable > 0)
            {
                total -= totalUnavailable;
                DebugLogger.LogFormat("  After subtracting unavailable orbits: {0:F3} - {1:F2} = {2:F3}",
                    star.MaxAllowableOrbit - star.MinAllowableOrbit, totalUnavailable, total);
            }

            // Check if this is a multi-star system (has non-Companion companions)
            bool hasNonCompanionCompanions = false;
            foreach (var companion in primaryObject.celestrialObjectOrbits)
            {
                if (companion.celestrialObject is Star companionStar &&
                    companionStar.starOrbitType != Starhelper.starOrbitType.Companion)
                {
                    hasNonCompanionCompanions = true;
                    break;
                }
            }

            // If multi-star system and star doesn't have Companion companion and Total > 0, add 1
            bool hasCompanionCompanion = HasCompanionOrbitCompanion(starObj);
            if (hasNonCompanionCompanions && !hasCompanionCompanion && total > 0)
            {
                total += 1;
                DebugLogger.LogFormat("  Multi-star system, no Companion companion, and Total > 0: +1 = {0:F3}", total);
            }

            // Clamp to 0 if negative (star has no usable orbits)
            if (total < 0)
            {
                DebugLogger.LogFormat("  Total Available Orbits is negative ({0:F3}), clamping to 0", total);
                total = 0;
            }

            star.TotalAvailableOrbits = total;
            DebugLogger.LogFormat("  Final Total Available Orbits: {0:F3}", star.TotalAvailableOrbits);
        }

        private void CalculateSystemTotalWorlds()
        {
            SystemTotalWorlds = GasGiantCount + PlanetoidBeltCount + TerrestrialPlanetCount;

            DebugLogger.Log("");
            DebugLogger.LogSection("CALCULATING SYSTEM TOTAL WORLDS");
            DebugLogger.LogFormat("Gas Giants: {0}", GasGiantCount);
            DebugLogger.LogFormat("Planetoid Belts: {0}", PlanetoidBeltCount);
            DebugLogger.LogFormat("Terrestrial Planets: {0}", TerrestrialPlanetCount);
            DebugLogger.LogFormat("System Total Worlds: {0}", SystemTotalWorlds);
        }

        private void AssignWorldsToStars()
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("ASSIGNING WORLDS TO STARS");

            // Get all non-Companion companions
            List<(Star star, CelestrialObject obj)> nonCompanionStars = new List<(Star, CelestrialObject)>();

            // Add primary
            if (primaryObject.celestrialObject is Star primaryStar)
            {
                nonCompanionStars.Add((primaryStar, primaryObject));
            }

            // Add Close/Near/Far companions
            foreach (var companion in primaryObject.celestrialObjectOrbits)
            {
                if (companion.celestrialObject is Star companionStar &&
                    companionStar.starOrbitType != Starhelper.starOrbitType.Companion)
                {
                    nonCompanionStars.Add((companionStar, companion));
                }
            }

            // Filter to only include stars with TotalAvailableOrbits > 0
            List<(Star star, CelestrialObject obj)> starsWithOrbits = new List<(Star, CelestrialObject)>();
            List<(Star star, CelestrialObject obj)> starsWithoutOrbits = new List<(Star, CelestrialObject)>();

            foreach (var (star, obj) in nonCompanionStars)
            {
                if (star.TotalAvailableOrbits > 0)
                {
                    starsWithOrbits.Add((star, obj));
                }
                else
                {
                    starsWithoutOrbits.Add((star, obj));
                    star.WorldsAssigned = 0;
                    DebugLogger.LogFormat("{0} star has no available orbits - assigning 0 worlds", star.starOrbitType);
                }
            }

            // If no stars have available orbits, all get 0 worlds
            if (starsWithOrbits.Count == 0)
            {
                DebugLogger.Log("No stars have available orbits - no worlds can be assigned");
                return;
            }

            // Recalculate SystemTotalAvailableOrbits using only stars with orbits
            float effectiveSystemTotalAvailableOrbits = 0;
            foreach (var (star, obj) in starsWithOrbits)
            {
                effectiveSystemTotalAvailableOrbits += star.TotalAvailableOrbits;
            }
            DebugLogger.LogFormat("Effective System Total Available Orbits (excluding stars with 0): {0:F2}", effectiveSystemTotalAvailableOrbits);

            // Check if we only have one star with available orbits
            if (starsWithOrbits.Count == 1)
            {
                Star onlyStar = starsWithOrbits[0].star;
                onlyStar.WorldsAssigned = SystemTotalWorlds;
                DebugLogger.LogFormat("Only one star has available orbits - assigning all {0} worlds to {1} star",
                    SystemTotalWorlds, onlyStar.starOrbitType);
                return;
            }

            // Multiple stars with available orbits - need to distribute worlds
            DebugLogger.LogFormat("System has {0} stars with available orbits - distributing worlds...", starsWithOrbits.Count);

            // Find outermost star with available orbits
            // Order: Primary, Close, Near, Far
            Star? outermostStar = null;
            Starhelper.starOrbitType outermostType = Starhelper.starOrbitType.Primary;

            // Check in reverse order to find outermost
            foreach (var (star, obj) in starsWithOrbits)
            {
                if (star.starOrbitType == Starhelper.starOrbitType.Far)
                {
                    outermostStar = star;
                    outermostType = Starhelper.starOrbitType.Far;
                    break;
                }
            }
            if (outermostStar == null)
            {
                foreach (var (star, obj) in starsWithOrbits)
                {
                    if (star.starOrbitType == Starhelper.starOrbitType.Near)
                    {
                        outermostStar = star;
                        outermostType = Starhelper.starOrbitType.Near;
                        break;
                    }
                }
            }
            if (outermostStar == null)
            {
                foreach (var (star, obj) in starsWithOrbits)
                {
                    if (star.starOrbitType == Starhelper.starOrbitType.Close)
                    {
                        outermostStar = star;
                        outermostType = Starhelper.starOrbitType.Close;
                        break;
                    }
                }
            }
            if (outermostStar == null)
            {
                // Must be only primary (shouldn't happen because we checked count == 1 above)
                outermostStar = starsWithOrbits[0].star;
                outermostType = Starhelper.starOrbitType.Primary;
            }

            DebugLogger.LogFormat("Outermost star with available orbits: {0}", outermostType);

            // Assign worlds to each star with available orbits
            int totalAssigned = 0;

            // Process in order: Primary, Close, Near, Far
            Starhelper.starOrbitType[] order = new[]
            {
                Starhelper.starOrbitType.Primary,
                Starhelper.starOrbitType.Close,
                Starhelper.starOrbitType.Near,
                Starhelper.starOrbitType.Far
            };

            foreach (var orbitType in order)
            {
                Star? star = starsWithOrbits.FirstOrDefault(s => s.star.starOrbitType == orbitType).star;
                if (star == null) continue;

                if (star == outermostStar)
                {
                    // Outermost star gets remaining worlds
                    star.WorldsAssigned = SystemTotalWorlds - totalAssigned;
                    DebugLogger.LogFormat("{0} star (outermost): {1} worlds (remaining)", orbitType, star.WorldsAssigned);
                }
                else
                {
                    // Calculate proportional allocation using effective system total
                    float proportion = (SystemTotalWorlds * star.TotalAvailableOrbits) / effectiveSystemTotalAvailableOrbits;

                    if (orbitType == Starhelper.starOrbitType.Primary)
                    {
                        // Round up for primary
                        star.WorldsAssigned = (int)Math.Ceiling(proportion);
                        DebugLogger.LogFormat("{0} star: ({1} × {2:F3}) / {3:F2} = {4:F3}, rounded UP to {5} worlds",
                            orbitType, SystemTotalWorlds, star.TotalAvailableOrbits,
                            effectiveSystemTotalAvailableOrbits, proportion, star.WorldsAssigned);
                    }
                    else
                    {
                        // Round down for non-primary, non-outermost
                        star.WorldsAssigned = (int)Math.Floor(proportion);
                        DebugLogger.LogFormat("{0} star: ({1} × {2:F3}) / {3:F2} = {4:F3}, rounded DOWN to {5} worlds",
                            orbitType, SystemTotalWorlds, star.TotalAvailableOrbits,
                            effectiveSystemTotalAvailableOrbits, proportion, star.WorldsAssigned);
                    }

                    totalAssigned += star.WorldsAssigned;
                }
            }

            DebugLogger.Log("");
            DebugLogger.LogFormat("Total worlds assigned: {0} (should equal System Total Worlds: {1})",
                totalAssigned + outermostStar.WorldsAssigned, SystemTotalWorlds);
        }

        private void CalculateOrbitsAndWorlds()
        {
            // Calculate total available orbits for each star
            CalculateAllTotalAvailableOrbits();

            // Calculate system total worlds
            CalculateSystemTotalWorlds();

            // Assign worlds to stars
            AssignWorldsToStars();
        }

        private void CalculateAllSystemBaselineNumbers()
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("CALCULATING SYSTEM BASELINE NUMBERS");

            // Calculate for Primary star only (companions don't get baseline numbers)
            if (primaryObject.celestrialObject is Star primaryStar &&
                primaryStar.starOrbitType == Starhelper.starOrbitType.Primary)
            {
                CalculateSystemBaselineNumberForStar(primaryStar, primaryObject);
            }
        }

        private bool IsHZCOInUnavailableOrbits(Star star)
        {
            // Check if HZCO lies within any unavailable orbit range
            foreach (var range in star.UnavailableOrbitRanges)
            {
                if (star.HZCO >= range.min && star.HZCO <= range.max)
                {
                    return true;
                }
            }
            return false;
        }

        private void CalculateSystemBaselineNumberForStar(Star star, CelestrialObject starObj)
        {
            DebugLogger.Log("");
            DebugLogger.LogFormat("Calculating System Baseline Number for {0} star...", star.starOrbitType);

            // Check if star has any worlds assigned
            if (star.WorldsAssigned <= 0)
            {
                star.SystemBaselineNumber = 0;
                star.InnerZoneWorldCount = 0;
                star.OuterZoneWorldCount = 0;
                DebugLogger.Log("  Star has no worlds assigned - System Baseline Number = 0");
                return;
            }

            // Check if HZCO lies within unavailable orbits
            bool hzcoInUnavailable = IsHZCOInUnavailableOrbits(star);

            if (hzcoInUnavailable)
            {
                DebugLogger.LogFormat("  HZCO ({0:F3}) lies within unavailable orbits", star.HZCO);
                CalculateBaselineNumberScenarioA(star, starObj);
            }
            else
            {
                DebugLogger.LogFormat("  HZCO ({0:F3}) does NOT lie within unavailable orbits", star.HZCO);
                CalculateBaselineNumberScenarioB(star, starObj);
            }
        }

        private void CalculateBaselineNumberScenarioA(Star star, CelestrialObject starObj)
        {
            DebugLogger.Log("  Scenario A: HZCO in unavailable orbits");

            // Find the unavailable range that contains HZCO
            (float min, float max)? containingRange = null;
            foreach (var range in star.UnavailableOrbitRanges)
            {
                if (star.HZCO >= range.min && star.HZCO <= range.max)
                {
                    containingRange = range;
                    break;
                }
            }

            if (containingRange == null)
            {
                // Shouldn't happen, but handle gracefully
                star.SystemBaselineNumber = 0;
                star.InnerZoneWorldCount = 0;
                star.OuterZoneWorldCount = star.WorldsAssigned;
                DebugLogger.Log("  ERROR: Could not find unavailable range containing HZCO");
                return;
            }

            float unavailableStart = containingRange.Value.min;
            float unavailableEnd = containingRange.Value.max;

            DebugLogger.LogFormat("  HZCO is in unavailable range {0:F2} to {1:F2}", unavailableStart, unavailableEnd);

            // Check if there are available orbits between MinAllowableOrbit and the unavailable range
            if (star.MinAllowableOrbit >= unavailableStart)
            {
                // No available orbits closer to primary than HZCO
                star.SystemBaselineNumber = 0;
                star.InnerZoneWorldCount = 0;
                star.OuterZoneWorldCount = star.WorldsAssigned;
                DebugLogger.LogFormat("  MinAllowableOrbit ({0:F3}) >= unavailable range start ({1:F2})",
                    star.MinAllowableOrbit, unavailableStart);
                DebugLogger.Log("  No available orbits closer to primary than HZCO");
                DebugLogger.Log("  System Baseline Number = 0");
                DebugLogger.LogFormat("  All {0} worlds assigned to outer zone", star.WorldsAssigned);
                return;
            }

            // There ARE available orbits in the inner region (MinAllowableOrbit to unavailableStart)
            float innerRegionSize = unavailableStart - star.MinAllowableOrbit;
            DebugLogger.LogFormat("  Inner region available: {0:F3} to {1:F2} (size: {2:F2})",
                star.MinAllowableOrbit, unavailableStart, innerRegionSize);

            // Calculate maximum worlds that can fit in inner region
            int maxInnerWorlds = (int)Math.Floor(innerRegionSize / 0.01f);
            DebugLogger.LogFormat("  Maximum worlds that can fit in inner region: ({0:F2} / 0.01) = {1}",
                innerRegionSize, maxInnerWorlds);

            // Cap at System Total Worlds
            maxInnerWorlds = Math.Min(maxInnerWorlds, star.WorldsAssigned);
            DebugLogger.LogFormat("  Capped at worlds assigned to this star: {0}", maxInnerWorlds);

            // Randomly determine how many worlds to place in inner region (0 to maxInnerWorlds)
            int innerWorldCount = 0;
            if (maxInnerWorlds > 0)
            {
                innerWorldCount = Starhelper.diceRoll(maxInnerWorlds + 1, 1, dice) - 1; // Roll 1 to (max+1), subtract 1 to get 0 to max
                DebugLogger.LogFormat("  Random roll for inner worlds: {0} (range 0 to {1})", innerWorldCount, maxInnerWorlds);
            }
            else
            {
                DebugLogger.Log("  No room for worlds in inner region (maxInnerWorlds = 0)");
            }

            star.InnerZoneWorldCount = innerWorldCount;
            star.OuterZoneWorldCount = star.WorldsAssigned - innerWorldCount;
            star.SystemBaselineNumber = 1 + innerWorldCount;

            DebugLogger.LogFormat("  Inner zone worlds: {0}", star.InnerZoneWorldCount);
            DebugLogger.LogFormat("  Outer zone worlds: {0}", star.OuterZoneWorldCount);
            DebugLogger.LogFormat("  System Baseline Number = 1 + {0} = {1}", innerWorldCount, star.SystemBaselineNumber);
        }

        private void CalculateBaselineNumberScenarioB(Star star, CelestrialObject starObj)
        {
            DebugLogger.Log("  Scenario B: HZCO not in unavailable orbits");

            // All worlds are in a single zone (not split between inner/outer)
            star.InnerZoneWorldCount = 0;
            star.OuterZoneWorldCount = 0;

            // Roll 2D6
            int baseRoll = Starhelper.diceRoll(6, 2, dice);
            DebugLogger.LogDiceRoll(2, baseRoll, "System Baseline Number base roll");

            int modifiers = 0;

            // Primary has a Companion companion: +2
            if (HasCompanionOrbitCompanion(starObj))
            {
                modifiers += 2;
                DebugLogger.Log("  +2 (Primary has Companion companion)");
            }

            // Primary star class modifiers
            if (star.starclass == "Ia" || star.starclass == "Ib" || star.starclass == "II")
            {
                modifiers += 3;
                DebugLogger.LogFormat("  +3 (Primary is class {0})", star.starclass);
            }
            else if (star.starclass == "III")
            {
                modifiers += 2;
                DebugLogger.Log("  +2 (Primary is class III)");
            }
            else if (star.starclass == "IV")
            {
                modifiers += 1;
                DebugLogger.Log("  +1 (Primary is class IV)");
            }
            else if (star.starclass == "VI")
            {
                modifiers -= 1;
                DebugLogger.Log("  -1 (Primary is class VI)");
            }

            // Primary is white dwarf: -2
            if (star.type == "D")
            {
                modifiers -= 2;
                DebugLogger.Log("  -2 (Primary is white dwarf)");
            }

            // Worlds Assigned modifiers
            if (star.WorldsAssigned < 6)
            {
                modifiers -= 4;
                DebugLogger.LogFormat("  -4 (Worlds Assigned {0} < 6)", star.WorldsAssigned);
            }
            else if (star.WorldsAssigned >= 6 && star.WorldsAssigned <= 9)
            {
                modifiers -= 3;
                DebugLogger.LogFormat("  -3 (Worlds Assigned {0} is 6-9)", star.WorldsAssigned);
            }
            else if (star.WorldsAssigned >= 10 && star.WorldsAssigned <= 12)
            {
                modifiers -= 2;
                DebugLogger.LogFormat("  -2 (Worlds Assigned {0} is 10-12)", star.WorldsAssigned);
            }
            else if (star.WorldsAssigned >= 13 && star.WorldsAssigned <= 15)
            {
                modifiers -= 1;
                DebugLogger.LogFormat("  -1 (Worlds Assigned {0} is 13-15)", star.WorldsAssigned);
            }
            else if (star.WorldsAssigned >= 18 && star.WorldsAssigned <= 20)
            {
                modifiers += 1;
                DebugLogger.LogFormat("  +1 (Worlds Assigned {0} is 18-20)", star.WorldsAssigned);
            }
            else if (star.WorldsAssigned > 20)
            {
                modifiers += 2;
                DebugLogger.LogFormat("  +2 (Worlds Assigned {0} > 20)", star.WorldsAssigned);
            }

            // For each Close/Near/Far companion: -1
            int companionCount = 0;
            foreach (var companion in primaryObject.celestrialObjectOrbits)
            {
                if (companion.celestrialObject is Star companionStar &&
                    companionStar.starOrbitType != Starhelper.starOrbitType.Companion)
                {
                    companionCount++;
                }
            }

            if (companionCount > 0)
            {
                modifiers -= companionCount;
                DebugLogger.LogFormat("  -{0} ({1} Close/Near/Far companion(s))", companionCount, companionCount);
            }

            int finalRoll = baseRoll + modifiers;
            star.SystemBaselineNumber = finalRoll;

            if (modifiers != 0)
            {
                DebugLogger.LogFormat("  Final roll: {0} + ({1}) = {2}", baseRoll, modifiers, finalRoll);
            }
            else
            {
                DebugLogger.LogFormat("  Final roll: {0}", finalRoll);
            }

            DebugLogger.LogFormat("  System Baseline Number = {0}", star.SystemBaselineNumber);
        }

        private void CalculateAllBaselineOrbits()
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("CALCULATING BASELINE ORBITS");

            // Calculate for Primary star only
            if (primaryObject.celestrialObject is Star primaryStar &&
                primaryStar.starOrbitType == Starhelper.starOrbitType.Primary)
            {
                CalculateBaselineOrbitForStar(primaryStar, primaryObject);
            }
        }

        private void CalculateBaselineOrbitForStar(Star star, CelestrialObject starObj)
        {
            DebugLogger.Log("");
            DebugLogger.LogFormat("Calculating Baseline Orbit for {0} star...", star.starOrbitType);

            // Check if star has any worlds assigned
            if (star.WorldsAssigned <= 0)
            {
                star.BaselineOrbit = 0;
                star.InsideBaseline = 0;
                star.OutsideBaseline = 0;
                DebugLogger.Log("  Star has no worlds assigned - Baseline Orbit = 0");
                return;
            }

            // Log input values
            DebugLogger.LogFormat("  System Baseline Number: {0}", star.SystemBaselineNumber);
            DebugLogger.LogFormat("  System Total Worlds: {0}", SystemTotalWorlds);
            DebugLogger.LogFormat("  Worlds Assigned: {0}", star.WorldsAssigned);
            DebugLogger.LogFormat("  HZCO: {0:F3}", star.HZCO);
            DebugLogger.LogFormat("  Min Allowable Orbit: {0:F3}", star.MinAllowableOrbit);

            // Determine scenario
            if (star.SystemBaselineNumber < 1)
            {
                DebugLogger.Log("  Scenario 2: System Baseline Number < 1");
                CalculateBaselineOrbitScenario2(star, starObj);
            }
            else if (star.SystemBaselineNumber > SystemTotalWorlds || star.SystemBaselineNumber > star.WorldsAssigned)
            {
                DebugLogger.Log("  Scenario 3: System Baseline Number > System Total Worlds OR > Worlds Assigned");
                CalculateBaselineOrbitScenario3(star, starObj);
            }
            else // star.SystemBaselineNumber >= 1 && <= SystemTotalWorlds && <= WorldsAssigned
            {
                DebugLogger.Log("  Scenario 1: System Baseline Number >= 1 AND <= System Total Worlds AND <= Worlds Assigned");
                CalculateBaselineOrbitScenario1(star, starObj);
            }

            // Apply post-calculation adjustments
            ApplyBaselineOrbitAdjustments(star);
        }

        private void CalculateBaselineOrbitScenario1(Star star, CelestrialObject starObj)
        {
            int diceRoll = Starhelper.diceRoll(6, 2, dice);
            DebugLogger.LogDiceRoll(2, diceRoll, "Baseline orbit variance roll");

            float variance;
            if (star.HZCO >= 1)
            {
                variance = (diceRoll - 7) / 10f;
                DebugLogger.LogFormat("  HZCO >= 1: variance = ({0} - 7) / 10 = {1:F3}", diceRoll, variance);
            }
            else
            {
                variance = (diceRoll - 7) / 100f;
                DebugLogger.LogFormat("  HZCO < 1: variance = ({0} - 7) / 100 = {1:F4}", diceRoll, variance);
            }

            star.BaselineOrbit = star.HZCO + variance;
            DebugLogger.LogFormat("  Baseline Orbit = HZCO + variance = {0:F3} + {1:F4} = {2:F4}",
                star.HZCO, variance, star.BaselineOrbit);

            // Calculate InsideBaseline and OutsideBaseline
            star.OutsideBaseline = star.WorldsAssigned - star.SystemBaselineNumber;
            star.InsideBaseline = star.WorldsAssigned - (star.OutsideBaseline + 1);

            DebugLogger.LogFormat("  OutsideBaseline = Worlds Assigned - System Baseline # = {0} - {1} = {2}",
                star.WorldsAssigned, star.SystemBaselineNumber, star.OutsideBaseline);
            DebugLogger.LogFormat("  InsideBaseline = Worlds Assigned - (OutsideBaseline + 1) = {0} - ({1} + 1) = {2}",
                star.WorldsAssigned, star.OutsideBaseline, star.InsideBaseline);
        }

        private void CalculateBaselineOrbitScenario2(Star star, CelestrialObject starObj)
        {
            int diceRoll = Starhelper.diceRoll(6, 2, dice);
            DebugLogger.LogDiceRoll(2, diceRoll, "Baseline orbit variance roll");

            float variance;
            if (star.MinAllowableOrbit >= 1)
            {
                variance = (diceRoll - 2) / 10f;
                DebugLogger.LogFormat("  Min Allowable Orbit >= 1: variance = ({0} - 2) / 10 = {1:F3}",
                    diceRoll, variance);
                star.BaselineOrbit = star.HZCO - star.SystemBaselineNumber + star.WorldsAssigned + variance;
                DebugLogger.LogFormat("  Baseline Orbit = HZCO - System Baseline # + Worlds Assigned + variance");
                DebugLogger.LogFormat("  Baseline Orbit = {0:F3} - {1} + {2} + {3:F3} = {4:F4}",
                    star.HZCO, star.SystemBaselineNumber, star.WorldsAssigned, variance, star.BaselineOrbit);
            }
            else
            {
                variance = (diceRoll - 2) / 100f;
                DebugLogger.LogFormat("  Min Allowable Orbit < 1: variance = ({0} - 2) / 100 = {1:F4}",
                    diceRoll, variance);
                float systemBaselineComponent = star.SystemBaselineNumber / 10f;
                star.BaselineOrbit = star.HZCO - systemBaselineComponent + star.WorldsAssigned + variance;
                DebugLogger.LogFormat("  Baseline Orbit = HZCO - (System Baseline # / 10) + Worlds Assigned + variance");
                DebugLogger.LogFormat("  Baseline Orbit = {0:F3} - {1:F3} + {2} + {3:F4} = {4:F4}",
                    star.HZCO, systemBaselineComponent, star.WorldsAssigned, variance, star.BaselineOrbit);
            }

            star.OutsideBaseline = star.WorldsAssigned - 1;
            star.InsideBaseline = 0;

            DebugLogger.LogFormat("  OutsideBaseline = Worlds Assigned - 1 = {0} - 1 = {1}",
                star.WorldsAssigned, star.OutsideBaseline);
            DebugLogger.Log("  InsideBaseline = 0");
        }

        private void CalculateBaselineOrbitScenario3(Star star, CelestrialObject starObj)
        {
            int diceRoll = Starhelper.diceRoll(6, 2, dice);
            DebugLogger.LogDiceRoll(2, diceRoll, "Baseline orbit variance roll");

            float calculatedValue = star.HZCO - star.SystemBaselineNumber + star.WorldsAssigned;
            DebugLogger.LogFormat("  Calculated value = HZCO - System Baseline # + Worlds Assigned");
            DebugLogger.LogFormat("  Calculated value = {0:F3} - {1} + {2} = {3:F4}",
                star.HZCO, star.SystemBaselineNumber, star.WorldsAssigned, calculatedValue);

            float variance = (diceRoll - 7) / 5f;
            DebugLogger.LogFormat("  Variance = ({0} - 7) / 5 = {1:F3}", diceRoll, variance);

            if (calculatedValue >= 1)
            {
                star.BaselineOrbit = calculatedValue + variance;
                DebugLogger.LogFormat("  Calculated value >= 1: Baseline Orbit = {0:F4} + {1:F3} = {2:F4}",
                    calculatedValue, variance, star.BaselineOrbit);
            }
            else
            {
                star.BaselineOrbit = (star.SystemBaselineNumber + star.WorldsAssigned + variance) / 10f;
                DebugLogger.LogFormat("  Calculated value < 1: Baseline Orbit = (System Baseline # + Worlds Assigned + variance) / 10");
                DebugLogger.LogFormat("  Baseline Orbit = ({0} + {1} + {2:F3}) / 10 = {3:F4}",
                    star.SystemBaselineNumber, star.WorldsAssigned, variance, star.BaselineOrbit);
            }

            star.OutsideBaseline = 0;
            star.InsideBaseline = star.WorldsAssigned - 1;

            DebugLogger.Log("  OutsideBaseline = 0");
            DebugLogger.LogFormat("  InsideBaseline = Worlds Assigned - 1 = {0} - 1 = {1}",
                star.WorldsAssigned, star.InsideBaseline);
        }

        private void ApplyBaselineOrbitAdjustments(Star star)
        {
            DebugLogger.LogFormat("  Baseline Orbit before adjustments: {0:F4}", star.BaselineOrbit);

            if (star.BaselineOrbit < 0)
            {
                DebugLogger.Log("  Baseline Orbit < 0, applying adjustment...");

                float adjustedValue = star.HZCO - 0.1f;
                DebugLogger.LogFormat("  Adjusted value = HZCO - 0.1 = {0:F3} - 0.1 = {1:F4}",
                    star.HZCO, adjustedValue);

                if (adjustedValue < star.MinAllowableOrbit)
                {
                    star.BaselineOrbit = star.MinAllowableOrbit + (star.WorldsAssigned * 0.01f);
                    DebugLogger.LogFormat("  Adjusted value < Min Allowable Orbit ({0:F3})", star.MinAllowableOrbit);
                    DebugLogger.LogFormat("  Final Baseline Orbit = Min Allowable Orbit + (Worlds Assigned × 0.01)");
                    DebugLogger.LogFormat("  Final Baseline Orbit = {0:F3} + ({1} × 0.01) = {2:F4}",
                        star.MinAllowableOrbit, star.WorldsAssigned, star.BaselineOrbit);
                }
                else
                {
                    star.BaselineOrbit = adjustedValue;
                    DebugLogger.LogFormat("  Final Baseline Orbit = {0:F4}", star.BaselineOrbit);
                }
            }
            else
            {
                DebugLogger.Log("  No adjustment needed (Baseline Orbit >= 0)");
            }

            DebugLogger.LogFormat("  FINAL VALUES:");
            DebugLogger.LogFormat("    Baseline Orbit: {0:F4}", star.BaselineOrbit);
            DebugLogger.LogFormat("    InsideBaseline: {0}", star.InsideBaseline);
            DebugLogger.LogFormat("    OutsideBaseline: {0}", star.OutsideBaseline);
        }

        private void CalculateEmptyOrbits(Random dice)
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("CALCULATING EMPTY ORBITS");

            int diceRoll = Starhelper.diceRoll(6, 2, dice);
            DebugLogger.LogDiceRoll(2, diceRoll, "System Total Potential Empty Orbits");

            int systemPotentialEmpty = 0;
            if (diceRoll <= 9)
                systemPotentialEmpty = 0;
            else if (diceRoll == 10)
                systemPotentialEmpty = 1;
            else if (diceRoll == 11)
                systemPotentialEmpty = 2;
            else if (diceRoll >= 12)
                systemPotentialEmpty = 3;

            DebugLogger.LogFormat("System Total Potential Empty Orbits: {0}", systemPotentialEmpty);

            if (systemPotentialEmpty <= 0)
            {
                DebugLogger.Log("No empty orbits for this system");
                return;
            }

            // Distribute to Close/Near/Far companions first
            foreach (var companion in primaryObject.celestrialObjectOrbits)
            {
                if (systemPotentialEmpty <= 0) break;

                if (companion.celestrialObject is Star companionStar)
                {
                    if (companionStar.starOrbitType != Starhelper.starOrbitType.Companion &&
                        companionStar.WorldsAssigned > 0)
                    {
                        companionStar.EmptyOrbits = 1;
                        companionStar.WorldsAssigned += 1;
                        systemPotentialEmpty--;

                        DebugLogger.LogFormat("  {0} companion: Added 1 empty orbit (Worlds Assigned now {1})",
                            companionStar.starOrbitType, companionStar.WorldsAssigned);
                    }
                }
            }

            // Remaining goes to primary
            if (systemPotentialEmpty > 0 && primaryObject.celestrialObject is Star primaryStar)
            {
                primaryStar.EmptyOrbits = systemPotentialEmpty;
                primaryStar.WorldsAssigned += systemPotentialEmpty;

                DebugLogger.LogFormat("  Primary: Added {0} empty orbit(s) (Worlds Assigned now {1})",
                    systemPotentialEmpty, primaryStar.WorldsAssigned);
            }
        }

        private void CalculateSystemSpread()
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("CALCULATING SYSTEM SPREAD");

            if (primaryObject.celestrialObject is Star primaryStar)
            {
                if (primaryStar.WorldsAssigned <= 0)
                {
                    primaryStar.SystemSpread = 0;
                    DebugLogger.Log("Primary has no worlds - System Spread = 0");
                    return;
                }

                // Treat System Baseline Number < 1 as 1
                int effectiveBaselineNumber = primaryStar.SystemBaselineNumber < 1 ? 1 : primaryStar.SystemBaselineNumber;

                if (effectiveBaselineNumber != primaryStar.SystemBaselineNumber)
                {
                    DebugLogger.LogFormat("System Baseline # ({0}) < 1, treating as 1 for spread calculation",
                        primaryStar.SystemBaselineNumber);
                }

                float spread = (primaryStar.BaselineOrbit - primaryStar.MinAllowableOrbit) /
                               effectiveBaselineNumber;
                primaryStar.SystemSpread = spread;

                DebugLogger.LogFormat("System Spread = (Baseline Orbit - Min Allowable Orbit) / System Baseline #");
                DebugLogger.LogFormat("System Spread = ({0:F4} - {1:F3}) / {2} = {3:F4}",
                    primaryStar.BaselineOrbit, primaryStar.MinAllowableOrbit,
                    effectiveBaselineNumber, spread);
            }
        }

        private bool IsOrbitInUnavailableRange(float orbit, List<(float min, float max)> ranges, out float rangeWidth)
        {
            rangeWidth = 0;
            foreach (var range in ranges)
            {
                if (orbit >= range.min && orbit <= range.max)
                {
                    rangeWidth = range.max - range.min;
                    return true;
                }
            }
            return false;
        }

        private float GetSmallestOrbitSeparation(CelestrialObject starCobj, Star star)
        {
            const float DEFAULT_SEPARATION = 0.2f;

            // Get all occupied orbit numbers
            List<float> occupiedOrbits = new List<float>();
            foreach (var orbit in starCobj.celestrialObjectOrbits)
            {
                occupiedOrbits.Add(orbit.orbit);
            }

            // Need at least 2 orbits to calculate separation
            if (occupiedOrbits.Count < 2)
            {
                return DEFAULT_SEPARATION;
            }

            // Sort orbits
            occupiedOrbits.Sort();

            // Find minimum separation in AU
            float minSeparation = float.MaxValue;
            for (int i = 0; i < occupiedOrbits.Count - 1; i++)
            {
                float orbitAU1 = new CelestrialObject().OrbitAU(occupiedOrbits[i]);
                float orbitAU2 = new CelestrialObject().OrbitAU(occupiedOrbits[i + 1]);
                float separation = orbitAU2 - orbitAU1;

                if (separation < minSeparation)
                {
                    minSeparation = separation;
                }
            }

            return Math.Max(DEFAULT_SEPARATION, minSeparation);
        }

        private float SelectRandomAvailableOrbit(Star star, CelestrialObject starCobj, Random dice, int maxAttempts = 100)
        {
            float minSeparation = GetSmallestOrbitSeparation(starCobj, star);

            DebugLogger.LogFormat("  Attempting to find random orbit (min separation: {0:F2} AU, max attempts: {1})",
                minSeparation, maxAttempts);

            // Get all occupied orbits for checking
            List<float> occupiedOrbits = starCobj.celestrialObjectOrbits.Select(o => o.orbit).OrderBy(o => o).ToList();

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Generate random orbit between MinAllowableOrbit and MaxAllowableOrbit
                float minOrbit = star.MinAllowableOrbit;
                float maxOrbit = star.MaxAllowableOrbit;
                float candidateOrbit = minOrbit + (float)(dice.NextDouble() * (maxOrbit - minOrbit));

                // Round to 4 decimal places for consistency
                candidateOrbit = (float)Math.Round(candidateOrbit, 4);

                if (attempt < 5 || attempt % 10 == 0)
                {
                    DebugLogger.LogFormat("    Attempt {0}: candidate orbit {1:F4}", attempt + 1, candidateOrbit);
                }

                // Check if in unavailable range
                if (IsOrbitInUnavailableRange(candidateOrbit, star.UnavailableOrbitRanges, out float rangeWidth))
                {
                    if (attempt < 5 || attempt % 10 == 0)
                    {
                        DebugLogger.LogFormat("      Rejected: in unavailable range (width {0:F2})", rangeWidth);
                    }
                    continue;
                }

                // Calculate candidate orbit AU
                float candidateAU = new CelestrialObject().OrbitAU(candidateOrbit);

                // Check separation from all existing orbits
                bool separationValid = true;
                foreach (float existingOrbit in occupiedOrbits)
                {
                    float existingAU = new CelestrialObject().OrbitAU(existingOrbit);
                    float separation = Math.Abs(candidateAU - existingAU);

                    if (separation < minSeparation)
                    {
                        if (attempt < 5 || attempt % 10 == 0)
                        {
                            DebugLogger.LogFormat("      Rejected: too close to orbit {0:F4} (separation {1:F2} AU < {2:F2} AU)",
                                existingOrbit, separation, minSeparation);
                        }
                        separationValid = false;
                        break;
                    }
                }

                if (!separationValid)
                {
                    continue;
                }

                // Check if this is the furthest orbit - apply 25% rule
                if (occupiedOrbits.Count > 0)
                {
                    float maxOccupiedOrbit = occupiedOrbits.Max();

                    if (candidateOrbit > maxOccupiedOrbit)
                    {
                        float maxOccupiedAU = new CelestrialObject().OrbitAU(maxOccupiedOrbit);
                        float maxAllowedAU = maxOccupiedAU * 1.25f;

                        if (candidateAU > maxAllowedAU)
                        {
                            // Adjust candidate to 125% of furthest orbit
                            float adjustedOrbit = ConvertAUToOrbitNumber(maxAllowedAU);

                            // Check if adjusted orbit is in unavailable range
                            if (IsOrbitInUnavailableRange(adjustedOrbit, star.UnavailableOrbitRanges, out float adjustedRangeWidth))
                            {
                                // Move out 10%
                                adjustedOrbit = adjustedOrbit * 1.1f;
                                DebugLogger.LogFormat("      Candidate beyond 125% limit, adjusted to {0:F4} and moved out 10% to {1:F4}",
                                    ConvertAUToOrbitNumber(maxAllowedAU), adjustedOrbit);
                            }
                            else
                            {
                                DebugLogger.LogFormat("      Candidate beyond 125% limit ({0:F2} AU > {1:F2} AU), adjusted to {2:F4}",
                                    candidateAU, maxAllowedAU, adjustedOrbit);
                            }

                            candidateOrbit = adjustedOrbit;
                            candidateAU = new CelestrialObject().OrbitAU(candidateOrbit);
                        }
                    }
                }

                // All constraints satisfied
                DebugLogger.LogFormat("    ✓ Valid orbit found: {0:F4} ({1:F2} AU)", candidateOrbit, candidateAU);
                return candidateOrbit;
            }

            DebugLogger.Log("    ✗ No valid orbit found after maximum attempts");
            return -1;
        }

        private void ApplyOrbitReduction(List<float> orbits, int skipIndex, float percentage = 0.05f)
        {
            for (int i = 0; i < orbits.Count; i++)
            {
                if (i != skipIndex)
                {
                    orbits[i] = orbits[i] * (1 - percentage);
                }
            }
        }

        private void PlacePrimaryStarOrbits(Random dice)
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("PLACING PRIMARY STAR ORBITS");

            if (!(primaryObject.celestrialObject is Star primaryStar))
            {
                DebugLogger.Log("No primary star found");
                return;
            }

            if (primaryStar.WorldsAssigned <= 0)
            {
                DebugLogger.Log("Primary has no worlds assigned - skipping orbit placement");
                return;
            }

            List<float> orbits = new List<float>();
            float spread = primaryStar.SystemSpread;

            // Handle zero or negative spread
            if (spread <= 0)
            {
                DebugLogger.Log("System Spread is 0 or negative - using fallback value of 0.5");
                spread = 0.5f;
            }

            DebugLogger.LogFormat("Total objects to place: {0}", primaryStar.WorldsAssigned);
            DebugLogger.LogFormat("Inside Baseline: {0}", primaryStar.InsideBaseline);
            DebugLogger.LogFormat("Outside Baseline: {0}", primaryStar.OutsideBaseline);
            DebugLogger.LogFormat("System Spread: {0:F4}", spread);

            // Phase A: Inside Baseline objects
            if (primaryStar.InsideBaseline > 0)
            {
                DebugLogger.Log("");
                DebugLogger.Log("PHASE A: Placing Inside Baseline objects");

                for (int i = 0; i < primaryStar.InsideBaseline; i++)
                {
                    float orbit;
                    int varianceRoll = Starhelper.diceRoll(6, 2, dice) - 7;
                    float variance = (varianceRoll * spread) / 10f;

                    if (i == 0)
                    {
                        orbit = (primaryStar.MinAllowableOrbit + spread) + variance;
                        DebugLogger.LogDiceRoll(2, varianceRoll + 7, $"First inside baseline object variance");
                        DebugLogger.LogFormat("  Variance: ({0} * {1:F4}) / 10 = {2:F4}", varianceRoll, spread, variance);
                        DebugLogger.LogFormat("  Orbit = (Min Allowable + Spread) + variance");
                        DebugLogger.LogFormat("  Orbit = ({0:F3} + {1:F4}) + {2:F4} = {3:F4}",
                            primaryStar.MinAllowableOrbit, spread, variance, orbit);

                        // Check for negative orbit
                        if (orbit < primaryStar.MinAllowableOrbit)
                        {
                            orbit = primaryStar.MinAllowableOrbit;
                            DebugLogger.LogFormat("  Orbit adjusted to Min Allowable: {0:F3}", orbit);
                        }
                    }
                    else
                    {
                        float previousOrbit = orbits[orbits.Count - 1];
                        orbit = previousOrbit + spread + variance;
                        DebugLogger.LogDiceRoll(2, varianceRoll + 7, $"Inside baseline object {i + 1} variance");
                        DebugLogger.LogFormat("  Variance: ({0} * {1:F4}) / 10 = {2:F4}", varianceRoll, spread, variance);
                        DebugLogger.LogFormat("  Orbit = Previous + Spread + variance");
                        DebugLogger.LogFormat("  Orbit = {0:F4} + {1:F4} + {2:F4} = {3:F4}",
                            previousOrbit, spread, variance, orbit);
                    }

                    orbits.Add(orbit);
                    DebugLogger.LogFormat("  Placed inside baseline object {0} at orbit {1:F4}", i + 1, orbit);
                }

                // Check if last orbit + spread exceeds baseline
                DebugLogger.Log("");
                DebugLogger.Log("Checking inside baseline spacing...");
                float lastOrbitPlusSpread = orbits[orbits.Count - 1] + spread;
                DebugLogger.LogFormat("  Last orbit + Spread = {0:F4} + {1:F4} = {2:F4}",
                    orbits[orbits.Count - 1], spread, lastOrbitPlusSpread);
                DebugLogger.LogFormat("  Baseline Orbit = {0:F4}", primaryStar.BaselineOrbit);

                int iterations = 0;
                while (lastOrbitPlusSpread > primaryStar.BaselineOrbit && iterations < 100)
                {
                    DebugLogger.LogFormat("  Last orbit + Spread ({0:F4}) > Baseline ({1:F4}) - applying 5% reduction",
                        lastOrbitPlusSpread, primaryStar.BaselineOrbit);

                    ApplyOrbitReduction(orbits, 0, 0.05f);
                    lastOrbitPlusSpread = orbits[orbits.Count - 1] + spread;
                    iterations++;

                    DebugLogger.LogFormat("    After reduction: last orbit + Spread = {0:F4}", lastOrbitPlusSpread);
                }

                if (iterations >= 100)
                {
                    DebugLogger.Log("  WARNING: Maximum iterations reached for inside baseline adjustment");
                }
            }

            // Phase B: Baseline object (always exactly 1)
            DebugLogger.Log("");
            DebugLogger.Log("PHASE B: Placing Baseline object");
            orbits.Add(primaryStar.BaselineOrbit);
            DebugLogger.LogFormat("  Placed baseline object at orbit {0:F4}", primaryStar.BaselineOrbit);

            // Phase C: Outside Baseline objects
            if (primaryStar.OutsideBaseline > 0)
            {
                DebugLogger.Log("");
                DebugLogger.Log("PHASE C: Placing Outside Baseline objects");

                int baselineIndex = orbits.Count - 1;

                for (int i = 0; i < primaryStar.OutsideBaseline; i++)
                {
                    float orbit;
                    int varianceRoll = Starhelper.diceRoll(6, 2, dice) - 7;
                    float variance = (varianceRoll * spread) / 10f;

                    if (i == 0)
                    {
                        orbit = primaryStar.BaselineOrbit + spread + variance;
                        DebugLogger.LogDiceRoll(2, varianceRoll + 7, "First outside baseline object variance");
                        DebugLogger.LogFormat("  Variance: ({0} * {1:F4}) / 10 = {2:F4}", varianceRoll, spread, variance);
                        DebugLogger.LogFormat("  Orbit = Baseline + Spread + variance");
                        DebugLogger.LogFormat("  Orbit = {0:F4} + {1:F4} + {2:F4} = {3:F4}",
                            primaryStar.BaselineOrbit, spread, variance, orbit);
                    }
                    else
                    {
                        float previousOrbit = orbits[orbits.Count - 1];
                        orbit = previousOrbit + spread + variance;
                        DebugLogger.LogDiceRoll(2, varianceRoll + 7, $"Outside baseline object {i + 1} variance");
                        DebugLogger.LogFormat("  Variance: ({0} * {1:F4}) / 10 = {2:F4}", varianceRoll, spread, variance);
                        DebugLogger.LogFormat("  Orbit = Previous + Spread + variance");
                        DebugLogger.LogFormat("  Orbit = {0:F4} + {1:F4} + {2:F4} = {3:F4}",
                            previousOrbit, spread, variance, orbit);
                    }

                    // Check for unavailable orbit ranges
                    if (IsOrbitInUnavailableRange(orbit, primaryStar.UnavailableOrbitRanges, out float rangeWidth))
                    {
                        float adjustedOrbit = orbit + rangeWidth;
                        DebugLogger.LogFormat("  Orbit {0:F4} falls in unavailable range (width {1:F4})",
                            orbit, rangeWidth);
                        DebugLogger.LogFormat("  Adjusting to {0:F4}", adjustedOrbit);
                        orbit = adjustedOrbit;
                    }

                    orbits.Add(orbit);
                    DebugLogger.LogFormat("  Placed outside baseline object {0} at orbit {1:F4}", i + 1, orbit);
                }

                // Check if last orbit exceeds orbit 20
                DebugLogger.Log("");
                DebugLogger.Log("Checking outside baseline maximum orbit...");
                float lastOrbit = orbits[orbits.Count - 1];
                DebugLogger.LogFormat("  Last orbit = {0:F4}", lastOrbit);

                int iterations = 0;
                while (lastOrbit > 20 && iterations < 100)
                {
                    DebugLogger.LogFormat("  Last orbit ({0:F4}) > 20 - applying 5% reduction", lastOrbit);

                    // Reduce all except baseline
                    for (int i = baselineIndex + 1; i < orbits.Count; i++)
                    {
                        orbits[i] = orbits[i] * 0.95f;
                    }

                    // Re-check for unavailable ranges
                    for (int i = baselineIndex + 1; i < orbits.Count; i++)
                    {
                        if (IsOrbitInUnavailableRange(orbits[i], primaryStar.UnavailableOrbitRanges, out float rangeWidth))
                        {
                            orbits[i] = orbits[i] + rangeWidth;
                            DebugLogger.LogFormat("    Orbit {0} adjusted for unavailable range to {1:F4}",
                                i, orbits[i]);
                        }
                    }

                    lastOrbit = orbits[orbits.Count - 1];
                    iterations++;

                    DebugLogger.LogFormat("    After reduction: last orbit = {0:F4}", lastOrbit);
                }

                if (iterations >= 100)
                {
                    DebugLogger.Log("  WARNING: Maximum iterations reached for outside baseline adjustment");
                }
            }

            // Add all orbits to the primary star's celestrialObjectOrbits
            DebugLogger.Log("");
            DebugLogger.Log("Adding celestial bodies to primary star:");
            for (int i = 0; i < orbits.Count; i++)
            {
                primaryObject.AddCelestialBody(orbits[i], dice);
                float au = primaryObject.celestrialObjectOrbits[primaryObject.celestrialObjectOrbits.Count - 1].orbitAU;
                DebugLogger.LogFormat("  Object {0}: Orbit {1:F4} ({2:F2} AU)", i + 1, orbits[i], au);
            }

            DebugLogger.LogFormat("Total objects placed: {0}", orbits.Count);
        }

        private void PlaceCompanionStarOrbits(Random dice)
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("PLACING COMPANION STAR ORBITS");

            if (!(primaryObject.celestrialObject is Star primaryStar))
            {
                DebugLogger.Log("No primary star found");
                return;
            }

            float systemSpread = primaryStar.SystemSpread;

            // Handle zero or negative spread
            if (systemSpread <= 0)
            {
                DebugLogger.Log("System Spread is 0 or negative - using fallback value of 0.5");
                systemSpread = 0.5f;
            }

            foreach (var companionObj in primaryObject.celestrialObjectOrbits)
            {
                if (!(companionObj.celestrialObject is Star companionStar))
                    continue;

                // Only process Close/Near/Far companions
                if (companionStar.starOrbitType == Starhelper.starOrbitType.Companion)
                    continue;

                if (companionStar.WorldsAssigned <= 0)
                {
                    DebugLogger.LogFormat("{0} companion has no worlds assigned - skipping",
                        companionStar.starOrbitType);
                    continue;
                }

                DebugLogger.Log("");
                DebugLogger.LogFormat("Placing orbits for {0} companion:", companionStar.starOrbitType);
                DebugLogger.LogFormat("  Worlds Assigned: {0}", companionStar.WorldsAssigned);
                DebugLogger.LogFormat("  Using System Spread: {0:F4}", systemSpread);
                DebugLogger.LogFormat("  Min Allowable Orbit: {0:F3}", companionStar.MinAllowableOrbit);
                DebugLogger.LogFormat("  Max Allowable Orbit: {0:F3}", companionStar.MaxAllowableOrbit);

                List<float> orbits = new List<float>();

                for (int i = 0; i < companionStar.WorldsAssigned; i++)
                {
                    float orbit;
                    int varianceRoll = Starhelper.diceRoll(6, 2, dice) - 7;
                    float variance = (varianceRoll * systemSpread) / 10f;

                    if (i == 0)
                    {
                        orbit = (companionStar.MinAllowableOrbit + systemSpread) + variance;
                        DebugLogger.LogDiceRoll(2, varianceRoll + 7, "First object variance");
                        DebugLogger.LogFormat("    Variance: ({0} * {1:F4}) / 10 = {2:F4}",
                            varianceRoll, systemSpread, variance);
                        DebugLogger.LogFormat("    Orbit = (Min Allowable + System Spread) + variance");
                        DebugLogger.LogFormat("    Orbit = ({0:F3} + {1:F4}) + {2:F4} = {3:F4}",
                            companionStar.MinAllowableOrbit, systemSpread, variance, orbit);

                        // Check for negative orbit
                        if (orbit < companionStar.MinAllowableOrbit)
                        {
                            orbit = companionStar.MinAllowableOrbit;
                            DebugLogger.LogFormat("    Orbit adjusted to Min Allowable: {0:F3}", orbit);
                        }
                    }
                    else
                    {
                        float previousOrbit = orbits[orbits.Count - 1];
                        orbit = previousOrbit + systemSpread + variance;
                        DebugLogger.LogDiceRoll(2, varianceRoll + 7, $"Object {i + 1} variance");
                        DebugLogger.LogFormat("    Variance: ({0} * {1:F4}) / 10 = {2:F4}",
                            varianceRoll, systemSpread, variance);
                        DebugLogger.LogFormat("    Orbit = Previous + System Spread + variance");
                        DebugLogger.LogFormat("    Orbit = {0:F4} + {1:F4} + {2:F4} = {3:F4}",
                            previousOrbit, systemSpread, variance, orbit);
                    }

                    orbits.Add(orbit);
                    DebugLogger.LogFormat("    Placed object {0} at orbit {1:F4}", i + 1, orbit);
                }

                // Check if last orbit exceeds max allowable
                DebugLogger.Log("");
                DebugLogger.Log("  Checking maximum orbit constraint...");
                float lastOrbit = orbits[orbits.Count - 1];
                DebugLogger.LogFormat("    Last orbit = {0:F4}", lastOrbit);
                DebugLogger.LogFormat("    Max Allowable = {0:F3}", companionStar.MaxAllowableOrbit);

                int iterations = 0;
                while (lastOrbit > companionStar.MaxAllowableOrbit && iterations < 100)
                {
                    DebugLogger.LogFormat("    Last orbit ({0:F4}) > Max Allowable ({1:F3}) - applying 5% reduction",
                        lastOrbit, companionStar.MaxAllowableOrbit);

                    ApplyOrbitReduction(orbits, 0, 0.05f);
                    lastOrbit = orbits[orbits.Count - 1];
                    iterations++;

                    DebugLogger.LogFormat("      After reduction: last orbit = {0:F4}", lastOrbit);
                }

                if (iterations >= 100)
                {
                    DebugLogger.Log("    WARNING: Maximum iterations reached for companion orbit adjustment");
                }

                // Add all orbits to the companion star's celestrialObjectOrbits
                DebugLogger.Log("");
                DebugLogger.LogFormat("  Adding celestial bodies to {0} companion:", companionStar.starOrbitType);
                for (int i = 0; i < orbits.Count; i++)
                {
                    companionObj.AddCelestialBody(orbits[i], dice);
                    float au = companionObj.celestrialObjectOrbits[companionObj.celestrialObjectOrbits.Count - 1].orbitAU;
                    DebugLogger.LogFormat("    Object {0}: Orbit {1:F4} ({2:F2} AU)", i + 1, orbits[i], au);
                }

                DebugLogger.LogFormat("  Total objects placed: {0}", orbits.Count);
            }
        }

        private bool DetermineDPlanetarySystem(Random dice)
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("D PRIMARY - PLANETARY SYSTEM CHECK");

            int roll = Starhelper.diceRoll(6, 2, dice);
            DebugLogger.LogDiceRoll(2, roll, "Planetary system presence check");

            int modifiers = 0;

            // -2 if the system includes more than one white dwarf
            int dStarCount = CountDStarsInSystem();
            if (dStarCount > 1)
            {
                modifiers -= 2;
                DebugLogger.LogFormat("  {0} D stars in system (more than one): -2", dStarCount);
            }

            int finalRoll = roll + modifiers;
            if (modifiers != 0)
            {
                DebugLogger.LogFormat("  Modified roll: {0} + ({1}) = {2}", roll, modifiers, finalRoll);
            }

            if (finalRoll >= 8)
            {
                DebugLogger.Log("  D primary has a planetary system (rolled 8+)");
                return true;
            }
            else
            {
                DebugLogger.Log("  D primary does NOT have a planetary system (rolled less than 8)");
                DebugLogger.Log("  Gas Giants: 0, Planetoid Belts: 0, Terrestrial Planets: 0");
                return false;
            }
        }

        private int DetermineGasGiants(Random dice)
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("DETERMINING GAS GIANTS");

            bool isPrimaryBD = false;
            bool isPrimaryD = false;
            if (primaryObject.celestrialObject is Star primaryStar)
            {
                if (primaryStar.type == "BD") isPrimaryBD = true;
                if (primaryStar.type == "D") isPrimaryD = true;
            }

            // Check if gas giants are present
            int presenceRoll = Starhelper.diceRoll(6, 2, dice);
            DebugLogger.LogDiceRoll(2, presenceRoll, "Gas giant presence check");

            if (isPrimaryBD)
            {
                // BD primary: present on 2-7, not present on 8+
                if (presenceRoll >= 8)
                {
                    DebugLogger.Log("  No gas giants in system (BD primary, rolled 8+)");
                    return 0;
                }
                DebugLogger.Log("  Gas giants present (BD primary system) - determining count");
            }
            else if (isPrimaryD)
            {
                // D primary: present on 2-5, not present on 6+
                if (presenceRoll >= 6)
                {
                    DebugLogger.Log("  No gas giants in system (D primary, rolled 6+)");
                    return 0;
                }
                DebugLogger.Log("  Gas giants present (D primary system) - determining count");
            }
            else
            {
                // Normal: present on 2-9, not present on 10+
                if (presenceRoll >= 10)
                {
                    DebugLogger.Log("  No gas giants in system (rolled 10+)");
                    return 0;
                }
                DebugLogger.Log("  Gas giants present - determining count");
            }

            // Roll for count
            int countRoll;
            if (isPrimaryBD || isPrimaryD)
            {
                // BD/D primary: roll 2d6-2
                countRoll = Starhelper.diceRoll(6, 2, dice) - 2;
                DebugLogger.LogFormat("Base roll: 2d6 - 2 = {0}", countRoll);
            }
            else
            {
                // Normal: roll 2d6
                countRoll = Starhelper.diceRoll(6, 2, dice);
                DebugLogger.LogDiceRoll(2, countRoll, "Gas giant count (base)");
            }

            int modifiers = 0;

            if (isPrimaryBD || isPrimaryD)
            {
                // BD/D primary: only D star and 4+ star modifiers apply
                // Each D star: -1
                int dStarCount = CountDStarsInSystem();
                if (dStarCount > 0)
                {
                    modifiers -= dStarCount;
                    DebugLogger.LogFormat("  {0} D star(s) in system: -{1}", dStarCount, dStarCount);
                }

                // 4 or more stars: -1
                int totalStars = CountStarsInSystem();
                if (totalStars >= 4)
                {
                    modifiers -= 1;
                    DebugLogger.LogFormat("  {0} stars in system (4+): -1", totalStars);
                }
            }
            else
            {
                // Normal modifiers
                // Single Class V star: +1
                if (IsSingleClassVStar())
                {
                    modifiers += 1;
                    DebugLogger.Log("  System has single Class V star: +1");
                }

                // Primary is BD or D: -2
                if (IsPrimaryBDOrD())
                {
                    modifiers -= 2;
                    DebugLogger.Log("  Primary is BD or D: -2");
                }

                // Each D star: -1
                int dStarCount = CountDStarsInSystem();
                if (dStarCount > 0)
                {
                    modifiers -= dStarCount;
                    DebugLogger.LogFormat("  {0} D star(s) in system: -{1}", dStarCount, dStarCount);
                }

                // 4 or more stars: -1
                int totalStars = CountStarsInSystem();
                if (totalStars >= 4)
                {
                    modifiers -= 1;
                    DebugLogger.LogFormat("  {0} stars in system (4+): -1", totalStars);
                }
            }

            int finalRoll = countRoll + modifiers;
            if (modifiers != 0)
            {
                DebugLogger.LogFormat("  Modified roll: {0} + ({1}) = {2}", countRoll, modifiers, finalRoll);
            }

            // Determine count from table (ensure minimum of 0)
            int gasGiantCount;
            if (finalRoll <= 0) gasGiantCount = 0;
            else if (finalRoll <= 4) gasGiantCount = 1;
            else if (finalRoll <= 6) gasGiantCount = 2;
            else if (finalRoll <= 8) gasGiantCount = 3;
            else if (finalRoll <= 11) gasGiantCount = 4;
            else if (finalRoll == 12) gasGiantCount = 5;
            else gasGiantCount = 6;

            DebugLogger.LogFormat("  Result: {0} gas giant(s)", gasGiantCount);
            return gasGiantCount;
        }

        private int DeterminePlanetoidBelts(Random dice, int gasGiantCount)
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("DETERMINING PLANETOID BELTS");

            bool isPrimaryD = primaryObject.celestrialObject is Star pStar && pStar.type == "D";

            // Check if planetoid belts are present
            int presenceRoll = Starhelper.diceRoll(6, 2, dice);
            DebugLogger.LogDiceRoll(2, presenceRoll, "Planetoid belt presence check");

            if (isPrimaryD)
            {
                // D primary: present on 6+
                if (presenceRoll < 6)
                {
                    DebugLogger.Log("  No planetoid belts in system (D primary, need 6+)");
                    return 0;
                }
                DebugLogger.Log("  Planetoid belts present (D primary system) - determining count");
            }
            else
            {
                // Normal: present on 8+
                if (presenceRoll < 8)
                {
                    DebugLogger.Log("  No planetoid belts in system (need 8+)");
                    return 0;
                }
                DebugLogger.Log("  Planetoid belts present - determining count");
            }

            // Roll for count
            int countRoll;
            if (isPrimaryD)
            {
                // D primary: roll 2d6+1
                countRoll = Starhelper.diceRoll(6, 2, dice) + 1;
                DebugLogger.LogFormat("Base roll: 2d6 + 1 = {0}", countRoll);
            }
            else
            {
                countRoll = Starhelper.diceRoll(6, 2, dice);
                DebugLogger.LogDiceRoll(2, countRoll, "Planetoid belt count (base)");
            }

            int modifiers = 0;

            if (isPrimaryD)
            {
                // D primary modifiers
                // At least 1 gas giant: +1
                if (gasGiantCount >= 1)
                {
                    modifiers += 1;
                    DebugLogger.Log("  System has gas giant(s): +1");
                }

                // Each D star (including primary): +1
                int dStarCount = CountDStarsInSystem();
                if (dStarCount > 0)
                {
                    modifiers += dStarCount;
                    DebugLogger.LogFormat("  {0} D star(s) in system (including primary): +{1}", dStarCount, dStarCount);
                }

                // 2 or more stars: +1
                int totalStars = CountStarsInSystem();
                if (totalStars >= 2)
                {
                    modifiers += 1;
                    DebugLogger.LogFormat("  {0} stars in system (2+): +1", totalStars);
                }
            }
            else
            {
                // Normal modifiers
                // At least 1 gas giant: +1
                if (gasGiantCount >= 1)
                {
                    modifiers += 1;
                    DebugLogger.Log("  System has gas giant(s): +1");
                }

                // Primary is D: +1
                if (primaryObject.celestrialObject is Star primaryStar && primaryStar.type == "D")
                {
                    modifiers += 1;
                    DebugLogger.Log("  Primary is D: +1");
                }

                // Each D star: +1
                int dStarCount = CountDStarsInSystem();
                if (dStarCount > 0)
                {
                    modifiers += dStarCount;
                    DebugLogger.LogFormat("  {0} D star(s) in system: +{1}", dStarCount, dStarCount);
                }

                // 2 or more stars: +1
                int totalStars = CountStarsInSystem();
                if (totalStars >= 2)
                {
                    modifiers += 1;
                    DebugLogger.LogFormat("  {0} stars in system (2+): +1", totalStars);
                }
            }

            int finalRoll = countRoll + modifiers;
            if (modifiers != 0)
            {
                DebugLogger.LogFormat("  Modified roll: {0} + {1} = {2}", countRoll, modifiers, finalRoll);
            }

            // Determine count from table
            int beltCount;
            if (finalRoll <= 6) beltCount = 1;
            else if (finalRoll <= 11) beltCount = 2;
            else beltCount = 3;

            DebugLogger.LogFormat("  Result: {0} planetoid belt(s)", beltCount);
            return beltCount;
        }

        private int DetermineTerrestrialPlanets(Random dice)
        {
            DebugLogger.Log("");
            DebugLogger.LogSection("DETERMINING TERRESTRIAL PLANETS");

            bool isPrimaryD = primaryObject.celestrialObject is Star pStar && pStar.type == "D";

            int baseRoll;
            if (isPrimaryD)
            {
                // D primary: roll d6 - 2
                baseRoll = Starhelper.diceRoll(6, 1, dice) - 2;
                DebugLogger.LogFormat("Base roll: d6 - 2 = {0} (D primary)", baseRoll);
            }
            else
            {
                // Normal: roll 2d6 - 2
                baseRoll = Starhelper.diceRoll(6, 2, dice) - 2;
                DebugLogger.LogFormat("Base roll: 2d6 - 2 = {0}", baseRoll);
            }

            int modifiers = 0;

            if (isPrimaryD)
            {
                // D primary: -1 for each additional D star (not counting the primary)
                int dStarCount = CountDStarsInSystem();
                int additionalDStars = dStarCount - 1; // Exclude the primary
                if (additionalDStars > 0)
                {
                    modifiers -= additionalDStars;
                    DebugLogger.LogFormat("  {0} additional D star(s) in system: -{1}", additionalDStars, additionalDStars);
                }
            }
            else
            {
                // Normal: each D star -1
                int dStarCount = CountDStarsInSystem();
                if (dStarCount > 0)
                {
                    modifiers -= dStarCount;
                    DebugLogger.LogFormat("  {0} D star(s) in system: -{1}", dStarCount, dStarCount);
                }
            }

            int originalTotal = baseRoll + modifiers;
            if (modifiers != 0)
            {
                DebugLogger.LogFormat("  Modified roll: {0} + ({1}) = {2}", baseRoll, modifiers, originalTotal);
            }
            else
            {
                DebugLogger.LogFormat("  Total: {0}", originalTotal);
            }

            int finalCount;

            // Adjust based on original total
            if (originalTotal < 3)
            {
                DebugLogger.Log("  Total < 3, regenerating with 1d3 + 2");
                int regenRoll = Starhelper.diceRoll(3, 1, dice);
                finalCount = regenRoll + 2;
                DebugLogger.LogFormat("  Regenerated: 1d3 + 2 = {0} + 2 = {1}", regenRoll, finalCount);
            }
            else
            {
                DebugLogger.Log("  Total >= 3, adding 1d3 - 1");
                int additionalRoll = Starhelper.diceRoll(3, 1, dice) - 1;
                finalCount = originalTotal + additionalRoll;
                DebugLogger.LogFormat("  Final: {0} + (1d3 - 1) = {0} + {1} = {2}", originalTotal, additionalRoll, finalCount);
            }

            DebugLogger.LogFormat("  Result: {0} terrestrial planet(s)", finalCount);
            return finalCount;
        }

        public Star? primary { get; set; }
        public CelestrialObject primaryObject { get; set; } = null!;

        public int GasGiantCount { get; private set; }
        public int PlanetoidBeltCount { get; private set; }
        public int TerrestrialPlanetCount { get; private set; }

        public float SystemTotalAvailableOrbits { get; private set; }
        public int SystemTotalWorlds { get; private set; }

        public static Dictionary<int, float> orbitValues = new Dictionary<int, float>();
    }
}
