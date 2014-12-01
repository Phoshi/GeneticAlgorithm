using System;
using GeneticAlgorithm.One;
using GeneticAlgorithm.Three;
using GeneticAlgorithm.Two;

namespace GeneticAlgorithm
{
    class Program {
        static void Main(string[] args) {
            DateTime start = DateTime.Now;
            /*var one = new SolveOne();
            var oneSolution = one.Solve();
            Console.WriteLine(oneSolution);
            Console.WriteLine(DateTime.Now - start);

            start = DateTime.Now;
            var two = new SolveTwo();
            Console.WriteLine(two.Solve());
            Console.WriteLine(DateTime.Now - start);*/

            start = DateTime.Now;
            var three = new SolveThree();
            Console.WriteLine(three.Solve());
            Console.WriteLine(DateTime.Now - start);

            Console.Read();
        }
    }
}
