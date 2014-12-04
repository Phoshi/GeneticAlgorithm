using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithm.Three {
    /// <summary>
    /// An implementation of a two-layer neural network.
    /// </summary>
    class NeuralNetwork2 {
        private readonly double[] inputLayer;
        private readonly Neuron2[] hiddenLayer;
        private readonly Neuron2[] secondHiddenLayer;
        private readonly Neuron2[] outputLayer;

        public NeuralNetwork2(IEnumerable<double> structure, int numInput, int numHidden, int numHidden2, int numOutput) {
            //Instantiate all our layers
            inputLayer = new double[numInput];
            hiddenLayer = new Neuron2[numHidden];
            secondHiddenLayer = new Neuron2[numHidden2];
            outputLayer = new Neuron2[numOutput];

            var listStructure = new List<double>(structure);

            //For each layer, we just interpret the structure as a list of weights, destroying it as we go.
            for (int i = 0; i < numHidden; i++) {
                var weights = listStructure.Take(numInput + 1).ToArray();
                listStructure.RemoveRange(0, numInput + 1);

                hiddenLayer[i] = new Neuron2{Weights = weights};
            }

            for (int i = 0; i < numHidden2; i++) {
                var weights = listStructure.Take(numHidden + 1).ToArray();
                listStructure.RemoveRange(0, numHidden + 1);

                secondHiddenLayer[i] = new Neuron2{Weights = weights};
            }

            for (int i = 0; i < numOutput; i++) {
                var weights = listStructure.Take(numHidden2 + 1).ToArray();
                listStructure.RemoveRange(0, numHidden2 + 1);

                outputLayer[i] = new Neuron2{Weights = weights};
            }
        }

        public IList<double> Output(IEnumerable<double> inputs) {
            //To work out the outputs, we set the inputs
            var inputsList = new List<double>(inputs);
            for (int i = 0; i < inputsList.Count(); i++) {
                inputLayer[i] = inputsList[i];
            }

            //Then go through each layer propagating the output
            foreach (var neuron in hiddenLayer) {
                var sum = 0d;
                for (int i = 0; i < inputLayer.Length; i++) {
                    sum += inputLayer[i]*neuron.Weights[i];
                }
                sum += neuron.Weights.Last(); //bias

                neuron.Output = Sigmoid(sum);
            }

            foreach (var neuron in secondHiddenLayer) {
                var sum = 0d;
                for (int i = 0; i < hiddenLayer.Length; i++) {
                    sum += hiddenLayer[i].Output * neuron.Weights[i];
                }
                sum += neuron.Weights.Last(); //bias

                neuron.Output = Sigmoid(sum);
            }

            foreach (var neuron in outputLayer) {
                var sum = 0d;
                for (int i = 0; i < secondHiddenLayer.Length; i++) {
                    sum += secondHiddenLayer[i].Output*neuron.Weights[i];
                }
                sum += neuron.Weights.Last(); //bias

                neuron.Output = Limit(sum);
            }

            return outputLayer.Select(outp => outp.Output).ToList();
        }

        private double Sigmoid(double result) {
            //Sigmoid function has parameters set to tighten the curve, which seems to produce better results.
            double value = 1.0 / (1.0 + Math.Pow(Math.E, -3.5 * result - 1));
            return value;
        }

        private double Limit(double result) {
            //Limit function is just straight down the middle.
            if (result > 0.5) {
                return 1;
            }
            return 0;
        }
    }

    /// <summary>
    /// Neuron2 is just a simple data store, no behaviour.
    /// </summary>
    class Neuron2 {
        public double[] Weights;
        public double Output;
    }
}
