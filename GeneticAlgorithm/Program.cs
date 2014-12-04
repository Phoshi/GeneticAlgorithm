using System;
using GeneticAlgorithm.One;
using GeneticAlgorithm.Three;
using GeneticAlgorithm.Two;

namespace GeneticAlgorithm
{
    class Program {
        static void Main(string[] args) {
            //Entry point simply wants to kick off a run. There isn't any real configuration to do.
            //So we just run each in turn, and print out the results.
            var one = new SolveOne();
            Console.WriteLine(one.Solve());

            var two = new SolveTwo();
            Console.WriteLine(two.Solve());

            var three = new SolveThree();
            Console.WriteLine(three.Solve());

            Console.Read();
        }
    }
}
