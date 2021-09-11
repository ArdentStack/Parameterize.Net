    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parameterize
{

    public struct Constraint
    {
        static Random baseRng = new Random();
        short percision;
        float minVal, maxVal;
        static Constraint any = new Constraint();
        
        public Constraint(float minVal, float maxVal, short percision)
        {
            this.percision = percision;
            this.minVal = minVal;
            this.maxVal = maxVal;
        }
        public short Percision { get => percision; }
        public float MinVal { get => minVal; }
        public float MaxVal { get => maxVal; }
        public static Constraint Any { get => any; }

        public float Random(Random rng = null)
        {
            if (rng == null)
            {
                rng = baseRng;
            }
            if (MinVal == maxVal)
            {
                return minVal;
            }
            if (minVal > maxVal)
            {
                return minVal;
            }
            if(percision == 0)
            {
                return (int)rng.Next((int)minVal, (int)maxVal+1);
            }
            return (float)Math.Round(minVal + (((rng.NextDouble()) * (maxVal - MinVal))), percision);
        }
        public float Clip(float x,bool round =true)
        {
            
            if (minVal == maxVal || maxVal<minVal)
            {
                return minVal;
            }
            
            if (x > maxVal)
            {
                return maxVal;
            }
            else if (x < minVal)
            {
                return minVal;
            }
            else
            {
                return round?(float)Math.Round(x, percision):x;
            }
        }
        public float GetIthValue(float i, float total)
        {
            if (i < 0 || i > total)
            {
                throw new IndexOutOfRangeException();
            }
           
            return Clip(MinVal + (float)Math.Round(((maxVal - MinVal) * (((float)i) / ((float)total))), percision));
        }
        public override bool Equals(object obj)
        {
            
            if (obj is Constraint c)
            {
               
                return c.minVal == minVal && c.maxVal == maxVal && c.percision == percision;
            }
            return false;
        }
        public static bool operator ==(Constraint a, Constraint b){
            return a.Equals(b);
        }
        public static bool operator !=(Constraint a, Constraint b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return (minVal.ToString() + ":" + maxVal.ToString() + ":" + percision.ToString()).GetHashCode();
        }
        public override string ToString()
        {
            return "[" + minVal.ToString() + "-" + maxVal.ToString() + "] % " + percision.ToString();
        }
        public static float[] GetRandom(Random rng, Constraint[] constraints)
        {
            var ret = new float[constraints.Length];
            for(int i = 0; i < ret.Length; i++)
            {
                ret[i] = constraints[i].Random(rng);
            }
            return ret;
        }
    }
}
