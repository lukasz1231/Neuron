using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

interface Neuron
{
    public void InitTrain();
    public double TrainToBias(double biasToleration, long maxIterations);
    public double TrainToIterations(long maxIterations);
    public double SingleIterationTrain();

    public double Test();
    public double Calculate(double[] X, int index);
    public void ChangeFunction(int func);
    public void newData(List<List<double>> trainingX, double[] trainingD, List<List<double>> testingX, double[] testingD);
    public List<List<double>> GetWeights();
    public List<double> GetTrainingErrors();

}
