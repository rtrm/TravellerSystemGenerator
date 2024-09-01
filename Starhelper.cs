using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravellerSystemGenerator
{
    public static class Starhelper
    {
        
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
        
        private static int roll (int sides, Random dice)
        {
            
            int r = dice.Next(1, sides +1 );
            //Console.Write(r);
            return r;
        }
    }
}
