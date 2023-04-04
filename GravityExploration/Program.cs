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
            List<double> objects = new List<double>();

            ReadFile(DataPath, ref objects);

        }

        static private void ReadFile(string path, ref List<double> list)
        {
            using (StreamReader sr = new StreamReader(path))
            {



                //foreach (var number in sr.ReadLine()!.Split(' '))
                //{
                //    list.Add(double.Parse(number));
                //}
            };
        }
    }

    public class Strata
    {
        public double Depth { get; private set; }      // Глубина залегания
        public double Radius { get; private set; }     // Радиус шара
        public double Density { get; private set; }    // Плотность шара
        public double weight;

        public double Weight
        {
            get { return weight; }
            private set { weight = M(); }
        }

        private double M()
        {
            return (4 / 3) * PI * Pow(Radius, 3) * Density;
        }
    }
}