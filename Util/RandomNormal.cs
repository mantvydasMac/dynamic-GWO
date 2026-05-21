namespace GWO.Util;

public class RandomNormal
{
    private static bool _hasSpare = false;
    private static double _spare;

    public static double NextGaussian(Random rand, double mean = 0.0, double stdDev = 1.0)
    {
        if (_hasSpare)
        {
            _hasSpare = false;
            return _spare * stdDev + mean;
        }

        _hasSpare = true;

        double u, v, s;

        do
        {
            u = 2.0 * rand.NextDouble() - 1.0;
            v = 2.0 * rand.NextDouble() - 1.0;
            s = u * u + v * v;
        }
        while (s >= 1.0 || s == 0.0);

        s = Math.Sqrt(-2.0 * Math.Log(s) / s);

        _spare = v * s;

        return (u * s) * stdDev + mean;
    }
    
    public static Vector GetVector(Random rand, int size)
    {
        double[] arr = new double[size];

        for (int i = 0; i < size; ++i)
        {
            arr[i] = NextGaussian(rand);
        }

        return new Vector(arr);
    }
}