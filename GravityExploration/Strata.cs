using static System.Math;

namespace GravityExploration
{
    public class Strata
    {
        public double Depth { get; set; }      // Глубина залегания
        public double Radius { get; set; }     // Радиус шара
        public double Density { get; set; }    // Плотность шара
        public double Shift { get; set; }      // Смещение от OY
        public double Weight                   // Избыточная масса шара
        {
            get { return weight; }
        }
        private double weight;

        public void SetMass()
        {
            weight = (4.0 / 3.0) * PI * Pow(Radius, 3) * Density;
        }
    }
}
