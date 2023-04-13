using static System.Math;

namespace GravityExploration
{
    public class Strata
    {
        public double Z_Start { get; set; }     // Начальная координата Z
        public double Z_Stop { get; set; }      // Конечная координата Z
        public double X_Start { get; set; }     // Начальная координата X
        public double X_Stop { get; set; }      // Конечная координата X
        public double Y_Start { get; set; }     // Начальная координата Y
        public double Y_Stop { get; set; }      // Конечная координата Y
        public double Density { get; set; }     // Плотность
        public double Weight { get; set; }      // Масса ( вычисляется отдельно )

        #region Comments
        //public double Depth { get; set; }      // Глубина залегания
        //public double Radius { get; set; }     // Радиус шара
        //public double Shift { get; set; }      // Смещение от OY

        //public double Weight                    // Масса
        //{
        //    get { return weight; }
        //}
        //private double weight;

        //public void SetMass()
        //{
        //    weight = (4.0 / 3.0) * PI * Pow(Radius, 3) * Density;   // Избыточная масса шара
        //} 
        #endregion Comments
    }
}