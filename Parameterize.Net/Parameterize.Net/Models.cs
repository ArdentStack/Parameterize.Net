
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Parameterize
{
    public enum ModelParameterType
    {
        Categorizer, FunctionParameter, NumericalParameter
    }
    public class ModelParamerter
    {
        int id;
        bool isLocked;
        object lockedValue;
        ParameterDescriptor descriptor;
        Constraint constraint;
        public int Id { get => id; set => id = value; }
        public bool IsLocked { get => isLocked; set => isLocked = value; }
        public ParameterDescriptor Descriptor { get => descriptor; set => descriptor = value; }
        public object LockedValue { get => getLockedValue(); }
        public Constraint Constraint { get => constraint; set => constraint = value; }
        public ModelParamerter(int id, bool isLocked, ParameterDescriptor descriptor)
        {
            this.id = id;
            this.isLocked = isLocked;
            this.descriptor = descriptor;
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



    public class ParameterSegment
    {

        int length;
        bool canBeDisabled = false;
        Type baseType;
        ParameterSegment parent;
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
    public class Parameterizer
    {
        static HashSet<Type> parameterizedTypes;
        static Dictionary<Type, HashSet<Type>> possibleTypes = new Dictionary<Type, HashSet<Type>>();
        public static List<Type> GetPossibleTypesFor(Type t)
        {
            initTypes();
            if (possibleTypes.Keys.Contains(t))
            {
                return possibleTypes[t].ToList();
            }
            else
            {
                HashSet<Type> stypes = new HashSet<Type>();
                
                foreach( var i in parameterizedTypes)
                {
                    if (t.IsAssignableFrom(i)&& !i.IsAbstract)
                    {
                        stypes.Add(i);
                    }
                }
                possibleTypes[t] = stypes;
                return stypes.ToList();
            }
        }
        public static List<Type> GetAllComponents(Type t)
        {

            initTypes();
            return parameterizedTypes.ToList();

        }

        static void initTypes()
        {
            if (parameterizedTypes == null)
            {

                parameterizedTypes = new HashSet<Type>();
                foreach (var i in AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes()).Where(t => t.IsClass))
                {
                    if (Attribute.GetCustomAttributes(i,typeof(ParameterizedAttribute)).Length>0)
                    {
                        
                        parameterizedTypes.Add(i);

                    }
                    
                }

            }
        }
        
        public static T Create<T>(float[] param, params object[] args)
        {
            if(args.Length>0&&args[0] is ParameterSegmentConfiguration aa)
            {
                var nar = new object[args.Length - 1];
                if (args.Length > 1) {
                    Array.Copy(args, 1, nar, 0, args.Length - 1);
                }
                return CreateC<T>(param, aa,nar);
            }
            return CreateC<T>(param, null, args);
        }
        /// <summary>
        /// Create an object of type T with the set parameters
        /// </summary>
        /// <typeparam name="T">The type of the object to be created</typeparam>
        /// <param name="param">Array of parameters matching the object type constraint</param>
        /// <param name="config">Constraint configuration</param>
        /// <param name="args">Creation arugments for defined init function</param>
        /// <returns></returns>
        static T CreateC<T>(float[] param,ParameterSegmentConfiguration config,object [] args)
        {
            var cons = GetConstraints<T>(config);
            if (param.Length != cons.Length)
            {
                throw new Exception($"invalid param length, expected float[{cons.Length}] got float[{param.Length}]");
            }
            // List of float paramaters encoding the object
            var workingparam = new float[param.Length];
            //Copy from arugments
            Array.Copy(param, 0, workingparam, 0, param.Length);
            //Create parameter pack dictionary
            Dictionary<int, ParameterPack> packs = new Dictionary<int, ParameterPack>();
            //Create parameter segment for root type
            var rootSegment = new ParameterSegment(typeof(T), null, false,null,config);
            //??????
            rootSegment.SelectorParameter.IsLocked = false;
            //Get all configurable/parameterized segments in the root semgent
            var segments = rootSegment.GetAllChildren();
            //Add root segment to the list of all segments
            segments.Add(rootSegment);
            //Get all root segment parameters
            var allparams = rootSegment.GetAllParameters();
            //Check for locked values
            for(int iu = 0;iu<allparams.Count;iu++)
            {
                var i = allparams[iu];

                if (i.Id < param.Length && i.IsLocked)
                {
                    if (i.LockedValue is int)
                    {
                        workingparam[i.Id] = (int)i.LockedValue;

                    }
                    else if (i.LockedValue is float)
                    {
                        workingparam[i.Id] = (float)i.LockedValue;

                    }

                }
            }
            //create pack for each segment (child & child of child etc...)
            foreach (var i in segments)
            {

                //Don't Create if it is selectable and the value is 0
                if (i.CanBeDisabled && i.SelectorParameter.Constraint.Clip(workingparam[i.SelectorParameter.Id]) == 0)
                {
                    continue;
                }
                //Create
                else
                {

                    //Get type
                    var type = i.GetSelectorType((int)i.SelectorParameter.Constraint.Clip(workingparam[i.SelectorParameter.Id]));
                    //Get parameter pack for the object
                    var pack = ParameterPack.CreatePackFor(type);
                    //set each parameter of the pack
                    foreach (var j in i.Parameters)
                    {
                       
                        
                        if (pack.ContainsKey(j.Key))
                        {

                            
                            if (j.Value.IsLocked)
                            {

                                pack.Set(j.Key, j.Value.LockedValue);
                            }
                            else
                            {
                                //set from working params
                                pack.Set(j.Key, j.Value.Constraint.Clip(workingparam[j.Value.Id]));

                            }
                        }
                    }
                    //add to finished packs
                    packs.Add(i.SelectorParameter.Id, pack);
                }
            }
            //Go through all segments
            foreach (var i in segments)
            {
                //if this segment is set
                if (packs.ContainsKey(i.SelectorParameter.Id))
                {
                    //go through the segments children
                    foreach (var j in i.Children)
                    {
                        if (!packs.ContainsKey(j.SelectorParameter.Id))
                        {
                            continue;
                        }
                        if (j.Descriptor == null)
                        {
                            var name = "";
                            bool ismany = false;
                            foreach (var x in packs[i.SelectorParameter.Id].Descriptor.Descriptors)
                            {
                                if (x.Subtype.IsAssignableFrom(j.BaseType))
                                {
                                    name = x.Name;
                                    if (x.Type == ParameterType.PARAMETERIZEDES)
                                    {
                                        ismany = true;
                                    }
                                }
                            }
                            if (ismany)
                            {
                                packs[i.SelectorParameter.Id].AddSubcomponent(name, packs[j.SelectorParameter.Id]);
                            }
                            else
                            {
                                packs[i.SelectorParameter.Id].Set(name, packs[j.SelectorParameter.Id]);
                            }

                        }
                        else
                        {
                            if (packs[i.SelectorParameter.Id].ContainsKey(j.Descriptor.Name))
                            {
                                if (j.Descriptor.Type == ParameterType.PARAMETERIZEDES)
                                {
                                    packs[i.SelectorParameter.Id].AddSubcomponent(j.Descriptor.Name, packs[j.SelectorParameter.Id]);
                                }
                                else
                                {
                                    packs[i.SelectorParameter.Id].Set(j.Descriptor.Name, packs[j.SelectorParameter.Id]);
                                }
                            }
                        }
                    }
                }
            }
            return (T)packs[0].Create(args);
        }
        public static ParameterSegmentConfiguration GetConfigFor(Type t)
        {
            var seg = new ParameterSegment(t, null, false);
            return new ParameterSegmentConfiguration(seg);
        }
        public static Constraint[] GetConstraints<T>(ParameterSegmentConfiguration config=null)
        {
            var rootSegment = new ParameterSegment(typeof(T), null, false,null,config);
            var all = rootSegment.GetAllParameters();
            
            var constraints = new Constraint[all.Count];
         
            foreach (var i in all)
            {   constraints[i.Id] = i.Constraint;
            }
            return constraints;
        }
    }
   
}
 