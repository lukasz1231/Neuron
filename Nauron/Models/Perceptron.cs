using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nauron.Models
{
    internal class Perceptron : Neuron
    {
        private double[] trainingX;
        private double[] trainingY;
        private double[] testingX;
        private double[] testingY;
        private double[] W;
        private double w0;
        private int func = 1; //default
        private double biasToleration = 0.01; //default
        private List<double> trainingErrors = new List<double>();

        public Perceptron(double[] trainingX, double[] trainingY, double[] testingX, double[] testingY, double biasToleration, int func) {
            this.trainingX = trainingX;
            this.trainingY = trainingY;
            this.testingY = testingY;
            this.testingX = testingX;
            this.biasToleration = biasToleration;
            this.func = func;
            this.W = new double[trainingX.Length+1];
            Random rand = new Random();
            w0 = rand.NextDouble() > biasToleration ? 1 : -1; // NIE WIEM CZY DOBRZE
        }

        public double Train(double[] X, double[] Y, int maxEpochs)
        {
            Random rand = new Random();
            for (int i = 1; i < W.Length; i++)
            {
                W[i] = rand.NextDouble();
            }

            trainingErrors.Clear(); // reset przed nowym treningiem

            double trainedError = double.MaxValue;
            int eras = 0;

            while (trainedError > biasToleration && eras++ <= maxEpochs) // ograniczenie epok - nie zawsze osiąga docelowy błąd
            {
                double sumSquaredError = 0.0;
                for (int t = 0; t < X.Length; t++)
                {
                    double output = Calculate(trainingX);
                    double error = Y[t] - output;

                    if (error == 0)
                        W[t + 1] = W[t];
                    else
                        W[t + 1] = W[t] + trainingX[t] * trainingY[t];

                    sumSquaredError += error * error;
                }

                trainedError = sumSquaredError / trainingX.Length;
                trainingErrors.Add(trainedError); // ZAPIS DO HISTORII
            }

            return trainedError;
        }

        public double Test()
        {
            double sumSquaredError = 0.0;
            for (int t = 0; t < testingX.Length; t++)
            {
                double output = Calculate(testingX);
                double error = testingY[t] - output;
                sumSquaredError += error * error;
            }
            return sumSquaredError / testingX.Length;
        }
        public double Calculate(double[] X)
        {
            double sum = 1*w0;
            for (int i = 0; i < X.Length; i++)
            {
                sum += X[i] * W[i];
            }
            return ActivationFunction(sum);
        }

        private double ActivationFunction(double input)
        {
            switch (func)
            {
                case 0: // StepBinary
                    return input > 0 ? 1.0 : 0.0;

                case 1: // StepBipolar
                    return input > 0 ? 1.0 : -1.0;

                case 2: // Sigmoid
                    return 1.0 / (1.0 + Math.Exp(-input));

                case 3: // Tanh
                    return Math.Tanh(input);

                case 4: // ReLU
                    return Math.Max(0.0, input);

                case 5: // Leaky ReLU
                    return input > 0 ? input : 0.01 * input;

                default:
                    throw new ArgumentException("Unknown activation function");
            }
        }
        public void ChangeFunction(int func)
        {
            this.func = func;
        }
        public void newData(double[] trainingX, double[] trainingY, double[] testingX, double[] testingY)
        {
            this.trainingX = trainingX;
            this.testingX = testingX;
            this.trainingY = trainingY;
            this.testingY = testingY;
        }
        public double[] GetWeights()
        {
            return W;
        }
        public List<double> GetTrainingErrors() => trainingErrors;

    }
}
