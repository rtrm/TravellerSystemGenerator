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
    internal class StarSystem
    {
        
        internal StarSystem()
        {
            DebugLogger.LogSection("STAR SYSTEM GENERATION");
            Random dice = new Random();
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

            Star? star = primaryObject.celestrialObject as Star;

            // Print console output header
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("              TRAVELLER STAR SYSTEM GENERATION                 ");
            Console.WriteLine($"                        Version {Version.VersionString}                        ");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine();

            DebugLogger.LogSection("SYSTEM SUMMARY");
            if (star != null)
                PrintStar(star, 0, primaryObject, dice);

            if (primaryObject.celestrialObjectOrbits.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"System contains {primaryObject.celestrialObjectOrbits.Count} companion star(s)");
                Console.WriteLine();
                DebugLogger.Log($"Total companion stars found: {primaryObject.celestrialObjectOrbits.Count}");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Single star system (no companions)");
                Console.WriteLine();
                DebugLogger.Log("No companion stars in this system");
            }

            foreach (CelestrialObject Cobj in primaryObject.celestrialObjectOrbits)
            {
                if (Cobj.celestrialObject is Star)
                {
                    Star starObj = (Star)Cobj.celestrialObject;
                    PrintStar(starObj, Cobj.orbit, Cobj, dice);

                    // Check if this companion has its own Companion orbit companions
                    if (Cobj.celestrialObjectOrbits.Count > 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"  {starObj.starOrbitType} star has {Cobj.celestrialObjectOrbits.Count} Companion orbit companion(s)");
                        Console.WriteLine();
                        DebugLogger.Log($"  {starObj.starOrbitType} companion has {Cobj.celestrialObjectOrbits.Count} sub-companion(s)");

                        foreach (CelestrialObject subCobj in Cobj.celestrialObjectOrbits)
                        {
                            if (subCobj.celestrialObject is Star)
                            {
                                Star subStarObj = (Star)subCobj.celestrialObject;
                                Console.WriteLine($"    Orbiting the {starObj.starOrbitType} companion:");
                                DebugLogger.LogFormat("    Sub-companion of {0} companion:", starObj.starOrbitType);
                                PrintStar(subStarObj, subCobj.orbit, subCobj, dice);
                            }
                        }
                    }
                }
            }

            // Print non-stellar objects summary
            Console.WriteLine();
            Console.WriteLine("─────────────────────────────────────────────────────────────");
            Console.WriteLine("NON-STELLAR OBJECTS");
            Console.WriteLine("─────────────────────────────────────────────────────────────");
            Console.WriteLine($"Gas Giants:          {GasGiantCount}");
            Console.WriteLine($"Planetoid Belts:     {PlanetoidBeltCount}");
            Console.WriteLine($"Terrestrial Planets: {TerrestrialPlanetCount}");
            Console.WriteLine();
            Console.WriteLine($"System Total Worlds: {SystemTotalWorlds}");
            Console.WriteLine($"System Total Available Orbits: {SystemTotalAvailableOrbits:F2}");

            DebugLogger.Log("");
            DebugLogger.Log("NON-STELLAR OBJECTS SUMMARY:");
            DebugLogger.LogFormat("  Gas Giants: {0}", GasGiantCount);
            DebugLogger.LogFormat("  Planetoid Belts: {0}", PlanetoidBeltCount);
            DebugLogger.LogFormat("  Terrestrial Planets: {0}", TerrestrialPlanetCount);
            DebugLogger.LogFormat("  System Total Worlds: {0}", SystemTotalWorlds);
            DebugLogger.LogFormat("  System Total Available Orbits: {0:F2}", SystemTotalAvailableOrbits);

            // Print console output footer
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");


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

        

        private void PrintStar (Star star, float orbit, CelestrialObject Cobj, Random dice)
        {
            if (star.starOrbitType == Starhelper.starOrbitType.Primary)
            {
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine("PRIMARY STAR");
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                DebugLogger.Log("");
                DebugLogger.LogFormat("{0} STAR:", star.starOrbitType.ToString().ToUpper());
            }
            else
            {
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine($"{star.starOrbitType.ToString().ToUpper()} COMPANION STAR");
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine($"Orbital Position:    {orbit:F2} ({Cobj.orbitAU:F2} AU)");
                Console.WriteLine($"Eccentricity:        {Cobj.orbitEccentricity:F3}");

                DebugLogger.Log("");
                DebugLogger.LogFormat("{0} STAR:", star.starOrbitType.ToString().ToUpper());
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
            }

            // Habitable zone (except for Companion orbit stars)
            if (star.starOrbitType != Starhelper.starOrbitType.Companion)
            {
                float hzMin = Math.Max(0, star.HZCO - 1);  // Clamp to 0
                float hzMax = star.HZCO + 1;
                Console.WriteLine($"Habitable Zone Center: {star.HZCO:F3}");
                Console.WriteLine($"Habitable Zone:      {hzMin:F3} to {hzMax:F3}");
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

                // orbital period = sqrt(orbit^2 / (M + m))
                float orbitAU = cObj.orbitAU;
                float period = (float)Math.Sqrt((orbitAU * orbitAU) / (M + m));

                cObj.OrbitalPeriodYears = period;
                DebugLogger.LogFormat("    Orbital period: {0:F6} years", period);
            }
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
            // Convert to days
            float days = years * 365.25f;

            // If less than 1 day, show in hours
            if (days < 1.0f)
            {
                float hours = days * 24;
                return $"{hours:F2} hours";
            }
            // If less than 5 years, show in days
            else if (years < 5.0f)
            {
                return $"{days:F2} days";
            }
            // Otherwise show in years
            else
            {
                return $"{years:F2} years";
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
            Random dice = new Random();
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
            Random dice = new Random();
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
