using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithm.GeneticAlgorithm;

namespace GeneticAlgorithm.Three {
    /// <summary>
    /// A rule individual is a double individual specifically tailored for a ruleset
    /// </summary>
    class RuleIndividual :IIndividual<double> {
        private const int RuleSize = 6*2 + 1; //The size of each rule
        private readonly IReadOnlyList<double> rules;

        public RuleIndividual(int num, Random rng) {
            //Making a new rule individual is just like the other doubleindividual, but 0-1. 
            var newRules = new List<double>();
            for (int i = 0; i < num; i++) {
                newRules.Add(rng.NextDouble());
            }

            rules = newRules;
        }

        public RuleIndividual(IEnumerable<double> newRules) {
            rules = new List<double>(newRules);
        }

        private double newRuleState(Random rng) {
            var rand = rng.NextDouble();
            return rand;
        }

        public IIndividual<double> Crossover(IIndividual<double> other, int point) {
            //Crossover is just standard single-point crossover, with some sizing calculations for variable length rules.
            var len = other.Genotype.Count();
            var newGenotype = new double[len];
            Array.Copy(Genotype, newGenotype, point);
            Array.Copy(other.Genotype, point, newGenotype, point, len - point);

            return new RuleIndividual(newGenotype);
        }

        public double[] Genotype { get { return rules.ToArray(); } }
        public IIndividual<double> Mutate(double chance, Random rng) {
            //Mutation gives a chance for each element to creep, and some chances to support variable rulesets.
            var newRules = new List<double>(rules);
            for (int i = 0; i < newRules.Count; i++) {
                if (rng.NextDouble() < chance) {
                    //We use a large gaussian parameter because large jumps can be very helpful in this dataset.
                    newRules[i] += NextGaussian(rng, 1);
                    //Then constrain to our range.
                    if (newRules[i] > 1)
                        newRules[i] = 1;
                    if (newRules[i] < 0)
                        newRules[i] = 0;
                }
            }

            //And give some chances to mutate the rules on a larger scale.
            if (rng.NextDouble() < 1/50d) {
                newRules = Shuffle(newRules, rng);
            }

            if (rng.NextDouble() < 1/30d) {
                newRules = AddRule(newRules, rng);
            }

            if (rng.NextDouble() < 1/100d) {
                newRules = RemoveRule(newRules, rng);
            }
            return new RuleIndividual(newRules);
        }

        private static double NextGaussian(Random r, double mu = 0, double sigma = 1) {
            //Taken from http://stackoverflow.com/a/218600/160783
            var u1 = r.NextDouble();
            var u2 = r.NextDouble();

            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2);

            var randNormal = mu + sigma * randStdNormal;

            return randNormal;
        }

        private List<double> Shuffle(List<double> list, Random rng) {
            //Shuffling just randomises rule order
            var newList = new List<double>();
            var numRules = list.Count/7;
            var indexes = Enumerable.Range(0, numRules).OrderBy(num=>rng.Next());
            foreach(var index in indexes){
                newList.AddRange(list.Skip(index * RuleSize).Take(RuleSize));
            }
            return newList;
        }

        private List<double> AddRule(IEnumerable<double> list, Random rng) {
            //Adding a rule adds one rule.
            var newRule = Enumerable.Range(0, RuleSize).Select(n => newRuleState(rng));
            return newRule.Concat(list).ToList();
        }

        private List<double> RemoveRule(List<double> list, Random rng) {
            //Removing a rule removes one rule.
            var index = rng.Next(list.Count/RuleSize);

            return list.Take(index*RuleSize).Concat(list.Skip((index + 1)*RuleSize)).ToList();
        } 
    }
}
