using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticAlgorithm.GeneticAlgorithm;

namespace GeneticAlgorithm.One {
    class SolveOne {
        private FitnessLogger logger;

        public SolveOne(FitnessLogger logger) {
            this.logger = logger;
        }

        public IIndividual<bool> Solve() {
            var matcher = GetMatcher(@"Data\data1.txt");
            Func<IIndividual<bool>, double> Fitness = matcher.CountMatches;
            var population = new Population<IIndividual<bool>, bool>(100, 64, (size, rng)=>new BoolIndividual(size, rng), ind => ind, Fitness);

            var i = 0;
            while (true) {
                var generation = population.Generation();
                var best = generation.OrderBy(Fitness).Last();
                var average = generation.Select(Fitness).Sum()/generation.Count;

                logger.LogFitness(Fitness(best), average);
                if ((int)Fitness(best) == 64) {
                    return best;
                }
                if (i++ > 500) {
                    return best;
                }
            }
        }

        public static FullDataMatch<bool> GetMatcher(string path) {
            var dict = new Dictionary<int, int>();
            var file = File.ReadAllLines(path);
            foreach (var line in file) {
                var pair = line.Split(new[] { ' ' });

                var element = Convert.ToInt32(pair[0], 2);

                var expectedResult = int.Parse(pair[1]);

                dict[element] = expectedResult;
            }

            return new FullDataMatch<bool>(dict);
        }
    }
}
