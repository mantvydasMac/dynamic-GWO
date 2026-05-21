using GWO.Util;

namespace GWO.Functions;

public class Griewank : OptimizationFunction
{
    public Griewank(int dimension, Func<int, double> shiftFunction)
    {
        D = dimension;
        Bounds = new Bounds(-600, 600);
        Shift = shiftFunction;

        Optimum = new Vector(dimension);
    }

    public override double CalculateDynamic(Vector input, int iteration)
    {
        if (input.Size() != D) throw new Exception("input array invalid length");
        
        double shift = Shift(iteration);

        double c1 = 0;
        double c2 = 1;
        
        for (int i = 0; i < D; ++i)
        {
            c1 += Math.Pow(input.Get(i) - shift, 2);
            c2 *= Math.Cos((input.Get(i) - shift) / Math.Sqrt(i+1));
        }

        c1 /= 4000;

        return 1 + c1 - c2;
    }
}