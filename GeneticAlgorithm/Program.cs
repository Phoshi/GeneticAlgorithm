using System;
using GeneticAlgorithm.One;
using GeneticAlgorithm.Three;
using GeneticAlgorithm.Two;

namespace GeneticAlgorithm
{
    class Program {
        static void Main(string[] args) {
            var oneLogger = new FitnessLogger();
            var twoLogger = new FitnessLogger();
            var netLogger = new FitnessLogger();
            var rulLogger = new FitnessLogger();
            

            DateTime start = DateTime.Now;
            var one = new SolveOne(oneLogger);
            var oneSolution = one.Solve();
            Console.WriteLine(oneSolution);
            Console.WriteLine(DateTime.Now - start);

            start = DateTime.Now;
            var two = new SolveTwo(twoLogger);
            Console.WriteLine(two.Solve());
            Console.WriteLine(DateTime.Now - start);

            start = DateTime.Now;
            var three = new SolveThree(rulLogger, netLogger);
            Console.WriteLine(three.Solve());
            Console.WriteLine(DateTime.Now - start);

            oneLogger.Save("one.csv");
            twoLogger.Save("two.csv");
            netLogger.Save("net.csv");
            rulLogger.Save("rul.csv");

            Console.Read();
        }
    }
}
