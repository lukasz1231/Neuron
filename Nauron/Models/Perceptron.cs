using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nauron.Models
{
    internal class Perceptron : Neuron
    {
        List<List<double>> trainingX;
        double[] trainingD;
        List<List<double>> testingX;
        double[] testingD;
        List<List<double>> W;
        double trainedError;
        int func;
        List<double> trainingErrors;
        Random rand;
        public Perceptron(int func) {
            this.func = func;
            rand = new Random();
        }
        public void InitTrain()
        {
            W.Clear();
            for (int i = 0; i < trainingX.Count; i++)
            {
                for (int j = 0; j < trainingX[i].Count; j++)
                {
                    if (j == 0) W.Add(new List<double>(trainingX[i].Count));
                    W[i].Add(rand.NextDouble());
                }
            }
            trainingErrors.Clear();
            trainedError = double.MaxValue;
        }
        public double TrainToBias(double biasToleration, long maxIterations)
        {
            if (biasToleration <= 0)
                throw new ArgumentException("Złożoność tolerancji mniejsza lub równa 0");
            if (maxIterations <= 0)
                throw new ArgumentException("Maksymalna liczba iteracji mniejsza lub równa 0");
            if (W.Count == 0)
                InitTrain();
            while (trainedError > biasToleration && maxIterations>=0)
            {
                maxIterations--;
                double sumSquaredError = 0.0;
                for (int t = 0; t < trainingX.Count; t++)
                {
                    double output = Calculate(trainingX[t].ToArray(), t);
                    double error = trainingD[t] - output;

                    if (!(error == 0))
                        for(int i = 0; i < trainingX[t].Count; i++)
                        W[t][i] = W[t][i] + trainingX[t][i] * error;

                    sumSquaredError += error * error;
                }

                trainedError = sumSquaredError / trainingX.Count;
                trainingErrors.Add(trainedError); // ZAPIS DO HISTORII
            }
            return trainedError;
        }
        public double TrainToIterations(long maxIterations)
        {
            if (maxIterations <= 0)
                throw new ArgumentException("Maksymalna liczba iteracji mniejsza lub równa 0");
            if (W.Count == 0)
                InitTrain();

            while (maxIterations >= 0)
            {
                maxIterations--;
                double sumSquaredError = 0.0;
                for (int t = 0; t < trainingX.Count; t++)
                {
                    double output = Calculate(trainingX[t].ToArray(), t);
                    double error = trainingD[t] - output;

                    if (!(error == 0))
                        for (int i = 0; i < trainingX[t].Count; i++)
                            W[t][i] = W[t][i] + trainingX[t][i] * error;

                    sumSquaredError += error * error;
                }

                trainedError = sumSquaredError / trainingX.Count;
                trainingErrors.Add(trainedError); // ZAPIS DO HISTORII
            }
            return trainedError;
        }
        public double SingleIterationTrain()
        {
            if (W.Count==0)
                InitTrain();
            double sumSquaredError = 0.0;
            for (int t = 0; t < trainingX.Count; t++)
            {
                double output = Calculate(trainingX[t].ToArray(), t);
                double error = trainingD[t] - output;

                if (!(error == 0))
                    for (int i = 0; i < trainingX[t].Count; i++)
                        W[t][i] = W[t][i] + trainingX[t][i] * error;

                sumSquaredError += error * error;
            }

            trainedError = sumSquaredError / trainingX.Count;
            trainingErrors.Add(trainedError);
            return trainedError;
        }

        public double Test()
        {
            double sumSquaredError = 0.0;
            for (int t = 0; t < testingX.Count; t++)
            {
                double output = Calculate(testingX[t].ToArray(), t);
                double error = testingD[t] - output;
                sumSquaredError += error * error;
            }
            return sumSquaredError / testingX.Count;
        }
        public double Calculate(double[] X, int index)
        {
            double sum = W[index][0];
            for (int i = 0; i < X.Length; i++)
            {
                sum += X[i] * W[index][i];
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
        public void newData(List<List<double>> trainingX, double[] trainingD, List<List<double>> testingX, double[] testingD)
        {
            this.trainingX = trainingX;
            this.testingX = testingX;
            this.trainingD = trainingD;
            this.testingD = testingD;
            this.W = new List<List<double>>(trainingX.Count);
            trainingErrors = new List<double>();
        }
        public List<List<double>> GetWeights()
        {
            return W;
        }
        public List<double> GetTrainingErrors(){
            return trainingErrors;
        }

    }
}
