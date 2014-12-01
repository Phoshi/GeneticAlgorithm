using System;
using System.Linq;
using GeneticAlgorithm.GeneticAlgorithm;

namespace GeneticAlgorithm.One {
    internal class BoolIndividual : IIndividual<bool> {
        private readonly bool[] genotype;
        public bool[] Genotype { 
            get { return genotype.Select(x=>x).ToArray(); } }

        public BoolIndividual(int size, Random rng) {
            var genotype = new bool[size];
            for (int i = 0; i < size; i++) {
                genotype[i] = rng.NextDouble() > 0.5;
            }

            this.genotype = genotype;
        }

        public BoolIndividual(bool[] genotype) {
            this.genotype = genotype;
        }

        public IIndividual<bool> Mutate(double chance, Random rng) {
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
            var newGenotype = new bool[genotype.Length];
            Array.Copy(Genotype, newGenotype, point);
            Array.Copy(other.Genotype, point, newGenotype, point, newGenotype.Length - point);

            return new BoolIndividual(newGenotype);
        }

        public override string ToString() {
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