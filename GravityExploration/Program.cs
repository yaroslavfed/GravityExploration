using System;
using static System.Math;

namespace GravityExploration
{
    internal class Program
    {
        public static readonly double G = 6.672e-8;

        static void Main(string[] args)
        {
            string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data.txt");
            List<Strata> Units = new List<Strata>();

            List<string[]> _units = ReadFile(DataPath);
            InitUnit(_units, ref Units);

            foreach(Strata _unit in Units)
            {
                Console.WriteLine(_unit.Weight);
            }
        }

        private static List<string[]> ReadFile(string path)
        {
            List<string[]> input = new();
            using (StreamReader sr = new StreamReader(path))
            {
                //string test = sr.ReadToEnd() ?? throw new Exception("Пустой файл");
                //sr.BaseStream.Position = 0;
                while (!sr.EndOfStream)
                { 
                    string? line = sr.ReadLine();
                    input.Add(line!.Split(' '));
                }
            };
            return input;
        }

        private static void InitUnit(List<string[]> _units, ref List<Strata> list)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                Strata unit = new Strata();
                unit.Depth = double.Parse(_units[i][0]);
                unit.Radius = double.Parse(_units[i][1]);
                unit.Density = double.Parse(_units[i][2]);
                unit.SetMass();
                list.Add(unit);
            }
        }


    }

    public class Strata
    {
        public double Depth { get; set; }      // Глубина залегания
        public double Radius { get; set; }     // Радиус шара
        public double Density { get; set; }    // Плотность шара
        public double Weight                   // Избыточная масса шара
        {
            get { return weight; }
        }
        private double weight;

        public void SetMass()
        {
            weight = (4 / 3) * PI * Pow(Radius, 3) * Density;
        }
    }
}