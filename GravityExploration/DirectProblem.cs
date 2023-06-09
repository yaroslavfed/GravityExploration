﻿using System;
using System.Diagnostics;
using static System.Math;

namespace GravityExploration
{
    internal class DirectProblem
    {
        private readonly int number;
        private readonly List<Strata> Generation = new();
        public List<List<double>> Z = new();

        public DirectProblem(int number, Generation _generation)
        {
            this.number = number;
            Generation = _generation.individual;
            Z = _generation.data;
        }

        public void Decision()
        {
            List<Piece> Pieces = new();

            foreach (var unit in Generation)
                SplitGrid(ref Pieces, unit.CentreZ, unit.StepZ, unit.CentreX, unit.StepX, unit.CentreY, unit.StepY, unit.Density);

            #region Output
            //int p = 0;
            //foreach (var strata in Generation)
            //{
            //    Console.WriteLine(String.Format("Stratas: {0}\t x: {1} +- {2}\n\t\t y: {3} +- {4}\n\t\t z: {5} +- {6}\n\t{7}", p, strata.CentreX, strata.StepX, strata.CentreY, strata.StepY, strata.CentreZ, strata.StepZ, strata.Density));
            //    Console.WriteLine();
            //    p++;
            //}

            // int k = 0;
            // foreach (var piece in Pieces)
            // {
            //     Console.WriteLine(String.Format("pieces: {0}\t x: {1} -> {2}\n\t\t y: {3} -> {4}\n\t\t z: {5} -> {6}\n\t{7}", k, piece.X_Start, piece.X_Stop, piece.Y_Start, piece.Y_Stop, piece.Z_Start, piece.Z_Stop, piece.Density));
            //     Console.WriteLine();
            //     k++;
            // }
            #endregion Output

            GetAnomalyMap(Pieces, Z);
            AddToTxt(Pieces, Generation, Z);
        }

        private void SplitGrid(ref List<Piece> Pieces, double zC, double zS, double xC, double xS, double yC, double yS, double RO)
        {
            //Console.WriteLine("x: {0} -> {1}", xC - xS, xC + xS);
            //Console.WriteLine("y: {0} -> {1}", yC - yS, yC + yS);
            //Console.WriteLine("z: {0} -> {1}", zC - zS, zC + zS);

            double delta_x = Abs((xC + xS) - (xC - xS));
            double delta_y = Abs((yC + yS) - (yC - yS));
            double delta_z = Abs((zC + zS) - (zC - zS));

            double h;
            List<double> comparison = new()
            {
                delta_z,
                delta_x,
                delta_y
            };

            if (comparison.Max() * 0.4 > comparison.Min())
                h = comparison.Min();
            else
                h = comparison.Max() * 0.4;
            //Console.WriteLine("h: " + h);
            //Console.WriteLine();

            GetSplit(ref Pieces, h, delta_y/h, delta_x/h, delta_z/h, zC - zS, xC - xS, yC - yS, RO);
        }

        private static void GetSplit(ref List<Piece> Items, double h, double yk, double xk, double zk, double z0, double x0, double y0, double RO)
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
                        Piece item = new()
                        {
                            Z_Start = _z0,
                            Z_Stop = _z0 + h,
                            X_Start = _x0,
                            X_Stop = _x0 + h,
                            Y_Start = _y0,
                            Y_Stop = _y0 + h,
                            Density = RO
                        };
                        Items.Add(item);

                        _y0 += h;
                    }
                    _x0 += h;
                }
                _z0 += h;
            }
        }

        //private static void GetFunctional(List<List<double>> trueReadings, List<List<double>> localReadings, List<double> functional)
        //{
        //    if (trueReadings is not null)
        //    {
        //        int n = trueReadings.Count * trueReadings[0].Count;
        //        double E_pogr = 1e-8;
        //        double w;
        //        double result = 0;
        //        for (int i = 0; i < localReadings.Count; i++)
        //        {
        //            for (int j = 0; j < localReadings[i].Count; j++)
        //            {
        //                if (Math.Abs(trueReadings[i][j]) < Math.Abs(E_pogr))
        //                    w = 1 / Math.Abs(E_pogr);
        //                else
        //                    w = 1 / Math.Abs(trueReadings[i][j]);
        //                result += Math.Pow(w * (localReadings[i][j] - trueReadings[i][j]), 2);
        //            }
        //        }
        //        result /= n;
        //        functional.Add(result);
        //    }
        //}

        private void GetAnomalyMap(List<Piece> Pieces, List<List<double>> Z)
        {
            double res = 0;
            foreach (double y in GeneralData.Y)
            {
                List<double> list = new();
                foreach (double x in GeneralData.X)
                {
                    foreach (var piece in Pieces)
                    {
                        res += GetAnomaly(x, y, piece);
                    }
                    list.Add(res);
                    res = 0;
                }
                Z.Add(list);
            }
        }

        private double GetAnomaly(double x, double y, Piece piece, double z = 0)
        {
            double result = GeneralData.G * (piece.Density - GeneralData.SoilDensity) * IntegralCalculation(x, y, z, piece.X_Start, piece.X_Stop, piece.Y_Start, piece.Y_Stop, piece.Z_Start, piece.Z_Stop);
            return result;
        }

        private double IntegralCalculation(double xReceiver, double yReceiver, double zReceiver, double x0, double x1, double y0, double y1, double z0, double z1)
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

        private void AddToTxt(List<Piece> Pieces, List<Strata> Units, List<List<double>> Z)
        {
            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "output" + number + ".txt");
            using (StreamWriter sw = new(outputPath, false))
            {
                // Вывод дробленной фигуры
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

                // Вывод целой фигуры
#if true
                sw.WriteLine(Units.Count);
                foreach (var unit in Units)
                {
                    sw.Write(unit.CentreX - unit.StepX + " ");
                    sw.Write(unit.CentreX + unit.StepX + " ");
                    sw.Write(unit.CentreY - unit.StepY + " ");
                    sw.Write(unit.CentreY + unit.StepY + " ");
                    sw.Write(unit.CentreZ - unit.StepZ + " ");
                    sw.Write(unit.CentreZ + unit.StepZ + " ");
                    sw.WriteLine();
                }
#endif

                // Запись в файл сетки аномалии
                foreach (double x in GeneralData.X)
                    sw.Write(x + " ");
                sw.WriteLine();

                foreach (double x in GeneralData.Y)
                    sw.Write(x + " ");
                sw.WriteLine();

                sw.WriteLine(GeneralData.Y.Count);

                foreach (var z in Z)
                {
                    foreach (var _z in z)
                    {
                        sw.Write(_z + " ");
                    }
                    sw.WriteLine();
                }
            };
        }


    }
}
