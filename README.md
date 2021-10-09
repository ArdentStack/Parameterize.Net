# Parametrize.Net
Parameterize.Net is a library that allows developer to represent complex objects using float array.

# Todo
* allow for static float array parameters.
* more flexible constraint definition

# Usage
```c#
class Program
{
    static void Main(string[] args)
    {
        // Create random number generator
        var rng = new Random();
        Console.WriteLine(Parameterize.ParameterSegment.PrettyStringSegment(new ParameterSegment(typeof(PetStore), null, false),0));
        // Get the constraints for the float array.
        var constraints = Parameterizer.GetConstraints<PetStore>();
        while (true)
        {
            // Generate random float array according to the constraints
            float[] parameters = Constraint.GetRandom(rng, constraints);
            // Create the object from the parameters
            var petstore = Parameterizer.Create<PetStore>(parameters);

            foreach (var animal in petstore.Animals)
            {
                Console.WriteLine(animal);
            }
            Console.ReadLine();
        }
    }
}
[Parameterized]
class PetStore
{
    // 2 to 10 animals
    [Parameter(2,10,0)]
    public List<Animal> Animals { get; set; }
}
[Parameterized]
abstract class Animal
{
    //weight is 5 to 45 with 1 decimal point
    [Parameter(5, 45, 1)]

    public float Weight { get; set; }
    public override string ToString()
    {
        return $"Animal weighing {Weight} Kg ";
    }
}
[Parameterized]
class Dog:Animal
{
    //1 to 10 spots (int)
    [Parameter(1,10,0)]
    public int Spots { get; set; }

    public override string ToString()
    {
        return base.ToString()+$"Dog with {Spots} spots ";
    }
}
[Parameterized]
class Cat:Animal
{
    // 6 to 9 lives
    [Parameter(6, 9, 0)]
    public int Lives { get; set; }
    public override string ToString()
    {
        return base.ToString() + $"Cat with {Lives} lives ";
    }
}
```
