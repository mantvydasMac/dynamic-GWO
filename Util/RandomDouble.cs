namespace GWO.Util;

public class RandomDouble
{
    public static double Get(Random rand, Bounds bounds)
    {
        return bounds.Lower + rand.NextDouble() * (bounds.Upper - bounds.Lower);
    }

    public static Vector GetVector(Random rand, int size, Bounds bounds)
    {
        double[] arr = new double[size];

        for (int i = 0; i < size; ++i)
        {
            arr[i] = Get(rand, bounds);
        }

        return new Vector(arr);
    }
}