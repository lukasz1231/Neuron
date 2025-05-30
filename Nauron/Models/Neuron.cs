using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface Neuron
{
    public void InitTrain();
    public double TrainToBias(double biasToleration, long maxIterations);
    public double TrainToIterations(long maxIterations);
    public double SingleIterationTrain();
    public double Calculate(int index);
    public void ChangeLearningRate(double s);
    public void ChangeFunction(int func);
    public void ChangeWeights(double[] weights);

    public void newData(List<List<double>> trainingX, List<double> trainingD);
    public double[] GetWeights();
    public List<double> GetTrainingErrors();
    public void SetTrainingErrors(List<double> er);
    public int GetFunction();
    public (List<List<double>> trainingX, List<double> trainingD) GetData();
}
