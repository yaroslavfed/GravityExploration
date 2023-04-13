using System;
using System.Diagnostics;
using static System.Math;

namespace GravityExploration
{
    internal class DirectProblem
    {
        public static void Decision(/*double G, ref List<double> X, ref List<double> Y, ref List<double> Z*/)
        {
            string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data.txt");
            List<Strata> Units = new List<Strata>();
            List<Piece> Pieces = new List<Piece>();

            List<string[]> _units = ReadFile(DataPath);
            InitUnit(_units, ref Units);

            foreach (Strata unit in Units)
                SplitGrid(ref Pieces, unit.Z_Start, unit.Z_Stop, unit.X_Start, unit.X_Stop, unit.Y_Start, unit.Y_Stop, unit.Density);

            #region Output
            int k = 0;
            foreach (Piece _item in Pieces)
            {
                Console.WriteLine(String.Format("pieces:\t{0}\t x: {1} -> {2}\n\t\t z: {3} -> {4}\n\t\t y: {5} -> {6}\n\t{7}", k, _item.X_Start, _item.X_Stop, _item.Z_Start, _item.Z_Stop, _item.Y_Start, _item.Y_Stop, _item.Density));
                Console.WriteLine();
                k++;
            }
            #endregion Output

            #region Receivers
            for (int xs = -15; xs <= 15; xs += 1)
            {
                GeneralData.X.Add(xs);
            }
            for (int ys = -15; ys <= 15; ys += 1)
            {
                GeneralData.Y.Add(ys);
            }
            #endregion Receivers

            GetAnomalyMap(Pieces);
            DrawPlot(Pieces, Units);
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

        private static void InitUnit(List<string[]> _units, ref List<Strata> list)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                Strata unit = new Strata();
                unit.Z_Start = double.Parse(_units[i][0]);
                unit.Z_Stop = double.Parse(_units[i][1]);
                unit.X_Start = double.Parse(_units[i][2]);
                unit.X_Stop = double.Parse(_units[i][3]);
                unit.Y_Start = double.Parse(_units[i][4]);
                unit.Y_Stop = double.Parse(_units[i][5]);
                unit.Density = double.Parse(_units[i][6]);
                list.Add(unit);
            }
        }

        private static void SplitGrid(ref List<Piece> Pieces, double z0, double z1, double x0, double x1, double y0, double y1, double RO)
        {
            double delta_z = Round(Abs(z1 - z0), MidpointRounding.ToEven);
            double delta_x = Round(Abs(x1 - x0), MidpointRounding.ToEven);
            double delta_y = Round(Abs(y1 - y0), MidpointRounding.ToEven);

            double h;
            List<double> comparison = new();
            comparison.Add(delta_z);
            comparison.Add(delta_x);
            comparison.Add(delta_y);

            if (comparison.Max() * 0.2 > comparison.Min())
                h = comparison.Min();
            else
                h = comparison.Max() * 0.2;
            Console.WriteLine("h: " + h);

            GetSplit(ref Pieces, h, delta_y/h, delta_x/h, delta_z/h, z0, x0, y0, RO);
        }

        public static void GetSplit(ref List<Piece> Items, double h, double yk, double xk, double zk, double z0, double x0, double y0, double RO)
        {
            double _z0 = z0;
            for (int i = 0; i < zk; i++)
            {
                double _x0 = x0;
                for (int j = 0; j < xk; j++)
                {
                    double _y0 = y0;
                    for (int k = 0; k < yk; k++)
                    {
                        Piece item = new Piece();
                        item.Z_Start = _z0;
                        item.Z_Stop = _z0 + h;
                        item.X_Start = _x0;
                        item.X_Stop = _x0 + h;
                        item.Y_Start = _y0;
                        item.Y_Stop = _y0 + h;
                        item.Density = RO;
                        Items.Add(item);

                        _y0 += h;
                    }
                    _x0 += h;
                }
                _z0 += h;
            }
        }

        private static void GetAnomalyMap(List<Piece> Pieces)
        {
            double res = 0;
            foreach (double x in GeneralData.X)
            {
                foreach (double y in GeneralData.Y)
                {
                    foreach (var piece in Pieces)
                    {
                        res += GetAnomaly(x, y, piece);
                    }
                    Console.WriteLine(String.Format("x: {0}\ty: {1}\tz: {2}", x, y, res));
                    GeneralData.Z.Add(res);
                    res = 0;
                }
            }
        }

