using GWO.Util;

namespace GWO.Functions;

public abstract class OptimizationFunction
{
    public int D;
    public Bounds Bounds;
    public Func<int, double> Shift;
    public Vector Optimum;
    
    public abstract double CalculateDynamic(Vector input, int iteration);
}