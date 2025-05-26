using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nauron.Models
{
    internal class Adaline : Neuron
    {
        List<List<double>> trainingX;
        double[] trainingD;
        List<List<double>> testingX;
        double[] testingD;
        double[] W;
        double w0;
        double trainedError;
        int func;
        List<double> trainingErrors;
        Random rand;
        double learningRate;
        public Adaline(int func)
        {
            this.func = func;
            rand = new Random();
            trainingErrors = new List<double>();
        }
        public void InitTrain()
        {
            W = new double[2];
            W[0] = rand.Next(1, 11) + (rand.Next(2, 30) * rand.NextDouble()) / Math.Pow(10, rand.Next(2, 10));
            W[1] = rand.Next(1, 11) + (rand.Next(2, 30) * rand.NextDouble()) / Math.Pow(10, rand.Next(2, 10));

            w0 = rand.NextDouble();

            trainingErrors.Clear();
        }
        public double TrainToBias(double biasToleration, long maxIterations)
        {
            trainedError = double.MaxValue;
            if (biasToleration <= 0)
                throw new ArgumentException("Złożoność tolerancji mniejsza lub równa 0");
            if (maxIterations <= 0)
                throw new ArgumentException("Maksymalna liczba iteracji mniejsza lub równa 0");
            while (trainedError > biasToleration && maxIterations >= 0)
            {
                maxIterations--;
                SingleIterationTrain();
            }
            return trainedError;
        }
        public double TrainToIterations(long maxIterations)
        {
            if (maxIterations <= 0)
                throw new ArgumentException("Maksymalna liczba iteracji mniejsza lub równa 0");

            while (maxIterations >= 0)
            {
                maxIterations--;
                SingleIterationTrain();
            }
            return trainedError;
        }
        public double SingleIterationTrain()
        {
            if (W == null)
                throw new ArgumentException("Nie zainicjalizowano danych");
            if (learningRate == 0.0)
                throw new ArgumentException("Nie zainicjalizowano danych lub nie podano tempa uczenia");

            double sumSquaredError = 0.0;
            for (int t = 0; t < trainingX.Count; t++)
            {
                double output = Calculate(t);
                double error = trainingD[t] - ActivationFunction(output);

                if (error != 0)
                {
                    for (int i = 0; i < trainingX[t].Count; i++)
                        W[i] = W[i] + learningRate * trainingX[t][i] * (trainingD[t] - output);
                    w0 = w0 + learningRate * (trainingD[t] - output);
                }

                sumSquaredError += error * error;
            }

            trainedError = sumSquaredError / trainingX.Count;
            trainingErrors.Add(trainedError);
            return trainedError;
        }
        public double Calculate(int index)
        {
            double sum = w0;
            for (int i = 0; i < trainingX[index].Count; i++)
            {
                sum += trainingX[index][i] * W[i];
            }
            return sum;
        }

        private double ActivationFunction(double input)
        {
            switch (func)
            {
                case 0: // StepBinary
                    return input > 0 ? 1.0 : 0.0; // dziala z danymi

                case 1: // StepBipolar
                    return input > 0 ? 1.0 : -1.0;

                case 2: // Sigmoid
                    return 1.0 / (1.0 + Math.Exp(-input)); // dziala z danymi

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
        public void ChangeLearningRate(double s)
        {
            learningRate = s;
        }
        public void ChangeFunction(int func)
        {
            this.func = func;
        }
        public void newData(List<List<double>> trainingX, double[] trainingD, List<List<double>> testingX, double[] testingD)
        {
            this.trainingX = trainingX;
            this.testingX = testingX;
            this.trainingD = trainingD;
            this.testingD = testingD;
        }
        public double[] GetWeights()
        {
            double[] w = new double[3];
            w[0] = w0;
            w[1] = W[0];
            w[2] = W[1];
            return w;
        }
        public List<double> GetTrainingErrors()
        {
            return trainingErrors;
        }
        public (List<List<double>> trainingX, double[] trainingD, List<List<double>> testingX, double[] testingD) GetData()
        {
            return (trainingX, trainingD, testingX, testingD);
        }
    }
}
