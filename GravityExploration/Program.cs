using System;
using System.Collections.Generic;

namespace GravityExploration
{
    class GeneralData
    {
        // Константы
        public static readonly double G = 6.672e-8;
        public static readonly double SoilDensity = 2600;

        // Входные данные для обратной задачи (сетка датчиков и их показания)
        public static readonly List<double> X = new();
        public static readonly List<double> Y = new();
        public static readonly List<List<double>> Z = new();
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            int objectsNums = 3;                                       // Количество объектов у особи
            double xs = -5, xe = 5;                                    // Границы сетки по OX
            double ys = -5, ye = 5;                                    // Границы сетки по OY

            List<List<Strata>> Population = new();      // Список особей (особь - набор объектов)
            List<string[]> _units;                                     // Задание параметров для объектов особи

            // Подготовка к решению обратной задачи (прямая задача)
            // Получение значений аномалии по заданным объектам
            #region Receivers
            for (double i = xs; i <= xe; i += 1)
            {
                GeneralData.X.Add(i);
            }
            for (double i = ys; i <= ye; i += 1)
            {
                GeneralData.Y.Add(i);
            }
            #endregion Receivers

            string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data.txt");

            _units = ReadFile(DataPath);
            Population.Add(AddPopulation(_units));

            DirectProblem forward = new(Population[0], GeneralData.Z);
            forward.Decision();

            //Population.Clear();

            Thread.Sleep(5000);

            // Решение обратной задачи
            _units = SetPrimaryGeneration(objectsNums, xs, xe, ys, ye);
            Population.Add(AddPopulation(_units));

            List<List<double>> Z = new();
            DirectProblem back = new(Population[1], Z);
            back.Decision();
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

        private static List<string[]> SetPrimaryGeneration(int num, double xs, double xe, double ys, double ye)
        {
            List<string[]> PrimaryGeneration = new();
            Random rand = new Random();
            double[] borders = new double[4];
            borders[0] = xs;
            borders[1] = xe;
            borders[2] = ys;
            borders[3] = ye;
            double borderMax = borders.Max();
            double borderMin = borders.Min();

            for (int j = 0; j < num; j++)
            {
                string[] arr = new string[7];
                for (int i = 0; i < arr.Length - 1; i++)
                {
                    double depth = rand.NextDouble() * -100;
                    switch (i)
                    {
                        case 0:
                            arr[i] = Convert.ToString(RandomDoubleInRange(xs, xe));
                            continue;
                        case 2:
                            arr[i] = Convert.ToString(RandomDoubleInRange(ys, ye));
                            continue;
                        case 4:
                            arr[i] = Convert.ToString(RandomDoubleInRange(depth, 0));
                            continue;
                        default:
                            arr[i] = Convert.ToString(RandomDoubleInRange(borderMin, borderMax));
                            continue;
                    }
                }
                Random rd = new Random();
                arr[arr.Length - 1] = Convert.ToString(rd.Next(2000, 4000));
                PrimaryGeneration.Add(arr);
            }

            double RandomDoubleInRange(double minValue, double maxValue)
            {
                Random rand = new Random();
                return minValue + rand.NextDouble() * (maxValue - minValue);
            }

            return PrimaryGeneration;
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