using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeneticAlgorithm.GeneticAlgorithm;
using MoreLinq;

namespace GeneticAlgorithm.Two {
    class SolveTwo {
        private readonly AverageLogger averageLoggerlogger = new AverageLogger();

        public IIndividual<Rule> Solve() {
            const int firstSetSize = (int)(2048 * 0.8);
            const int secondSetSize = (int)(2048 * 0.2);
            var matcher = GetMatcher(@"Data\data2.txt", firstSetSize, 0);
            var test = GetMatcher(@"Data\data2.txt", secondSetSize, firstSetSize);

            IIndividual<Rule> best = null;

            const int maximumIteration = 3;
            for (var run = 0.0; run <= 2; run += 0.1) {
                Console.WriteLine("Running two with pop=" + run);
                            var iteration = 0;
                var fitness = 0;
                while (iteration++ < maximumIteration) {
                    //Make a new population with some sane defaults
                    var population = new Population<IIndividual<Rule>, Rule>(100, 15,
                        (size, rng) => new RuleIndividual(size, rng), id => id, matcher.CountMatches) {
                            MutationMultiplier = run
                        };
                    var logger = new FitnessLogger();

                    for (int i = 0;; i++) {
                        //Then just run with it.
                        var generation = population.Generation();
                        best = generation.AsParallel().MaxBy(matcher.CountMatches);
                        var average = generation.AsParallel().Sum(rule => matcher.CountMatches(rule, true))/
                                      generation.Count;
                        var averageTest = generation.AsParallel().Sum(rule => test.CountMatches(rule, true))/
                                          generation.Count;
                        //logging as we go.
                        logger.LogFitness(matcher.CountMatches(best, true), average, test.CountMatches(best, true),
                            averageTest);

                        //And stop moving once we match both sets. We predicate on both sets here because rulset 2 has a habit
                        //of matching ruleset 1 in a specific way, then generalising. We could either give it a few hundred generations to generalise, or just 
                        //wait until it matches the test set too. I figure that this isn't letting the test set influence evolution at all, so it's fine.
                        if ((int) (matcher.CountMatches(best, true)/firstSetSize) == 1 &&
                            (int) test.CountMatches(best, true)/secondSetSize == 1) {
                            fitness += i;
                            break;
                        }

                        //If we've been going for too long, just cut off.
                        if (i > 3000) {
                            fitness += i;
                            break;
                        }
                    }
                    logger.Save("two-mutation-" + run + ".csv");
                }
                averageLoggerlogger.LogFitness(fitness / 3);
            }

            averageLoggerlogger.Save("two-mutation-runs.csv");

            return best;
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
