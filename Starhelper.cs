using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravellerSystemGenerator
{
    public static class Starhelper
    {
        public const float AU = 149597870.9F;

        public static int diceRoll(int sides, int num, Random dice)
        {
            
            int result = 0;
            //Console.Write('(');
            for (int i = 1; i <= num; i++)
            {
                result += roll(sides, dice);
            }
            //Console.Write(')');
            //Console.WriteLine();
            return result;
        }

        public static float systemAge = 0;
        
        private static int roll (int sides, Random dice)
        {
            
            int r = dice.Next(1, sides +1 );
            //Console.Write(r);
            return r;
        }

        public enum starOrbitType { Close, Near, Far, Companion, Primary }

        public static Dictionary<int, float> orbitValues = new Dictionary<int, float>();

        public static void LoadOrbitalValues ()
        {
            orbitValues.Add(0, 0);
            orbitValues.Add(1, 0.4F);
            orbitValues.Add(2, 0.7F);
            orbitValues.Add(3, 1);
            orbitValues.Add(4, 1.6F);
            orbitValues.Add(5, 2.8F);
            orbitValues.Add(6, 5.2F);
            orbitValues.Add(7, 10);
            orbitValues.Add(8, 20);
            orbitValues.Add(9, 40);
            orbitValues.Add(10, 77);
            orbitValues.Add(11, 154);
            orbitValues.Add(12, 308);
            orbitValues.Add(13, 615);
            orbitValues.Add(14, 1230);
            orbitValues.Add(15, 2500);
            orbitValues.Add(16, 4900);
            orbitValues.Add(17, 9800);
            orbitValues.Add(18, 19500);
            orbitValues.Add(19, 39500);
            orbitValues.Add(20, 78700);
        }
        

        
    }
}
