using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithm.GeneticAlgorithm;

namespace GeneticAlgorithm.One
{
    class FullDataMatch<T>
    {
        private readonly Dictionary<int, int> matches;

        public FullDataMatch(Dictionary<int, int> dict)
        {
            this.matches = dict;
        }

        public double CountMatches(IIndividual<T> individual)
        {
            var index = 0;

            return individual.Genotype.Count(bit => (matches[index++].Equals(1)).Equals(bit));
        }
    }
}