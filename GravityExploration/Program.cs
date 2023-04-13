namespace GravityExploration
{
    class GeneralData
    {
        public static readonly double G = 6.672e-8;
        public static readonly double SoilDensity = 2600;
        public static readonly List<double> X = new();
        public static readonly List<double> Z = new();
        public static readonly List<double> Y = new();
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            DirectProblem.Decision();
        }
    }
}