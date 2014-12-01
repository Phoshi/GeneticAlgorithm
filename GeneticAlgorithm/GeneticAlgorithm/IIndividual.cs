using System;

namespace GeneticAlgorithm.GeneticAlgorithm
{
    interface IIndividual<T>
    {
        IIndividual<T> Crossover(IIndividual<T> other, int point);
        T[] Genotype { get; }
        IIndividual<T> Mutate(double chance, Random rng);
    }
}
