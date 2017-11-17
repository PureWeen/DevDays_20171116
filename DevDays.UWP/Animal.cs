using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevDays.UWP
{
    public enum AnimalFamily
    {
        Mammal,
        Reptile,
        Fish,
        Amphibian,
        Bird
    }

    public class Animal : ReactiveObject
    {
        public string Name { get; }
        public string Type { get; }
        public AnimalFamily Family { get; }

        private int _animalRating;
        public int AnimalRating
        {
            get => _animalRating;
            set => this.RaiseAndSetIfChanged(ref _animalRating, value);
        }

        public Animal(string name, string type, AnimalFamily family)
        {
            Name = name;
            Type = type;
            Family = family;
        }

        public override string ToString()
        {
            return Name;
        }



        public static SourceList<Animal> CreateMeSomeAnimalsPlease()
        {
            var sourceList = new SourceList<Animal>();
            Animal[] _items = new[]
            {
                new Animal("Holly", "Cat", AnimalFamily.Mammal),
                new Animal("Rover", "Dog", AnimalFamily.Mammal),
                new Animal("Rex", "Dog", AnimalFamily.Mammal),
                new Animal("Whiskers", "Cat", AnimalFamily.Mammal),
                new Animal("Nemo", "Fish", AnimalFamily.Fish),
                new Animal("Moby Dick", "Whale", AnimalFamily.Mammal),
                new Animal("Fred", "Frog", AnimalFamily.Amphibian),
                new Animal("Isaac", "Next", AnimalFamily.Amphibian),
                new Animal("Sam", "Snake", AnimalFamily.Reptile),
                new Animal("Sharon", "Red Backed Shrike", AnimalFamily.Bird),
            };


            sourceList.AddRange(_items);
            return sourceList;

        }
    }
}
