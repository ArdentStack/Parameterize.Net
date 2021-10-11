# Parametrize.Net
Parameterize.Net is a library that allows developers to represent complex objects using float arrays.


# Todo
* Documentation and code comments
* Allow for float array "parameters".
* more flexible constraint definition
# Installation

[![Nuget](https://img.shields.io/nuget/v/Parameterize.Net)](https://www.nuget.org/packages/Parameterize.Net/)


# Usage
```c#
[Parameterized]
class PetStore
{
    string name;
    // 2 to 10 animals
    [Parameter(2, 10)]
    public List<Animal> Animals { get; set; }
    [OnInitFunction] // will be called if this is the root object (the main type being created) 
    public void OnInit(string name)
    {
        this.name = name;
        Console.WriteLine($"Name = {name}");
    }
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
class Dog : Animal
{
    //1 to 10 spots (int)
    [Parameter(1, 10)]
    public int Spots { get; set; }
    [OnInitFunction] // will be called whenever the library creates this type of object
    public void DogInit()
    {
        //pet
    }

    public override string ToString()
    {
        return base.ToString() + $"Dog with {Spots} spots ";
    }
}
[Parameterized]
class Cat : Animal
{
    // 6 to 9 lives
    [Parameter(6, 9)]
    public int Lives { get; set; }
    public override string ToString()
    {
        return base.ToString() + $"Cat with {Lives} lives ";
    }
}


class Program
{
    static void Main(string[] args)
    {
        // Config allow changing constraints in real time
        dynamic config = Parameterizer.GetConfigFor(typeof(PetStore));
        // Change how many objects in a parameterized object collection
        config.Animals.CountConstraint = new Constraint(1,2);
        // Change the constraint for an object parameter
        config.Animals.Lives = new Constraint(1,5);

        // Get the constraints for the float array.
        var constraints = Parameterizer.GetConstraints<PetStore>(config);

        while (true)
        {
            // Generate random float array according to the constraints

            float[] parameters = Constraint.GetRandom(constraints);
            // Create the object from the parameters                        
            var petstore = Parameterizer.Create<PetStore>(parameters,config,"My PetStore Name"); //The string argument at the end will go to an OnInitFunction in the type PetStore.

            foreach (var animal in petstore.Animals)
            {
                Console.WriteLine(animal);
            }
            Console.ReadLine();
        }
    }
}
```

# LICENSE

Copyright 2021 Faisal Alsajjan

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
