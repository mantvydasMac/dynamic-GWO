using GWO.Util;

namespace GWO.Functions;

public class Rastrigin : OptimizationFunction
{
    public Rastrigin(int dimension, Func<int, double> shiftFunction)
    {
        D = dimension;
        Bounds = new Bounds(-5.12, 5.12);
        Shift = shiftFunction;

        Optimum = new Vector(dimension);
    }

    public override double CalculateDynamic(Vector input, int iteration)
    {
        if (input.Size() != D) throw new Exception("input array invalid length");

        double sum = 0;
        double shift = Shift(iteration);

        for (int i = 0;i<D;++i)
        {
            double x = input.Get(i) - shift;
            
            sum += Math.Pow(x, 2) - 10 * Math.Cos(2 * Math.PI * x);
        }

        return 10 * D + sum;
    }
}