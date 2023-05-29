﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GravityExploration
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Входные данные
            int objectsNums;                                    // Количество объектов у особи
            int individualsNums = 5;                            // Количество особей
            double xs = -2, xe = 2;                             // Границы сетки по OX
            double ys = -2, ye = 2;                             // Границы сетки по OY

            // Условия для обратной задачи
            double mutationPercent = 0.05;                       // Процент мутации
            double SwapOldIndividualPercent = 0.6;                // Процент замены новой особи на старую
            double Eps = 0.15;                                   // Точность
            int MaxP = 1000;                                    // Максимальное количество итераций
            double Fp_best = 1;                                 // Лучшее значение
            int p = 0;                                          // Итерация

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

            // Подготовка к решению обратной задачи (прямая задача для подготовки экспериментальных данных)
            // Получение значений аномалии по ЗАРАНЕЕ ЗАДАННЫМ объектам
            string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data.txt");
            _units = ReadFile(DataPath);

            Generation _experimentalGeneration = new(AddGeneration(_units));
            DirectProblem forward = new(-1, _experimentalGeneration);
            forward.Decision();
            GeneralData.trueReadings = _experimentalGeneration.data;

            // Расчет порогового значения Eps
            List<double>? tempEps = new();
            foreach (var list in GeneralData.trueReadings)
            {
                tempEps.Add(list.Max());
            }
            double E_pogr = tempEps.Max() * 1e-2;

            // Решение обратной задачи
            // Массив особей для обратной задачи
            List<List<Generation>> populationsOfIndividuals = new();

            // Генерация случайных особей (первоначальное поколение)
            List<Generation> tempPopulation = new();
            for (int i = 0; i < individualsNums; i++)
            {
                Random rdCount = new();
                objectsNums = rdCount.Next(1, 5);
                _units = SetPrimaryGeneration(objectsNums, xs, xe, ys, ye);
                Generation _generation = new(AddGeneration(_units));
                tempPopulation.Add(_generation);
            }

            // Получение решения для первоначальных особей
            reproduction = Solution(tempPopulation, weights, out Fp_best, E_pogr).ToList();

            // Запись в список популяции
            populationsOfIndividuals.Add(tempPopulation);

            // Вывод нужной особи
            System.Console.WriteLine("Решение первоначальное");
            OutputGraphs(populationsOfIndividuals[p].Count-1);

            // Решение обратной задачи
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nПОКОЛЕНИЕ {0}", p);
                Console.ResetColor();

                double strongIndividual = weights.Min();
                int strongIndividualIndex = weights.IndexOf(strongIndividual);
                weights.Clear();

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Лучшая особь: {0}\tФункционал: {1}\n", strongIndividualIndex, strongIndividual);
                Console.ResetColor();

                if (Fp_best <= Eps)
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
#if true
                Console.WriteLine("\tКроссинговер");
                List<Generation> temp = new(CrossingOver(populationsOfIndividuals[p], in reproduction));
                List<Generation> newestPopulation = new();

                foreach(var item in temp)
                {
                    Generation _generation = new(item.individual);
                    newestPopulation.Add(_generation);
                }

                populationsOfIndividuals.Add(newestPopulation);
#endif


                // Мутация
#if true
                Mutation(xs, ye, mutationPercent, ref newestPopulation);
#endif

                // Решение прямой задачи для полученных особей
                reproduction = Solution(newestPopulation, weights, out Fp_best, E_pogr).ToList();

                // Добавление сильной особи из прошлого поколения
                Generation newGeneration = new(populationsOfIndividuals[p][strongIndividualIndex].individual);
                newestPopulation.Add(newGeneration);

                // Удаление слабой особи из текущего поколения
                double weakIndividual = weights.Max();
                int weakIndividualIndex = weights.IndexOf(weakIndividual);
                newestPopulation.Remove(newestPopulation[weakIndividualIndex]);

                // Остановка для просмотра промежуточных особей
#if false
                Thread.Sleep(1000);
                string? test = Console.ReadLine();
                if (test == "test")
                    OutputGraphs(newestPopulation.Count - 1);
#endif


                // Добавление ещё одной СЛУЧАЙНОЙ особи
#if false
                if ((weights.Max() - weights.Min()) < 1e-3)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("Добавлена новая особь");
                    Console.ResetColor();
                    AddIndividual(newestPopulation, xs, xe, ys, ye);
                }
#endif


                // Замена слабых особей из нового поколения на сильных особей прошлого поколения (селекция)
