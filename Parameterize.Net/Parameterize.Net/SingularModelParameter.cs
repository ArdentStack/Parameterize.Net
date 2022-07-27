
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Parameterize
{
    /// <summary>
    /// A parameter that has a single value
    /// </summary>
    public class SingularModelParameter: ModelParamerter
    {
        public SingularModelParameter(int id, bool isLocked, ParameterDescriptor descriptor) : base(id, isLocked, descriptor)
        {
            
        }

        public SingularModelParameter(int id, bool isLocked, ParameterDescriptor descriptor, Constraint c):base(id,isLocked,descriptor,c)
        {
           
        }
        public SingularModelParameter(int id, bool isLocked, Constraint constraint) : base(id, isLocked, constraint)
        {
        }




    }
   
}
 