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

                            ret.Add(new ParameterDescriptor(i.Name, ParameterType.STRING, paramt));
                        }

                        else if (i.PropertyType.IsGenericType && i.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))// && IsParameterized(i.PropertyType.GenericTypeArguments[0]))
                        {

                            ret.Add(new ParameterDescriptor(i.Name, ParameterType.PARAMETERIZEDES, i.PropertyType.GenericTypeArguments[0], paramt));
                        }
                        /*else if (IsParameterized(i.PropertyType))
                        {
                            

                            ret.Add(new ParameterDescriptor(i.Name, ParameterType.PARAMETERPACK, i.PropertyType,paramt));
                        }*/
                        else if (i.PropertyType.IsArray && i.MemberType.GetType().Equals(typeof(float)))
                        {
                            ret.Add(new ParameterDescriptor(i.Name, ParameterType.ARRAY, i.PropertyType, paramt));
                        }
                        else if (!i.PropertyType.IsValueType) 
                        {
                            ret.Add(new ParameterDescriptor(i.Name, ParameterType.PARAMETERPACK, i.PropertyType, paramt));
                        }
                        else
                        {
                            ret.Add(new ParameterDescriptor(i.Name, ParameterType.PARAMETERPACK, i.PropertyType, paramt));
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
    
   
    
}
