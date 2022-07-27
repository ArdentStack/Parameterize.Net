using System;
using System.Collections.Generic;
using System.Text;

namespace Parameterize.Net
{
    public class ParametricFactory<T>
    {
        ParameterSegment segement;
        Constraint[] constraints;
        Random random;
        public ParametricFactory()
        {
            random= new Random();
            segement = new ParameterSegment(typeof(T), null, false);
            constraints = Parameterizer.GetConstraints<T>();

        }
        public class CreationResult
        {
            public T Value { get; private set; }
            public float[] Parameters { get; private set; }
            public CreationResult(T val,float[] param)
            {
                Value = val;
                Parameters = param;
            }
        }
        public float[] GetRandomParameters()
        {
            return Constraint.GetRandom(constraints, random);
        }
        public CreationResult Create(float[] param = null) {
            if(param== null)
            {
                param = GetRandomParameters();
            }
            return new CreationResult(Parameterizer.Create<T>(param),param);
        }
    }
}
