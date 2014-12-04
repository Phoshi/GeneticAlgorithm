using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithm.GeneticAlgorithm {
    /// <summary>
    /// A Population type to implement a genetic algorithm
    /// </summary>
    /// <typeparam name="TOutput">The type which is to be used for output and in the fitness function</typeparam>
    /// <typeparam name="TAlphabet">The alphabet type of the individuals</typeparam>
    internal class Population<TOutput, TAlphabet> {
        private readonly Random rng = new Random();
        private List<IIndividual<TAlphabet>> individuals = new List<IIndividual<TAlphabet>>();

        private readonly Func<IIndividual<TAlphabet>, TOutput> encodeFunc;

        private Func<IIndividual<TAlphabet>, double> selector;

        /// <summary>
        /// A multiplier for multiplication. When set to 1, should result in 1 mutation per individual.
        /// </summary>
        public double MutationMultiplier = 1;
        /// <summary>
        /// The crossover chance. 0 is 0% and 1 is 100%.
        /// </summary>
        public double CrossoverChance = 0.6;

        /// <summary>
        /// Instantiates a new population with the given configuration.
        /// </summary>
        /// <param name="populationSize">The size of the population. This does not vary.</param>
        /// <param name="individualSize">The length of the starting individuals. This may vary.</param>
        /// <param name="maker">A function which takes a size and a random number generator and produces a new individual</param>
        /// <param name="encode">A function which takes an individual and produces an output type</param>
        /// <param name="fitnessFunc">A function which takes an output type and determines its fitness</param>
        public Population(int populationSize, int individualSize, Func<int, Random, IIndividual<TAlphabet>> maker, Func<IIndividual<TAlphabet>, TOutput> encode, Func<TOutput, double> fitnessFunc) {
            //Make a new population
            for (int i = 0; i < populationSize; i++) {
                individuals.Add(maker(individualSize, rng));
            }

            encodeFunc = encode;
            selector = individual => fitnessFunc(encodeFunc(individual)); //Fitness function can be wrapped in the encoder for convenience.
        }

        /// <summary>
        /// Run n generations, returning the results of the most recent.
        /// </summary>
        /// <param name="number">The number of generations to run</param>
        /// <returns>The results of the latest generation</returns>
        public IList<TOutput> Generation(int number) {
            for (var i = 0; i < number; i++) {
                Generation();
            }
            return Generation();
        } 

        /// <summary>
        /// Runs one generation, returning the results
        /// </summary>
        /// <returns>The results of this generation</returns>
        public IList<TOutput> Generation() {
            var nextGenerationCandidates = new List<IIndividual<TAlphabet>>();
            
            //Take the top ten percent of this generation and keep them around.
            var currentBest = individuals.AsParallel().OrderByDescending(selector).Take(individuals.Count / 10).ToList();

            //Individuals is assumed to be ordered semi-randomly, as only we touch it.
            //Semi randomly meaning that it's all random, except for the last ten percent, which is the top ten percent of last generation ordered randomly.
            while (individuals.Count > 0) {
                //Because the previous generation is randomly ordered, we can just take pairs as they come.
                var individual1 = individuals[0];
                var individual2 = individuals[1];

                individuals.RemoveRange(0, 2);

                if (rng.NextDouble() < CrossoverChance) {
                    //If we're crossing over, we need to do that, and then mutate.
                    var crossoverPoint = rng.Next(1, Math.Min(individual1.Genotype.Length, individual2.Genotype.Length));
                    nextGenerationCandidates.Add(
                        individual1.Crossover(individual2, crossoverPoint)
                            .Mutate(
                                MutationMultiplier * 1.0d/individual2.Genotype.Length, 
                                rng));
                    nextGenerationCandidates.Add(
                        individual2.Crossover(individual1, crossoverPoint)
                            .Mutate(
                                MutationMultiplier * 1.0d/individual1.Genotype.Length, 
                                rng));
                }
                else {
                    //If we are not crossing over, we just want to mutate.
                    nextGenerationCandidates.Add(
                        individual1.Mutate(
                            MutationMultiplier * 1.0d / individual1.Genotype.Length, 
                            rng));
                    nextGenerationCandidates.Add(
                        individual2.Mutate(
                            MutationMultiplier * 1.0d / individual2.Genotype.Length, 
                            rng));
                }
            }

            //Once we've gone through the entire generation to create another, we want to select the next generation.
            //We do this via proportional selection, but only select 90% of our population size.
            var nextGeneration = ProportionalSelection(nextGenerationCandidates, selector, nextGenerationCandidates.Count - (nextGenerationCandidates.Count / 10), rng);

            //The top ten percent of this generation is simply added back, ensuring we never lose the best individuals
            nextGeneration.AddRange(currentBest);

            //And our generation is complete. The next generation is now the current generation.
            individuals = nextGeneration;

            //We return the current generation transformed into the output type.
            return nextGeneration.Select(encodeFunc).ToList();
        }

        /// <summary>
        /// Implements proportional selection.
        /// </summary>
        /// <typeparam name="T">The output type</typeparam>
        /// <param name="source">The source to select from</param>
        /// <param name="fitnessFunction">The fitness function</param>
        /// <param name="numberToSelect">The number of selections necessary</param>
        /// <param name="random">A reliable random number generator</param>
        /// <returns>A list of size numberToSelect which probably contains elements from source with high fitness values more than low ones</returns>
        private List<T> ProportionalSelection<T>(IEnumerable<T> source, Func<T, double> fitnessFunction, int numberToSelect, Random random) {
            //Map the values into their fitnesses
            var values = source.AsParallel().Select(elem => new { element = elem, value = fitnessFunction(elem) }).ToList();

            //Find the total fitness
            var sum = values.Sum(val => val.value);

            var returnList = new List<T>();
            for (var i = 0; i < numberToSelect; i++) {
                //For every element to return, we need to pick one. NextDouble()*sum will give us a number evenly
                //distributed between 0 and the fitness sum, which means that if we use that as an index it's more likely to 
                //land on things with a larger fitness.
                var selection = random.NextDouble() * sum;
                foreach (var element in values) {
                    //So for each element we subtract the fitness
                    selection -= element.value;
                    if (selection <= 0) {
                        //and when we hit zero, we've found our element.
                        returnList.Add(element.element);
                        break;
                    }
                }
            }

            return returnList;
        }

        /// <summary>
        /// Stringifies a population for human consumption.
        /// </summary>
        /// <returns>A string representing this population.</returns>
        public override string ToString()
        {
            return individuals.Select(i => i.ToString()).Aggregate((a, b) => a + b);
        }
    }
}