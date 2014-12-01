using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using GeneticAlgorithm.GeneticAlgorithm;
using GeneticAlgorithm.One;
using GeneticAlgorithm.Two;

namespace GeneticAlgorithm.Three {
    class RuleIndividual :IIndividual<double> {
        private const int ruleSize = 6*2 + 1;
        private readonly IReadOnlyList<double> rules;

        public RuleIndividual(int num, Random rng) {
            var newRules = new List<double>();
            for (int i = 0; i < num; i++) {
                newRules.Add(newRuleState(rng));
            }

            rules = newRules;
        }

        public RuleIndividual(IEnumerable<double> newRules) {
            rules = new List<double>(newRules);
        }

        private double newRuleState(Random rng) {
            var rand = rng.NextDouble();
            return rand;
        }

        public IIndividual<double> Crossover(IIndividual<double> other, int point) {
            var len = other.Genotype.Count();
            var newGenotype = new double[len];
            Array.Copy(Genotype, newGenotype, point);
            Array.Copy(other.Genotype, point, newGenotype, point, len - point);

            return new RuleIndividual(newGenotype);
        }

        public double[] Genotype { get { return rules.ToArray(); } }
        public IIndividual<double> Mutate(double chance, Random rng) {
            var newRules = new List<double>(rules);
            for (int i = 0; i < newRules.Count; i++) {
                if (rng.NextDouble() < chance) {
                    newRules[i] += NextGaussian(rng, 0.05);
                    if (newRules[i] > 1)
                        newRules[i] = 1;
                    if (newRules[i] < 0)
                        newRules[i] = 0;
                }
            }

            if (rng.NextDouble() < 1/50d) {
                newRules = Shuffle(newRules, rng);
            }

            if (rng.NextDouble() < 1/30d) {
                newRules = AddRule(newRules, rng);
            }

            if (rng.NextDouble() < 1/100d) {
                newRules = RemoveRule(newRules, rng);
            }
            return new RuleIndividual(newRules);
        }

        private static double NextGaussian(Random r, double mu = 0, double sigma = 1) {
            var u1 = r.NextDouble();
            var u2 = r.NextDouble();

            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2);

            var randNormal = mu + sigma * randStdNormal;

            return randNormal;
        }

        private List<double> Shuffle(List<double> list, Random rng) {
            var newList = new List<double>();
            var numRules = list.Count/7;
            var indexes = Enumerable.Range(0, numRules).OrderBy(num=>rng.Next());
            foreach(var index in indexes){
                newList.AddRange(list.Skip(index * ruleSize).Take(ruleSize));
            }
            return newList;
        }

        private List<double> AddRule(List<double> list, Random rng) {
            var newRule = Enumerable.Range(0, ruleSize).Select(n => newRuleState(rng));
            return newRule.Concat(list).ToList();
        }

        private List<double> RemoveRule(List<double> list, Random rng) {
            var index = rng.Next(list.Count/ruleSize);

            return list.Take(index*ruleSize).Concat(list.Skip((index + 1)*ruleSize)).ToList();
        } 
    }
}
