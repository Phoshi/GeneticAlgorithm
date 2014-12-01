using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GeneticAlgorithm.GeneticAlgorithm;
using MoreLinq;

namespace GeneticAlgorithm.Two {
    class SolveTwo {
        private readonly FitnessLogger logger;

        public SolveTwo(FitnessLogger logger) {
            this.logger = logger;
        }

        public IIndividual<Rule> Solve() {
            var firstSetSize = (int)(2048 * 0.8);
            var secondSetSize = (int)(2048 * 0.2);
            var matcher = GetMatcher(@"Data\data2.txt", firstSetSize, 0);
            var test = GetMatcher(@"Data\data2.txt", secondSetSize, firstSetSize);
            var population = new Population<IIndividual<Rule>, Rule>(100, 15, (size, rng) => new RuleIndividual(size, rng), id => id, matcher.CountMatches);

            for (int i = 0;; i++) {
                var generation = population.Generation();
                var best = generation.AsParallel().MaxBy(matcher.CountMatches);
                var average = generation.AsParallel().Sum(rule => matcher.CountMatches(rule, true))/generation.Count;
                var averageTest = generation.AsParallel().Sum(rule => test.CountMatches(rule, true)) / generation.Count;
                logger.LogFitness(matcher.CountMatches(best, true), average, test.CountMatches(best, true), averageTest);

                if ((int)(matcher.CountMatches(best, true)/firstSetSize) == 1 && (int)test.CountMatches(best, true)/secondSetSize == 1) {
                    return best;
                }

                if (i > 3000) {
                    return best;
                }
            }
        }

        public RuleMatcher GetMatcher(string path, int take, int skip) {
            var dict = new Dictionary<string, int>();
            var file = File.ReadAllLines(path);
            foreach (var line in file.Skip(skip).Take(take)) {
                var pair = line.Split(new[] { ' ' });

                var expectedResult = int.Parse(pair[1]);

                dict[pair[0]] = expectedResult;
            }
            return new RuleMatcher(dict);
        }
    }
}
