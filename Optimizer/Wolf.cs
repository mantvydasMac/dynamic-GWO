using GWO.Util;

namespace GWO.Optimizer;

public class Wolf
{
    public Vector Solution;
    public double Fitness;
    public int Dimension;

    public Wolf(int dimension)
    {
        Dimension = dimension;
        Solution = new Vector(dimension);
    }

    public void GenerateSolution(Bounds bounds)
    {
        Random random = new Random();

        Solution = RandomDouble.GetVector(random, Dimension, bounds);
    }

    public static Wolf DefaultWolf()
    {
        Wolf wolf = new Wolf(1);
        wolf.Fitness = Double.PositiveInfinity;

        return wolf;
    }
}