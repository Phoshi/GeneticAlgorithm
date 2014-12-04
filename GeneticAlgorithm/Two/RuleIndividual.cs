using System;
using System.Collections.Generic;
using System.Linq;
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
            //Making a rule population is easy too, we just make totally random rules.
            rules = new List<Rule>();
            for (int i = 0; i < size; i++) {
                rules.Add(NewRule(rng));
            }
        }

        private Rule NewRule(Random rng) {
            //A totally random rule is comprised evenly of each character in our alphabet.
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
            //Rule crossover is still single-point, but.
            //This structure is dumb, I don't know what I was thinking when I did it. Go look at dataset 3's ruleset for something sane.

            //This thing, though, is still single-point crossover. We take the rule at point and cross over around that, but on that rule we use the point
            //as a seed to cross over inside that rule. Upshot is we get intra-rule crossover, but the downside is that each point always crosses over 
            //in the same part of the rule, which is pretty dumb, but unavoidable due to the bad structure here.
            //Protip: Your individual encoder function should not be the identity function. Do not program while tired.
            return new RuleIndividual(rules.Take(point-1).Concat(new []{rules[point].Crossover(other.Genotype[point], point)}).Concat(other.Genotype.Skip(point)));
        }

        public Rule[] Genotype { get { return rules.ToArray(); } }

        private static long _count, _sum;

        public IIndividual<Rule> Mutate(double chance, Random rng) {
            //Mutation is unnecessarily complicated due to the bad structure.
            //Seriously, this /works/, but go look at dataset 3. 
            //This is all theoretically sound, but the implementation sucks.
            const bool debug = false;
            var mutations = 0;
            var newRules = new List<Rule>();
            foreach (var rule in rules) {
                var newRule = rule.Match;
                var newExpected = rule.ExpectedResult;
                for (int i = 0; i < newRule.Length; i++) {
                    if (rng.NextDouble() < chance / (newRule.Length+1)) {
                        //Every bit of every rule has an adjusted chance to mutate
                        //This chance works out to about 1 mutation per run.
                        newRule[i] = (Trit) (((int)newRule[i] + rng.Next(1,3)) % 3);
                        mutations++;
                        if (debug) Console.Write("Bitflip-");
                    }
                }
                if (rng.NextDouble() < chance / (newRule.Length+1)) {
                    //We give a seperate check for the class flip.
                    newExpected = !newExpected;
                    mutations++;
                    if (debug) Console.Write("Expectedflip-");
                }
                newRules.Add(new Rule(newExpected, newRule));
            }
            //and more checks for variable ruleset size support
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

            _count++;
            _sum += mutations;
            if (debug) Console.WriteLine(_sum/(double)_count);
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
