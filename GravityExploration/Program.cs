using System.Diagnostics;
using static System.Math;

namespace GravityExploration
{
    internal class Program
    {
        public static readonly double G = 6.672e-8;
        private static readonly List<double> X = new();
        private static readonly List<double> Y = new();

        static void Main(string[] args)
        {
            string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data.txt");
            List<Strata> Units = new List<Strata>();

            List<string[]> _units = ReadFile(DataPath);
            InitUnit(_units, ref Units);

            for (int i = -150000; i <= 150000; i+=10000)
            {
                X.Add(i);
            }
            
            foreach (double x in X)
            {
                double res = 0;
                foreach (Strata _unit in Units)
                {
                    res += GetAnomaly(x, _unit.Depth, _unit.Shift, _unit.Weight);
                }
                Y.Add(res);
            }

            foreach (double y in Y)
                Console.WriteLine(y);

            DrawPlot(_units);
        }

        private static List<string[]> ReadFile(string path)
        {
            List<string[]> input = new();
            using (StreamReader sr = new(path))
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
                unit.Shift = double.Parse(_units[i][3]);
                unit.SetMass();
                list.Add(unit);
            }
        }

        private static double GetAnomaly(double x, double H, double S, double W)
        {
            double commonFactor = G * H * W;
            //Console.WriteLine(commonFactor);

            double result = commonFactor / (Pow(Sqrt(Pow(x - S, 2) + Pow(H, 2)), 3));

            return result;
        }

        private static void DrawPlot(List<string[]> _units)
        {
            IEnumerable<double>? result = X.Concat(Y);

            using Process myProcess = new Process();
            myProcess.StartInfo.FileName = "python";
            myProcess.StartInfo.Arguments = @"script.py";
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardOutput = false;
            myProcess.Start();

            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "output.txt");
            using (StreamWriter sw = new StreamWriter(outputPath))
            {
                sw.WriteLine(_units.Count);

                foreach (string[] unit in _units)
                {
                    sw.Write(unit[0] + " ");
                    sw.Write(unit[1] + " ");
                    sw.Write(unit[3] + " ");
                    sw.WriteLine();
                }

                foreach (double x in X)
                    sw.Write(x + " ");
                sw.WriteLine();

                foreach (double y in Y)
                    sw.Write(y + " ");
                sw.WriteLine();
            };
        }
    }
}