using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithm.Three {
    /// <summary>
    /// A ruleset represents a set of real-valued rules
    /// </summary>
    class RuleSet {
        private readonly List<Tuple<Condition, Concequence>> rules = new List<Tuple<Condition, Concequence>>();

        public List<Tuple<Condition, Concequence>> Rules {
            get { return rules; }
        }

        public RuleSet(IEnumerable<double> structure) {
            //We just instantiate this from a double structure of ConditionSize + 1 rules.
            const int conditionSize = 6 * 2;
            var listStructure = new List<double>(structure);
            while (listStructure.Count > 0) {
                var condition = listStructure.Take(conditionSize).ToArray();
                var concequence = listStructure.Skip(conditionSize).First();

                listStructure.RemoveRange(0, conditionSize + 1);

                rules.Add(Tuple.Create(new Condition(condition), new Concequence(concequence)));
            }
        }

        public override string ToString() {
            return string.Format("{1} rules: {0}", string.Join("\n", Rules.Select(tuple=>tuple.Item1 + "->" + tuple.Item2)), rules.Count);
        }
    }

    internal class Concequence {
        public int Value { get; set; }

        public Concequence(double concequence) {
            //We round the concequence, as we're dealing with a binary class.
            Value = (int) concequence > 0.5 ? 1 : 0;
        }

        public override string ToString() {
            return string.Format("{0}", Value);
        }
    }

    internal class Condition {
        public double[] Value { get; set; }

        public Condition(double[] value) {
            Value = value;
        }

        public bool Matches(IEnumerable<double> input) {
            //We just parse the condition doublestring simply.
            var conditionIndex = 0;
            foreach (var inputValue in input) {
                var minBound = Value[conditionIndex++];
                var maxBound = Value[conditionIndex++];

                //If the bounds are the wrong way around, we flip them here. Without this, performance is impacted!
                if (minBound > maxBound) {
                    var temp = minBound;
                    minBound = maxBound;
                    maxBound = temp;
                }

                if (!(inputValue > minBound && inputValue < maxBound)) {
                    return false;
                }
            }
            return true;
        }

        public override string ToString() {
            //For output purposes, we want to actually pair these up. 
            //So match each element with its index.
            var pairs = Value.Select((val, i) => new {val, i}).ToList();
            //Then zip the odd pairs up with the even pairs!
            var paired = pairs.Where(o => o.i%2 == 0).Zip(pairs.Where(o => o.i%2 != 0), (o1, o2) => o1.val + "-" + o2.val);
            return string.Format("{0}", string.Join("\t", paired));
        }
    }
}
