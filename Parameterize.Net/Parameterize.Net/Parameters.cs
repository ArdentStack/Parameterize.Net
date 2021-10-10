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
    public enum ParameterConstraintType
    {
        None,MinMax
    }

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
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ParameterizedAttribute:System.Attribute
    {

    }
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class OnInitFunctionAttribute : System.Attribute
    {

    }
    public enum ParameterType
    {
        FLOAT,INT,PARAMETERPACK,STRING,PARAMETERIZEDES,ARRAY
    }
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
    /// <summary>
    /// Describes the parameters for a whole class
    /// </summary>
    public class ParameterPackDescriptor
    {
        Type type;
        List<ParameterDescriptor> descriptors;
        public ParameterPackDescriptor() { }
        public ParameterPackDescriptor(Type t,List<ParameterDescriptor> descriptors)
        {
            this.type = t;
            this.descriptors = descriptors;
        }

        public Type Type { get => type; set => type = value; }
        public List<ParameterDescriptor> Descriptors { get => descriptors; set => descriptors = value; }

        public static bool IsParameterized(Type t)
        {
            return Attribute.GetCustomAttributes(t, typeof(ParameterizedAttribute)).Length>0;
        }

        public static ParameterPackDescriptor GetParameterPackDescriptor(Type t)
        {
            
            var reto = new ParameterPackDescriptor();
            var ret = new List<ParameterDescriptor>();
            var p = t.GetProperties().ToList();
            p.Sort((a, b) => a.Name.CompareTo(b.Name));

            foreach (var i in p)
            {
                
                foreach (var j in Attribute.GetCustomAttributes(i, typeof(ParameterAttribute)))
                {
                    if (j is ParameterAttribute paramt)
                    {
                        ParameterType ptp;
                        if (i.PropertyType == typeof(float))
                        {
                           
                            ret.Add(new ParameterDescriptor(i.Name, ParameterType.FLOAT, paramt));

                        }
                        else if (i.PropertyType == typeof(int))
                        {
                            
                            ret.Add(new ParameterDescriptor(i.Name, ParameterType.INT, paramt));
                        }
                        else if (i.PropertyType == typeof(string))
                        {
                            
                            ret.Add(new ParameterDescriptor(i.Name, ParameterType.STRING,paramt));
                        }
                        
                        else if (i.PropertyType.IsGenericType && i.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) && IsParameterized(i.PropertyType.GenericTypeArguments[0]))
                        {
                           
                            ret.Add(new ParameterDescriptor(i.Name, ParameterType.PARAMETERIZEDES, i.PropertyType.GenericTypeArguments[0], paramt));
                        }
                        else if (IsParameterized(i.PropertyType))
                        {
                            

                            ret.Add(new ParameterDescriptor(i.Name, ParameterType.PARAMETERPACK, i.PropertyType,paramt));
                        }
                        else if (i.PropertyType.IsArray&&i.MemberType.GetType().Equals(typeof(float)))
                        {
                            ret.Add(new ParameterDescriptor(i.Name, ParameterType.ARRAY, i.PropertyType, paramt));
                        }
                        else
                        {
                            throw new Exception();
                        }



                    }
                    else
                    {
                        
                    }

                }

            }
            
           


            reto.type = t;
            reto.descriptors = ret;
            return reto;
        }
        public ParameterDescriptor Get(string name)
        {
            foreach(var i in descriptors)
            {
                if (i.Name == name)
                {
                    return i;
                }
            }
            throw new Exception("Descriptor not found for: " + name);
        }
    }
    /// <summary>
    /// An object that stores the parameters of an object that's being constructed
    /// </summary>
    public class ParameterPack
    {
        Type type;
        ParameterPackDescriptor descriptor;
        Dictionary<string, object> values;

        public Dictionary<string, object> Values { get => values; set => values = value; }
        public ParameterPackDescriptor Descriptor { get => descriptor; set => descriptor = value; }

        public ParameterPack(ParameterPackDescriptor descriptor)
        {
            this.type = descriptor.Type;
            values = new Dictionary<string, object>();
            this.descriptor = descriptor;
            foreach(var i in descriptor.Descriptors)
            {
                switch (i.Type)
                {
                    case (ParameterType.FLOAT):
                        break;
                    case (ParameterType.INT):
                        break;
                    case (ParameterType.STRING):
                        break;
                    case (ParameterType.PARAMETERIZEDES):
                        values.Add(i.Name, new List<ParameterPack>());
                            break;
                    case (ParameterType.PARAMETERPACK):
                        break;
                   
                }
            }
        }
        
        public void AddSubcomponent(string parameterName, ParameterPack p)
        {
            if (!values.ContainsKey(parameterName))
            {
                throw new Exception("attempted to add subcomponent to non-existant subspace(list): " + parameterName);
            }
            ((List<ParameterPack>)values[parameterName]).Add(p);
        }
        public bool ContainsParam(string name)
        {
            return values.ContainsKey(name);
        }
        public bool ContainsKey(string name)
        {
            return Descriptor.Descriptors.Exists((a) => a.Name == name);
        }
        public void Set(string name,object value)
        {
           
            if (descriptor.Descriptors.Exists((a) => a.Name == name))
            {
                var d = descriptor.Descriptors.Where((a) => a.Name == name).FirstOrDefault();
                //if(value.GetType().IsSubclassOf(d.Subtype))
                if (values.ContainsKey(name))
                {
                    values[name] = value;
                    
                }
                else
                {
                    values.Add(name, value);
                }
            }
            
            else
            {
                throw new Exception("Attempted to add incorrect parameter: " + name);
            }
        }
        public T Get<T>(string name)
        {
 
            return (T)values[name];
        }
        public object Create(params object[] args) 
        {
            var ret =  Activator.CreateInstance(type);
            fill(ret);
            foreach(var i in ret.GetType().GetMethods())
            {
                if (Attribute.GetCustomAttributes(i, typeof(OnInitFunctionAttribute)).Length > 0&&i.GetParameters().Length==args.Length)
                {
                    i.Invoke(ret, args) ;
                }
            }
            return ret;
        }
        public static ParameterPack CreatePackFor(Type t)
        {
            var p = ParameterPackDescriptor.GetParameterPackDescriptor(t);
            return new ParameterPack(p);
        }
        IList createList(Type myType)
        {
            Type genericListType = typeof(List<>).MakeGenericType(myType);
            return (IList)Activator.CreateInstance(genericListType);
        }
        object get(object item, string name)
        {
            Type myType = item.GetType();
            PropertyInfo myPropInfo = myType.GetProperty(name);
            return myPropInfo.GetValue(item, null);
        }
        void set(object item,string name,object value)
        {
            Type myType = item.GetType();
            PropertyInfo myPropInfo = myType.GetProperty(name);
            myPropInfo.SetValue(item, value, null);
        }
        void fill(object item)
        {
            

                foreach (var i in Values.Keys)
                {
                    object value = Values[i];
                    var d = Descriptor;

                    if (d.Get(i).Type == ParameterType.PARAMETERIZEDES)
                    {
                        if (get(item,i) == null)
                        {
                            set(item,i, createList(d.Get(i).Subtype));
                        }
                        foreach (var x in (value as IList))
                        {
                            (get(item,i) as IList).Add((x as ParameterPack).Create());
                        }
                    }
                    else if (d.Get(i).Type == ParameterType.PARAMETERPACK)
                    {

                       set(item, i, (value as ParameterPack).Create());
                    }
                    else
                    {
                        set(item, i, Convert.ChangeType(value, item.GetType().GetProperty(i).PropertyType));
                    }

                }
            
        }
    }
    
   
    
}
