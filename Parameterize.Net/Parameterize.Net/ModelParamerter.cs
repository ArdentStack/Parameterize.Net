
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Parameterize
{
    /// <summary>
    /// A parameter in an object that's being constructed
    /// </summary>
    public class ModelParamerter
    {
        int id;
        bool isLocked;
        object lockedValue;
        ParameterDescriptor descriptor;
        Constraint constraint;
        /// <summary>
        /// The parameter's id (also its index in the parameter array)
        /// </summary>
        public int Id { get => id; set => id = value; }
        /// <summary>
        /// Is the value of this parameter locked
        /// </summary>
        public bool IsLocked { get => isLocked; set => isLocked = value; }
        /// <summary>
        /// The descriptor of the parameter 
        /// </summary>
        public ParameterDescriptor Descriptor { get => descriptor; set => descriptor = value; }
        /// <summary>
        /// The value this paramter is set to (and locked to)
        /// </summary>
        public object LockedValue { get => getLockedValue(); }
        /// <summary>
        /// The constraint governing this parameter
        /// </summary>
        public Constraint Constraint { get => constraint; set => constraint = value; }
        
        public ModelParamerter(int id, bool isLocked, ParameterDescriptor descriptor)
        {
            this.id = id;
            this.isLocked = isLocked;
            this.descriptor = descriptor;
            // if the parameter is non numeric and is a single object/value then there's no constraint
            if (descriptor.Type != ParameterType.PARAMETERPACK && descriptor.Type != ParameterType.STRING)
            {
                constraint = descriptor.GetConstraint();
            }
        }


        public ModelParamerter(int id, bool isLocked, ParameterDescriptor descriptor, Constraint c)
        {
            this.id = id;
            this.isLocked = isLocked;
            this.descriptor = descriptor;
            constraint = c;
        }
        public ModelParamerter(int id, bool isLocked, Constraint constraint)
        {
            this.id = id;
            this.isLocked = isLocked;
            this.constraint = constraint;
        }
        /// <summary>
        /// Lock this parameter to a value
        /// </summary>
        /// <param name="value"></param>
        public void Lock(object value)
        {
            isLocked = true;
            lockedValue = value;
        }
        protected object getLockedValue()
        {
            if (!isLocked)
            {
                throw new Exception();
            }
            return lockedValue;
        }
    }
   
}
 