using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
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
        public static List<List<double>> trueReadings = new();
    }

    public class Generation
    {
        public Generation(List<Strata> individual)
        {
            this.individual = individual;
        }

        public List<Strata> individual = new();
        public List<List<double>> data = new();
        public double Functional { get; private set; }

        public void GetFunctional(List<List<double>> trueReadings)
        {
            if (trueReadings is not null)
            {
                int n = trueReadings.Count * trueReadings[0].Count;
                double E_pogr = 1e-8;
                double w;
                for (int i = 0; i < data.Count; i++)
                {
                    for (int j = 0; j < data[i].Count; j++)
                    {
                        if (Math.Abs(trueReadings[i][j]) < Math.Abs(E_pogr))
                            w = 1 / Math.Abs(E_pogr);
                        else
                            w = 1 / Math.Abs(trueReadings[i][j]);
                        Functional += Math.Pow(w * (data[i][j] - trueReadings[i][j]), 2);
                    }
                }
                Functional /= n;
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            // Входные данные
            int objectsNums = 2;                                // Количество объектов у особи
            int individualsNums = 10;                           // Количество особей
            double xs = -5, xe = 5;                             // Границы сетки по OX
            double ys = -5, ye = 5;                             // Границы сетки по OY

            // Условия для обратной задачи
            double Eps = 1e-3;                                  // Точность
            int MaxP = 100;                                     // Максимальное количество итераций
            double Fp_best = 1;                                 // Лучшее значение
            int p = 1;                                          // Итерация

            List<string[]> _units;                              // Задание параметров для объектов особи
            List<double> weights = new();
            List<double> reproduction;

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

            // Подготовка к решению обратной задачи (прямая задача)
            // Получение значений аномалии по ЗАРАНЕЕ ЗАДАННЫМ объектам
            // Подготовка эсперементальных данных

            string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data.txt");

            _units = ReadFile(DataPath);

            Generation _experimentalGeneration = new(AddGeneration(_units));
            DirectProblem forward = new(-1, _experimentalGeneration);
            forward.Decision();
            GeneralData.trueReadings = _experimentalGeneration.data.ToList();

            // Решение обратной задачи
            // Массив особей для обратной задачи
            List<Generation>[] populationsOfIndividuals = new List<Generation>[2];

            // Генерация случайных особей (первоначальное поколение)
            List<Generation> tempPopulation = new();
            for (int i = 0; i < individualsNums; i++)
            {
                _units = SetPrimaryGeneration(objectsNums, xs, xe, ys, ye);
                Generation _generation = new(AddGeneration(_units));
                tempPopulation.Add(_generation);
            }
            populationsOfIndividuals[0] = tempPopulation.ToList();

            // Получение решения для первоначальных особей
            reproduction = Solution(0, populationsOfIndividuals, weights, out Fp_best).ToList();

            // Вывод нужной особи и удаление лишних файлов
            System.Console.WriteLine("Решение первоначальное");
            OutputGraphs(populationsOfIndividuals[0].Count-1);

            // Решение обратной задачи
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nПОКОЛЕНИЕ {0}", p);
                Console.ResetColor();

                double weightMin = weights.Min();
                Console.WriteLine("weightMin = " + weightMin);
                int strongIndividual = weights.IndexOf(weightMin);
                Console.WriteLine("strongIndividual = " + strongIndividual);
                weights.Clear();

                if(Fp_best <= Eps)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Достигнута искомая точность");
                    Console.ResetColor();
                    break;
                }
                if (p >= MaxP)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Достигнут предел поколений");
                    Console.ResetColor();
                    break;
                }

                DeleteFiles();

                // Операция кроссинговера для получения новых особей
                populationsOfIndividuals[1] = CrossingOver(populationsOfIndividuals[0], in reproduction).ToList();

                // Решение прямой задачи для полученных особей
                reproduction = Solution(1, populationsOfIndividuals, weights, out Fp_best).ToList();

                // Замена слабых особей из нового поколения на сильных особей нового поколения
                double weightMax = weights.Max();
                Console.WriteLine("weightMin = " + weightMin);
                Console.WriteLine("weightMax = " + weightMax);
                int weakIndividual = weights.IndexOf(weightMax);
                Console.WriteLine("strongIndividual = " + strongIndividual);
                Console.WriteLine("weakIndividual = " + weakIndividual);

#if false
                if (weightMax > weightMin)
                {
                    Console.WriteLine(" Заменена особь {0} на {1}", weakIndividual, strongIndividual);



                    //populationsOfIndividuals[1] = SwapWeakToStrongIndividual(in populationsOfIndividuals, weakIndividual, strongIndividual).ToList();
                } 
#endif

                foreach (var item in weights)
                    Console.WriteLine("\t" + item);
                Console.WriteLine();

                populationsOfIndividuals[0] = populationsOfIndividuals[1].ToList();
                populationsOfIndividuals[1].Clear();

                p++;

                Thread.Sleep(1000);
                string? test = Console.ReadLine();
                if (test == "test")
                    OutputGraphs(populationsOfIndividuals[0].Count - 1);
            }

            // Вывод нужной особи и удаление лишних файлов при выходе из программы
            System.Console.WriteLine("\nИтоговые фигуры");
            OutputGraphs(populationsOfIndividuals[0].Count - 1);
        }

        private static List<(List<Strata>, List<List<double>>, List<double>)> SwapWeakToStrongIndividual(in List<(List<Strata>, List<List<double>>, List<double>)>[] input, int weakIndividual, int strongIndividual)
        {
            List<(List<Strata>, List<List<double>>, List<double>)> temp = new();
            for (int i = 0; i < input[1].Count; i++)
            {
                if (i != weakIndividual)
                {
                    (List<Strata>, List<List<double>>, List<double>) addTemp = input[1][i];
                    temp.Add(addTemp);
                }
                else
                {
                    (List<Strata>, List<List<double>>, List<double>) addTemp = input[0][strongIndividual];
                    temp.Add(addTemp);
                }
            }

            return temp;
        }

        private static List<double> Solution(int i, List<Generation>[] populationsOfIndividuals, List<double> weights, out double Fp_best)
        {
            int q = 0;
            foreach (var pop in populationsOfIndividuals[i])
            {
                pop.data.Clear();
                DirectProblem back = new(q, pop);
                back.Decision();

                pop.GetFunctional(GeneralData.trueReadings);
                q++;

                Console.Write("Функционал: ");
                Console.WriteLine(pop.Functional);

                weights.Add(pop.Functional);
            }

            List<double> reproduction = GetReproduction(weights);

            Console.Write("Функционалы поколения: ");
            Fp_best = weights.Min();
            foreach (var func in weights)
            {
                if(func == weights[weights.IndexOf(Fp_best)])
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("{0:F3} ", func);
                    Console.ResetColor();
                }
                else
                    Console.Write("{0:F3} ", func);
            }
            Console.WriteLine();

            
            Console.Write("Лучшая особь ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("{0} ", weights.IndexOf(Fp_best));
            Console.ResetColor();
            Console.Write("с функционалом ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("{0:F5}\n", Fp_best);
            Console.ResetColor();
            Console.WriteLine();

            return reproduction;
        }

        private static List<double> GetReproduction(List<double> weights)
        {
            double functionalitySum = 0;
            List<double> temp = new();
            List<double> reproduction = new() { 0 };

            foreach (var item in weights)
            {
                functionalitySum += item;
            }
            foreach (var item in weights)
            {
                temp.Add(functionalitySum / item);
            }

            functionalitySum = 0;
            foreach (var item in temp)
            {
                functionalitySum += item;
            }
            foreach (var item in temp)
            {
                reproduction.Add(item / functionalitySum);
            }

            Console.WriteLine("\nВерятность выбора:");
            foreach (var item in reproduction)
                Console.Write(item + " ");
            Console.WriteLine();

            for (int i = 1; i < reproduction.Count; i++)
                reproduction[i] += reproduction[i - 1];

            Console.WriteLine("\nПрямая 0 -> 1:");
            foreach (var item in reproduction)
                Console.Write(item + " ");
            Console.WriteLine();
            Console.WriteLine();

            return reproduction;
        }

        private static Tuple<int, int> GetTuple(in int rd1, in int rd2)
        {
            var aTuple = Tuple.Create<int, int>(rd1, rd2);
            return aTuple;
        }

        private static void CheckingForRepetition(ref int item1, ref int item2, int Count, ref List<Tuple<int, int>> pairs, ref bool repeat, in List<double> reproduction)
        {
            Random rand = new();
            do
            {
                item1 = randI(Count, in reproduction);
                item2 = randI(Count, in reproduction);

                Tuple<int, int> _tuple = GetTuple(in item1, in item2);
                Tuple<int, int> _tupleback = GetTuple(in item2, in item1);

                if (!pairs.Contains(_tuple) && !pairs.Contains(_tupleback))
                {
                    repeat = false;
                    pairs.Add(_tuple);
                    pairs.Add(_tupleback);
                }
                else
                {
                    //Console.WriteLine("Повторение особей");
                    repeat = true;
                }

            } while (item1 == item2 || repeat);

            int randI(int count, in List<double> Reproduction)
            {
                double rdIndexItem = rand.NextDouble();
                //Console.WriteLine("Тык: " + rdIndexItem);
                int rdI = GetItemIndex(count, rdIndexItem, in Reproduction);
                return rdI;
            }
        }

        public static int GetItemIndex(int count, double rdIndexItem, in List<double> Reproduction)
        {
            int index = -1;

            for (int i = 0; i < count; i++)
            {
                if (rdIndexItem >= Reproduction[i] && rdIndexItem < Reproduction[i + 1])
                {
                    index = i;
                }
            }

            return index;
        }

        private static void CheckingForRepetition(ref int item1, ref int item2, int Count1, int Count2, ref List<Tuple<int, int>> pairs, ref bool repeat)
        {
            Random rand = new();
            do
            {
                item1 = rand.Next(Count1);
                item2 = rand.Next(Count2);

                Tuple<int, int> _tuple = GetTuple(in item1, in item2);
                Tuple<int, int> _tupleback = GetTuple(in item2, in item1);

                if (!pairs.Contains(_tuple) && !pairs.Contains(_tupleback))
                {
                    repeat = false;
                    pairs.Add(_tuple);
                    pairs.Add(_tupleback);
                }
                else
                {
                    repeat = true;
                }

            } while (repeat);
        }

        private static List<Generation> CrossingOver(List<Generation> individuals, in List<double> reproduction)
        {
            List<Generation> generation = new();

            Random rdIndCount = new();                  // Рандом для получения количества особей для замен
            int indCount = rdIndCount.Next(1, individuals.Count + 1);
            List<Tuple<int, int>> indPairs = new();
            bool indRepeat = true;
            for (int j = 0; j < indCount; j++)
            {      
                int _item1 = 0, _item2 = 0;
                CheckingForRepetition(ref _item1, ref _item2, individuals.Count, ref indPairs, ref indRepeat, in reproduction);
                System.Console.WriteLine("Особь: {0} <-> {1}", _item1, _item2);

                Random rdObjCount = new();              // Рандом для получения количества объектов особей для замен
                int objCount = rdObjCount.Next(1, individuals[0].individual.Count + 1);

                List<Tuple<int, int>> objPairs = new();
                bool objRepeat = true;
                for (int i = 0; i < objCount; i++)
                {
                    int _obj1 = 0, _obj2 = 0;
                    CheckingForRepetition(ref _obj1, ref _obj2, individuals[_item1].individual.Count, individuals[_item2].individual.Count, ref objPairs, ref objRepeat);
                    System.Console.WriteLine("\tОбъект: {0} <-> {1}", _obj1, _obj2);

                    Random rdParamsCount = new();       // Рандом для получения количества параметров объекта для замены
                    int paramsCount = rdParamsCount.Next(1, individuals[0].individual[0].Params!.Count + 1);

                    List<int> randomIndexes = new();
                    bool indexesRepeat;
                    for (int k = 0; k < paramsCount; k++)
                    {
                        int _indexParam;
                        do
                        {
                            _indexParam = rdParamsCount.Next(individuals[0].individual[0].Params!.Count);
                            if (!randomIndexes.Contains(_indexParam))
                            {
                                indexesRepeat = false;
                                randomIndexes.Add(_indexParam);
                            }
                            else
                            {
                                indexesRepeat = true;
                                //System.Console.WriteLine("\t\tПовторение");
                            }
                        } while (indexesRepeat);
                        Swap(_indexParam, individuals[_item1].individual[_obj1], individuals[_item2].individual[_obj2]);
                    }
                }
            }

            //foreach (var item in individuals)
            //{
            //    List<double> functional = new();
            //    List<List<double>> Z = new();
            //    List<Strata> list = new();
            //    foreach (var unit in item.Item1)
            //        list.Add(unit);
            //    generation.Add((list, Z, functional));
            //}
            //return generation;

            return individuals;
        }

        private static void Swap(int index, Strata item1, Strata item2)
        {
            double temp;
            if (item1.Params is not null && item2.Params is not null)
            {
                temp = item1.Params[index];
                item1.Params[index] = item2.Params[index];
                item2.Params[index] = temp;

                System.Console.WriteLine("\t\tПараметр: {0}", index);
                System.Console.WriteLine("\t\t\tЗамена: {0} <-> {1}", item1.Params[index], item2.Params[index]);

                Init(item1);
                Init(item2);
            }

            static void Init(Strata item) => item.GetFromList();
        }

        private static void OutputGraphs(int indCount)
        {
            Console.WriteLine("Введите номер особи из {0}: ", indCount);
            while (true)
            {
                string? a = Console.ReadLine();
                if (a != "next")
                {
                    if (int.TryParse(a, out int result))
                        DrawPlot(result);
                    else
                        Console.WriteLine("Некорректное значение");
                }
                else
                    break;
            }
        }

        private static void DeleteFiles()
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
            List<string[]> primaryGeneration = new();
            Random rand = new();

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
                    double depth = rand.NextDouble() * Math.Abs(borderMax) * -2;
                    if (depth >= 0) depth = -2;
                    
                    switch (i)
                    {
                        case 0:
                            arr[i] = Convert.ToString(RandomDoubleInRange(xs, xe));
                            continue;
                        case 2:
                            arr[i] = Convert.ToString(RandomDoubleInRange(ys, ye));
                            continue;
                        case 4:
                            arr[i] = Convert.ToString(RandomDoubleInRange(depth, -1));
                            continue;
                        case 5:
                            arr[i] = Convert.ToString(RandomDoubleInRange(1, Math.Abs(double.Parse(arr[4]))));
                            continue;
                        default:
                            arr[i] = Convert.ToString(RandomDoubleInRange(1, borderMax));
                            continue;
                    }
                }
                Random rd = new();
                //arr[arr.Length - 1] = Convert.ToString(rd.Next(2000, 4000));
                arr[arr.Length - 1] = "3000";
                primaryGeneration.Add(arr);
            }

            static double RandomDoubleInRange(double minValue, double maxValue)
            {
                Random rand = new();
                return minValue + rand.NextDouble() * (maxValue - minValue);
            }   

            return primaryGeneration;
        }

        private static List<Strata> AddGeneration(List<string[]> _units)
        {
            List<Strata> Units = new();

            for (int i = 0; i < _units.Count; i++)
            {
                Strata unit = new(i, _units);
                unit.SetToList();
                Units.Add(unit);
            }

            return Units;
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