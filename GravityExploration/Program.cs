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
            int objectsNums = 3;                                                // Количество объектов у особи
            int individualsNums = 3;                                            // Количество особей
            double xs = -5, xe = 5;                                             // Границы сетки по OX
            double ys = -5, ye = 5;                                             // Границы сетки по OY

            List<(List<Strata>, List <List<double>>)> Population = new();       // Список особей (особь - набор объектов)
            List<string[]> _units;                                              // Задание параметров для объектов особи

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
            // Массив особей для кроссинговера
            List<(List<Strata>, List<List<double>>)>[] populationsOfIndividuals = new List<(List<Strata>, List<List<double>>)>[2];

            // Генерация случайных особей (первоначальное поколение)
            for (int i = 0; i < individualsNums; i++)
            {
                List<List<double>> Z = new();
                _units = SetPrimaryGeneration(objectsNums, xs, xe, ys, ye);
                Population.Add(AddGeneration(_units, Z));
            }

            // Массив для получения новых особей
            populationsOfIndividuals[0] = Population;

            // Получение решения для первоначальных особей
            int q = 0;
            foreach(var pop in populationsOfIndividuals[0])
            {
                DirectProblem back = new(q, pop);
                back.Decision();
                q++;
            }

            // Вывод нужной особи и удаление лишних файлов при выходе из программы
            System.Console.WriteLine("Решение первоначальное");
            OutputGraphs(populationsOfIndividuals[0].Count-1);

            // Условия для обратной задачи
            double Eps = 1e-5;          // Точность
            int MaxP = 10;              // Максимальное количество итераций
            double Fp_best = 1;         // Лучшее значение
            int p = 0;                  // Итерация

            // Решение обратной задачи
            // while (true)
            // {
            //     if (Fp_best <= Eps || p >= MaxP)
            //     {
            //         break;
            //     }
            //     populationsOfIndividuals[1] = CrossingOver(populationsOfIndividuals[0]);
            //     p++;
            // }

            populationsOfIndividuals[1] = CrossingOver(populationsOfIndividuals[0]);

            // Получение решения для полученных особей
            int k = 0;
            foreach(var pop in populationsOfIndividuals[1])
            {
                DirectProblem back = new(k, pop);
                back.Decision();
                k++;
            }

            // Вывод нужной особи и удаление лишних файлов при выходе из программы
            System.Console.WriteLine("Решение после свапа");
            OutputGraphs(populationsOfIndividuals[1].Count-1);
        }

        public static Tuple<int, int> getTuple(in int rd1, in int rd2)
        {
            var aTuple = Tuple.Create<int, int>(rd1, rd2);
            return aTuple;
        }

        private static void CheckingForRepetition(ref int item1, ref int item2, int Count1, int Count2, ref List<Tuple<int, int>> pairs, ref bool repeat)
        {
            Random rand = new();
            do
            {
                item1 = rand.Next(Count1);
                item2 = rand.Next(Count2);

                Tuple<int, int> _tuple = getTuple(in item1, in item2);
                Tuple<int, int> _tupleback = getTuple(in item2, in item1);

                if (!pairs.Contains(_tuple) && !pairs.Contains(_tupleback))
                {
                    repeat = false;
                    pairs.Add(_tuple);
                    pairs.Add(_tupleback);
                }
                else
                {
                    //System.Console.WriteLine("Повторение {0} или {1}", _tuple, _tupleback);
                    repeat = true;
                }

            } while (item1 == item2 || repeat);
        }

        private static List<(List<Strata>, List <List<double>>)> CrossingOver(List<(List<Strata>, List <List<double>>)> individuals)
        {
            List<(List<Strata>, List <List<double>>)> generation = new();

            //Random rdIndCount = new();              // Рандом для получения количества особей для замен
            //Random rdIndividual = new();            // Рандом для выбора случайной особи
            //Random rdParam = new();                 // Рандом для выбора параметров для свапа у объекта особи

            int indCount = 3;
            List<Tuple<int, int>> indPairs = new();
            bool indRepeat = true;
            for (int j = 0; j < indCount; j++)
            {      
                int _item1 = 0, _item2 = 0;
                CheckingForRepetition(ref _item1, ref _item2, individuals.Count, individuals.Count, ref indPairs, ref indRepeat);
                System.Console.WriteLine("Особь первая: {0}\tвторая: {1}", _item1, _item2);

                Random rdObjCount = new();          // Рандом для получения количества объектов особей для замен
                int objCount = rdObjCount.Next(1, individuals[0].Item1.Count);

                List<Tuple<int, int>> objPairs = new();
                bool objRepeat = true;
                for (int i = 0; i < objCount; i++)
                {
                    int _obj1 = 0, _obj2 = 0;
                    CheckingForRepetition(ref _obj1, ref _obj2, individuals[_item1].Item1.Count, individuals[_item2].Item1.Count, ref objPairs, ref objRepeat);
                    System.Console.WriteLine("Объект первый: {0}\tвторой: {1}", _item1, _item2);

                    Random rdParamsCount = new();
                    int paramsCount = rdParamsCount.Next(1, individuals[0].Item1[0].Params!.Count);

                    for (int k = 0; k < paramsCount; k++)
                    {
                        int _indexParam = rdParamsCount.Next(individuals[0].Item1[0].Params!.Count);
                        Swap(_indexParam, individuals[_item1].Item1[_obj1], individuals[_item2].Item1[_obj2]);
                    }
                }
            }
            
            // List<Strata> individual1 = individuals[_item1].Item1;
            // List<Strata> individual2 = individuals[_item2].Item1;

            // List<List<double>> Z1 = new();
            // generation.Add((individual1, Z1));

            // List<List<double>> Z2 = new();
            // generation.Add((individual2, Z2));

            foreach (var item in individuals)
            {
                List<List<double>> Z = new();
                List<Strata> list = new();
                foreach (var unit in item.Item1)
                    list.Add(unit);
                generation.Add((list, Z));
            }

            return generation;
        }

        private static void Swap(int index, Strata item1, Strata item2)
        {
            double temp = 0;
            if (item1.Params is not null && item2.Params is not null)
            {
                temp = item1.Params[index];
                item1.Params[index] = item2.Params[index];
                item2.Params[index] = temp;

                System.Console.WriteLine("Index: {0}", index);
                System.Console.WriteLine("Swap to: {0} from: {1}", item1.Params[index], item2.Params[index]);

                Init(item1);
                Init(item2);
            }

            void Init(Strata item)
            {
                item.GetFromList();
            }
        }

        private static void OutputGraphs(int indCount)
        {
            Console.WriteLine("Введите номер особи от -1 до {0} (где -1 - это дано, а от 0 до {0} - особи первоначального поколения): ", indCount);
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
            borders[0] = Math.Abs(xs);
            borders[1] = Math.Abs(xe);
            borders[2] = Math.Abs(ys);
            borders[3] = Math.Abs(ye);
            double borderMax = borders.Max();

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
                            arr[i] = Convert.ToString(RandomDoubleInRange(1, borderMax));
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
                unit.SetToList();
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