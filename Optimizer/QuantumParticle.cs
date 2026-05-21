using GWO.Util;

namespace GWO.Optimizer.Populations;

public class QuantumParticle
{
    public Vector Solution;
    public Vector? PersonalBest;
    public double Fitness;
    public double PersonalBestFitness;
    public int Dimension;
    
    private Random _random = new Random();
    
    public QuantumParticle(int dimension)
    {
        Dimension = dimension;
        Solution = new Vector(dimension);
    }
    
    public void GenerateSolution(Bounds bounds)
    {
        Solution = RandomDouble.GetVector(_random, Dimension, bounds);
    }

    public void SetNewSolution(Bounds bounds, Vector globalBest)
    {
        double stDev = Vector.EuclideanDistance(PersonalBest, globalBest);
        stDev = Math.Max(stDev, 1e-10);
        
        for (int i = 0; i < Dimension; ++i)
        {
            double mean = (PersonalBest.Get(i) + globalBest.Get(i)) / 2;
            
            Solution.Set(i, RandomNormal.NextGaussian(_random, mean, stDev));
        }
        
        Solution.Clamp(bounds);
    }

    public void UpdatePersonalBest(Vector solution, double fitness)
    {
        if (PersonalBest == null || fitness < PersonalBestFitness)
        {
            PersonalBest = solution.Clone();
            PersonalBestFitness = fitness;
        }
    }
    
    
    // Vector x = RandomNormal.GetVector(_random, Dimension);
    //
    // double dist = 0;
    //
    //     for (int i = 0; i < Dimension; ++i)
    // {
    //     dist += Math.Pow(x.Get(i), 2);
    // }
    //
    // dist = Math.Sqrt(dist);
    //
    // double u = RandomDouble.Get(_random, new Bounds(0, 1));
    //
    // Solution = cloudRadius * x * Math.Pow(u, Math.Pow(Dimension, -1)) / dist;
}