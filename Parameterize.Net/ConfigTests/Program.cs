using Parameterize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigTests
{
    class Program
    {
        static void Main(string[] args)
        {
            dynamic config = Parameterizer.GetConfigFor(typeof(A));

            config.Ourlist.CountConstraint = new Constraint(0, 5, 0);

            var s = new ParameterSegment(typeof(A), null, false, null, config);
            Console.WriteLine(ParameterSegment.PrettyStringSegment(s, 2));
            var g = Constraint.GetRandom(new Random(), Parameterizer.GetConstraints<A>(config));

            while (Console.ReadLine().Length == 0)
            {
                var a = Parameterizer.Create<A>(g, config);
                Console.WriteLine(a.X);
                Console.WriteLine(a.Ourlist[0].Y);
                foreach(var i in a.Ourlist)
                {
                    Console.WriteLine(i.Y);
                }
            }
        }
    }
    [Parameterized]
    public class A
    {
        [Parameter(0,10,1)]
        public float X { get; set; }

        [Parameter]
        public B Child { get; set; }

        [Parameter( 0, 100, 0)]
        public List<B> Ourlist { get; set; }
    }

    [Parameterized]
    public class B
    {
        [Parameter( 0, 10, 1)]
        public float Y { get; set; }
    }
    [Parameterized]
    public class InputPacket
    {
        [Parameter(-1,1,2)]
        float HorizontalAxis { get; set; }
        [Parameter(-1,1,2)]
        float VerticalAxis { get; set; }
        [Parameter(0, 1, 0)]
        int Button1 { get; set; }
    }
}
