using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GeneticAlgorithm.Three {
    /// <summary>
    /// The neuron type does most of the heavy lifting for the single layer neural net
    /// </summary>
    class Neuron : INeuron {
        private readonly List<INeuron> inputs;
        private readonly Dictionary<INeuron, double> weights;

        private double output;

        public Neuron(IEnumerable<INeuron> inputs, Dictionary<INeuron, double> weights) {
            //If given an input dictionary, we can build this neuron easily
            this.inputs = new List<INeuron>(inputs);
            this.weights = weights;
        }

        public Neuron(IEnumerable<INeuron> inputs, IEnumerable<double> settings) {
            //If just given a list we need to format it.
            this.inputs = new List<INeuron>(inputs);
            //So we tie the inputs and weights together, then turn it into a dictionary. 
            weights = settings.Zip(this.inputs, (weight, neuron) => new {neuron, weight}).ToDictionary(elem=>elem.neuron, elem=>elem.weight);
        }

        public double Output() {
            //The output function just sums the inputs, recursively.
            var sum = 0d;
            foreach (var neuron in inputs) {
                var weight = weights[neuron];
                sum += neuron.Output()*weight;
            }
            output = Sigmoid(sum);
            return output;
        }

        public IDictionary<INeuron, double> Weights { get { return weights; } }

        private double Sigmoid(double result) {
            double value = 1.0 / (1.0 + Math.Pow(Math.E, -1.0 * result));
            return value;
        }

        public override string ToString() {
            return string.Format("[Neuron {1} | {0}]", 
                string.Join(", ", weights.Values.Select(key=>key.ToString(CultureInfo.InvariantCulture))), GetHashCode());
        }
    }

    /// <summary>
    /// Input neurons allow their outputs to be set manually.
    /// </summary>
    class InputNeuron : INeuron{
        private double input;

        public void SetInput(double newInput) {
            input = newInput;
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
