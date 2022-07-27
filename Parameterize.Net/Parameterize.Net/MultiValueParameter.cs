
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Parameterize
{
    /// <summary>
    /// Work in progress :)
    /// </summary>
    public class MultiValueParameter:ModelParamerter
    {
        public MultiValueParameter(int id, bool isLocked, ParameterDescriptor descriptor) : base(id, isLocked, descriptor)
        {

        }

        public MultiValueParameter(int id, bool isLocked, ParameterDescriptor descriptor, Constraint c) : base(id, isLocked, descriptor, c)
        {

        }
        public MultiValueParameter(int id, bool isLocked, Constraint constraint) : base(id, isLocked, constraint)
        {
        }
    }
   
}
 