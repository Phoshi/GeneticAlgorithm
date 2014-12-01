using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.Three {
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
            inputLayer = new List<InputNeuron>();
            for (int i = 0; i < numInput; i++) {
                inputLayer.Add(new InputNeuron(0));
            }

            for (int i = 0; i < numHidden; i++) {
                hiddenLayer.Add(new Neuron(inputLayer.Concat(new[]{bias}), structure.Skip(i*(numInput+1)).Take(numInput+1).ToList()));
            }

            for (int i = 0; i < numOutput; i++) {
                outputLayer.Add(new Neuron(hiddenLayer.Concat(new[]{bias}), structure.Skip(numHidden*(numInput+1) + (numHidden+1)*i).Take(numHidden+1).ToList()));
            }
        }

        public IList<double> Output(IEnumerable<double> inputs) {
            foreach (var input in inputLayer.Zip(inputs, Tuple.Create)) {
                input.Item1.SetInput(input.Item2);
            }

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
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NeuralNetwork) obj);
        }
    }
}
