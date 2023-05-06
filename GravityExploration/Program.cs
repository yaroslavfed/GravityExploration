using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public static readonly List<List<double>> trueReadings = new();
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            int objectsNums = 3;                                        // Количество объектов у особи
            int individualsNums = 3;                                    // Количество особей
            double xs = -5, xe = 5;                                     // Границы сетки по OX
            double ys = -5, ye = 5;                                     // Границы сетки по OY

            List<(List<Strata>, List <List<double>>)> Population = new();       // Список особей (особь - набор объектов)
            List<string[]> _units;                                      // Задание параметров для объектов особи

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
            Population.Add(AddGeneration(_units, GeneralData.trueReadings));

            DirectProblem forward = new(-1 ,Population[0]);
            forward.Decision();
            Population.Clear();

            Thread.Sleep(1000);

            // Решение обратной задачи
            List<(List<Strata>, List<List<double>>)>[] populationsOfIndividuals = new List<(List<Strata>, List<List<double>>)>[2];

            // Генерация случайных особей (первоначальное поколение)
            for (int i = 0; i < individualsNums; i++)
            {
                List<List<double>> Z = new();
                _units = SetPrimaryGeneration(objectsNums, xs, xe, ys, ye);
                Population.Add(AddGeneration(_units, Z));

                DirectProblem back = new(i, Population[i]);
                back.Decision();

                //Thread.Sleep(1000);
            }

            // Массив для получения новых особей
            populationsOfIndividuals[0] = Population;

            //double Eps = 1e-5;
            //int MaxP = 10;
            //double Fp_best = 1;
            //int p = 0;

            //while (true)
            //{
            //    if (Fp_best <= Eps || p >= MaxP)
            //    {
            //        break;
            //    }
            //}

            // Вывод нужной особи и удаление лишних файлов при выходе из программы
            Console.WriteLine("Введите номер особи от -1 до {0} (где -1 - это дано, а от 0 до {0} - особи первоначального поколения): ", populationsOfIndividuals[0].Count-1);
            while (true)
            {
                string? a = Console.ReadLine();
                if (a == "exit")
                {
                    string catalog = Directory.GetCurrentDirectory();
                    string fileName = "output*.txt";
                    foreach (string findedFile in Directory.EnumerateFiles(catalog, fileName, SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Delete(findedFile);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("The deletion failed: {0}", e.Message);
                            break;
                        }
                    }
                    break;
                }
                else
                {
                    if (int.TryParse(a, out int result))
                        DrawPlot(result);
                    else
                        Console.WriteLine("Некорректное значение");
                }
            }
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
                    double depth = rand.NextDouble() * -50;
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
                Random rd = new();
                arr[arr.Length - 1] = Convert.ToString(rd.Next(2000, 4000));
                PrimaryGeneration.Add(arr);
            }

            static double RandomDoubleInRange(double minValue, double maxValue)
            {
                Random rand = new();
                return minValue + rand.NextDouble() * (maxValue - minValue);
            }   

            return PrimaryGeneration;
        }

        private static (List<Strata>, List<List<double>>) AddGeneration(List<string[]> _units, List<List<double>> Result)
        {
            List<Strata> Units = new();
            for (int i = 0; i < _units.Count; i++)
            {
                Strata unit = new(i, _units);
                Units.Add(unit);
            }
            return (Units, Result);
        }

        private static void DrawPlot(int number)
        {
            string outputNumberPath = Path.Combine(Directory.GetCurrentDirectory(), "output.txt");
            using (StreamWriter sw = new(outputNumberPath, false))
            {
                sw.WriteLine(number);
            }

            using Process myProcess = new();
            myProcess.StartInfo.FileName = "python";
            myProcess.StartInfo.Arguments = @"script.py";
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardOutput = false;
            myProcess.Start();
        }
    }
}