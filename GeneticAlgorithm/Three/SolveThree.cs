using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticAlgorithm.GeneticAlgorithm;
using GeneticAlgorithm.One;

namespace GeneticAlgorithm.Three {
    class SolveThree {
        private readonly FitnessLogger loggerRule;
        private readonly FitnessLogger loggerNet;

        public SolveThree(FitnessLogger loggerRule, FitnessLogger loggerNet) {
            this.loggerRule = loggerRule;
            this.loggerNet = loggerNet;
        }

        public RuleSet Solve() {
            var totalData = 2000;
            var trainSet = totalData*0.8;
            var testSet = totalData*0.2;
            var matcher = GetMatcher(@"Data\data3.txt", (int) (trainSet), 0);
            var test = GetMatcher(@"Data\data3.txt", (int) (testSet), (int) (trainSet));

            //Hidden nodes have $#input + 1 bias weights each
            //Second hidden have $#hidden + 1 bias weights each
            //Output nodes have $#sndHidden + 1 bias weights each

            //Thus, ($#hidden * ($#input + 1)) + ($#sndHidden * ($#hidden + 1)) + ($#output * ($#sndHidden + 1)
            var numInput = 6;
            var numHidden = 6;
            var numSecondHidden = 3;
            var numOutput = 2;
            var population = new Population<NeuralNetwork2, double>(100, (numHidden * (numInput+1)) + (numSecondHidden * (numHidden + 1)) + (numOutput * (numSecondHidden + 1)),
                (length, rng) => new DoubleIndividual(length, rng),
                individual => new NeuralNetwork2(individual.Genotype, numInput, numHidden, numSecondHidden, numOutput), matcher.CountMatches);

            var RulePopulation = new Population<RuleSet, double>(100, 13*((6*2)+1),
                (num, random) => new RuleIndividual(num, random), individual => new RuleSet(individual.Genotype),
                matcher.CountMatches);

            NeuralNetwork2 allTimeBest = null;
            RuleSet allTimeBestRule = null;

            var i = 0;
            while (true) {
                var netGeneration = population.Generation();    
                var ruleGeneration = RulePopulation.Generation();

                var netBest = netGeneration.AsParallel().OrderBy(matcher.CountMatches).Last();
                var ruleBest = ruleGeneration.AsParallel().OrderBy(matcher.CountMatches).Last();

                var netAverage = netGeneration.AsParallel().Select(net=>matcher.CountMatches(net, true)).Sum()/netGeneration.Count;
                var ruleAverage = ruleGeneration.AsParallel().Select(rul=>matcher.CountMatches(rul, true)).Sum() / ruleGeneration.Count;
                var netTestAverage = netGeneration.AsParallel().Select(net => test.CountMatches(net, true)).Sum() / netGeneration.Count;
                var ruleTestAverage = ruleGeneration.AsParallel().Select(rul => test.CountMatches(rul, true)).Sum() / ruleGeneration.Count;

                loggerNet.LogFitness(matcher.CountMatches(netBest, true), netAverage, test.CountMatches(netBest, true), netTestAverage);
                loggerRule.LogFitness(matcher.CountMatches(ruleBest, true), ruleAverage, test.CountMatches(ruleBest, true), ruleTestAverage);


                if (allTimeBest == null || matcher.CountMatches(allTimeBest) < matcher.CountMatches(netBest)) {
                    allTimeBest = netBest;
                }
                if (allTimeBestRule == null || matcher.CountMatches(allTimeBestRule) < matcher.CountMatches(ruleBest)) {
                    allTimeBestRule = ruleBest;
                }

                if ((int)(matcher.CountMatches(ruleBest, true)) >= (int)trainSet * 0.9) {
                    return ruleBest;
                }

                if (i > 3000) {
                    return ruleBest;
                }

                i++;
            }
        }


        public static NeuralNetworkMatch GetMatcher(string path, int take, int skip) {
            var dict = new Dictionary<IEnumerable<double>, int>();
            var file = File.ReadAllLines(path);
            foreach (var line in file.Skip(skip).Take(take)) {
                var pair = line.Split(new[] { ' ' });

                var elements = pair.Select(Convert.ToDouble).ToList();

                dict[elements.Take(pair.Count()-1)] = (int)elements.Last();
            }

            return new NeuralNetworkMatch(dict);
        }
    }

    internal class NeuralNetworkMatch {
        private readonly Dictionary<IEnumerable<double>, int> matches;

        private readonly ConcurrentDictionary<NeuralNetwork2, double> memoize = new ConcurrentDictionary<NeuralNetwork2, double>();
        private readonly ConcurrentDictionary<RuleSet, double> memoizeRules = new ConcurrentDictionary<RuleSet, double>(); 

        public NeuralNetworkMatch(Dictionary<IEnumerable<double>, int> matches) {
            this.matches = matches;
        }

        public double CountMatches(NeuralNetwork2 net, bool fullMatchOnly) {
            if (memoize.Count > 100000)
                memoize.Clear();
            if (!memoize.ContainsKey(net) || fullMatchOnly) {
                var matched = 0d;
                var score = 0d;
                foreach (var pair in matches) {
                    var output = net.Output(pair.Key);
                    int normalisedOutput = -1;
                    if (output.First() > 0.5) {
                        normalisedOutput = 1;
                    }
                    else if (output.Skip(1).First() > 0.5) {
                        normalisedOutput = 0;
                    }

                    if ((int) normalisedOutput == (int) pair.Value) {
                        matched += 1;
                        score += 1;
                    }
                    else {
                        //score += 1 - Math.Abs(pair.Value - output.First());
                    }
                }
                if (fullMatchOnly)
                    return matched;
                memoize[net] = score;
            }
            return memoize[net];
        }

        public double CountMatches(NeuralNetwork2 net)
        {
            return CountMatches(net, false);
        }

        public double CountMatches(RuleSet rules, bool onlyMatches) {
            if (memoizeRules.Count > 100000)
                memoizeRules.Clear();
            if (!memoizeRules.ContainsKey(rules) || onlyMatches) {
                var matched = 0;
                foreach (var match in matches) {
                    var rule = rules.Rules.FirstOrDefault(r => r.Item1.Matches(match.Key));
                    if (rule == null)
                        continue;
                    if (rule.Item2.Value == match.Value) {
                        matched++;
                    }
                }
                if (onlyMatches)
                    return matched;
                memoizeRules[rules] = matched - rules.Rules.Count;
            }
            return memoizeRules[rules] > 0 ? memoizeRules[rules] : 0;
        }

        public double CountMatches(RuleSet rules) {
            return CountMatches(rules, false);
        }

    }
}
