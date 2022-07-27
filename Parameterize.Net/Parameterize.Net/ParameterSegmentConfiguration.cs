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
    public class ParameterSegmentConfiguration :DynamicObject
    {

        ParameterSegment segment;
        public Constraint CountConstraint { get; set; }
        public Dictionary<string, ParameterSegmentConfiguration> ChildrenConfigs { get => childrenConfigs; set => childrenConfigs = value; }
        public Dictionary<string, Constraint> Constraints { get => constraints; set => constraints = value; }

        Dictionary<string, Constraint> constraints;
        Dictionary<string, ParameterSegmentConfiguration> childrenConfigs=new Dictionary<string, ParameterSegmentConfiguration>();
        public ParameterSegmentConfiguration(ParameterSegment segment)
        {
            constraints = new Dictionary<string, Constraint>();
            this.segment = segment;
            foreach(var i in segment.GetAllParameters())
            {
                if (i == segment.SelectorParameter || i.Descriptor == null)
                {
                    continue;
                }
                
                if (!constraints.ContainsKey(i.Descriptor.Name))
                {
                    constraints.Add(i.Descriptor.Name, i.Constraint);
                }
            }
            foreach(var i in segment.GetAllChildren())
            {
                if (!childrenConfigs.ContainsKey(i.Descriptor.Name))
                {
                    
                    childrenConfigs.Add(i.Descriptor.Name,new ParameterSegmentConfiguration(i));
                    if (i.Descriptor != null && i.Descriptor.Type == ParameterType.PARAMETERIZEDES)
                    {
                        childrenConfigs[i.Descriptor.Name].CountConstraint = i.Descriptor.GetConstraint();
                    }
                }
            }
        }
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            string[] ret = new string[constraints.Count + childrenConfigs.Count];
            var n = 0;
            foreach(var i in constraints.Keys)
            {
                ret[n++] = i;
            }
            foreach(var i in childrenConfigs.Keys)
            {
                ret[n++] = i;
            }
            return ret;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (constraints.ContainsKey(binder.Name))
            {
                result = constraints[binder.Name];
                return true;
            }else if (childrenConfigs.ContainsKey(binder.Name))
            {
                
                result= childrenConfigs[binder.Name];
                return true;
            }
            result = null;
            return false;
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (constraints.ContainsKey(binder.Name))
            {
                constraints[binder.Name] =(Constraint) value ;
                return true;
            }
            else if (childrenConfigs.ContainsKey(binder.Name))
            {

                childrenConfigs[binder.Name].CountConstraint = (Constraint)value; ;
                return true;
            }
            return false;
        }
        
    }
    
   
    
}
