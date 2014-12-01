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
        public IIndividual<Rule> Solve() {
            var firstSetSize = (int)(2048 * 0.8);
            var secondSetSize = (int)(2048 * 0.2);
            var matcher = GetMatcher(@"Data\data2.txt", firstSetSize, 0);
            var test = GetMatcher(@"Data\data2.txt", secondSetSize, firstSetSize);
            var population = new Population<IIndividual<Rule>, Rule>(100, 15, (size, rng) => new RuleIndividual(size, rng), id => id, matcher.CountMatches);

            int turnsSinceImprovement = 0;

            DateTime time = DateTime.Now;
            IIndividual<Rule> allTimeBest = null;
            for (int i = 0;; i++) {
                var generation = population.Generation(50);
                var best = generation.MaxBy(matcher.CountMatches);
                Console.WriteLine("Generation " + i);
                Console.WriteLine(turnsSinceImprovement++ + " turns since improvement");
                Console.WriteLine(best);
                Console.Write(matcher.CountMatches(best) + " - ");
                Console.Write((matcher.CountMatches(best, true) / firstSetSize) * 100 + "% - ");
                Console.Write((test.CountMatches(best, true) / secondSetSize) * 100 + "% - (Best: ");
                if (allTimeBest != null)
                    Console.WriteLine((test.CountMatches(allTimeBest, true)/secondSetSize) * 100 + "%)");

                //Console.WriteLine("Average: " + generation.Sum(rule => matcher.CountMatches(rule)) / generation.Count);

                if (allTimeBest == null || test.CountMatches(best, true) > test.CountMatches(allTimeBest, true)) {
                    allTimeBest = best;
                    turnsSinceImprovement = 0;
                }

                if ((int)(matcher.CountMatches(best, true)/firstSetSize) == 1 && (int)test.CountMatches(best, true)/secondSetSize == 1) {
                    return best;
                }

                Console.WriteLine(DateTime.Now - time);
                time = DateTime.Now;
            }

            return population.Generation(1).OrderBy(matcher.CountMatches).Last();
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
