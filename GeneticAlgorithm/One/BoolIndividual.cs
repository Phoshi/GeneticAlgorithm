using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithm.GeneticAlgorithm;

namespace GeneticAlgorithm.One {
    /// <summary>
    /// An individual type to represent a naive bitstring.
    /// </summary>
    internal class BoolIndividual : IIndividual<bool> {
        private readonly bool[] genotype;
        public bool[] Genotype { 
            get { return genotype.Select(x=>x).ToArray(); } }

        public BoolIndividual(int size, Random rng) {
            //Making a new bitstring is easy. We just want true/false with even probability.
            var newGenotype = new bool[size];
            for (int i = 0; i < size; i++) {
                newGenotype[i] = rng.NextDouble() > 0.5;
            }

            genotype = newGenotype;
        }

        public BoolIndividual(IEnumerable<bool> genotype) {
            this.genotype = genotype.ToArray();
        }

        public IIndividual<bool> Mutate(double chance, Random rng) {
            //Mutation in the case of a bitstring is also easy. We flip the bit with probability `chance`.
            var newGenotype = new bool[genotype.Length];
            for (var i = 0; i < genotype.Length; i++) {
                newGenotype[i] = genotype[i];
                if (rng.NextDouble() < chance) {
                    newGenotype[i] = !newGenotype[i];
                }
            }

            return new BoolIndividual(newGenotype);
        }

        public IIndividual<bool> Crossover(IIndividual<bool> other, int point) {
            //Crossover, also easy. We do single-point crossover around the point.
            var newGenotype = new bool[genotype.Length];
            Array.Copy(Genotype, newGenotype, point);
            Array.Copy(other.Genotype, point, newGenotype, point, newGenotype.Length - point);

            return new BoolIndividual(newGenotype);
        }

        public override string ToString() {
            //We stringify into a proper bitstring, with 1 for true and 0 for false.
            return "{" + genotype.Select(bit => bit ? "1" : "0").Aggregate((a, b) => a + b) + "}";
        }

        public override bool Equals(object obj)
        {
            if (obj is BoolIndividual){
                var other = obj as BoolIndividual;
                return genotype.Intersect(other.genotype).Count() == genotype.Length;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return genotype.GetHashCode();
        }
    }
}