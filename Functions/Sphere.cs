using GWO.Util;

namespace GWO.Functions;

public class Sphere : OptimizationFunction
{
    public Sphere(int dimension, Func<int, double> shiftFunction)
    {
        D = dimension;
        Bounds = new Bounds(-50, 50);
        Shift = shiftFunction;

        Optimum = new Vector(dimension);
    }

    public override double CalculateDynamic(Vector input, int iteration)
    {
        if (input.Size() != D) throw new Exception("input array invalid length");

        double sum = 0;
        double shift = Shift(iteration);
        
        for (int i = 0; i < D; ++i)
        {
            sum += Math.Pow(input.Get(i) - shift, 2);
        }

        return sum;
    }
}