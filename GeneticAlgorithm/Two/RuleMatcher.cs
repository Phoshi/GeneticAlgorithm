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
            //Matcher here is a little more complex, and memoised, because RAM is cheap and processing time is not.
            if (!cache.ContainsKey(individual) || onlyCorrect) {
                if (cache.Count > 100000) {
                    cache.Clear();
                }

                int numberCorrect = 0;
                int score = 0;
                foreach (var rule in individual.Genotype) {
                    //NumberMatched has zero semantics. It is used nowhere. It is purely a debugging/pretty output tool.
                    rule.NumberMatched = 0;
                }
                foreach (var mapping in mappings) {
                    //Find a match. If we have a match, increase the score.
                    var match = individual.Genotype.FirstOrDefault(rule => rule.Matches(mapping.Key));
                    if (match != null) {
                        if (match.ExpectedResult == (mapping.Value == 1)) {
                            match.NumberMatched++;
                            numberCorrect++;
                            score += 20;  //This number and the length multiplier balance to adjust the relative pressure to grow and shrink.
                        }
                    }
                }

                if (onlyCorrect)
                    return numberCorrect;
                var result = score
                             - (individual.Genotype.Length*1);
                var toReturn = result > 0 ? result : 0;
                cache[individual] = toReturn;
            }
            return cache[individual];
        }

        public double CountMatches(IIndividual<Rule> individual) {
            return CountMatches(individual, false);
        }
    }
}