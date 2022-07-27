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
    /// <summary>
    /// Describes a class parameter
    /// </summary>
    public class ParameterDescriptor
    {
        ParameterType type;
        string name;
        ParameterAttribute paramt;
        Type subtype;
        public ParameterDescriptor(string name,ParameterType type,ParameterAttribute paramt)
        {
            this.type = type;
            this.name = name;
            this.paramt = paramt;
        }
        
        public ParameterDescriptor(string name, ParameterType type,Type subtype, ParameterAttribute paramt)
        {
            this.type = type;
            this.name = name;
            this.subtype = subtype;
            this.paramt = paramt;
        }
      


        public override bool Equals(object obj)
        {
            if(obj is ParameterDescriptor d)
            {

                return type == d.type && name == d.name && subtype == d.subtype ;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return (type.ToString() + ":" + subtype.Name).GetHashCode();
        }
        public ParameterType Type { get => type; set => type = value; }
        public string Name { get => name; set => name = value; }
        public Type Subtype { get => subtype; set => subtype = value; }
        public ParameterAttribute Paramt { get => paramt; set => paramt = value; }

        public Constraint GetConstraint()
        {
            
                return paramt.DefaultConstraint;
          
        }
        
    }
    
   
    
}
