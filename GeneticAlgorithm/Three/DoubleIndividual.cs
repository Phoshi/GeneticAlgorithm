using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithm.Annotations;
using GeneticAlgorithm.GeneticAlgorithm;

namespace GeneticAlgorithm.Three {
    /// <summary>
    /// An individual to represent a string of real numbers
    /// </summary>
    class DoubleIndividual : IIndividual<double> {
        private readonly IReadOnlyList<double> genotype;

        public DoubleIndividual(IEnumerable<double> newGenotype) {
            this.genotype = new List<double>(newGenotype);
        }

        public DoubleIndividual(int length, Random rng) {
            //Making a new double individual is still easy, we just want every element evenly distributed between -1 and 1.
            var newGenotype = new List<double>();
            for (var i = 0; i < length; i++) {
                var newGene = (rng.NextDouble()*2) - 1;
                newGenotype.Add(newGene);
            }

            genotype = new List<double>(newGenotype);
        }

        public IIndividual<double> Crossover(IIndividual<double> other, int point) {
            //Crossover in this case just merges the two numbers at the point. Anything more is too much.
            var newGenotype = Genotype;
            newGenotype[point] = (newGenotype[point] + other.Genotype[point])/2;

            return new DoubleIndividual(newGenotype);
        }

        public double[] Genotype { get { return genotype.ToArray(); }}

        //Two debugging values to work out the average mutation rate
        private static long _count;
        private static long _sum;

        public IIndividual<double> Mutate(double chance, Random rng) {
            //Mutation gives each element a `chance` probability to be creeped.
            var newGenotype = new List<double>(genotype);
            for (int i = 0; i < genotype.Count; i++) {
                if (rng.NextDouble() < chance) {
                    //If we're mutating, we want to move it a little. 
                    //We do this according to a normal distribution.
                    newGenotype[i] += NextGaussian(rng, 0, 0.2);
                    //And then constrain the values to our range.
                    if (newGenotype[i] > 1) newGenotype[i] = 1;
                    if (newGenotype[i] < -1) newGenotype[i] = -1;
                    _sum++;
                }
            }

            _count++;


            if (false) {
                Console.WriteLine(_sum/(double) _count);
            }

            return new DoubleIndividual(newGenotype);
        }

        private static double NextGaussian(Random rng, double mu = 0, double sigma = 1) {
            //Taken from http://stackoverflow.com/a/218600/160783
            var u1 = rng.NextDouble();
            var u2 = rng.NextDouble();

            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2);

            var randNormal = mu + sigma * randStdNormal;

            return randNormal;
        }
    }
}
