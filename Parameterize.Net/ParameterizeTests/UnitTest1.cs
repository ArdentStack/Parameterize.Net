using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Parameterize;
using System.Collections.Generic;

namespace ParameterizeTests
{
    [TestClass]
    public class FactoryTests
    {
        int count = 10000;
        float timeperCreationBasic1 = 0.001f;
        float timeperCreationBasic2 = 0.002f;
        float timeperCreationComplex1 = 0.008f;
        [TestMethod]
        public void Basic1TestCreationTime()
        {
            var factory = new ParametricFactory<Basic1>();
            DateTime t =DateTime.Now; 
            for(int i = 0; i < count; i++)
            {
                factory.Create();
            }
            Assert.IsTrue((DateTime.Now - t).TotalSeconds / count < timeperCreationBasic1);
        }
        [TestMethod]
        public void Basic2TestCreationTime()
        {
            var factory = new ParametricFactory<Basic2>();
            DateTime t = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                factory.Create();
            }
            Assert.IsTrue((DateTime.Now - t).TotalSeconds / count < timeperCreationBasic2);

        }
        [TestMethod]
        public void Complex1TestCreationTime()
        {
            var factory = new ParametricFactory<Complex1>();
            DateTime t = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                factory.Create();
            }
            Assert.IsTrue((DateTime.Now - t).TotalSeconds / count < timeperCreationComplex1);

        }
        [TestMethod]
        public void TestCreationConsistency1()
        {
            var factory = new ParametricFactory<Complex1>();
            var gene = factory.GetRandomParameters();
            var bench = factory.Create(gene);
            
            for(int i=0; i<count; i++)
            {
                var n = factory.Create(gene);
                Assert.AreEqual(bench.Value.CompareTo(n.Value), 0);
            }
        }
        [TestMethod]
        public void TestCreationConsistency2()
        {
            var factory = new ParametricFactory<Complex1>();
           

            for (int i = 0; i < count; i++)
            {
                var n = factory.Create();
                n.Value.Test();
            }
        }
    }
    [Parameterized]
    public class Basic1 : IComparable<Basic1>
    {
        [Parameter(0,10,10)]
        public float A { get; set; }

        public int CompareTo(Basic1 other)
        {
            return A.CompareTo(other.A);
        }
        public void Test()
        {
            Assert.IsTrue(A >= 0 && A <= 10);
        }
    }

    [Parameterized]
    public class Basic2 : IComparable<Basic2>
    {
        [Parameter(0,10)]
        [ElementConstraint(0,10,10)]
        public float[] B { get; set; }
        public void Test()
        {
            foreach(var i in B)
            {
                Assert.IsTrue(i >= 0 && i <= 10);
            }
        }
        public int CompareTo(Basic2 other)
        {
            for(int i = 0; i < B.Length; i++)
            {
                if (B[i] != other.B[i])
                {
                    return B[i].CompareTo(other.B[i]); 
                }
            }
            return 0;
        }
    }

    [Parameterized]
    public class Complex1 : IComparable<Complex1>
    {
        [Parameter]
        public Basic1 Basic1 { get; set; }
        [Parameter]
        public Basic2 Basic2 { get; set; }
        [Parameter(0,10)]
        public List<Basic1> Basics { get; set; }
        public int CompareTo(Complex1 other)
        {
            var a =Basic1.CompareTo(other.Basic1) | Basic2.CompareTo(other.Basic2);
            for(int i =0;i<Basics.Count;i++)
            {
                a|= Basics[i].CompareTo(other.Basics[i]);
            }
            return a;
        }
        public void Test()
        {
            Basic1.Test();
            Basic2.Test();
            foreach(var i in Basics)
            {
                i.Test();
            }
        }
    
    }

}
