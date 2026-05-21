using GWO.Util;

namespace GWO.Optimizer;

public class Particle
{
    public Vector Solution;
    public Vector Velocity;
    public Vector? PersonalBest;
    public double Fitness;
    public double PersonalBestFitness;
    public int Dimension;

    public Particle(int dimension)
    {
        Dimension = dimension;
        Solution = new Vector(dimension);
        Velocity = new Vector(dimension);
    }

    public void GenerateSolution(Bounds bounds)
    {
        Random random = new Random();

        Solution = RandomDouble.GetVector(random, Dimension, bounds);
    }

    public void UpdatePersonalBest(Vector solution, double fitness)
    {
        if (PersonalBest == null || fitness < PersonalBestFitness)
        {
            PersonalBest = solution.Clone();
            PersonalBestFitness = fitness;
        }
    }
}