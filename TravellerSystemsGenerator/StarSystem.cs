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
                }
            }

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

            DebugLogger.LogFormat("  Mass: {0:F2} solar masses", star.mass);
            DebugLogger.LogFormat("  Temperature: {0} K", star.temperture);
            DebugLogger.LogFormat("  Diameter: {0:F4} solar diameters", star.diameter);
            DebugLogger.LogFormat("  Luminosity: {0:F6}", star.luminosity);
            DebugLogger.LogFormat("  Age: {0:F2} billion years", star.age);
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

        private void GenerateAdditionalStars(CelestrialObject cObj, Random dice)
        {
            
            int closeStarPresent = CheckCompanionTypePresent(Starhelper.starOrbitType.Close, dice);
            int nearStarPresent = CheckCompanionTypePresent(Starhelper.starOrbitType.Near, dice);
            int farStarPresent = CheckCompanionTypePresent(Starhelper.starOrbitType.Far, dice);
            int companionStarPresent = CheckCompanionTypePresent(Starhelper.starOrbitType.Companion, dice);


            if (closeStarPresent >= 10)
                {
                Console.WriteLine("Close Star present");
                DebugLogger.Log("CLOSE STAR DETECTED - Generating orbital position");
                int baseOrb = Starhelper.diceRoll(6, 1, dice) - 1;
                DebugLogger.LogFormat("  Base orbit: {0}", baseOrb);
                float fractionalOrbit = FractionalOrbit(baseOrb, dice, Starhelper.starOrbitType.Close);
                DebugLogger.LogFormat("  Fractional orbit: {0:F2}", fractionalOrbit);
                cObj.AddStar(fractionalOrbit, Starhelper.starOrbitType.Close, dice);
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

        }

        private float FractionalOrbit (float orbitNum, Random dice, Starhelper.starOrbitType orbitType)
        {
            float fractionalOrbit = 0;
            int roll = Starhelper.diceRoll(10, 1, dice);
            roll++; //so roll is 1-10
            if (orbitType == Starhelper.starOrbitType.Close && orbitNum < 0)
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

        public Star? primary { get; set; }
        public CelestrialObject primaryObject { get; set; } = null!;

        public static Dictionary<int, float> orbitValues = new Dictionary<int, float>();
    }
}
