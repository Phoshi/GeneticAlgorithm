using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.Two {
    public enum Trit {
        On,
        Off,
        Wildcard,
    }

    public static class TritExtensions {
        public static bool Matches(this Trit trit, char elem) {
            if (trit == Trit.On && elem == '1')     return true;
            if (trit == Trit.Off && elem == '0')    return true;
            return trit == Trit.Wildcard;
        }

        public static string FriendlyName(this Trit trit) {
            var names = new Dictionary<Trit, string>() {
                {Trit.On, "1"},
                {Trit.Off, "0"},
                {Trit.Wildcard, "#"}
            };
            return names[trit];
        }
    }
}
