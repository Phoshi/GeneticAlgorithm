using System;
using GeneticAlgorithm.GeneticAlgorithm;

namespace GeneticAlgorithm.One
{
    class IndividualFactory
    {
        public IIndividual<T> Make<T>(int size, Random rng)
        {
            if (typeof(T) == typeof(bool))
            {
                return (IIndividual<T>)new BoolIndividual(size, rng);
            }
            throw new InvalidOperationException();
        }
    }
}
