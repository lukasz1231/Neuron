using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

interface Neuron
{
    public double Train(double[] X, double[] Y, int maxEpochs);
    public double Test();
    public double Calculate(double[] X);
    public void newData(double[] trainingX, double[] trainingY, double[] testingX, double[] testingY);
    public double[] GetWeights();

    public void ChangeFunction(int func);
    public List<double> GetTrainingErrors();

}
