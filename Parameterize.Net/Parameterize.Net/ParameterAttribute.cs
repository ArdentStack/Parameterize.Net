using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Parameterize
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ParameterAttribute : System.Attribute
    {
        ParameterConstraintType type;
        Constraint def;
        public ParameterAttribute()
        {
            this.type = ParameterConstraintType.None;
        }
        public ParameterAttribute(float min,float max,short percision)
        {
            this.type = ParameterConstraintType.MinMax;
            this.def = new Constraint(min, max, percision);
        }
        public ParameterAttribute(int min, int max)
        {
            this.type = ParameterConstraintType.MinMax;
            this.def = new Constraint(min, max);
        }
        public ParameterConstraintType Type { get => type; set => type = value; }
       public Constraint DefaultConstraint { get => def; set => def = value; }
    }
    
   
    
}
