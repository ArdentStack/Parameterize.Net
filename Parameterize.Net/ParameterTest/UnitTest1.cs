using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Parameterize;
using System.Collections.Generic;

namespace ParameterTest
{
    [TestClass]
    public class CreationTests
    {
        [TestMethod]
        public void CreationBasicTest()
        {
            var constraints = Parameterizer.GetConstraints<Parameterized1>();
            var obj = Parameterizer.Create<Parameterized1>(Constraint.GetRandom(new Random(), constraints));

        }
        public void NestedParameterizedsCreationTest()
        {

            
        }
    }
    [Parameterized]
    class Parameterized1
    {
        [Parameter("",ParameterConstraintType.MinMax,0,100,6)]
        public int X { get; set; }
    }
    [Parameterized]
    class Parameterized2
    {
        [Parameter("", ParameterConstraintType.None, 0, 0, 0)]
        public Parameterized1 A { get; set; }
        [Parameter("", ParameterConstraintType.MinMax, 1, 100, 0)]
        public List<Parameterized1> Bs { get; set; }

    }
}
