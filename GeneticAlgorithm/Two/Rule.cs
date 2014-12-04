using System;
using System.Linq;

namespace GeneticAlgorithm.Two {
    /// <summary>
    /// Rule type represents one rule
    /// </summary>
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

            //We just want to check if the 1s and 0s match
            //Wildcards implicitly match
            for (int i = 0; i < str.Length; i++) {
                if (match[i] == Trit.On && str[i] != '1') return false;
                if (match[i] == Trit.Off && str[i] != '0') return false;
            }
            return true;
        }

        public Rule Crossover(Rule other, int seed) {
            //Intra-rule crossover is a bit of a hack, but should produce not-too-obviously deterministic behaviour.
            //It's like Inter-rule crossover, except smaller and done because ruleset 2 is structured poorly.
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
