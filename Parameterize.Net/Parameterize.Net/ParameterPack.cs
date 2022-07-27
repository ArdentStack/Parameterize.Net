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
