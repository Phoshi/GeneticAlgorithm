using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeneticAlgorithm.GeneticAlgorithm {
    internal class Population<TOutput, TAlphabet> {
        private readonly Random rng = new Random();
        private List<IIndividual<TAlphabet>> individuals = new List<IIndividual<TAlphabet>>();

        private readonly Func<IIndividual<TAlphabet>, TOutput> encodeFunc;
        private readonly Func<TOutput, double> fitnessFunc;

        private Func<IIndividual<TAlphabet>, double> selector;

        public Population(int populationSize, int individualSize, Func<int, Random, IIndividual<TAlphabet>> maker, Func<IIndividual<TAlphabet>, TOutput> encode, Func<TOutput, double> fitnessFunc) {
            for (int i = 0; i < populationSize; i++) {
                individuals.Add(maker(individualSize, rng));
            }

            this.encodeFunc = encode;
            this.fitnessFunc = fitnessFunc;
            this.selector = individual => fitnessFunc(encodeFunc(individual));
        }

        public IList<TOutput> Generation(int number) {
            for (var i = 0; i < number; i++) {
                Generation();
            }
            return Generation();
        } 

        public IList<TOutput> Generation() {
            var nextGenerationCandidates = new List<IIndividual<TAlphabet>>();

            
            var currentBest = individuals.AsParallel().OrderByDescending(selector).Take(individuals.Count / 10).ToList();
            while (individuals.Count > 0) {
                var individual1 = individuals[0];
                var individual2 = individuals[1];

                individuals.RemoveRange(0, 2);

                var point = rng.Next(1, Math.Min(individual1.Genotype.Length, individual2.Genotype.Length));

                if (rng.NextDouble() < 0.6) {
                    nextGenerationCandidates.Add(
                        individual1.Crossover(individual2, point).Mutate(1.0d/individual2.Genotype.Length, rng));
                    nextGenerationCandidates.Add(
                        individual2.Crossover(individual1, point).Mutate(1.0d/individual1.Genotype.Length, rng));
                }
                else {
                    nextGenerationCandidates.Add(
                        individual1.Mutate(1.0d / individual1.Genotype.Length, rng));
                    nextGenerationCandidates.Add(
                        individual2.Mutate(1.0d / individual2.Genotype.Length, rng));
                }
            }
            var nextGeneration = ProportionalSelection(nextGenerationCandidates, selector, nextGenerationCandidates.Count - (nextGenerationCandidates.Count / 10), rng);
            nextGeneration.AddRange(currentBest);

            individuals = nextGeneration;

            return nextGeneration.Select(encodeFunc).ToList();
        }

        private List<T> ProportionalSelection<T>(IEnumerable<T> source, Func<T, double> selector, int numberToSelect, Random rng) {
            var values = source.AsParallel().Select(elem => new { element = elem, value = selector(elem) }).ToList();

            var sum = values.Sum(val => val.value);

            var returnList = new List<T>();
            for (var i = 0; i < numberToSelect; i++) {
                var selection = rng.NextDouble() * sum;
                foreach (var element in values) {
                    selection -= element.value;
                    if (selection <= 0) {
                        returnList.Add(element.element);
                        break;
                    }
                }
            }

            return returnList;
        }


        public override string ToString()
        {
            return individuals.Select(i => i.ToString()).Aggregate((a, b) => a + b);
        }
    }
}