using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithm.Three {
    /// <summary>
    /// A single layer neural network implementation
    /// </summary>
    class NeuralNetwork {
        protected bool Equals(NeuralNetwork other) {
            return Equals(hiddenLayer, other.hiddenLayer) && Equals(outputLayer, other.outputLayer);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (inputLayer != null ? inputLayer.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (hiddenLayer != null ? hiddenLayer.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (outputLayer != null ? outputLayer.GetHashCode() : 0);
                return hashCode;
            }
        }

        private readonly List<InputNeuron> inputLayer; 
        private readonly List<INeuron> hiddenLayer = new List<INeuron>();
        private readonly List<INeuron> outputLayer = new List<INeuron>();
        private readonly InputNeuron bias = new InputNeuron(1);

        public NeuralNetwork(IList<double> structure, int numInput, int numHidden, int numOutput) {
            //To make a neural network, we interpret the structure list as being the list of weights.

            //Each input layer is an input neuron, they can just begin at zero.
            inputLayer = new List<InputNeuron>();
            for (int i = 0; i < numInput; i++) {
                inputLayer.Add(new InputNeuron(0));
            }

            //The structure parsing is needlessly confusing. It's best to assume it works and leave it at that.
            for (int i = 0; i < numHidden; i++) {
                hiddenLayer.Add(new Neuron(inputLayer.Concat(new[]{bias}), structure.Skip(i*(numInput+1)).Take(numInput+1).ToList()));
            }

            for (int i = 0; i < numOutput; i++) {
                outputLayer.Add(new Neuron(hiddenLayer.Concat(new[]{bias}), structure.Skip(numHidden*(numInput+1) + (numHidden+1)*i).Take(numHidden+1).ToList()));
            }
        }

        public IList<double> Output(IEnumerable<double> inputs) {
            //To work out the output, we set the inputs properly
            foreach (var input in inputLayer.Zip(inputs, Tuple.Create)) {
                input.Item1.SetInput(input.Item2);
            }

            //And then just take the output of each output neuron
            return outputLayer.Select(neuron => neuron.Output()).ToList();
        }

        public override string ToString() {
            var hidden = "Hidden: " + string.Join(", ", hiddenLayer);
            var output = "Output: " + string.Join(", ", outputLayer);

            return string.Join("; ", hidden, output);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NeuralNetwork) obj);
        }
    }
}
