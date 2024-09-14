using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravellerSystemGenerator
{
    

    internal class CelestrialObject
    {

        public float orbit { get; set; }
        public float orbitEccentricity { get; set; }
        public Object celestrialObject { get; set; }
        public float orbitAU {  get; set; }
        public float orbitMinSep {  get; set; }
        public float orbitMaxSep { get; set; }

        private float[,] starMAO =
            {
                {0.63F,0.55F, 0.5F, 1.67F, 3.34F, 4.17F, 4.42F, 5, 5.21F, 5.34F, 5.59F,6.17F, 6.8F, 7.2F, 7.8F },
                {0.6F, 0.5F, 0.35F, 0.63F, 1.4F, 2.17F, 2.5F, 3.25F, 3.59F, 3.84F, 4.17F, 4.84F, 5.42F, 6.17F, 6.59F},
                {0.55F, 0.45F, 0.3F, 0.35F, 0.75F, 1.17F, 1.33F, 1.87F, 2.24F, 2.67F, 3.17F, 4, 4.59F, 5.3F, 5.92F },
                {0.53F, 0.38F, 0.25F, 0.15F, 0.13F, 0.13F, 0.13F, 0.13F, 0.25F, 0.38F, 0.5F, 1, 1.68F, 3, 4.34F },
                {0,0, 0.2F, 0.13F, 0.1F, 0.07F, 0.07F, 0.06F, 0.07F, 0.1F, 0.15F, 0,0,0,0 },
                {0.5F, 0.3F, 0.18F, 0.09F, 0.06F, 0.05F, 0.04F, 0.03F, 0.03F, 0.02F, 0.02F, 0.02F, 0.02F , 0.01F, 0.01F },
                {0.01F, 0.01F, 0.01F, 0.01F, 0, 0, 0, 0, 0.02F, 0.02F, 0.02F, 0.01F, 0.01F, 0.01F, 0.01F }
            };

        private float[,] EccValues =
        {
            {5, 7, 9, 10, 11, 12 },
            {-0.001F, 0, 0.03F, 0.05F, 0.05F, 0.3F },
            {1, 1, 1, 1, 2,1 },
            {1000, 200, 100, 20, 20, 20 }
        };

        

        public List<CelestrialObject> celestrialObjectOrbits { get; set; }

        public CelestrialObject()
        {
            celestrialObjectOrbits = new List<CelestrialObject>();
        }

        public void OrbitEccentricity(int starsOrbited, Random dice, bool belt = false)
        {
            int dm = 0;
            if (this.celestrialObject is Star)
                dm = dm + 2;
            dm = dm + 1 * (starsOrbited - 1);
            if(Starhelper.systemAge > 1.0 && this.orbit <1.0)
                dm = dm - 1;
            if(belt == true)
                dm = dm + 1;
            int roll1 = Starhelper.diceRoll(6, 2, dice) + dm;

            if(roll1 > 12)
                roll1 = 12;
            
            int x = 0;
            bool check = false;

            while(check == false)
            {
                if ((int)EccValues[0, x] >= roll1)
                {
                    check = true;
                }
                else
                    x++;
            }
            float eccBase = EccValues[1, x];
            float e = eccBase + (Starhelper.diceRoll(6, (int)EccValues[2, x], dice) / EccValues[3, x]);
            this.orbitEccentricity = eccBase + (Starhelper.diceRoll(6, (int)EccValues[2, x], dice)/ EccValues[3, x]);
        }

        public void AddStar(float orbit, Starhelper.starOrbitType starOrbitType, Random dice)
        {
            CelestrialObject Cobj = new CelestrialObject();
            Cobj.orbit = orbit;
            Cobj.OrbitEccentricity(0, dice, false);
            Cobj.orbitAU = Cobj.OrbitAU(orbit);
            Cobj.orbitMinSep = Cobj.orbitAU * (1 - Cobj.orbitEccentricity);
            Cobj.orbitMaxSep = Cobj.orbitAU * (1 + Cobj.orbitEccentricity);
            Star NewStar = new Star(orbit, starOrbitType, dice);
            Cobj.celestrialObject = NewStar;
            celestrialObjectOrbits.Add(Cobj);
        }

        public float OrbitAU(float orbit)
        {
            int wholeOrbitNum = (int)Math.Truncate(orbit);
            float orbitFraction = orbit - (float)Math.Truncate(orbit);
            int f = (int)Math.Truncate(orbitFraction * 10);
            return Extrapolate(Starhelper.orbitValues[wholeOrbitNum], Starhelper.orbitValues[wholeOrbitNum + 1], f);
        }

        private float Extrapolate(float lnumber, float unumber, int factor)
        {
            float per = (float)factor / 10;
            return lnumber + (per * (unumber - lnumber));
        }
    }
}
