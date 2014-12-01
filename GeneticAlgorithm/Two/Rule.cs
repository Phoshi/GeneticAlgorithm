using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.Two {
    class Rule {
        private readonly Trit[] match;
        private readonly bool expectedResult;

        private int matched = 0;
        public Rule(bool expectedResult, Trit[] match) {
            this.expectedResult = expectedResult;
            this.match = match;
        }

        public Trit[] Match {
            get { return (Trit[])match.Clone(); }
        }

        public bool ExpectedResult {
            get { return expectedResult; }
        }

        public int NumberMatched { get { return matched; } set { matched = value; } }

        public bool Matches(string str) {
            if (str.Length != match.Length) {
                return false;
            }

            for (int i = 0; i < str.Length; i++) {
                if (match[i] == Trit.On && str[i] != '1') return false;
                if (match[i] == Trit.Off && str[i] != '0') return false;
            }
            return true;
        }

        public Rule Crossover(Rule other, int seed) {
            var rng = new Random(seed);
            var point = rng.Next(match.Length);
            var newRule = new Trit[match.Length];
            Array.Copy(match, newRule, point);
            Array.Copy(other.Match, point, newRule, point, match.Length - point);
            return new Rule(point == match.Length-1 ? expectedResult : other.expectedResult, newRule);
        }

        public override string ToString() {
            return string.Join("", match.Select(trit=>trit.FriendlyName())) + "=>" + (expectedResult ? 1 : 0) + "(" + matched + ")";
        }
    }
}
