using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticAlgorithm.GeneticAlgorithm;
using GeneticAlgorithm.One;

namespace GeneticAlgorithm.Three {
    class DoubleIndividual : IIndividual<double> {
        private readonly IReadOnlyList<double> genotype;

        public DoubleIndividual(IEnumerable<double> newGenotype) {
            this.genotype = new List<double>(newGenotype);
        }

        public DoubleIndividual(int length, Random rng) {
            var newGenotype = new List<double>();
            for (int i = 0; i < length; i++) {
                var newGene = (rng.NextDouble()*2) - 1;
                newGenotype.Add(newGene);
            }

            this.genotype = new List<double>(newGenotype);
        }

        public IIndividual<double> Crossover(IIndividual<double> other, int point) {
            var newGenotype = Genotype;
            newGenotype[point] = (newGenotype[point] + other.Genotype[point])/2;

            return new DoubleIndividual(newGenotype);
        }

        public double[] Genotype { get { return genotype.ToArray(); }}
        private static long count, sum;
        public IIndividual<double> Mutate(double chance, Random rng) {
            var newGenotype = new List<double>(genotype);
            for (int i = 0; i < genotype.Count; i++) {
                if (rng.NextDouble() < chance) {
                    newGenotype[i] += NextGaussian(rng, 0, 0.2);
                    if (newGenotype[i] > 1) newGenotype[i] = 1;
                    if (newGenotype[i] < -1) newGenotype[i] = -1;
                    sum++;
                }
            }

            count++;

            

            //Console.WriteLine(sum/(double)count);

            return new DoubleIndividual(newGenotype);
        }

        private static double NextGaussian(Random r, double mu = 0, double sigma = 1) {
            var u1 = r.NextDouble();
            var u2 = r.NextDouble();

            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2);

            var randNormal = mu + sigma * randStdNormal;

            return randNormal;
        }
    }
}
