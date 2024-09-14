using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace TravellerSystemGenerator
{
    internal class Star
    {
        public bool primary { get; set; }
        public string type { get; set; }
        public string starclass { get; set; }
        public string subType { get; set; }
        public string colour { get; set; }
        public float mass { get; set; }
        public float temperture { get; set; }
        public float diameter { get; set; }
        public float luminosity { get; set; }
        public float age { get; set; }
        public Starhelper.starOrbitType starOrbitType { get; set; }
        public List<CelestrialObject> orbits { get; set; }

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
        public Star(Random dice)
        {
            CreateStar(0, Starhelper.starOrbitType.Primary, dice);
        }
        public Star(float orbit, Starhelper.starOrbitType starOrbitType, Random dice)
        {
            CreateStar(orbit, starOrbitType, dice);
        }


        public void CreateStar(float orbit, Starhelper.starOrbitType orbitType, Random dice)
        {
            starOrbitType = orbitType;
            int starTypeNum = Starhelper.diceRoll(6, 2, dice);
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
                GetMassTempertureDiameter(type, subType, dice);
            }

            StarVariance(dice);

            luminosity = (float)Math.Round((Math.Pow((float)diameter, 2.00F)) * (float)(Math.Pow((float)(temperture / solTemperture), 4.00F)), 3);

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

            Starhelper.systemAge = age;

        }

        private void GetMassTempertureDiameter (string sType, string sSubtype, Random dice)
        {

            if (sSubtype == "0" || sSubtype == "5")
            {
                mass = starMass[GetClassIndex(starclass), GetTypeIndex(type, subType)];
                diameter = starDiameter[GetClassIndex(starclass), GetTypeIndex(type, subType)];
                temperture = starTemperture[GetTypeIndex(type, subType)];
            }
            else if (sType == "M" && sSubtype == "9")
            {
                mass = starMass[GetClassIndex(starclass), GetTypeIndex(type, subType)];
                diameter = starDiameter[GetClassIndex(starclass), GetTypeIndex(type, subType)];
                temperture = starTemperture[GetTypeIndex(type, subType)];
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
                lowerSubtypeIndex = GetTypeIndex(sType, adjustedSubtype.ToString());
                upperSubtypeIndex = lowerSubtypeIndex + 1;

                lowerMass = starMass[GetClassIndex(starclass),lowerSubtypeIndex];
                lowerTemperture = starTemperture[lowerSubtypeIndex];
                lowerDiameter = starDiameter[GetClassIndex(starclass),lowerSubtypeIndex];
                upperMass = starMass[GetClassIndex(starclass), upperSubtypeIndex];
                upperTemperture = starTemperture[upperSubtypeIndex];
                upperDiameter = starDiameter[GetClassIndex(starclass), upperSubtypeIndex];
                int per = Convert.ToInt32(sSubtype);
                if (lowerMass <= upperMass)
                {
                    mass = Extrapolate(lowerMass, upperMass, per);
                    temperture = Extrapolate(lowerTemperture, upperTemperture, per);
                    diameter = Extrapolate(lowerDiameter, upperDiameter, per);
                }
                else
                {
                    mass = Extrapolate(upperMass, lowerMass, per);
                    temperture = Extrapolate(upperTemperture, lowerTemperture, per);
                    temperture = Extrapolate(upperTemperture, lowerTemperture, per);
                    diameter = Extrapolate(upperDiameter, lowerDiameter, per);
                }

                

            }
        }

        private float Extrapolate(float lnumber, float unumber, int factor)
        {
            float per = (float)factor / 10;
            return lnumber + (per * (unumber - lnumber));
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
    }
}
