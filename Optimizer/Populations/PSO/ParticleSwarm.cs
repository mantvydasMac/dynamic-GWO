using GWO.Functions;
using GWO.Util;

namespace GWO.Optimizer.Populations.PSO;

public class ParticleSwarm
{
    public int Size;
    public List<Particle> Particles;

    private OptimizationFunction _function;

    public Vector? GlobalBest;
    public double GlobalBestFitness = Double.PositiveInfinity;

    public double W;
    public double C1;
    public double C2;
    
    private readonly Random _random = new Random();

    public ParticleSwarm(int size, OptimizationFunction function, double w = 0.7, double c1 = 1.5, double c2 = 1.5)
    {
        Size = size;
        _function = function;
        Particles = new List<Particle>(Size);

        W = w;
        C1 = c1;
        C2 = c2;
        
        for (int i = 0; i < Size; ++i)
        {
            Particle particle = new Particle(_function.D);
            Particles.Add(particle);
        }
    }
    
    private void Initialize()
    {
        ResetGlobalBest();
        foreach (var particle in Particles)
        {
            particle.GenerateSolution(_function.Bounds);
            particle.Fitness = _function.CalculateDynamic(particle.Solution, 0);
            particle.UpdatePersonalBest(particle.Solution, particle.Fitness);
            
            UpdateGlobalBest(particle.Solution, particle.Fitness);
        }
    }

    public List<IterationResult> Process(int iterationNumber)
    {
        Initialize();

        List<IterationResult> results = new List<IterationResult>(iterationNumber);
        
        int T = iterationNumber;

        for (int t = 1; t <= T; ++t)
        {
            UpdateParticles(t);
            
            results.Add(new IterationResult(GlobalBestFitness, 0, CalculateDistanceToOptimum(t), CalculateDiversity()));
        }

        return results;
    }

    private void UpdateParticles(int iteration)
    {

        foreach (var particle in Particles)
        {
            particle.Velocity = W * particle.Velocity
                           + C1 * RandomDouble.GetVector(_random, _function.D, new Bounds(0, 1)) *
                           (particle.PersonalBest - particle.Solution)
                           + C2 * RandomDouble.GetVector(_random, _function.D, new Bounds(0, 1)) *
                           (GlobalBest - particle.Solution);
            particle.Solution += particle.Velocity;
            particle.Solution.Clamp(_function.Bounds);
            
            particle.Fitness = _function.CalculateDynamic(particle.Solution, iteration);
            
            particle.UpdatePersonalBest(particle.Solution, particle.Fitness);
            
            UpdateGlobalBest(particle.PersonalBest, particle.PersonalBestFitness);
        }
    }
    
    private void ResetGlobalBest()
    {
        GlobalBest = null;
        GlobalBestFitness = Double.PositiveInfinity;
    }

    private void UpdateGlobalBest(Vector solution, double fitness)
    {
        if (GlobalBest == null || fitness < GlobalBestFitness)
        {
            GlobalBest = solution.Clone();
            GlobalBestFitness = fitness;
        }
    }
    
    private double CalculateDiversity()
    {
        int dimension = _function.D;

        var centroid = new Vector(dimension);

        double sum;
        for (int i = 0; i < dimension; ++i)
        {
            sum = 0;

            foreach (var particle in Particles)
            {
                sum += particle.Solution.Get(i);
            }
            
            centroid.Set(i, sum/Size);
        }

        
        double diversity = 0;

        double distance;
        foreach (var particle in Particles)
        {
            distance = 0;

            for (int i = 0; i < dimension; i++)
            {
                distance += Math.Pow(
                    particle.Solution.Get(i) - centroid.Get(i),
                    2);
            }

            diversity += Math.Sqrt(distance);
        }

        return diversity / Size;
    }

    private double CalculateDistanceToOptimum(int iteration)
    {
        double distance = 0;
        double shift = _function.Shift(iteration);
            
        for (int i = 0; i < _function.D; ++i)
        {
            double optimum = _function.Optimum.Get(i) + shift;
                
            distance += Math.Pow(GlobalBest.Get(i) - optimum, 2);
        }

        return Math.Sqrt(distance);
    }
}