        private static double GetAnomaly(double x, double y, Piece piece, double z = 0)
        {
            double result = GeneralData.G * (piece.Density - GeneralData.SoilDensity) * IntegralCalculation(z, x, y, piece.Z_Start, piece.Z_Stop, piece.X_Start, piece.X_Stop, piece.Y_Start, piece.Y_Stop);
            return result;
        }

        private static double IntegralCalculation(double zReceiver, double xReceiver, double yReceiver, double z0, double z1, double x0, double x1, double y0, double y1)
        {
            double n = 1E+2;
            //Console.WriteLine(n);
            double h = (x1 - x0) / n;
            List<double> xs = new();

            double k = x0;
            for (int i = 0; i <= n; i++)
            {
                xs.Add(k);
                k += h;
            }

            double result = 0;
            for (int i = 1; i <= n; i++)
            {
                result += function(xs[i] - (h / 2));
            }
            result *= h;
            //Console.WriteLine(result);

            double function(double w)
            {
                return Asinh(((yReceiver - y0) * Sqrt(4 * Pow(zReceiver, 2) - 8 * z1 * zReceiver + 4 * Pow(xReceiver, 2) - 8 * w * xReceiver + 4 * Pow(w, 2) + 4 * Pow(z1, 2))) / (2 * Pow(zReceiver, 2) - 4 * z1 * zReceiver + 2 * Pow(xReceiver, 2) - 4 * w * xReceiver + 2 * Pow(w, 2) + 2 * Pow(z1, 2)))
                - Asinh(((yReceiver - y1) * Sqrt(4 * Pow(zReceiver, 2) - 8 * z1 * zReceiver + 4 * Pow(xReceiver, 2) - 8 * w * xReceiver + 4 * Pow(w, 2) + 4 * Pow(z1, 2))) / (2 * Pow(zReceiver, 2) - 4 * z1 * zReceiver + 2 * Pow(xReceiver, 2) - 4 * w * xReceiver + 2 * Pow(w, 2) + 2 * Pow(z1, 2)))
                - Asinh(((yReceiver - y0) * Sqrt(4 * Pow(zReceiver, 2) - 8 * z0 * zReceiver + 4 * Pow(xReceiver, 2) - 8 * w * xReceiver + 4 * Pow(w, 2) + 4 * Pow(z0, 2))) / (2 * Pow(zReceiver, 2) - 4 * z0 * zReceiver + 2 * Pow(xReceiver, 2) - 4 * w * xReceiver + 2 * Pow(w, 2) + 2 * Pow(z0, 2)))
                + Asinh(((yReceiver - y1) * Sqrt(4 * Pow(zReceiver, 2) - 8 * z0 * zReceiver + 4 * Pow(xReceiver, 2) - 8 * w * xReceiver + 4 * Pow(w, 2) + 4 * Pow(z0, 2))) / (2 * Pow(zReceiver, 2) - 4 * z0 * zReceiver + 2 * Pow(xReceiver, 2) - 4 * w * xReceiver + 2 * Pow(w, 2) + 2 * Pow(z0, 2)));
            }

            return result;
        }

        private static void DrawPlot(List<Piece> Pieces, List<Strata> Units)
        {
            using Process myProcess = new Process();
            myProcess.StartInfo.FileName = "python";
            myProcess.StartInfo.Arguments = @"script.py";
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardOutput = false;
            myProcess.Start();

            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "outputUnder.txt");
            using (StreamWriter sw = new StreamWriter(outputPath))
            {
#if false
                sw.WriteLine(Pieces.Count);
                foreach (var unit in Pieces)
                {
                    sw.Write(unit.X_Start + " ");
                    sw.Write(unit.X_Stop + " ");
                    sw.Write(unit.Y_Start + " ");
                    sw.Write(unit.Y_Stop + " ");
                    sw.Write(unit.Z_Start + " ");
                    sw.Write(unit.Z_Stop + " ");
                    sw.WriteLine();
                } 
#endif

#if true
                sw.WriteLine(Units.Count);
                foreach (var unit in Units)
                {
                    sw.Write(unit.X_Start + " ");
                    sw.Write(unit.X_Stop + " ");
                    sw.Write(unit.Y_Start + " ");
                    sw.Write(unit.Y_Stop + " ");
                    sw.Write(unit.Z_Start + " ");
                    sw.Write(unit.Z_Stop + " ");
                    sw.WriteLine();
                } 
#endif

                foreach (double x in GeneralData.X)
                    sw.Write(x + " ");
                sw.WriteLine();

                foreach (double x in GeneralData.Y)
                    sw.Write(x + " ");
                sw.WriteLine();

                foreach (double z in GeneralData.Z)
                    sw.Write(z + " ");
                sw.WriteLine();
            };
        }
    }
}