#if false
                Random rdSwapOldIndividual = new();
                double percent = rdSwapOldIndividual.NextDouble();
                if (percent <= SwapOldIndividualPercent)
                    for (int i = 0; i < replacementCount; i++)
                    {                    
                        if (strongWeights[i] < weakWeights[i])
                        {
                            ReplacementByFunctionality(p, newestPopulation, weakIndividuals[i], strongIndividuals[i]);
                        }
                    }
#endif

                if (p>0)
                {
                    populationsOfIndividuals[p - 1].Clear();
                }

                #region Comments
                //foreach (var pop in populationsOfIndividuals)
                //    foreach (var item in pop)
                //    {
                //        string outputPath = Path.Combine(Directory.GetCurrentDirectory(),"p=" + p + "_f=" + item.Functional + ".txt");
                //        using (StreamWriter sw = new(outputPath, false))
                //        {
                //            sw.WriteLine(item.Functional);
                //            foreach (var z in item.data)
                //            {
                //                foreach (var _z in z)
                //                {
                //                    sw.Write(_z + " ");
                //                }
                //                sw.WriteLine();
                //            }
                //        };
                //    }

                //(populationsOfIndividuals[0], populationsOfIndividuals[1]) = (populationsOfIndividuals[1], populationsOfIndividuals[0]);
                //populationsOfIndividuals[0].Clear();
                //populationsOfIndividuals[0] = populationsOfIndividuals[1].ToList();
                //populationsOfIndividuals[1].Clear();
                #endregion

                p++;

                void ReplacementByFunctionality(int p, List<Generation> newestIndividual, int weakIndividual, int strongIndividual)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("Заменена особь {0} на {1}", weakIndividual, strongIndividual);
                    Console.ResetColor();

                    (newestIndividual[weakIndividual], populationsOfIndividuals[p][strongIndividual]) = (populationsOfIndividuals[p][strongIndividual], newestIndividual[weakIndividual]);
                }
            }

            // Вывод нужной особи и удаление лишних файлов при выходе из программы
            System.Console.WriteLine("\nИтоговые фигуры");
            OutputGraphs(populationsOfIndividuals[0].Count - 1);
        }

        private static void AddIndividual(List<Generation> populationsOfIndividuals, double xs, double xe, double ys, double ye)
        {
            List<string[]> _unit = SetPrimaryGeneration(1, xs, xe, ys, ye);
            Generation _generation = new(AddGeneration(_unit));
            populationsOfIndividuals.Add(_generation);
        }

        private static void Mutation(double xs, double ye, double mutationPercent, ref List<Generation> generation)
        {
            for (int i = 0; i < generation.Count; i++)
            {
                Random rdMutation = new();
                Random rdIndex = new();
                Random rdIndexObjCount = new();

                double mutation = rdMutation.NextDouble();
                if (mutation <= mutationPercent)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Мутация: особь {0}", i);
                    Console.ResetColor();

                    int count = rdIndexObjCount.Next(1, generation[i].individual.Count + 1);
                    for (int j = 0; j < count; j++)
                    {
                        generation[i].individual[j].SetToList();
                        int index = rdIndex.Next(0, 6);
                        double param;
                        if (index < 2 && index >= 0)
                        {
                            param = RandomDoubleInRange(xs, ye);
                            generation[i].individual[j].Params[index] = param;
                        }
                        else if (index < 5 && index > 2)
                        {
                            param = rdIndex.NextDouble() * Math.Abs(xs);
                            generation[i].individual[j].Params[index] = param;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                            Console.WriteLine("Мутация не удалась");
                            Console.ResetColor();
                        }
                            
                        generation[i].individual[j].GetFromList();
                    }
                }
            }
        }

        private static List<double> Solution(List<Generation> generation, List<double> weights, out double Fp_best, double E_pogr)
        {
            List<Generation> populationsOfIndividuals = new(generation);
            int q = 0;
            foreach (var pop in populationsOfIndividuals)
            {
                pop.data.Clear();
                DirectProblem back = new(q, pop);
                back.Decision();
                
                pop.data = back.Z;

                pop.GetFunctional(E_pogr);
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
                if (func == weights[weights.IndexOf(Fp_best)])
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

            Console.WriteLine("\nВероятность выбора:");
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
                    repeat = true;
                }

            } while (item1 == item2 || repeat);

            int randI(int count, in List<double> Reproduction)
            {
                double rdIndexItem = rand.NextDouble();
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
            List<Generation> generation = new(individuals);

            Random rdIndCount = new();                  // Рандом для получения количества особей для замен
            int indCount = rdIndCount.Next(1, generation.Count + 1);
            List<Tuple<int, int>> indPairs = new();
            bool indRepeat = true;
            for (int j = 0; j < indCount; j++)
            {      
                int _item1 = 0, _item2 = 0;
                CheckingForRepetition(ref _item1, ref _item2, generation.Count, ref indPairs, ref indRepeat, in reproduction);
                //System.Console.WriteLine("Особь: {0} <-> {1}", _item1, _item2);

                Random rdObjCount = new();              // Рандом для получения количества объектов особей для замен
                int objTemp;
                if (generation[_item1].individual.Count >= generation[_item2].individual.Count)
                    objTemp = generation[_item1].individual.Count;
                else
                    objTemp = generation[_item2].individual.Count;
                int objCount = rdObjCount.Next(1, objTemp + 1);

                List<Tuple<int, int>> objPairs = new();
                bool objRepeat = true;
                for (int i = 0; i < objCount; i++)
                {
                    int _obj1 = 0, _obj2 = 0;
                    CheckingForRepetition(ref _obj1, ref _obj2, generation[_item1].individual.Count, generation[_item2].individual.Count, ref objPairs, ref objRepeat);
                    //System.Console.WriteLine("\tОбъект: {0} <-> {1}", _obj1, _obj2);

                    Random rdParamsCount = new();       // Рандом для получения количества параметров объекта для замены
                    int paramsCount = rdParamsCount.Next(1, generation[0].individual[0].Params!.Count + 1);

                    List<int> randomIndexes = new();
                    bool indexesRepeat;
                    for (int k = 0; k < paramsCount; k++)
                    {
                        int _indexParam;
                        do
                        {
                            _indexParam = rdParamsCount.Next(generation[0].individual[0].Params!.Count);
                            if (!randomIndexes.Contains(_indexParam))
                            {
                                indexesRepeat = false;
                                randomIndexes.Add(_indexParam);
                            }
                            else
                            {
                                indexesRepeat = true;
                            }
                        } while (indexesRepeat);
                        Swap(_indexParam, generation[_item1].individual[_obj1], generation[_item2].individual[_obj2]);
                    }
                    randomIndexes.Clear();
                }
                objPairs.Clear();
            }
            indPairs.Clear();

            return generation;
        }

        private static void Swap(int index, Strata item1, Strata item2)
        {
            //double temp;
            item1.SetToList();
            item2.SetToList();

            if (item1.Params is not null && item2.Params is not null)
            {
                //temp = item1.Params[index];
                //item1.Params[index] = item2.Params[index];
                //item2.Params[index] = temp;

                (item1.Params[index], item2.Params[index]) = (item2.Params[index], item1.Params[index]);
                //(item2.Params[index], item1.Params[index]) = (item2.Params[index], item1.Params[index]);

                //System.Console.WriteLine("\t\tПараметр: {0}", index);
                //System.Console.WriteLine("\t\t\tЗамена: {0} <-> {1}", item1.Params[index], item2.Params[index]);

                //Init(item1);
                //Init(item2);
            }
            item1.GetFromList();
            item2.GetFromList();

            static void Init(Strata item)
            {
                item.GetFromList();
            }
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
            foreach (string fundedFile in Directory.EnumerateFiles(catalog, fileName, SearchOption.AllDirectories))
            {
                try
                {
                    File.Delete(fundedFile);
                }
                catch (Exception e)
                {
                    Console.WriteLine("The deletion failed: {0}", e.Message);
                    break;
                }
            }

            //string fileName1 = "*f=*.txt";
            //foreach (string fundedFile in Directory.EnumerateFiles(catalog, fileName1, SearchOption.AllDirectories))
            //{
            //    try
            //    {
            //        File.Delete(fundedFile);
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine("The deletion failed: {0}", e.Message);
            //        break;
            //    }
            //}
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
                //arr[arr.Length - 1] = Convert.ToString(rd.Next(2500, 3500));
                arr[arr.Length - 1] = "3000";
                primaryGeneration.Add(arr);
            }

            return primaryGeneration;
        }

        private static double RandomDoubleInRange(double minValue, double maxValue)
        {
            Random rand = new();
            return minValue + rand.NextDouble() * (maxValue - minValue);
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