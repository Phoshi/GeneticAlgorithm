using System;

namespace GeneticAlgorithm.GeneticAlgorithm
{
    /// <summary>
    /// IIndividual is the individual interface, which describes what an individual must implement.
    /// All individuals must be immutable. Mutable individuals will not function as expected.
    /// </summary>
    /// <typeparam name="T">The alphabet type</typeparam>
    interface IIndividual<T>
    {
        /// <summary>
        /// Crosses this rule over with another rule. Should be the inverse of other.Crossover(this, point) when the arguments are the same. 
        /// Should always be pure, or things will probably not work as expected.
        /// </summary>
        /// <param name="other">The second parent</param>
        /// <param name="point">The point at which to cross</param>
        /// <returns>A new parent representing the crossed over set.</returns>
        IIndividual<T> Crossover(IIndividual<T> other, int point);
        /// <summary>
        /// An array of the alphabet representing the genome for this individual.
        /// </summary>
        T[] Genotype { get; }
        /// <summary>
        /// Should mutate this individual, ideally giving each character `chance` probability of mutation.
        /// For best results, when chance is equal to 1d/Genotype.Length, this should result in 1 mutation per run, on average.
        /// </summary>
        /// <param name="chance">The chance of mutating any character</param>
        /// <param name="rng">A reliable random number generator</param>
        /// <returns>The mutated individual</returns>
        IIndividual<T> Mutate(double chance, Random rng);
    }
}
