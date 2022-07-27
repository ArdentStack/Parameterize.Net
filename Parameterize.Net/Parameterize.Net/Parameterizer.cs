
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Parameterize
{
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
 