using static System.Math;

namespace GravityExploration
{
    public class Strata
    {
        public Strata(int i, List<string[]> _units)
        {
            CentreX = double.Parse(_units[i][0]);
            StepX = double.Parse(_units[i][1]);
            CentreY = double.Parse(_units[i][2]);
            StepY = double.Parse(_units[i][3]);
            CentreZ = double.Parse(_units[i][4]);
            StepZ = double.Parse(_units[i][5]);
            Density = double.Parse(_units[i][6]);
        }

        public double CentreZ { get; private set; }     // Начальная координата Z
        public double StepZ { get; private set; }      // Конечная координата Z
        public double CentreX { get; private set; }     // Начальная координата X
        public double StepX { get; private set; }      // Конечная координата X
        public double CentreY { get; private set; }     // Начальная координата Y
        public double StepY { get; private set; }      // Конечная координата Y
        public double Density { get; private set; }     // Плотность

        public double Weight { get; private set; }      // Масса ( вычисляется отдельно )
    }
}