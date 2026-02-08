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

            DebugLogger.Log("");
            DebugLogger.Log("NON-STELLAR OBJECTS SUMMARY:");
            DebugLogger.LogFormat("  Gas Giants: {0}", GasGiantCount);
            DebugLogger.LogFormat("  Planetoid Belts: {0}", PlanetoidBeltCount);
            DebugLogger.LogFormat("  Terrestrial Planets: {0}", TerrestrialPlanetCount);

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
            int roll = Starhelper.diceRoll(10, 1, dice);
            roll++; //so roll is 1-10
            if (orbitType == Starhelper.starOrbitType.Close && orbitNum <= 0)
                orbitNum = 0.5F;

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

        public static Dictionary<int, float> orbitValues = new Dictionary<int, float>();
    }
}
