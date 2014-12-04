using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeneticAlgorithm.GeneticAlgorithm;

namespace GeneticAlgorithm.Three {
    internal class SolveThree {
        public RuleSet Solve() {
            const int totalData = 2000;
            const double trainSet = totalData*0.8;
            const double testSet = totalData*0.2;
            var matcher = GetMatcher(@"Data\data3.txt", (int) (trainSet), 0);
            var test = GetMatcher(@"Data\data3.txt", (int) (testSet), (int) (trainSet));
            var averageLogger = new AverageLogger();

            //Hidden nodes have $#input + 1 bias weights each
            //Second hidden have $#hidden + 1 bias weights each
            //Output nodes have $#sndHidden + 1 bias weights each

            //Thus, ($#hidden * ($#input + 1)) + ($#sndHidden * ($#hidden + 1)) + ($#output * ($#sndHidden + 1)
            const int numInput = 6;
            const int numHidden = 6;
            const int numSecondHidden = 3;
            const int numOutput = 2;

            const int maximumIterations = 3;

            RuleSet ruleBest = null;


            for (var run = 0.0; run <= 2.0; run += 0.1) {
                Console.WriteLine("Running rul with var=" + run);

                var iterations = 0;
                var fitness = 0;

                while (iterations++ < maximumIterations) {
                    //We make two populations for three, so we can run the neural network and ruleset side by side.
                    var rulePopulation = new Population<RuleSet, double>(100, 10*((6*2) + 1),
                        (num, random) => new RuleIndividual(num, random), individual => new RuleSet(individual.Genotype),
                        matcher.CountMatches);
                    var netPopulation = new Population<NeuralNetwork2, double>(100,
                        (numHidden*(numInput + 1)) + (numSecondHidden*(numHidden + 1)) +
                        (numOutput*(numSecondHidden + 1)),
                        (length, rng) => new DoubleIndividual(length, rng),
                        individual =>
                            new NeuralNetwork2(individual.Genotype, numInput, numHidden, numSecondHidden, numOutput),
                        matcher.CountMatches);

                    rulePopulation.MutationMultiplier = run;
                    netPopulation.MutationMultiplier = run;

                    var loggerRule = new FitnessLogger();
                    var loggerNet = new FitnessLogger();

                    int i = 0;
                    while (true) {
                        const double threshold = 0.9;

                        var netGeneration = netPopulation.Generation();
                        var ruleGeneration = rulePopulation.Generation();

                        var netBest = netGeneration.AsParallel().OrderBy(matcher.CountMatches).Last();
                        ruleBest = ruleGeneration.AsParallel().OrderBy(matcher.CountMatches).Last();

                        var netAverage =
                            netGeneration.AsParallel().Select(net => matcher.CountMatches(net, true)).Sum()/
                            netGeneration.Count;
                        var ruleAverage =
                            ruleGeneration.AsParallel().Select(rul => matcher.CountMatches(rul, true)).Sum()/
                            ruleGeneration.Count;
                        var netTestAverage =
                            netGeneration.AsParallel().Select(net => test.CountMatches(net, true)).Sum()/
                            netGeneration.Count;
                        var ruleTestAverage =
                            ruleGeneration.AsParallel().Select(rul => test.CountMatches(rul, true)).Sum()/
                            ruleGeneration.Count;

                        loggerNet.LogFitness(matcher.CountMatches(netBest, true), netAverage,
                            test.CountMatches(netBest, true), netTestAverage);
                        loggerRule.LogFitness(matcher.CountMatches(ruleBest, true), ruleAverage,
                            test.CountMatches(ruleBest, true), ruleTestAverage);

                        //We finish when the ruleset hits the threshhold percentage, though. The neural network isn't very good.
                        if ((int) (matcher.CountMatches(ruleBest, true)) >= (int) trainSet*threshold) {
                            fitness += i;
                            break;
                        }

                        if (i > 3000) {
                            fitness += i;
                            break;
                        }

                        i++;
                    }
                    loggerRule.Save("rul-mutation" + run + ".csv");
                }
                averageLogger.LogFitness(fitness/maximumIterations);
            }
            averageLogger.Save("three-mutation-runs.csv");
            return ruleBest;
        }


        public static NeuralNetworkMatch GetMatcher(string path, int take, int skip) {
            var dict = new Dictionary<IEnumerable<double>, int>();
            string[] file = File.ReadAllLines(path);
            foreach (string line in file.Skip(skip).Take(take)) {
                //In this case we have to parse the elements as doubles.
                string[] pair = line.Split(new[] {' '});

                List<double> elements = pair.Select(Convert.ToDouble).ToList();

                dict[elements.Take(pair.Count() - 1)] = (int) elements.Last();
            }

            return new NeuralNetworkMatch(dict);
        }
    }

    internal class NeuralNetworkMatch {
        private readonly Dictionary<IEnumerable<double>, int> matches;

        private readonly ConcurrentDictionary<NeuralNetwork2, double> memoize =
            new ConcurrentDictionary<NeuralNetwork2, double>();

        private readonly ConcurrentDictionary<RuleSet, double> memoizeRules =
            new ConcurrentDictionary<RuleSet, double>();

        public NeuralNetworkMatch(Dictionary<IEnumerable<double>, int> matches) {
            this.matches = matches;
        }

        public double CountMatches(NeuralNetwork2 net, bool fullMatchOnly) {
            //This matcher is a little more complex. 
            //And memoised, because my dev machine has 32GB RAM and even a 4670k only goes so fast.
            if (memoize.Count > 100000)
                memoize.Clear();
            if (!memoize.ContainsKey(net) || fullMatchOnly) {
                //We have both matched and score to more easily support more complex scoring mechanisms.
                //Matched is the number correct. Score could be adjusted.
                double matched = 0d;
                double score = 0d;
                foreach (var pair in matches) {
                    //We just run the network, round the result, and compare.
                    IList<double> output = net.Output(pair.Key);
                    int normalisedOutput = -1;
                    if (output.First() > 0.5) {
                        normalisedOutput = 1;
                    }
                    else if (output.Skip(1).First() > 0.5) {
                        normalisedOutput = 0;
                    }

                    if (normalisedOutput == pair.Value) {
                        matched += 1;
                        score += 1;
                    }
                }
                if (fullMatchOnly)
                    return matched;
                memoize[net] = score;
            }
            return memoize[net];
        }

        public double CountMatches(NeuralNetwork2 net) {
            return CountMatches(net, false);
        }

        public double CountMatches(RuleSet rules, bool onlyMatches) {
            //The ruleset matcher is basically dataset 2 all over again.
            if (memoizeRules.Count > 100000)
                memoizeRules.Clear();
            if (!memoizeRules.ContainsKey(rules) || onlyMatches) {
                int matched = 0;
                foreach (var match in matches) {
                    //If we have a match, check it. If it's right, add a point.
                    Tuple<Condition, Concequence> rule = rules.Rules.FirstOrDefault(r => r.Item1.Matches(match.Key));
                    if (rule == null)
                        continue;
                    if (rule.Item2.Value == match.Value) {
                        matched++;
                    }
                }
                if (onlyMatches)
                    return matched;
                //Take the length off the points, to add some pressure to minimize the ruleset.
                memoizeRules[rules] = matched - rules.Rules.Count;
            }
            return memoizeRules[rules] > 0 ? memoizeRules[rules] : 0;
        }

        public double CountMatches(RuleSet rules) {
            return CountMatches(rules, false);
        }
    }
}