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
        public (double[] trainingX, double[] trainingY, double[] testingX, double[] testingY)
            LoadData(string fileName, double trainPercent)
        {
            string filePath = Path.Combine("../../../Data/", fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Plik {filePath} nie istnieje.");
            }
            if (trainPercent <= 0 || trainPercent >= 1)
                throw new ArgumentException("trainPercent musi być liczbą z przedziału (0, 1).");

            var allData = new List<(double x, double y)>();
            var lines = File.ReadAllLines(filePath);
            var culture = CultureInfo.InvariantCulture;

            foreach (var line in lines)
            {
                var parts = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) continue;

                if (double.TryParse(parts[0], NumberStyles.Any, culture, out var x) &&
                    double.TryParse(parts[1], NumberStyles.Any, culture, out var y))
                {
                    allData.Add((x, y));
                }
            }

            var rand = new Random();
            allData = allData.OrderBy(_ => rand.Next()).ToList();

            int trainCount = (int)(allData.Count * trainPercent);

            var trainingData = allData.Take(trainCount).ToArray();
            var testingData = allData.Skip(trainCount).ToArray();

            double[] trainingX = trainingData.Select(d => d.x).ToArray();
            double[] trainingY = trainingData.Select(d => d.y).ToArray();
            double[] testingX = testingData.Select(d => d.x).ToArray();
            double[] testingY = testingData.Select(d => d.y).ToArray();

            return (trainingX, trainingY, testingX, testingY);
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
