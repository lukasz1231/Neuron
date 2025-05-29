using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Nauron.Models
{
    internal class DataManager
    {
        public (List<List<double>> trainingX, List<double> trainingD)
            LoadData(string fileName)
        {
            string filePath = Path.Combine("../../../Data/", fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Plik {fileName} nie istnieje.");
            }
            var allData = new List<(List<double> x, double d)>();
            var lines = File.ReadAllLines(filePath);
            var culture = CultureInfo.InvariantCulture;
            var stack = new Stack<double>();
            double d;
            foreach (var line in lines)
            {
                var parts = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;
                stack.Clear();
                foreach(string s in parts)
                {
                    if(double.TryParse(s, NumberStyles.Any, culture, out var wart))
                    {
                        stack.Push(wart);
                    }
                }
                d=stack.Pop();
                if (stack.Count!=2)
                {
                    throw new ArgumentException("Niepoprawna ilość wartości w wektorze ( " + stack.Count+" ), gdy powinno być: 2");
                }
                if (!(d==1 || d == 0))
                {
                    throw new ArgumentException("Niepoprawna wartość wzorcowa w wierszu "+(allData.Count+1));
                }
                allData.Add((stack.ToList(), d));
            }

            var rand = new Random();
            allData = allData.OrderBy(_ => rand.Next()).ToList();


            List<List<double>> trainingX = allData.Select(d => d.x).ToList();
            List<double> trainingD = allData.Select(d => d.d).ToList();

            return (trainingX, trainingD);
        }
        public void SaveFile(string path, Neuron neuron)
        {
            try
            {
                var lines = new List<string>();
                (var X, var D) = neuron.GetData();
                if (neuron is Perceptron) lines.Add("P");
                else lines.Add("A");
                for (int i = 0; i < X.Count; i++)
                {
                    lines.Add(X[i][0] + " " + X[i][1] + " " + D[i]);
                }
                var W = neuron.GetWeights();
                lines.Add(W[0] + " " + W[1] + " " + W[2]);
                File.WriteAllLines(path, lines.ToArray());
            }
            catch(Exception e){
                MessageBox.Show("Wystąpił błąd:\n"+e.Message);
            }
        }
        public Neuron OpenFile(MainWindow main, string path)
        {
            var lines = File.ReadAllLines(path);
            if (lines != null)
            {
                Neuron n;
                main.FileNameBox.Text = "";
                if (lines[0] == "P") n = new Perceptron(0); 
                else n = new Adaline(0);
                List<List<double>> X = new();
                List<double> D = new();
                List<double> temp;
                for(int i=1;i<lines.Length-1;i++)
                {
                    temp= ArrayToDoubles(lines[i].Split());
                    X.Add(new List<double>([temp[0], temp[1]]));
                    D.Add(temp[2]);
                }
                n.newData(X, D);
                n.ChangeWeights(ArrayToDoubles(lines[lines.Length - 1].Split()).ToArray());
                return n;
            }
            throw new NullReferenceException();
        }
        List<double> ArrayToDoubles(string[] words)
        {
            List<double> a = new();
            foreach (string word in words) {
                a.Add(Convert.ToDouble(word));
            }
            return a;
        }
        public static void SaveToFile(string fileName, Neuron n)
        {
            try
            {
                var lines = DataToString(n);
                string filePath = Path.Combine("../../../Data/", fileName);
                if (File.Exists(filePath))
                {
                    var result=MessageBox.Show($"Plik {fileName} już istnije.\nCzy chcesz go nadpisać?", "Plik istnieje", MessageBoxButton.YesNo);
                    if( result.ToString() != "Yes")
                        return;
                }
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd zapisu do pliku: {ex.Message}");
            }
        }
        static string[] DataToString(Neuron n)
        {
            List<string> wynik;
            if (n != null)
            {
                (var trainingX, var trainingD) = n.GetData();
                wynik = new(trainingD.Count);
                for (int i = 0; i < trainingD.Count; i++)
                {
                    wynik.Add($"{trainingX[i][0]} {trainingX[i][1]} {trainingD[i]}");
                }
            }
            else
                wynik = new List<string>([""]);
            return wynik.ToArray();
        }
    }
}
