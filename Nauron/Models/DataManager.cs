using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nauron.Models
{
    internal class DataManager
    {
        public (List<List<double>> trainingX, double[] trainingD, List<List<double>> testingX, double[] testingD)
            LoadData(string fileName, double trainPercent)
        {
            string filePath = Path.Combine("../../../Data/", fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Plik {fileName} nie istnieje.");
            }
            if (trainPercent <= 0 || trainPercent >= 1)
                throw new ArgumentException("trainPercent musi być liczbą z przedziału (0, 1).");

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

            int trainCount = (int)(allData.Count * trainPercent);

            var trainingData = allData.Take(trainCount).ToArray();
            var testingData = allData.Skip(trainCount).ToArray();

            List<List<double>> trainingX = trainingData.Select(d => d.x).ToList();
            double[] trainingD = trainingData.Select(d => d.d).ToArray();
            List<List<double>> testingX = testingData.Select(d => d.x).ToList();
            double[] testingD = testingData.Select(d => d.d).ToArray();

            return (trainingX, trainingD, testingX, testingD);
        }

        // Przykładowa metoda do zapisu danych lub wag do pliku
        // Możesz ją rozbudować np. o zapisywanie wag perceptronu
        /*
        public void SaveToFile(string path, IEnumerable<string> linesToWrite)
        {
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllLines(path, linesToWrite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd zapisu do pliku: {ex.Message}");
            }
        }
        */
    }
}
