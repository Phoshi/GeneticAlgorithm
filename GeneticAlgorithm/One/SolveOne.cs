using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeneticAlgorithm.GeneticAlgorithm;

namespace GeneticAlgorithm.One {
    class SolveOne {
        public IIndividual<bool> Solve() {
            var matcher = GetMatcher(@"Data\data1.txt");
            Func<IIndividual<bool>, double> fitnessFunction = matcher.CountMatches;

            const int maximumIterations = 20; //This is fast, we may as well do loads.

            IIndividual<bool> best = null;
            var logger = new AverageLogger();

            for (var run = 0.0; run <= 2; run+=0.1) {
                Console.WriteLine("Running one with pop="+ run);

                var iteration = 0;
                var fitness = 0; //This is the fitness sum so far, for later averaging.

                while (iteration++ < maximumIterations) {
                    //Make a new population with some reasonable default values
                    var population = new Population<IIndividual<bool>, bool>(100, 64,
                        (size, rng) => new BoolIndividual(size, rng), ind => ind, fitnessFunction);
                    population.MutationMultiplier = run;
                    //and a logger
                    var runLogger = new FitnessLogger();

                    var i = 0;
                    while (true) {
                        //Run a generation
                        var generation = population.Generation();
                        //Find the best individual
                        best = generation.OrderBy(fitnessFunction).Last();
                        //Find the average fitness
                        var average = generation.Select(fitnessFunction).Sum()/generation.Count;
                        //Log what we have
                        runLogger.LogFitness(fitnessFunction(best), average);

                        if ((int) fitnessFunction(best) == 64) {
                            //If we've found the correct answer, we need to add how long it took us to the total and stop
                            fitness += i;
                            break;
                        }
                        if (i++ > 500) {
                            //If we've taken way too long, we just add how far we've got and stop trying
                            fitness += i;
                            break;
                        }
                    }

                    //Every run we save how it went for later crunching
                    runLogger.Save("one-mutation-" + run + ".csv");
                }
                //And we log how long everything took us, on average.
                logger.LogFitness(fitness/maximumIterations);
            }
            logger.Save("one-mutation-runs.csv");
            return best;
        }

        public static FullDataMatch<bool> GetMatcher(string path) {
            //We need a matcher to tell us how well we're doing. This dataset is formatted as attribute@bitstring class@bit
            var dict = new Dictionary<int, int>();
            var file = File.ReadAllLines(path);
            foreach (var line in file) {
                //Split each line on the space
                var pair = line.Split(new[] { ' ' });

                //We can take the first element as a binary number
                var element = Convert.ToInt32(pair[0], 2);

                //And the last as a class
                var expectedResult = int.Parse(pair[1]);

                //and build up a hashmap of these
                dict[element] = expectedResult;
            }

            return new FullDataMatch<bool>(dict);
        }
    }
}
