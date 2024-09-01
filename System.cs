using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace TravellerSystemGenerator
{
    internal class StarSystem
    {
        internal StarSystem()
        {
            Random dice = new Random();

            primary = new Star(true, dice);


            Console.WriteLine("Primary = " + primary.type + primary.subType + " " + primary.starclass);
            if (primary.type != "BD" && primary.type != "D")
                Console.WriteLine("Colour = " + primary.colour);
            Console.WriteLine("Mass = " + primary.mass);
            Console.WriteLine("Temperture = " + primary.temperture);
            Console.WriteLine("Diameter = " + primary.diameter);
            Console.WriteLine("Luminosity = " + primary.luminosity);
            Console.WriteLine("Age = " + primary.age);
        }

        
        public Star primary { get; set; }
    }
}
