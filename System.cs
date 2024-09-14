using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Deployment.Internal;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TravellerSystemGenerator
{
    internal class StarSystem
    {
        
        internal StarSystem()
        {
            Random dice = new Random();

            primaryObject = new CelestrialObject();

            primaryObject.celestrialObject = new Star(dice);

            Starhelper.LoadOrbitalValues();

            GenerateAdditionalStars(primaryObject, dice);

            Star star = primaryObject.celestrialObject as Star;

            PrintStar(star, 0, primaryObject, dice);

            foreach (CelestrialObject Cobj in primaryObject.celestrialObjectOrbits)
            {
                if (Cobj.celestrialObject is Star)
                {
                    Star starObj = (Star)Cobj.celestrialObject;
                    PrintStar(starObj, Cobj.orbit, Cobj, dice);
                }
            }


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
                Console.WriteLine(star.starOrbitType.ToString() + " Star");
            }
            else
            {
                Console.WriteLine("Orbit " + " " + orbit + " (" + Cobj.orbitAU + "AU) - " + star.starOrbitType.ToString() + " Star");
                Console.WriteLine("Orbit Eccentricity = " + Cobj.orbitEccentricity.ToString());
                if (Cobj.orbitEccentricity > 0)
                {
                    Console.WriteLine("Orbit Max seperation = " + Cobj.orbitMaxSep.ToString());
                    Console.WriteLine("Orbit Min seperation = " + Cobj.orbitMinSep.ToString());
                }
            }
            Console.WriteLine(star.type + star.subType + " " + star.starclass);
            if (star.type != "BD" && star.type != "D")
                Console.WriteLine("Colour = " + star.colour);
            
            Console.WriteLine("Mass = " + star.mass);
            Console.WriteLine("Temperture = " + star.temperture);
            Console.WriteLine("Diameter = " + star.diameter);
            Console.WriteLine("Luminosity = " + star.luminosity);
            Console.WriteLine("Age = " + star.age);
        }

        private string GetProperty(Object obj, string prop)
        {
            Type type = obj.GetType();
            string rtn = "";
            PropertyInfo property = type.GetProperty(prop);
            //if (property != null)
            //return property.GetValue(obj, null).ToString();
            //else return "";
            try
            {
                rtn = property.GetValue(obj, null).ToString();
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
            if (starOrbit == Starhelper.starOrbitType.Close)
            {
                if (GetProperty(primaryObject.celestrialObject, "starclass") == "Ia" ||
                    GetProperty(primaryObject.celestrialObject, "starclass") == "Ib" ||
                    GetProperty(primaryObject.celestrialObject, "starclass") == "II" ||
                    GetProperty(primaryObject.celestrialObject, "starclass") == "III")
                {
                    starPresent = 0;
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
                int baseOrb = Starhelper.diceRoll(6, 1, dice) - 1;
                float fractionalOrbit = FractionalOrbit(baseOrb, dice, Starhelper.starOrbitType.Close);
                cObj.AddStar(fractionalOrbit, Starhelper.starOrbitType.Close, dice);
                }
            if (nearStarPresent >= 10)
            {
                Console.WriteLine("Near Star present");
                int baseOrb = Starhelper.diceRoll(6, 1, dice) + 5;
                float fractionalOrbit = FractionalOrbit(baseOrb, dice, Starhelper.starOrbitType.Near);
                cObj.AddStar(fractionalOrbit, Starhelper.starOrbitType.Near, dice);
            }
            if (farStarPresent >= 10)
            {
                Console.WriteLine("Far Star present");
                int baseOrb = Starhelper.diceRoll(6, 1, dice) + 11;
                float fractionalOrbit = FractionalOrbit(baseOrb, dice, Starhelper.starOrbitType.Far);
                cObj.AddStar(fractionalOrbit, Starhelper.starOrbitType.Far, dice);
            }
            if(companionStarPresent >= 10)
            {
                Console.WriteLine("Companion Star present");
                int baseOrb = Starhelper.diceRoll(6, 1, dice) / 10 + (Starhelper.diceRoll(6, 2, dice) - 7) / 100;
                float fractionalOrbit = FractionalOrbit(baseOrb, dice, Starhelper.starOrbitType.Companion);
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

        public Star primary { get; set; }
        public CelestrialObject primaryObject { get; set; }

        public static Dictionary<int, float> orbitValues = new Dictionary<int, float>();
    }
}
