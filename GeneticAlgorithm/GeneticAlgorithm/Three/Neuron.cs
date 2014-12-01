using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.Three {
    class Neuron : INeuron {
        protected bool Equals(Neuron other) {
            return Equals(inputs, other.inputs) && Equals(weights, other.weights);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (inputs != null ? inputs.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (weights != null ? weights.GetHashCode() : 0);
                return hashCode;
            }
        }

        private List<INeuron> inputs;
        private Dictionary<INeuron, double> weights;

        private double error = 0;
        private double output = 0;

        public Neuron(IEnumerable<INeuron> inputs, Dictionary<INeuron, double> weights,
            Dictionary<INeuron, double> biases) {
            this.inputs = new List<INeuron>(inputs);
            this.weights = weights;
        }

        public Neuron(IEnumerable<INeuron> inputs, IList<double> settings) {

            this.inputs = new List<INeuron>(inputs);
            this.weights = settings.Zip(this.inputs, (weight, neuron) => new {neuron, weight}).ToDictionary(elem=>elem.neuron, elem=>elem.weight);
        }

        private IEnumerable<Tuple<int, T>> Enumerate<T>(IEnumerable<T> enumerable) {
            var second = enumerable as T[] ?? enumerable.ToArray();
            return Enumerable.Range(0, second.Count()).Zip(second, Tuple.Create);
        }

        public double Output() {
            var sum = inputs.Select(input => (input.Output() * weights[input])).Sum();
            output = Sigmoid(sum);
            return output;
        }

        public IDictionary<INeuron, double> Weights { get { return weights; } }

        private double Sigmoid(double result) {
            double value = 1.0 / (1.0 + Math.Pow(Math.E, -1.0 * result));
            return value;
        }

        private double Limit(double result) {
            return result > 0.0 ? 1 : 0;
        }

        public override string ToString() {
            return string.Format("[Neuron | {0}]", 
                string.Join(", ", weights.Values.Select(key=>key.ToString())));
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Neuron) obj);
        }
    }

    class InputNeuron : INeuron{
        private double input;

        public void SetInput(double input) {
            this.input = input;
        }

        public InputNeuron(double input) {
            this.input = input;
        }

        public double Output() {
            return input;
        }

        public IDictionary<INeuron, double> Weights { get {return new Dictionary<INeuron, double>();} }

        public override string ToString() {
            return "[" + input + "]";
        }
    }

    internal interface INeuron {
        double Output();

        IDictionary<INeuron, double> Weights { get; }
    }
}
