using System.Collections.Generic;

namespace GravityExploration
{
    class GeneralData
    {
        public static readonly double G = 6.672e-8;
        public static readonly double SoilDensity = 2600;
        public static readonly List<double> X = new();
        public static readonly List<double> Y = new();
        public static readonly List<List<double>> Z = new();
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            #region Receivers
            for (int xs = -5; xs <= 5; xs += 1)
            {
                GeneralData.X.Add(xs);
            }
            for (int ys = -5; ys <= 5; ys += 1)
            {
                GeneralData.Y.Add(ys);
            }
            #endregion Receivers

            string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data.txt");
            
            List<List<Strata>> Population = new();

            List<string[]> _units = ReadFile(DataPath);
            Population.Add(AddPopulation(_units));

            DirectProblem forward = new(Population);
            forward.Decision();
        }

        private static List<string[]> ReadFile(string path)
        {
            List<string[]> input = new();
            using (StreamReader sr = new(path))
            {
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine();
                    input.Add(line!.Split(' '));
                }
            };
            return input;
        }

        private static List<Strata> AddPopulation(List<string[]> _units)
        {
            List<Strata> Units = new();
            for (int i = 0; i < _units.Count; i++)
            {
                Strata unit = new(i, _units);
                Units.Add(unit);
            }
            return Units;
        }
    }
}