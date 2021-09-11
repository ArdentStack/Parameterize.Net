using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parameterize;
namespace FastTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var rng = new Random();
            var enemyconstraints = Parameterizer.GetConstraints<Enemy>();
            var playerConstraints = Parameterizer.GetConstraints<Player>();
            var seg = new ParameterSegment(typeof(Player), null, false);
            Console.WriteLine(ParameterSegment.PrettyStringSegment(seg,2));
            Player p = Parameterizer.Create<Player>(Constraint.GetRandom(rng, playerConstraints));
            Enemy e = Parameterizer.Create<Enemy>(Constraint.GetRandom(rng, enemyconstraints));
            while (true)
            {
                Console.WriteLine($"Your health:{p.Health} Enemy Health:{e.Health}");
                for (int i = 0; i < p.Abilities.Count; i += 1)
                {
                    Console.Write($"({i+1})");
                    Console.WriteLine(p.Abilities[i]);
                }

                Console.WriteLine($"({3}) Kill yourself");
                int x = 0;
                while (!int.TryParse(Console.ReadLine(), out x) || x < 1 || x > 3) 
                {
                    Console.WriteLine("Invalid input");
                }
                switch (x)
                {
                    case 1:
                        (int a, int b) = p.Abilities[0].Apply();
                        p.Health += a;
                        e.Health -= b;
                        break;
                    case 2:
                        ( a,  b) = p.Abilities[1].Apply();
                        p.Health += a;
                        e.Health -= b;
                        break;
                    case 3:

                        p = Parameterizer.Create<Player>(Constraint.GetRandom(rng, playerConstraints));
                        e = Parameterizer.Create<Enemy>(Constraint.GetRandom(rng, enemyconstraints));
                        break;
                }
                if (e.Health <= 0)
                {
                    e = Parameterizer.Create<Enemy>(Constraint.GetRandom(rng, enemyconstraints));
                    Console.WriteLine("Enemy died");
                }
                else
                {
                    Console.WriteLine($"Enemy attacks for {e.Damage}");
                    p.Health -= e.Damage;

                }
                if (p.Health <= 0)
                {
                    p = Parameterizer.Create<Player>(Constraint.GetRandom(rng, playerConstraints));
                    e = Parameterizer.Create<Enemy>(Constraint.GetRandom(rng, enemyconstraints));
                    Console.WriteLine("You died");
                }
               
            }

        }

    }

    [Parameterized]
    class Enemy
    {
        [Parameter( 10, 100, 0)]
        public int Health { get; set; }
        [Parameter( 10, 20, 0)]
        public int Damage { get; set; }
    }


    [Parameterized]
    abstract class Ability
    {

        [Parameter( 10, 50, 0)]
        public int Power { get; set; }
        public override string ToString()
        {
            return $"{this.GetType().Name}({Power})";
        }
        public abstract (int, int) Apply();
    }

    [Parameterized]
    class Fireball : Ability
    {
        public override (int, int) Apply()
        {
            Console.WriteLine("Fireball");
            return (0, Power);
        }
    }
    [Parameterized]
    class Melee : Ability
    {
        public override (int, int) Apply()
        {
            Console.WriteLine("Melee attack");
            return (-(Power/2),(int)( 1.5*Power));
        }
    }
    [Parameterized]
    class Heal : Ability
    {
        public override (int, int) Apply()
        {
            Console.WriteLine("Heal");
            return (Power, 0);
        }
    }

    [Parameterized]
    class Player
    {
        int health = 100;
        [Parameter(2,2,0)]
        public List<Ability> Abilities { get; set; }
        public int Health { get => health; set => health = value; }
    }
    public class ConfigTester
    {
        
    }

}
