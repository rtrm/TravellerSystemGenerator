using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
//using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace TravellerSystemGenerator
{
    internal class Star
    {
        public bool primary { get; set; }
        public string type { get; set; } = "";
        public string starclass { get; set; } = "";
        public string subType { get; set; } = "";
        public string colour { get; set; } = "";
        public float mass { get; set; }
        public float temperture { get; set; }
        public float diameter { get; set; }
        public float luminosity { get; set; }
        public float age { get; set; }
        public float MinAllowableOrbit { get; set; }
        public float MaxAllowableOrbit { get; set; }
        public List<(float min, float max)> UnavailableOrbitRanges { get; set; } = new List<(float, float)>();
        public float HZCO { get; set; }  // Habitable Zone Center Orbit
        public Starhelper.starOrbitType starOrbitType { get; set; }
        public List<CelestrialObject> orbits { get; set; } = new List<CelestrialObject>();

        //public float eccentricity { get; set; }

        private static float solTemperture = 5772;

        private float[,] starMass =
        {
            { 200, 80, 60, 30, 20, 15, 13, 12, 12, 13, 14, 18, 20, 25, 30 },
            { 150, 60, 40, 25, 15, 13, 12, 10, 10, 11, 12, 13, 15, 20, 25 },
            { 130, 40, 30, 20, 14, 11, 10, 8, 8, 10, 10, 12, 14, 16, 18 },
            { 110, 30, 20, 10, 8, 6, 4, 3, 2.5F, 2.4F, 1.1F, 1.5F, 1.8F, 2.4F, 8 },
            { 0, 0, 20, 20, 4, 2.3F, 2, 1.5F, 1.7F, 1.2F, 1.5F, 0, 0, 0, 0 },
            { 90, 60, 18, 5, 2.2F, 1.8F,1.5F, 1.3F, 1.1F, 0.9F, 0.8F, 0.7F, 0.5F, 0.16F, 0.08F },
            { 2, 1.5F, 0.5F, 0.4F, 0, 0, 0, 0, 0.8F, 0.7F, 0.6F, 0.5F, 0.4F, 0.12F, 0.075F }
        };

        private float[] starTemperture =
            { 50000, 40000, 30000, 15000, 10000, 8000, 7500, 6500, 6000, 5600, 5200, 4400, 3700, 3000, 2400};

        private float[,] starDiameter =
            {
                { 25,22,20,60, 120, 180, 210, 280, 330, 360, 420, 600, 900, 1200, 1800 },
                { 24, 20, 14, 25, 50, 75, 85, 115, 135, 150, 180, 260, 380, 600, 800 },
                { 22, 18, 12, 14, 30, 45, 50, 66, 77, 90, 110, 160, 230, 350, 500 },
                { 21, 15, 10, 6, 5, 5, 5, 5, 10, 15, 20, 40, 60, 100, 200 },
                { 0, 0, 8, 5, 4, 3, 3, 2, 3, 4, 6, 0, 0, 0, 0 },
                { 20, 12, 7, 3.5F, 2.2F, 2, 1.7F, 1.5F, 1.1F, 0.95F, 0.9F, 0.8F, 0.7F, 0.2F, 0.1F },
                { 0.18F, 0.18F, 0.2F, 0.5F, 0, 0, 0, 0, 0.8F, 0.7F, 0.6F, 0.5F, 0.4F, 0.1F, 0.08F }
            };

        private float[,] starMinOrbit =
            {
                // Ia:   O0,    O5,    B0,    B5,    A0,    A5,    F0,    F5,    G0,    G5,    K0,    K5,    M0,    M5,    M9
                { 0.63F, 0.55F, 0.5F, 1.67F, 3.34F, 4.17F, 4.42F, 5F, 5.21F, 5.34F, 5.59F, 6.17F, 6.8F, 7.2F, 7.8F },
                // Ib:
                { 0.6F, 0.5F, 0.35F, 0.63F, 1.4F, 2.17F, 2.5F, 3.25F, 3.59F, 3.84F, 4.17F, 4.84F, 5.42F, 6.17F, 6.59F },
                // II:
                { 0.55F, 0.45F, 0.3F, 0.35F, 0.75F, 1.17F, 1.33F, 1.87F, 2.24F, 2.67F, 3.17F, 4F, 4.59F, 5.3F, 5.92F },
                // III:
                { 0.53F, 0.38F, 0.25F, 0.15F, 0.13F, 0.13F, 0.13F, 0.13F, 0.25F, 0.38F, 0.5F, 1F, 1.68F, 3F, 4.34F },
                // IV:
                { 0, 0, 0.2F, 0.13F, 0.1F, 0.07F, 0.07F, 0.06F, 0.07F, 0.1F, 0.15F, 0, 0, 0, 0 },
                // V:
                { 0.5F, 0.3F, 0.18F, 0.09F, 0.06F, 0.05F, 0.04F, 0.03F, 0.03F, 0.02F, 0.02F, 0.02F, 0.02F, 0.01F, 0.01F },
                // VI:
                { 0.01F, 0.01F, 0.01F, 0.01F, 0, 0, 0, 0, 0.02F, 0.02F, 0.02F, 0.01F, 0.01F, 0.01F, 0.01F }
            };
        public Star(Random dice)
        {
            CreateStar(0, Starhelper.starOrbitType.Primary, dice);
        }
        public Star(float orbit, Starhelper.starOrbitType starOrbitType, Random dice)
        {
            CreateStar(orbit, starOrbitType, dice);
        }

        public Star(float orbit, Starhelper.starOrbitType starOrbitType, Star primaryStar, Random dice)
        {
            CreateCompanionStar(orbit, starOrbitType, primaryStar, dice);
        }


        public void CreateCompanionStar(float orbit, Starhelper.starOrbitType orbitType, Star primaryStar, Random dice)
        {
            DebugLogger.LogFormat("Creating {0} companion star based on primary {1}{2} {3}", orbitType, primaryStar.type, primaryStar.subType, primaryStar.starclass);
            starOrbitType = orbitType;

            // Check if primary is Class III or IV for dice modifier
            int diceModifier = 0;
            if (primaryStar.starclass == "III" || primaryStar.starclass == "IV")
            {
                diceModifier = 1;
                DebugLogger.Log("  Primary is Class III or IV - adding +1 to companion type roll");
            }

            // Special case: D and BD primaries always have same-type companions
            if (primaryStar.type == "D" || primaryStar.type == "BD")
            {
                type = primaryStar.type;
                subType = "";  // D and BD have no subtype
                starclass = "";  // D and BD have no starclass
                DebugLogger.LogFormat("  Primary is {0} - companion will also be {0}", primaryStar.type);

                // Set fixed properties for D and BD stars
                SetDwarfProperties(type);

                // Generate age
                while (age < 0.1F)
                {
                    if (mass <= 0.9)
                    {
                        age = (float)Math.Round(((((float)Starhelper.diceRoll(6, 1, dice)) * 2) + (float)Starhelper.diceRoll(3, 1, dice) - 1) + ((float)Starhelper.diceRoll(3, 1, dice) / 10), 2);
                        if (age > 12.0F)
                            age = 0;
                    }
                    else
                    {
                        age = (float)Math.Round(((float)Starhelper.diceRoll(6, 1, dice) - 1 + ((float)Starhelper.diceRoll(6, 1, dice) / 6)) / 6, 2);
                    }
                }
                DebugLogger.LogFormat("  Star age: {0:F2} billion years", age);
                return;
            }

            // Roll for companion type determination
            int companionTypeRoll = Starhelper.diceRoll(6, 2, dice) + diceModifier;
            DebugLogger.LogDiceRoll(2, companionTypeRoll - diceModifier, "Companion type determination");
            if (diceModifier > 0)
            {
                DebugLogger.LogFormat("  Roll after modifier: {0}", companionTypeRoll);
            }

            // Different thresholds for Close/Near/Far vs Companion
            bool isCompanionOrbit = (orbitType == Starhelper.starOrbitType.Companion);

            if (companionTypeRoll < 4)
            {
                // Other - D or BD
                DebugLogger.Log("  Companion category: Other");
                int otherRoll = Starhelper.diceRoll(6, 2, dice);
                DebugLogger.LogDiceRoll(2, otherRoll, "Other type determination");

                if (otherRoll < 8)
                {
                    type = "D";
                    DebugLogger.Log("    Result: White Dwarf (D)");
                }
                else
                {
                    type = "BD";
                    DebugLogger.Log("    Result: Brown Dwarf (BD)");
                }
                subType = "";  // D and BD have no subtype
                starclass = "";  // D and BD have no starclass
            }
            else if ((!isCompanionOrbit && companionTypeRoll >= 4 && companionTypeRoll <= 6) ||
                     (isCompanionOrbit && companionTypeRoll >= 4 && companionTypeRoll <= 5))
            {
                // Random - generate like primary but ensure not hotter
                DebugLogger.Log("  Companion category: Random");
                GenerateRandomCompanion(primaryStar, dice);
            }
            else if ((!isCompanionOrbit && companionTypeRoll >= 7 && companionTypeRoll <= 8) ||
                     (isCompanionOrbit && companionTypeRoll >= 6 && companionTypeRoll <= 7))
            {
                // Lesser - one type cooler
                DebugLogger.Log("  Companion category: Lesser");
                GenerateLesserCompanion(primaryStar, dice);
            }
            else if ((!isCompanionOrbit && companionTypeRoll >= 9 && companionTypeRoll <= 10) ||
                     (isCompanionOrbit && companionTypeRoll >= 8 && companionTypeRoll <= 9))
            {
                // Sibling - same type, higher subtype
                DebugLogger.Log("  Companion category: Sibling");
                GenerateSiblingCompanion(primaryStar, dice);
            }
            else // >= 11 for Close/Near/Far, >= 10 for Companion
            {
                // Twin - identical but slightly smaller
                DebugLogger.Log("  Companion category: Twin");
                GenerateTwinCompanion(primaryStar, dice);
                return; // Twin handles all properties internally
            }

            // Generate remaining properties for non-Twin companions
            if (type != "D" && type != "BD")
            {
                DebugLogger.LogFormat("  Final star type: {0}{1} {2}", type, subType, starclass);
                GetMassTempertureDiameter(type, subType, dice);
                DebugLogger.LogFormat("  Base mass: {0:F3} solar masses", mass);
                DebugLogger.LogFormat("  Base temperature: {0} K", temperture);
                DebugLogger.LogFormat("  Base diameter: {0:F4} solar diameters", diameter);

                StarVariance(dice);
                DebugLogger.LogFormat("  Mass after variance: {0:F3} solar masses", mass);

                GetMinAllowableOrbit(type, subType);
            }
            else
            {
                DebugLogger.LogFormat("  Final star type: {0} ({1})", type, type == "BD" ? "Brown Dwarf" : "White Dwarf");

                // Set fixed properties for D and BD stars
                SetDwarfProperties(type);
                DebugLogger.LogFormat("  Mass: {0:F3} solar masses", mass);
                DebugLogger.LogFormat("  Temperature: {0} K", temperture);
                DebugLogger.LogFormat("  Diameter: {0:F4} solar diameters", diameter);
                DebugLogger.LogFormat("  Minimum allowable orbit: {0:F3} (orbit number)", MinAllowableOrbit);
            }

            // Calculate luminosity using Stefan-Boltzmann law: L = D^2 * (T/T_sol)^4
            float diameterSquared = (float)Math.Pow((float)diameter, 2.00F);
            float tempRatio = (float)(temperture / solTemperture);
            float tempRatioFourth = (float)Math.Pow(tempRatio, 4.00F);
            luminosity = (float)Math.Round(diameterSquared * tempRatioFourth, 3);

            DebugLogger.Log("  Calculating luminosity using Stefan-Boltzmann law:");
            DebugLogger.LogFormat("    Formula: L = D^2 × (T/T_sol)^4");
            DebugLogger.LogFormat("    Diameter (D): {0:F4} solar diameters", diameter);
            DebugLogger.LogFormat("    D^2: {0:F6}", diameterSquared);
            DebugLogger.LogFormat("    Temperature (T): {0} K", temperture);
            DebugLogger.LogFormat("    Solar temp (T_sol): {0} K", solTemperture);
            DebugLogger.LogFormat("    T/T_sol: {0:F6}", tempRatio);
            DebugLogger.LogFormat("    (T/T_sol)^4: {0:F6}", tempRatioFourth);
            DebugLogger.LogFormat("    Calculated luminosity: {0:F6} solar luminosities", luminosity);

            while (age < 0.1F)
            {
                if (mass <= 0.9)
                {
                    age = (float)Math.Round(((((float)Starhelper.diceRoll(6, 1, dice)) * 2) + (float)Starhelper.diceRoll(3, 1, dice) - 1) + ((float)Starhelper.diceRoll(3, 1, dice) / 10), 2);
                    if (age > 12.0F)
                        age = 0;
                }
                else
                {
                    age = (float)Math.Round(((float)Starhelper.diceRoll(6, 1, dice) - 1 + ((float)Starhelper.diceRoll(6, 1, dice) / 6)) / 6, 2);
                }
            }

            DebugLogger.LogFormat("  Star age: {0:F2} billion years", age);
        }

        public void CreateStar(float orbit, Starhelper.starOrbitType orbitType, Random dice)
        {
            DebugLogger.LogFormat("Creating {0} star...", orbitType);
            starOrbitType = orbitType;
            int starTypeNum = Starhelper.diceRoll(6, 2, dice);
            DebugLogger.LogDiceRoll(2, starTypeNum, "Star type determination");
            //Console.WriteLine("starTypeNum = " + starTypeNum);
            if (starTypeNum <= 2)
            {
                int unusualTypeNum = Starhelper.diceRoll(6, 2, dice);
                //Console.WriteLine("unusualTypeNum = " + unusualTypeNum);
                if (unusualTypeNum <= 3)
                {
                    int unusualTypeReroll = Starhelper.diceRoll(6, 2, dice) + 1;
                    //Console.WriteLine("unusualTypeReroll = " + unusualTypeReroll);
                    if (unusualTypeReroll <= 6)
                    { type = "M"; colour = "Orange Red"; }
                    else if (unusualTypeReroll >= 7 && unusualTypeReroll <= 8)
                    { type = "K"; colour = "Light Orange"; }
                    else if (unusualTypeReroll >= 9 && unusualTypeReroll <= 11)
                    { type = "G"; colour = "Yellow"; }
                    else if (unusualTypeReroll >= 12)
                    {
                        int hotTypeNum = Starhelper.diceRoll(6, 2, dice);
                        if (hotTypeNum <= 11)
                        { type = "B"; colour = "Blue White"; }
                        else
                        {
                            { type = "O"; colour = "Blue"; }
                        }
                    }
                    starclass = "VI";
                }
                else if (unusualTypeNum == 4)
                {
                    int unusualTypeReroll = Starhelper.diceRoll(6, 2, dice);
                    //Console.WriteLine("unusualTypeReroll = " + unusualTypeReroll);
                    if (unusualTypeReroll >= 3 && unusualTypeReroll <= 6)
                        unusualTypeReroll = unusualTypeReroll + 5;
                    if (unusualTypeReroll <= 6)
                    { type = "M"; colour = "Orange Red"; }
                    else if (unusualTypeReroll >= 7 && unusualTypeReroll <= 8)
                    { type = "K"; colour = "Light Orange"; }
                    else if (unusualTypeReroll >= 9 && unusualTypeReroll <= 10)
                    { type = "G"; colour = "Yellow"; }
                    else if (unusualTypeReroll == 11)
                    { type = "F"; colour = "Yellow White"; }
                    else if (unusualTypeReroll >= 12)
                    {
                        int hotTypeNum = Starhelper.diceRoll(6, 2, dice);
                        //Console.WriteLine("hotTypeNum = " + hotTypeNum);
                        if (hotTypeNum <= 9)
                        { type = "A"; colour = "White"; }
                        else
                        {
                            { type = "B"; colour = "Blue White"; }
                        }
                    }
                    starclass = "IV";
                }
                else if (unusualTypeNum >= 5 && unusualTypeNum <= 7)
                    type = "BD";
                else if (unusualTypeNum >= 8 && unusualTypeNum <= 10)
                    type = "D";
                else if (unusualTypeNum == 11)
                {
                    starclass = "III";
                }
                else
                {
                    int giantClassReroll = Starhelper.diceRoll(6, 2, dice);
                    //Console.WriteLine("giantClassReroll = " + giantClassReroll);
                    if (giantClassReroll >= 2 && giantClassReroll <= 8)
                        starclass = "III";
                    if (giantClassReroll >= 9 && giantClassReroll <= 10)
                        starclass = "II";
                    if (giantClassReroll == 11)
                        starclass = "Ib";
                    else
                        starclass = "Ia";
                }
                if (starclass == "III" || starclass == "II" || starclass == "Ib" || starclass == "Ia")
                {
                    int giantTypeReroll = Starhelper.diceRoll(6, 2, dice);
                    //Console.WriteLine("giantTypeReroll = " + giantTypeReroll);
                    if (giantTypeReroll <= 6)
                    { type = "M"; colour = "Orange Red"; }
                    else if (giantTypeReroll >= 7 && giantTypeReroll <= 8)
                    { type = "K"; colour = "Light Orange"; }
                    else if (giantTypeReroll >= 9 && giantTypeReroll <= 10)
                    { type = "G"; colour = "Yellow"; }
                    else if (giantTypeReroll == 11)
                    { type = "F"; colour = "Yellow White"; }
                    else if (giantTypeReroll >= 12)
                    {
                        int hotTypeNum = Starhelper.diceRoll(6, 2, dice);
                        if (hotTypeNum <= 9)
                        { type = "A"; colour = "White"; }
                        else if (hotTypeNum >= 10 && hotTypeNum <= 11)
                        { type = "B"; colour = "Blue White"; }
                        else
                        { type = "O"; colour = "Blue"; }
                    }
                }
            }
            else if (starTypeNum >= 3 && starTypeNum <= 6)
            {
                type = "M";
                colour = "Orange Red";
                starclass = "V";
            }
            else if (starTypeNum >= 7 && starTypeNum <= 8)
            {
                type = "K";
                colour = "Light Orange";
                starclass = "V";
            }
            else if (starTypeNum >= 9 && starTypeNum <= 10)
            {
                type = "G";
                colour = "Yellow";
                starclass = "V";
            }
            else if (starTypeNum == 11)
            {
                type = "F";
                colour = "Yellow White";
                starclass = "V";
            }
            else if (starTypeNum >= 12)
            {
                int hotTypeNum = Starhelper.diceRoll(6, 2, dice);
                if (hotTypeNum <= 9)
                {
                    type = "A";
                    colour = "White";
                    starclass = "V";
                }
                else if (hotTypeNum == 10 || hotTypeNum == 11)
                {
                    type = "B";
                    colour = "Blue White";
                    starclass = "V";
                }
                else
                {
                    type = "O";
                    colour = "Blue";
                    starclass = "V";
                }
            }

            if (type != "BD" && type != "D")
            {
                subType = (Starhelper.diceRoll(10, 1, dice) - 1).ToString();
                DebugLogger.LogFormat("  Star type: {0}{1} {2}", type, subType, starclass);
                GetMassTempertureDiameter(type, subType, dice);
                DebugLogger.LogFormat("  Base mass: {0:F3} solar masses", mass);
                DebugLogger.LogFormat("  Base temperature: {0} K", temperture);
                DebugLogger.LogFormat("  Base diameter: {0:F4} solar diameters", diameter);

                StarVariance(dice);
                DebugLogger.LogFormat("  Mass after variance: {0:F3} solar masses", mass);

                GetMinAllowableOrbit(type, subType);

                // Calculate luminosity using Stefan-Boltzmann law: L = D^2 * (T/T_sol)^4
                float diameterSquared = (float)Math.Pow((float)diameter, 2.00F);
                float tempRatio = (float)(temperture / solTemperture);
                float tempRatioFourth = (float)Math.Pow(tempRatio, 4.00F);
                luminosity = (float)Math.Round(diameterSquared * tempRatioFourth, 3);

                DebugLogger.Log("  Calculating luminosity using Stefan-Boltzmann law:");
                DebugLogger.LogFormat("    Formula: L = D^2 × (T/T_sol)^4");
                DebugLogger.LogFormat("    Diameter (D): {0:F4} solar diameters", diameter);
                DebugLogger.LogFormat("    D^2: {0:F6}", diameterSquared);
                DebugLogger.LogFormat("    Temperature (T): {0} K", temperture);
                DebugLogger.LogFormat("    Solar temp (T_sol): {0} K", solTemperture);
                DebugLogger.LogFormat("    T/T_sol: {0:F6}", tempRatio);
                DebugLogger.LogFormat("    (T/T_sol)^4: {0:F6}", tempRatioFourth);
                DebugLogger.LogFormat("    Calculated luminosity: {0:F6} solar luminosities", luminosity);
            }
            else
            {
                // D and BD stars have no subtype or starclass
                subType = "";
                starclass = "";
                DebugLogger.LogFormat("  Star type: {0} ({1})", type, type == "BD" ? "Brown Dwarf" : "White Dwarf");

                // Set fixed properties for D and BD stars
                SetDwarfProperties(type);
                DebugLogger.LogFormat("  Mass: {0:F3} solar masses", mass);
                DebugLogger.LogFormat("  Temperature: {0} K", temperture);
                DebugLogger.LogFormat("  Diameter: {0:F4} solar diameters", diameter);
                DebugLogger.LogFormat("  Luminosity: {0:F6}", luminosity);
                DebugLogger.LogFormat("  Minimum allowable orbit: {0:F3} (orbit number)", MinAllowableOrbit);
            }

            while (age < 0.1F) {
            if (mass <= 0.9)
                {
                    age = (float)Math.Round(((((float)Starhelper.diceRoll(6, 1, dice)) * 2) + (float)Starhelper.diceRoll(3, 1, dice) - 1) + ((float)Starhelper.diceRoll(3, 1, dice) / 10), 2);
                    if (age > 12.0F)
                        age = 0;
                }
                else
                {
                    age = (float)Math.Round(((float)Starhelper.diceRoll(6, 1, dice) - 1 + ((float)Starhelper.diceRoll(6, 1, dice) / 6)) / 6, 2);
                }
            }

            DebugLogger.LogFormat("  Star age: {0:F2} billion years", age);
            Starhelper.systemAge = age;

        }

        private void GetMassTempertureDiameter (string sType, string sSubtype, Random dice)
        {

            if (sSubtype == "0" || sSubtype == "5")
            {
                int classIndex = GetClassIndex(starclass);
                int typeIndex = GetTypeIndex(type, subType);

                mass = starMass[classIndex, typeIndex];
                diameter = starDiameter[classIndex, typeIndex];
                temperture = starTemperture[typeIndex];

                DebugLogger.LogFormat("  Looking up values from tables for {0}{1} {2}", type, subType, starclass);
                DebugLogger.LogFormat("    Table indices: Class={0}, Type={1}", classIndex, typeIndex);
                DebugLogger.LogFormat("    Retrieved diameter: {0:F4} solar diameters", diameter);
            }
            else if (sType == "M" && sSubtype == "9")
            {
                int classIndex = GetClassIndex(starclass);
                int typeIndex = GetTypeIndex(type, subType);

                mass = starMass[classIndex, typeIndex];
                diameter = starDiameter[classIndex, typeIndex];
                temperture = starTemperture[typeIndex];

                DebugLogger.LogFormat("  Looking up values from tables for {0}{1} {2}", type, subType, starclass);
                DebugLogger.LogFormat("    Table indices: Class={0}, Type={1}", classIndex, typeIndex);
                DebugLogger.LogFormat("    Retrieved diameter: {0:F4} solar diameters", diameter);
            }
            else
            {
                int adjustedSubtype;
                int lowerSubtypeIndex;
                int upperSubtypeIndex;
                float upperMass;
                float lowerMass;
                float lowerTemperture;
                float upperTemperture;
                float lowerDiameter;
                float upperDiameter;
                if (Convert.ToInt32(sSubtype) < 5)
                    adjustedSubtype = 0;
                else
                    adjustedSubtype = 5;

                DebugLogger.LogFormat("  Interpolating values for {0}{1} {2}", type, sSubtype, starclass);
                DebugLogger.LogFormat("    Subtype {0} is between {1}{2} and next bracket", sSubtype, type, adjustedSubtype);

                lowerSubtypeIndex = GetTypeIndex(sType, adjustedSubtype.ToString());
                upperSubtypeIndex = lowerSubtypeIndex + 1;

                lowerMass = starMass[GetClassIndex(starclass),lowerSubtypeIndex];
                lowerTemperture = starTemperture[lowerSubtypeIndex];
                lowerDiameter = starDiameter[GetClassIndex(starclass),lowerSubtypeIndex];
                upperMass = starMass[GetClassIndex(starclass), upperSubtypeIndex];
                upperTemperture = starTemperture[upperSubtypeIndex];
                upperDiameter = starDiameter[GetClassIndex(starclass), upperSubtypeIndex];

                DebugLogger.LogFormat("    Lower bracket diameter: {0:F4}", lowerDiameter);
                DebugLogger.LogFormat("    Upper bracket diameter: {0:F4}", upperDiameter);

                int per = Convert.ToInt32(sSubtype);
                if (lowerMass <= upperMass)
                {
                    mass = Extrapolate(lowerMass, upperMass, per);
                    temperture = Extrapolate(lowerTemperture, upperTemperture, per);
                    diameter = Extrapolate(lowerDiameter, upperDiameter, per);
                    DebugLogger.LogFormat("    Interpolated diameter: {0:F4} solar diameters (factor: {1}/10)", diameter, per);
                }
                else
                {
                    mass = Extrapolate(upperMass, lowerMass, per);
                    temperture = Extrapolate(upperTemperture, lowerTemperture, per);
                    temperture = Extrapolate(upperTemperture, lowerTemperture, per);
                    diameter = Extrapolate(upperDiameter, lowerDiameter, per);
                    DebugLogger.LogFormat("    Interpolated diameter: {0:F4} solar diameters (factor: {1}/10, inverted)", diameter, per);
                }



            }
        }

        private float Extrapolate(float lnumber, float unumber, int factor)
        {
            float per = (float)factor / 10;
            return lnumber + (per * (unumber - lnumber));
        }

        private void GetMinAllowableOrbit(string sType, string sSubtype)
        {
            // Companion orbit companions don't have minimum allowable orbits
            if (starOrbitType == Starhelper.starOrbitType.Companion)
            {
                MinAllowableOrbit = 0;
                DebugLogger.Log("  Companion orbit star - no minimum allowable orbit");
                return;
            }

            if (sSubtype == "0" || sSubtype == "5")
            {
                int classIndex = GetClassIndex(starclass);
                int typeIndex = GetTypeIndex(type, subType);

                MinAllowableOrbit = starMinOrbit[classIndex, typeIndex];

                DebugLogger.LogFormat("  Looking up minimum allowable orbit from table for {0}{1} {2}", type, subType, starclass);
                DebugLogger.LogFormat("    Table indices: Class={0}, Type={1}", classIndex, typeIndex);
                DebugLogger.LogFormat("    Minimum allowable orbit: {0:F3} (orbit number)", MinAllowableOrbit);
            }
            else if (sType == "M" && sSubtype == "9")
            {
                int classIndex = GetClassIndex(starclass);
                int typeIndex = GetTypeIndex(type, subType);

                MinAllowableOrbit = starMinOrbit[classIndex, typeIndex];

                DebugLogger.LogFormat("  Looking up minimum allowable orbit from table for {0}{1} {2}", type, subType, starclass);
                DebugLogger.LogFormat("    Table indices: Class={0}, Type={1}", classIndex, typeIndex);
                DebugLogger.LogFormat("    Minimum allowable orbit: {0:F3} (orbit number)", MinAllowableOrbit);
            }
            else
            {
                int adjustedSubtype;
                int lowerSubtypeIndex;
                int upperSubtypeIndex;
                float lowerMinOrbit;
                float upperMinOrbit;

                if (Convert.ToInt32(sSubtype) < 5)
                    adjustedSubtype = 0;
                else
                    adjustedSubtype = 5;

                DebugLogger.LogFormat("  Interpolating minimum allowable orbit for {0}{1} {2}", type, sSubtype, starclass);
                DebugLogger.LogFormat("    Subtype {0} is between {1}{2} and next bracket", sSubtype, type, adjustedSubtype);

                lowerSubtypeIndex = GetTypeIndex(sType, adjustedSubtype.ToString());
                upperSubtypeIndex = lowerSubtypeIndex + 1;

                lowerMinOrbit = starMinOrbit[GetClassIndex(starclass), lowerSubtypeIndex];
                upperMinOrbit = starMinOrbit[GetClassIndex(starclass), upperSubtypeIndex];

                DebugLogger.LogFormat("    Lower bracket min orbit: {0:F2} (orbit number)", lowerMinOrbit);
                DebugLogger.LogFormat("    Upper bracket min orbit: {0:F2} (orbit number)", upperMinOrbit);

                int per = Convert.ToInt32(sSubtype);

                // Handle cases where values are 0 (represented as "-" in the table)
                if (lowerMinOrbit == 0 && upperMinOrbit == 0)
                {
                    MinAllowableOrbit = 0;
                    DebugLogger.Log("    Both brackets are undefined (-) - no minimum allowable orbit");
                }
                else if (lowerMinOrbit == 0)
                {
                    MinAllowableOrbit = upperMinOrbit;
                    DebugLogger.LogFormat("    Lower bracket undefined (-) - using upper bracket value: {0:F2} (orbit number)", MinAllowableOrbit);
                }
                else if (upperMinOrbit == 0)
                {
                    MinAllowableOrbit = lowerMinOrbit;
                    DebugLogger.LogFormat("    Upper bracket undefined (-) - using lower bracket value: {0:F2} (orbit number)", MinAllowableOrbit);
                }
                else
                {
                    // Both values exist, interpolate normally
                    MinAllowableOrbit = Extrapolate(lowerMinOrbit, upperMinOrbit, per);
                    DebugLogger.LogFormat("    Interpolated minimum allowable orbit: {0:F2} (orbit number, factor: {1}/10)", MinAllowableOrbit, per);
                }
            }
        }

        private void StarVariance(Random dice)
        {
            //Console.WriteLine("mass before variance = " + mass);
            float ranVar = Starhelper.diceRoll(6, 2, dice) - 7;
            ranVar = ranVar * 0.2F;
            //Console.WriteLine("ranVar = " + ranVar);
            float maxVar = mass * 0.2F;
            //Console.WriteLine("maxVar - " + maxVar);
            mass = (float)Math.Round(mass + (ranVar * maxVar),3);
        }

        private int GetTypeIndex(string sType, string sSubtype)
        {
            string sTypeSubtype = sType + sSubtype;


            switch (sTypeSubtype)
            {
                case "O0":
                    return 0;
                case "O5":
                    return 1;
                case "B0":
                    return 2;
                case "B5":
                    return 3;
                case "A0":
                    return 4;
                case "A5":
                    return 5;
                case "F0":
                    return 6;
                case "F5":
                    return 7;
                case "G0":
                    return 8;
                case "G5":
                    return 9;
                case "K0":
                    return 10;
                case "K5":
                    return 11;
                case "M0":
                    return 12;
                case "M5":
                    return 13;
                case "M9":
                    return 14;
                default:
                    throw new NotImplementedException();
            }
        }

        private int GetClassIndex (string sClass)
        {
            switch (sClass)
            {
                case "Ia":
                    return 0;
                case "Ib":
                    return 1;
                case "II":
                    return 2;
                case "III":
                    return 3;
                case "IV":
                    return 4;
                case "V":
                    return 5;
                case "VI":
                    return 6;
                default:
                    throw new NotImplementedException ();
            }
        }

        // Helper method to get the next cooler star type
        private string GetCoolerStarType(string currentType)
        {
            // Types from hottest to coolest: O B A F G K M
            switch (currentType)
            {
                case "O": return "B";
                case "B": return "A";
                case "A": return "F";
                case "F": return "G";
                case "G": return "K";
                case "K": return "M";
                case "M": return "M"; // Can't go cooler than M
                default: return currentType;
            }
        }

        // Helper method to determine if star1 is hotter than star2
        private bool IsHotterThan(string type1, string subType1, string type2, string subType2)
        {
            // Get type indices (higher = cooler)
            int typeIndex1 = GetStarTypeHeatIndex(type1);
            int typeIndex2 = GetStarTypeHeatIndex(type2);

            if (typeIndex1 < typeIndex2) return true; // Lower index = hotter
            if (typeIndex1 > typeIndex2) return false; // Higher index = cooler

            // Same type, compare subtypes (higher subtype = cooler)
            int sub1 = int.Parse(subType1);
            int sub2 = int.Parse(subType2);
            return sub1 < sub2;
        }

        // Helper to get heat index (lower = hotter)
        private int GetStarTypeHeatIndex(string type)
        {
            switch (type)
            {
                case "O": return 0;
                case "B": return 1;
                case "A": return 2;
                case "F": return 3;
                case "G": return 4;
                case "K": return 5;
                case "M": return 6;
                default: return 99; // D and BD
            }
        }

        // Helper to set colour based on star type
        private void SetColourFromType(string starType)
        {
            switch (starType)
            {
                case "O":
                    colour = "Blue";
                    break;
                case "B":
                    colour = "Blue White";
                    break;
                case "A":
                    colour = "White";
                    break;
                case "F":
                    colour = "Yellow White";
                    break;
                case "G":
                    colour = "Yellow";
                    break;
                case "K":
                    colour = "Light Orange";
                    break;
                case "M":
                    colour = "Orange Red";
                    break;
                default:
                    colour = "";
                    break;
            }
        }

        // Helper to set fixed properties for D and BD stars
        private void SetDwarfProperties(string starType)
        {
            if (starType == "BD")
            {
                // Brown Dwarf properties
                colour = "Brown";
                temperture = 1850;
                mass = 0.06f;
                diameter = 0.08f;
                luminosity = 0.000066f;
                MinAllowableOrbit = 0.005f;  // Orbit number
            }
            else if (starType == "D")
            {
                // White Dwarf properties
                colour = "White";
                temperture = 8000;
                mass = 0.6f;
                diameter = 0.017f;
                luminosity = 0.001f;
                MinAllowableOrbit = 0.001f;  // Orbit number
            }
        }

        // Generate a random subtype (0-9)
        private string GenerateSubType(Random dice)
        {
            return (Starhelper.diceRoll(10, 1, dice) - 1).ToString();
        }

        // Generate Random companion
        private void GenerateRandomCompanion(Star primaryStar, Random dice)
        {
            DebugLogger.Log("    Generating random companion (may need adjustment if hotter than primary)");

            // Generate a star using normal process
            int starTypeNum = Starhelper.diceRoll(6, 2, dice);
            DebugLogger.LogDiceRoll(2, starTypeNum, "Random companion base type");

            // Use similar logic to CreateStar for type determination
            DetermineStarTypeFromRoll(starTypeNum, dice);

            // If it's not BD or D, generate and validate subtype
            if (type != "BD" && type != "D")
            {
                subType = GenerateSubType(dice);
                DebugLogger.LogFormat("    Generated type: {0}{1} {2}", type, subType, starclass);

                // Check if it's hotter than primary
                if (IsHotterThan(type, subType, primaryStar.type, primaryStar.subType))
                {
                    DebugLogger.Log("    Companion is hotter than primary - adjusting to one type cooler");
                    type = GetCoolerStarType(primaryStar.type);
                    SetColourFromType(type);

                    // Recalculate subtype to ensure not hotter than primary
                    if (type == primaryStar.type)
                    {
                        // Same type - must have higher (cooler) subtype
                        int primarySubType = int.Parse(primaryStar.subType);
                        int companionSubType = Starhelper.diceRoll(10, 1, dice) - 1;
                        while (companionSubType <= primarySubType && companionSubType < 9)
                        {
                            companionSubType++;
                        }
                        subType = companionSubType.ToString();
                        DebugLogger.LogFormat("    Recalculated subtype to be cooler: {0}", subType);
                    }
                    else
                    {
                        // Different (cooler) type - any subtype is fine
                        subType = GenerateSubType(dice);
                    }

                    // Special case: if primary is M and new subtype is higher, make it BD
                    if (primaryStar.type == "M" && type == "M" && int.Parse(subType) > int.Parse(primaryStar.subType))
                    {
                        type = "BD";
                        colour = "";
                        DebugLogger.Log("    Primary is M and subtype too high - making companion a BD");
                    }
                    else
                    {
                        DebugLogger.LogFormat("    Adjusted to: {0}{1} {2}", type, subType, starclass);
                    }
                }
                else if (type == primaryStar.type)
                {
                    // Same type as primary - ensure subtype is higher (cooler)
                    int primarySubType = int.Parse(primaryStar.subType);
                    int companionSubType = int.Parse(subType);

                    if (companionSubType <= primarySubType)
                    {
                        DebugLogger.LogFormat("    Same type as primary but subtype not cooler ({0} vs {1}) - recalculating", subType, primaryStar.subType);
                        companionSubType = primarySubType + Starhelper.diceRoll(10, 1, dice);

                        if (companionSubType > 9)
                        {
                            if (type == "M")
                            {
                                type = "BD";
                                colour = "";
                                DebugLogger.Log("    Subtype would exceed 9 for M type - making companion a BD");
                            }
                            else
                            {
                                type = GetCoolerStarType(type);
                                subType = (companionSubType - 10).ToString();
                                SetColourFromType(type);
                                DebugLogger.LogFormat("    Subtype exceeded 9 - moving to cooler type: {0}{1}", type, subType);
                            }
                        }
                        else
                        {
                            subType = companionSubType.ToString();
                            DebugLogger.LogFormat("    Recalculated subtype: {0}", subType);
                        }
                    }
                }
            }
            else
            {
                DebugLogger.LogFormat("    Generated type: {0}", type);
            }
        }

        // Generate Lesser companion (one type cooler)
        private void GenerateLesserCompanion(Star primaryStar, Random dice)
        {
            DebugLogger.Log("    Making companion one type cooler than primary");

            type = GetCoolerStarType(primaryStar.type);
            starclass = primaryStar.starclass;
            SetColourFromType(type);

            // If same type as primary (can happen with M), ensure cooler subtype
            if (type == primaryStar.type)
            {
                int primarySubType = int.Parse(primaryStar.subType);
                int companionSubType = primarySubType + Starhelper.diceRoll(10, 1, dice);

                DebugLogger.LogFormat("    Same type (M) - calculating cooler subtype from {0}", primaryStar.subType);

                if (companionSubType > 9)
                {
                    type = "BD";
                    colour = "";
                    DebugLogger.Log("    Subtype would exceed 9 for M type - making companion a BD");
                }
                else
                {
                    subType = companionSubType.ToString();
                    DebugLogger.LogFormat("    Lesser companion: {0}{1} {2}", type, subType, starclass);
                }
            }
            else
            {
                // Different type - any subtype is fine since we're already cooler
                subType = GenerateSubType(dice);
                DebugLogger.LogFormat("    Lesser companion: {0}{1} {2}", type, subType, starclass);
            }
        }

        // Generate Sibling companion (same type, higher subtype)
        private void GenerateSiblingCompanion(Star primaryStar, Random dice)
        {
            DebugLogger.Log("    Making companion same type as primary with higher subtype");

            type = primaryStar.type;
            starclass = primaryStar.starclass;
            colour = primaryStar.colour;

            int diceRoll = Starhelper.diceRoll(6, 1, dice);
            int newSubType = int.Parse(primaryStar.subType) + diceRoll;
            DebugLogger.LogFormat("    Primary subtype: {0}, dice roll: {1}, new subtype: {2}", primaryStar.subType, diceRoll, newSubType);

            if (newSubType > 9)
            {
                if (primaryStar.type == "M")
                {
                    // M type -> BD
                    type = "BD";
                    colour = "";
                    DebugLogger.Log("    Primary is M and subtype > 9 - making companion a BD");
                }
                else
                {
                    // Move to next cooler type
                    type = GetCoolerStarType(primaryStar.type);
                    subType = (newSubType - 10).ToString();
                    SetColourFromType(type);
                    DebugLogger.LogFormat("    Subtype > 9 - moving to cooler type: {0}{1} {2}", type, subType, starclass);
                }
            }
            else
            {
                subType = newSubType.ToString();
                DebugLogger.LogFormat("    Sibling companion: {0}{1} {2}", type, subType, starclass);
            }
        }

        // Generate Twin companion (identical but smaller)
        private void GenerateTwinCompanion(Star primaryStar, Random dice)
        {
            DebugLogger.Log("    Making twin companion (same type/subtype, slightly smaller)");

            type = primaryStar.type;
            starclass = primaryStar.starclass;
            subType = primaryStar.subType;
            colour = primaryStar.colour;

            // Randomize mass reduction separately
            int massDiceRoll = Starhelper.diceRoll(6, 1, dice);
            float massReductionPercent = (massDiceRoll - 1) / 100.0f;
            DebugLogger.LogFormat("    Mass reduction: {0}% (dice roll: {1})", massReductionPercent * 100, massDiceRoll);

            // Randomize diameter reduction separately
            int diameterDiceRoll = Starhelper.diceRoll(6, 1, dice);
            float diameterReductionPercent = (diameterDiceRoll - 1) / 100.0f;
            DebugLogger.LogFormat("    Diameter reduction: {0}% (dice roll: {1})", diameterReductionPercent * 100, diameterDiceRoll);

            // Apply reductions to primary's properties
            mass = primaryStar.mass * (1.0f - massReductionPercent);
            diameter = primaryStar.diameter * (1.0f - diameterReductionPercent);
            temperture = primaryStar.temperture;
            age = primaryStar.age;
            MinAllowableOrbit = primaryStar.MinAllowableOrbit;
            DebugLogger.LogFormat("    Minimum allowable orbit: {0:F2} (orbit number, same as primary)", MinAllowableOrbit);

            // Recalculate luminosity based on new diameter
            float diameterSquared = (float)Math.Pow((float)diameter, 2.00F);
            float tempRatio = (float)(temperture / solTemperture);
            float tempRatioFourth = (float)Math.Pow(tempRatio, 4.00F);
            luminosity = (float)Math.Round(diameterSquared * tempRatioFourth, 3);

            DebugLogger.LogFormat("    Twin companion: {0}{1} {2}", type, subType, starclass);
            DebugLogger.LogFormat("    Mass: {0:F3} solar masses ({1:F1}% of primary)", mass, (1.0f - massReductionPercent) * 100);
            DebugLogger.LogFormat("    Diameter: {0:F4} solar diameters ({1:F1}% of primary)", diameter, (1.0f - diameterReductionPercent) * 100);
            DebugLogger.LogFormat("    Temperature: {0} K", temperture);

            DebugLogger.Log("    Recalculating luminosity for twin companion:");
            DebugLogger.LogFormat("      Formula: L = D^2 × (T/T_sol)^4");
            DebugLogger.LogFormat("      Primary diameter: {0:F4}, Twin diameter: {1:F4}", primaryStar.diameter, diameter);
            DebugLogger.LogFormat("      D^2: {0:F6}", diameterSquared);
            DebugLogger.LogFormat("      (T/T_sol)^4: {0:F6}", tempRatioFourth);
            DebugLogger.LogFormat("      Luminosity: {0:F6} solar luminosities", luminosity);

            DebugLogger.LogFormat("    Age: {0:F2} billion years", age);
        }

        // Helper method to determine star type from dice roll (extracted from CreateStar)
        private void DetermineStarTypeFromRoll(int starTypeNum, Random dice)
        {
            if (starTypeNum <= 2)
            {
                int unusualTypeNum = Starhelper.diceRoll(6, 2, dice);
                if (unusualTypeNum <= 3)
                {
                    int unusualTypeReroll = Starhelper.diceRoll(6, 2, dice) + 1;
                    if (unusualTypeReroll <= 6)
                    { type = "M"; colour = "Orange Red"; }
                    else if (unusualTypeReroll >= 7 && unusualTypeReroll <= 8)
                    { type = "K"; colour = "Light Orange"; }
                    else if (unusualTypeReroll >= 9 && unusualTypeReroll <= 11)
                    { type = "G"; colour = "Yellow"; }
                    else if (unusualTypeReroll >= 12)
                    {
                        int hotTypeNum = Starhelper.diceRoll(6, 2, dice);
                        if (hotTypeNum <= 11)
                        { type = "B"; colour = "Blue White"; }
                        else
                        { type = "O"; colour = "Blue"; }
                    }
                    starclass = "VI";
                }
                else if (unusualTypeNum == 4)
                {
                    int unusualTypeReroll = Starhelper.diceRoll(6, 2, dice);
                    if (unusualTypeReroll >= 3 && unusualTypeReroll <= 6)
                        unusualTypeReroll = unusualTypeReroll + 5;
                    if (unusualTypeReroll <= 6)
                    { type = "M"; colour = "Orange Red"; }
                    else if (unusualTypeReroll >= 7 && unusualTypeReroll <= 8)
                    { type = "K"; colour = "Light Orange"; }
                    else if (unusualTypeReroll >= 9 && unusualTypeReroll <= 10)
                    { type = "G"; colour = "Yellow"; }
                    else if (unusualTypeReroll == 11)
                    { type = "F"; colour = "Yellow White"; }
                    else if (unusualTypeReroll >= 12)
                    {
                        int hotTypeNum = Starhelper.diceRoll(6, 2, dice);
                        if (hotTypeNum <= 9)
                        { type = "A"; colour = "White"; }
                        else
                        { type = "B"; colour = "Blue White"; }
                    }
                    starclass = "IV";
                }
                else if (unusualTypeNum >= 5 && unusualTypeNum <= 7)
                    type = "BD";
                else if (unusualTypeNum >= 8 && unusualTypeNum <= 10)
                    type = "D";
                else if (unusualTypeNum == 11)
                {
                    starclass = "III";
                }
                else
                {
                    int giantClassReroll = Starhelper.diceRoll(6, 2, dice);
                    if (giantClassReroll >= 2 && giantClassReroll <= 8)
                        starclass = "III";
                    if (giantClassReroll >= 9 && giantClassReroll <= 10)
                        starclass = "II";
                    if (giantClassReroll == 11)
                        starclass = "Ib";
                    else
                        starclass = "Ia";
                }
                if (starclass == "III" || starclass == "II" || starclass == "Ib" || starclass == "Ia")
                {
                    int giantTypeReroll = Starhelper.diceRoll(6, 2, dice);
                    if (giantTypeReroll <= 6)
                    { type = "M"; colour = "Orange Red"; }
                    else if (giantTypeReroll >= 7 && giantTypeReroll <= 8)
                    { type = "K"; colour = "Light Orange"; }
                    else if (giantTypeReroll >= 9 && giantTypeReroll <= 10)
                    { type = "G"; colour = "Yellow"; }
                    else if (giantTypeReroll == 11)
                    { type = "F"; colour = "Yellow White"; }
                    else if (giantTypeReroll >= 12)
                    {
                        int hotTypeNum = Starhelper.diceRoll(6, 2, dice);
                        if (hotTypeNum <= 9)
                        { type = "A"; colour = "White"; }
                        else if (hotTypeNum >= 10 && hotTypeNum <= 11)
                        { type = "B"; colour = "Blue White"; }
                        else
                        { type = "O"; colour = "Blue"; }
                    }
                }
            }
            else if (starTypeNum >= 3 && starTypeNum <= 6)
            {
                type = "M";
                colour = "Orange Red";
                starclass = "V";
            }
            else if (starTypeNum >= 7 && starTypeNum <= 8)
            {
                type = "K";
                colour = "Light Orange";
                starclass = "V";
            }
            else if (starTypeNum >= 9 && starTypeNum <= 10)
            {
                type = "G";
                colour = "Yellow";
                starclass = "V";
            }
            else if (starTypeNum == 11)
            {
                type = "F";
                colour = "Yellow White";
                starclass = "V";
            }
            else if (starTypeNum >= 12)
            {
                int hotTypeNum = Starhelper.diceRoll(6, 2, dice);
                if (hotTypeNum <= 9)
                {
                    type = "A";
                    colour = "White";
                    starclass = "V";
                }
                else if (hotTypeNum == 10 || hotTypeNum == 11)
                {
                    type = "B";
                    colour = "Blue White";
                    starclass = "V";
                }
                else
                {
                    type = "O";
                    colour = "Blue";
                    starclass = "V";
                }
            }
        }
    }
}
