    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parameterize
{

    /// <summary>
    /// A constraint for numerical values, indicates min, max, percision (how many decimal places, 0 is int) 
    /// </summary>
    public struct Constraint
    {
        static Random baseRng = new Random();
        short percision;
        float minVal, maxVal;
        static Constraint any = new Constraint();
        /// <summary>
        /// Construct a constraint
        /// </summary>
        /// <param name="minVal">the min val (inclusive)</param>
        /// <param name="maxVal">the max val (inclusive)</param>
        /// <param name="percision">how many decimal places (0 is Int)</param>
        public Constraint(float minVal, float maxVal, short percision)
        {
            this.percision = percision;
            this.minVal = minVal;
            this.maxVal = maxVal;
        }
        /// <summary>
        /// Init an integer constraint
        /// </summary>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        public Constraint(int minVal,int maxVal)
        {
            this.minVal = minVal;
            this.maxVal = maxVal;
            percision = 0;
        }
        /// <summary>
        /// How many decimal places (0 int)
        /// </summary>
        public short Percision { get => percision; }
        /// <summary>
        /// The min value the cosntrained value can take
        /// </summary>
        public float MinVal { get => minVal; }
        /// <summary>
        /// The max value the constrained value can take
        /// </summary>
        public float MaxVal { get => maxVal; }
        /// <summary>
        /// Any value
        /// </summary>
        public static Constraint Any { get => any; }
        /// <summary>
        /// Returns a random value that's within the constraint
        /// </summary>
        /// <param name="rng"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Clip a value to meet the constraint
        /// </summary>
        /// <param name="x">the value</param>
        /// <param name="round">round the value to the specified percision?</param>
        /// <returns></returns>
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
        /// <summary>
        /// Get the minval+ ((i/total)*(maxval-minval)
        /// </summary>
        /// <param name="i">part out total</param>
        /// <param name="total"></param>
        /// <returns></returns>
        public float GetIthValue(float i, float total)
        {
            if (i < 0 || i > total)
            {
                throw new IndexOutOfRangeException();
            }
           
            return Clip(MinVal + (float)Math.Round(((maxVal - MinVal) * (((float)i) / ((float)total))), percision));
        }
        /// <summary>
        /// Compare to constraints
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            
            if (obj is Constraint c)
            {
               
                return c.minVal == minVal && c.maxVal == maxVal && c.percision == percision;
            }
            return false;
        }
        /// <summary>
        /// Are these two constraints the same
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Constraint a, Constraint b){
            return a.Equals(b);
        }
        /// <summary>
        /// !=
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Constraint a, Constraint b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return (minVal.ToString() + ":" + maxVal.ToString() + ":" + percision.ToString()).GetHashCode();
        }
        /// <summary>
        /// Pretty constraint represntation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[" + minVal.ToString() + "-" + maxVal.ToString() + "] % " + percision.ToString();
        }
        /// <summary>
        /// Returns a random float array using an array of constraints
        /// </summary>
        /// <param name="rng"></param>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public static float[] GetRandom(Constraint[] constraints,Random rng=null)
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
