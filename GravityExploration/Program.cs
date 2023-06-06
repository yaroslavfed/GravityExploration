using System;
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
            #region InputData
            // Входные данные
            int objectsNums;                                    // Количество объектов у особи
            int individualsNums = 5;                            // Количество особей
            double xs = -5, xe = 5;                             // Границы сетки по OX
            double ys = -5, ye = 5;                             // Границы сетки по OY

            // Условия для обратной задачи
            double mutationPercent = 0.07;                      // Процент мутации
            double Eps = 0.1;                                  // Точность
            int MaxP = 1000;                                    // Максимальное количество итераций 
            #endregion

            double Fp_best = 1;                                 // Лучшее значение
            int p;                                              // Итерация (номер поколения)
            List<string[]> _units;                              // Задание параметров для объектов особи

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

            // Запись экспериментальных данных в файл
            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "experimental.txt");
            using (StreamWriter sw = new(outputPath, false))
            {
                sw.WriteLine(GeneralData.trueReadings.Count);

                foreach (var z in GeneralData.trueReadings)
                {
                    foreach (var _z in z)
                    {
                        sw.Write(_z + " ");
                    }
                    sw.WriteLine();
                }
            }

            Console.WriteLine("Экспериментальные данные");
            OutputGraphs(1);

            // Расчет порогового значения Eps
            List<double>? tempEps = new();
            foreach (var list in GeneralData.trueReadings)
            {
                tempEps.Add(list.Max());
            }
            double E_pogr = tempEps.Max() * 1e-2;

            // Решение обратной задачи
            // Массив особей для обратной задачи (популяция)
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

            // Запись в список популяции
            populationsOfIndividuals.Add(tempPopulation);

            // Решение обратной задачи
            for(p = 0; p < MaxP; p++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nПОКОЛЕНИЕ {0}", p);
                Console.ResetColor();

                //for (int i = 0; i < populationsOfIndividuals[p].Count; i++)
                //    DeleteFiles(i);

                // Решение прямой задачи для особей текущего поколения и расчет функционалов
                List<double> weights = new(Solution(populationsOfIndividuals[p], E_pogr));

#if false
                foreach (var item in populationsOfIndividuals[p])
                    Console.WriteLine("\tФункционал: {0}", item.Functional);
#endif

                if (p == 0)
                {
                    System.Console.WriteLine("Случайная генерация:");
                    foreach (var item in populationsOfIndividuals[p])
                        Console.WriteLine("\tФункционал: {0}", item.Functional);
                    OutputGraphs(populationsOfIndividuals[0].Count - 1);
                }

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("\nAvg = {0}", CalculationAvg(weights));
                Console.ResetColor();

                // Сохранение сильной особи из текущего поколения
                double strongIndividual = weights.Min();
                int strongIndividualIndex = weights.IndexOf(strongIndividual);
                Generation tempListStrata = new(AddStrongIndividual(populationsOfIndividuals[p][strongIndividualIndex].individual));

                // Удаление слабой особи из текущего поколения
                double weakIndividual = weights.Max();
                int weakIndividualIndex = weights.IndexOf(weakIndividual);
                populationsOfIndividuals[p].Remove(populationsOfIndividuals[p][weakIndividualIndex]);
                weights.Remove(weakIndividual);
                DeleteFiles(weakIndividualIndex);

#if false
                Console.WriteLine("\nУдалена особь с наибольшим функционалом в поколении ({0})", weakIndividual);
                foreach (var item in populationsOfIndividuals[p])
                    Console.WriteLine("Функционал: {0}", item.Functional);  
#endif

                List<double> reproduction = new(GetReproduction(weights));
                Fp_best = weights.Min();

#if false
                Console.WriteLine("Приспособленность:");
                foreach (var item in reproduction)
                    Console.WriteLine(item);
                Console.WriteLine("Лучший функционал: {0}", Fp_best);  
#endif

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Лучшая особь: {0}\tФункционал: {1}\n", strongIndividualIndex, strongIndividual);
                Console.ResetColor();

                // Проверка на достигнутую точность
                if (Fp_best <= Eps)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Достигнута искомая точность");
                    Console.ResetColor();
                    break;
                }

                // Операция кроссинговера для формирования нового поколения
#if true
                Console.WriteLine("Кроссинговер\n");
                List<Generation> temp = new(CrossingOver(populationsOfIndividuals[p], in reproduction));
                List<Generation> newestPopulation = new();

                foreach(var item in temp)
                {
                    Generation _generation = new(item.individual);
                    newestPopulation.Add(_generation);
                }
#endif

                // Мутация
#if true
                Mutation(xs, ye, mutationPercent, ref newestPopulation);
#endif

                // Добавление сильной особи из прошлого поколения
                newestPopulation.Add(tempListStrata);

                // Остановка для просмотра промежуточных особей
#if true
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

                // Запись полученного в новое поколение
                populationsOfIndividuals.Add(newestPopulation);

                if (p>0)
                {
                    populationsOfIndividuals[p - 1].Clear();
                }

            }

            if(p >= MaxP)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Достигнут предел поколений");
                Console.ResetColor();
            }


            // Вывод нужной особи и удаление лишних файлов при выходе из программы
            System.Console.WriteLine("\nИтоговые фигуры");
            OutputGraphs(populationsOfIndividuals.Last().Count - 1);
        }

        private static double CalculationAvg(List<double> weights)
        {
            double res = 0;
            foreach (var item in weights)
                res += item;
            res /= weights.Count;
            return res;
        }

        private static List<Strata> AddStrongIndividual(List<Strata> strongIndividual)
        {
            List<Strata> tempListStrata = new();
            foreach (var item in strongIndividual)
            {
                List<double> tempParams;
                if (item.Params is null)
                {
                    item.SetToList();
                    tempParams = new(item.Params);
                }
                else
                {
                    tempParams = new(item.Params);
                }
                Strata newStrata = new(tempParams);
                tempListStrata.Add(newStrata);
            }

            return tempListStrata;
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

        private static List<double> Solution(List<Generation> generation, double E_pogr)
        {
            List<double> weights = new();
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

                weights.Add(pop.Functional);
            }

            return weights;
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

            for (int i = 1; i < reproduction.Count; i++)
                reproduction[i] += reproduction[i - 1];

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
                (item1.Params[index], item2.Params[index]) = (item2.Params[index], item1.Params[index]);
                //(item2.Params[index], item1.Params[index]) = (item2.Params[index], item1.Params[index]);

                //System.Console.WriteLine("\t\tПараметр: {0}", index);
                //System.Console.WriteLine("\t\t\tЗамена: {0} <-> {1}", item1.Params[index], item2.Params[index]);
            }
            item1.GetFromList();
            item2.GetFromList();

        }

        private static void OutputGraphs(int indCount)
        {
            Console.WriteLine("Введите номер особи из {0} или next для продолжения: ", indCount);
            while (true)
            {
                string? a = Console.ReadLine();
                if (a != "next" && a != "туче")
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

        private static void DeleteFiles(int num)
        {
            string catalog = Directory.GetCurrentDirectory();
            string fileName = "output" + num + ".txt";
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