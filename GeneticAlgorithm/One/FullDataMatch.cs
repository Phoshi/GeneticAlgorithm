using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithm.GeneticAlgorithm;

namespace GeneticAlgorithm.One {
    /// <summary>
    /// Performs matching duties on bitstrings.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class FullDataMatch<T> {
        private readonly Dictionary<int, int> matches;

        public FullDataMatch(Dictionary<int, int> dict) {
            matches = dict;
        }

        public double CountMatches(IIndividual<T> individual) {
            //This is an extremely straightforward matcher. We just compare bitstrings. There's not really much nuance.
            var index = 0;

            return individual.Genotype.Count(bit => (matches[index++].Equals(1)).Equals(bit));
        }
    }
}