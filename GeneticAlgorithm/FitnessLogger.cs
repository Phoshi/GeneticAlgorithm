using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm {
    class FitnessLogger {
        private readonly List<Tuple<double, double, double, double>> fitness = new List<Tuple<double, double, double, double>>();

        public List<Tuple<double, double, double, double>> Fitness {
            get { return fitness; }
        }

        public void LogFitness(double max, double average, double testMax = 0, double testAverage = 0) {
            fitness.Add(Tuple.Create(max, average, testMax, testAverage));
        }

        public void Save(string filepath) {
            var csv = fitness.Select(t=>t.Item1 + ", " + t.Item2 + ", " + t.Item3 + ", " + t.Item4);
            File.WriteAllLines(filepath, new []{"Training Best, Training Average, Test Best, Test Average"}.Concat(csv));
        }
    }

    class AverageLogger {
        private readonly List<int> fitness = new List<int>();

        public List<int> Fitness {
            get { return fitness; }
        }

        public void LogFitness(int numGenerations) {
            fitness.Add(numGenerations);
        }

        public void Save(string filepath) {
            File.WriteAllLines(filepath, fitness.Select(fit=>fit.ToString()));
        }
    }
}
