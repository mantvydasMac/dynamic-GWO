using GWO.Util;

namespace GWO.Functions;

public class GaussianPeak : OptimizationFunction
{
    private double Sigma;
    
    public GaussianPeak(int dimension, Func<int, double> shiftFunction, double sigma = 1.0)
    {
        D = dimension;
        Bounds = new Bounds(-5, 5);
        Shift = shiftFunction;
        Sigma = sigma;

        Optimum = new Vector(dimension);
    }

    public override double CalculateDynamic(Vector input, int iteration)
    {
        if (input.Size() != D) throw new Exception("input array invalid length");
    
        // Controls sharpness of the peak (lower = harder tracking)
        double sigma2 = Sigma * Sigma;

        
        // Moving optimum center
        double shift = Shift(iteration);

        double sumSq = 0.0;

        for (int i = 0; i < D; i++)
        {
            double diff = input.Get(i) - shift;
            sumSq += diff * diff;
        }

        // Gaussian peak (converted to minimization problem)
        
        double value = Math.Exp(-sumSq / (2.0 * sigma2));
        return 1.0 - value;
    }
}