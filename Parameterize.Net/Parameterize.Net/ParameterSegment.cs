
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Parameterize
{
    /// <summary>
    /// Contains the parameters of a single object along with its children (objects it contains)
    /// </summary>
    public class ParameterSegment
    {
        /// <summary>
        /// Is this parameter optional (used in lists of parameterized items
        /// </summary>
        bool canBeDisabled = false;
        /// <summary>
        /// The object's type
        /// </summary>
        Type baseType;
        /// <summary>
        /// The segment that contains this
        /// </summary>
        ParameterSegment parent;
        /// <summary>
        /// The segments of object that exist inside the paramterized object
        /// </summary>
        List<ParameterSegment> children;

        Dictionary<string, ModelParamerter> parameters;
        SingularModelParameter selectorParameter;
        ParameterDescriptor descriptor;
        ParameterSegmentConfiguration config;
        int ids;
        public ParameterSegment(Type baseType, ParameterSegment parent, bool canBeDisabled, ParameterDescriptor descriptor,ParameterSegmentConfiguration config)
        {
            this.config = config; 
            this.parent = parent;
            this.baseType = baseType;
            this.canBeDisabled = canBeDisabled;
            children = new List<ParameterSegment>();
            parameters = new Dictionary<string, ModelParamerter>();
            this.descriptor = descriptor;
            var param = GetAllDescirptorsForType(baseType);
            selectorParameter = new SingularModelParameter(getId(), false, new Constraint(0, getPossibleSelectorTypes(false).Count - (canBeDisabled ? 0 : 1), 0));

            foreach (var i in param)
            {
                

                if (i.Type == ParameterType.PARAMETERIZEDES)
                {
                    var cons = GetConstraint(i);
                    for (int j = 0; j < cons.MaxVal; j++)
                    {
                        bool can = false;
                        if (j >= cons.MinVal)
                        {
                            can = true;
                        }

                        children.Add(new ParameterSegment(i.Subtype, this, can, i,config!=null?config.ChildrenConfigs[i.Name]:null));


                    }
                }
                else if (i.Type == ParameterType.PARAMETERPACK)
                {


                    children.Add(new ParameterSegment(i.Subtype, this, false, i, config != null ? config.ChildrenConfigs[i.Name] : null));

                }else if(i.Type == ParameterType.ARRAY)
                {
                    if (parameters.ContainsKey(i.Name))
                    {
                        parameters[i.Name + "/"] = new MultiValueParameter(getId(), false, i, GetConstraint(i));
                    }
                    else
                    {
                        parameters[i.Name] = new MultiValueParameter(getId(), false, i, GetConstraint(i));
                    }
                    
                }
                else
                {
                    if (parameters.ContainsKey(i.Name))
                    {
                        parameters[i.Name+"/"] = new SingularModelParameter(getId(), false, i, GetConstraint(i));
                    }
                    else
                    {
                        parameters[i.Name] = new SingularModelParameter(getId(), false, i, GetConstraint(i));
                    }
                }

            }
        }
        public ParameterSegment(Type baseType, ParameterSegment parent, bool canBeDisabled, ParameterDescriptor descriptor) : this(baseType, parent, canBeDisabled, descriptor, null)
        {

           

        }
        public ParameterSegment(Type baseType, ParameterSegment parent, bool canBeDisabled) : this(baseType, parent, canBeDisabled, null)
        {

        }
        Constraint GetConstraint(ParameterDescriptor d)
        {
            if (config != null)
            {
                if (config.Constraints.ContainsKey(d.Name))
                {
                    return config.Constraints[d.Name];
                }
                if (config.ChildrenConfigs.ContainsKey(d.Name))
                {
                    return config.ChildrenConfigs[d.Name].CountConstraint;
                }
            }
            return d.GetConstraint();
            
        }
        int getId()
        {
            if (parent == null)
            {
                var ret = ids;
                ids += 1;
                return ret;
            }
            else
            {
                return parent.getId();
            }        
        }
        public static List<ParameterDescriptor> GetAllDescirptorsForType(Type t)
        {

            var ret = new List<ParameterDescriptor>();
            var all = Parameterizer.GetAllComponents(t);

            foreach (var i in all)
            {
                if (!i.IsSubclassOf(t)&& i!=t)
                {
                    continue;
                }
                var d = ParameterPackDescriptor.GetParameterPackDescriptor(i);
                foreach (var ds in d.Descriptors)
                {

                    if (!ret.Exists((a) => a.Equals(ds)))
                    {
                        ret.Add(ds);
                    }
                    else if (ret.Exists((a) => a.Name == ds.Name))
                    {

                    }

                }
            }

            return ret;
        }
        
        public static string PrettyStringSegment(ParameterSegment s, int space)
        {
            StringBuilder b = new StringBuilder();
            string spacestr = "";
            for (int i = 0; i < space; i++)
            {
                spacestr += " ";
            }
            b.Append(spacestr);
            b.Append(s.SelectorParameter.Id);
            b.Append(s.SelectorParameter.Constraint.ToString());
            b.Append(" : ");
            b.Append(s.BaseType.Name);
            b.Append("\n");
            foreach (var i in s.Parameters.Values)
            {
                b.Append(' ', space * 2);
                b.Append(i.Id);
                b.Append(i.Constraint);
                b.Append(" : ");
                if (i.Descriptor != null)
                {
                    b.Append(i.Descriptor.Name);
                }

                b.Append("\n");
            }
            foreach (var i in s.Children)
            {
                b.Append(PrettyStringSegment(i, space + 3));
            }
            return b.ToString();
        }
        public List<ParameterSegment> GetAllChildren()
        {
            var ret = new List<ParameterSegment>();
            Stack<ParameterSegment> explore = new Stack<ParameterSegment>(children);
            while (explore.Count > 0)
            {
                var current = explore.Pop();
                if (!ret.Contains(current))
                {
                    ret.Add(current);
                    foreach (var i in current.children)
                    {
                        explore.Push(i);
                    }
                }
            }
            return ret;
        }
        public int CalculateTotalLength()
        {
            return GetAllParameters().Count;
        }
        public Type GetSelectorType(int i)
        {
            if (CanBeDisabled)
            {
                i -= 1;
            }
            var pos = getPossibleSelectorTypes(true);
            return pos[i];
        }
        List<Type> getPossibleSelectorTypes(bool getSpecial)
        {
     
            var param = Parameterizer.GetPossibleTypesFor(baseType);
            var ret = new List<Type>();
            foreach (var i in param)
            {
                
                    ret.Add(i);
                
            }
            return ret;
        }
        public List<ModelParamerter> GetAllParameters()
        {
            var ret = new List<ModelParamerter>();
            ret.Add(selectorParameter);
            ret.AddRange(parameters.Values);
            foreach (var i in children)
            {
                ret.AddRange(i.GetAllParameters());
            }
            return ret;
        }

        public List<ParameterSegment> Children { get => children; set => children = value; }
        public Dictionary<string, ModelParamerter> Parameters { get => parameters; set => parameters = value; }
        public Type BaseType { get => baseType; set => baseType = value; }
        public SingularModelParameter SelectorParameter { get => selectorParameter; set => selectorParameter = value; }
        public bool CanBeDisabled { get => canBeDisabled; set => canBeDisabled = value; }
        public ParameterDescriptor Descriptor { get => descriptor; set => descriptor = value; }
   
    }
   
}
 