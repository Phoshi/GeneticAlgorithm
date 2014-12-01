using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.Three {
    class NeuralNetwork2 {
        private double[] inputLayer;
        private Neuron2[] hiddenLayer;
        private Neuron2[] secondHiddenLayer;
        private Neuron2[] outputLayer;

        public NeuralNetwork2(IList<double> structure, int numInput, int numHidden, int numHidden2, int numOutput) {
            inputLayer = new double[numInput];
            hiddenLayer = new Neuron2[numHidden];
            secondHiddenLayer = new Neuron2[numHidden2];
            outputLayer = new Neuron2[numOutput];

            var listStructure = new List<double>(structure);

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
            var inputsList = new List<double>(inputs);
            for (int i = 0; i < inputsList.Count(); i++) {
                inputLayer[i] = inputsList[i];
            }

            foreach (var neuron2 in hiddenLayer) {
                var sum = 0d;
                for (int i = 0; i < inputLayer.Length; i++) {
                    sum += inputLayer[i]*neuron2.Weights[i];
                }
                sum += neuron2.Weights.Last(); //bias

                neuron2.Output = Sigmoid(sum);
            }

            foreach (var neuron2 in secondHiddenLayer) {
                var sum = 0d;
                for (int i = 0; i < hiddenLayer.Length; i++) {
                    sum += hiddenLayer[i].Output * neuron2.Weights[i];
                }
                sum += neuron2.Weights.Last(); //bias

                neuron2.Output = Sigmoid(sum);
            }

            foreach (var neuron2 in outputLayer) {
                var sum = 0d;
                for (int i = 0; i < secondHiddenLayer.Length; i++) {
                    sum += secondHiddenLayer[i].Output*neuron2.Weights[i];
                }
                sum += neuron2.Weights.Last(); //bias

                neuron2.Output = Sigmoid(sum);
            }

            return outputLayer.Select(outp => outp.Output).ToList();
        }

        private double Sigmoid(double result) {
            double value = 1.0 / (1.0 + Math.Pow(Math.E, -3.5 * result - 1));
            return value;
        }

        private double Limit(double result) {
            if (result > 0.5) {
                return 1;
            }
            return 0;
        }
    }

    class Neuron2 {
        public double[] Weights;
        public double Output;
    }
}
