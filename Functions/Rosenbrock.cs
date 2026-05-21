using GWO.Util;

namespace GWO.Functions;

public class Rosenbrock : OptimizationFunction
{

    public Rosenbrock(int dimension, Func<int, double> shiftFunction)
    {
        D = dimension;
        Bounds = new Bounds(-5, 10);
        Shift = shiftFunction;

        Optimum = new Vector(Enumerable.Repeat(1.0, dimension).ToArray());
    }

    public override double CalculateDynamic(Vector input, int iteration)
    {
        if (input.Size() != D) throw new Exception("input array invalid length");
        
        double sum = 0;
        double shift = Shift(iteration);
        
        for (int i = 0; i < D - 1; ++i)
        {
            double x = input.Get(i) - shift;
            double xNext = input.Get(i + 1) - shift;

            sum += 100 * Math.Pow(xNext - Math.Pow(x, 2), 2) + Math.Pow(x - 1, 2);
        }

        return sum;
    }
}