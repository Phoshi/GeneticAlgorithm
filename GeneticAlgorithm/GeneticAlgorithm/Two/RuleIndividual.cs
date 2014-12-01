using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GeneticAlgorithm.GeneticAlgorithm;

namespace GeneticAlgorithm.Two {
    class RuleIndividual : IIndividual<Rule> {
        protected bool Equals(RuleIndividual other) {
            return Equals(rules, other.rules);
        }

        public override int GetHashCode() {
            return rules.GetHashCode();
        }

        private readonly IList<Rule> rules;
        public RuleIndividual(int size, Random rng) {
            rules = new List<Rule>();
            for (int i = 0; i < size; i++) {
                rules.Add(NewRule(rng));
            }
        }

        private Rule NewRule(Random rng) {
            var result = rng.NextDouble() < 0.5;
            var match = new Trit[11];
            for (int j = 0; j < match.Length; j++) {
                var tritValue = rng.Next(3);
                if (tritValue == 0) match[j] = Trit.On;
                if (tritValue == 1) match[j] = Trit.Off;
                if (tritValue == 2) match[j] = Trit.Wildcard;
            }
            return new Rule(result, match);
        }

        private RuleIndividual(IEnumerable<Rule> newRules) {
            this.rules = new List<Rule>(newRules);
        }


        public IIndividual<Rule> Crossover(IIndividual<Rule> other, int point) {
            return new RuleIndividual(rules.Take(point-1).Concat(new []{rules[point].Crossover(other.Genotype[point], point)}).Concat(other.Genotype.Skip(point)));
        }

        public Rule[] Genotype { get { return rules.ToArray(); } }

        private static long count, sum;

        public IIndividual<Rule> Mutate(double chance, Random rng) {
            var debug = false;
            var mutations = 0;
            var newRules = new List<Rule>();
            foreach (var rule in rules) {
                var newRule = rule.Match;
                var newExpected = rule.ExpectedResult;
                if (rng.NextDouble() < 1d/rules.Count) {
                    for (int i = 0; i < newRule.Length; i++) {
                        if (rng.NextDouble() < 1d/(newRule.Length+1)) {
                            newRule[i] = (Trit) (((int)newRule[i] + rng.Next(1,3)) % 3);
                            mutations++;
                            if (debug) Console.Write("Bitflip-");
                        }
                    }
                    if (rng.NextDouble() < 1d / (newRule.Length+1)) {
                        newExpected = !newExpected;
                        mutations++;
                        if (debug) Console.Write("Expectedflip-");
                    }
                }
                newRules.Add(new Rule(newExpected, newRule));
            }
            if (rng.NextDouble() < 1/50d) {
                newRules = newRules.OrderBy(rule => rng.Next()).ToList();
                mutations++;
                if (debug) Console.Write("Shuffle-");
            }
            if (rng.NextDouble() < 1/30d) {
                newRules.Insert(0, NewRule(rng));
                mutations++;
                if (debug) Console.Write("AddRule-");
            }

            if (rng.NextDouble() < 1/200d) {
                newRules.RemoveAt(rng.Next(newRules.Count));
                mutations++;
                if (debug) Console.Write("RemoveRule-");
                if (rng.NextDouble() < 1/5d) {
                    newRules.Insert(0, NewRule(rng));
                    if (debug) Console.Write("AddRule-");
                }
            }
            if (debug) Console.WriteLine(mutations);

            count++;
            sum += mutations;
            if (debug) Console.WriteLine(sum/(double)count);
            return new RuleIndividual(newRules);
        }

        public override string ToString() {
            return rules.Count + " rules: " + string.Join(";", rules.Select(rule => rule.ToString()));
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }
    }
}
