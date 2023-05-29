using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GravityExploration
{
    public class Generation
    {
        public Generation(List<Strata> individual)
        {
            this.individual = individual;
        }

        public List<Strata> individual = new();
        public List<List<double>> data = new();
        public double Functional { get; private set; }

        public void GetFunctional(double E_pogr)
        {
            if (GeneralData.trueReadings is not null)
            {
                int n = GeneralData.trueReadings.Count * GeneralData.trueReadings[0].Count;
                double w;
                for (int i = 0; i < data.Count; i++)
                {
                    for (int j = 0; j < data[i].Count; j++)
                    {
                        if (Math.Abs(GeneralData.trueReadings[i][j]) < Math.Abs(E_pogr))
                            w = 1 / Math.Abs(E_pogr);
                        else
                            w = 1 / Math.Abs(GeneralData.trueReadings[i][j]);
                        //Console.WriteLine("{0} - {1} = {2}", data[i][j], GeneralData.trueReadings[i][j], data[i][j] - GeneralData.trueReadings[i][j]);
                        Functional += Math.Pow(w * (data[i][j] - GeneralData.trueReadings[i][j]), 2);
                    }
                }
                Functional /= n;
            }
        }
    }
}
