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

        public static void SaveToFile(string fileName, string[] lines)
        {
            try
            {
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
    }
}
