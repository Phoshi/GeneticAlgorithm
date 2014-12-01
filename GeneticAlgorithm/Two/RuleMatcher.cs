using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithm.GeneticAlgorithm;

namespace GeneticAlgorithm.Two {
    class RuleMatcher {
        private readonly Dictionary<string, int> mappings;
        private readonly ConcurrentDictionary<IIndividual<Rule>, double> cache = new ConcurrentDictionary<IIndividual<Rule>, double>(); 

        public RuleMatcher(Dictionary<string, int> mappings) {
            this.mappings = mappings;
        }

        public double CountMatches(IIndividual<Rule> individual, bool onlyCorrect) {
            if (!cache.ContainsKey(individual) || onlyCorrect) {
                if (cache.Count > 100000) {
                    cache.Clear();
                }

                int numberCorrect = 0;
                int score = 0;
                foreach (var rule in individual.Genotype) {
                    rule.NumberMatched = 0;
                }
                foreach (var mapping in mappings) {
                    var match = individual.Genotype.FirstOrDefault(rule => rule.Matches(mapping.Key));
                    if (match != null) {
                        score += 0;
                        if (match.ExpectedResult == (mapping.Value == 1)) {
                            match.NumberMatched++;
                            numberCorrect++;
                            score += 20;
                        }
                    }
                }

                if (onlyCorrect)
                    return numberCorrect;
                var result = score
                             - (individual.Genotype.Length*1);
                var toReturn = result > 0 ? result : 0;
                //+ (individual.Genotype.Sum(rule => Math.Pow(rule.NumberMatched, 1.2)) / individual.Genotype.Length);
                //+ ((individual.Genotype.Sum(rule=>rule.Match.Count(trit=>trit == Trit.Wildcard)) / (individual.Genotype.Length*100)));
                cache[individual] = toReturn;
            }
            return cache[individual];
        }

        public double CountMatches(IIndividual<Rule> individual) {
            return CountMatches(individual, false);
        }
    }
}