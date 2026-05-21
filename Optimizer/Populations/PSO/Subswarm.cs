using GWO.Functions;
using GWO.Util;

namespace GWO.Optimizer.Populations.PSO;

public class Subswarm
{
    public int Size;
    public int ParticleCount;
    public int QuantumParticleCount;
    
    public List<Particle> Particles;
    public List<QuantumParticle> QuantumParticles;

    private OptimizationFunction _function;

    public Vector? GlobalBest;
    public double GlobalBestFitness = Double.PositiveInfinity;

    public double W;
    public double C1;
    public double C2;
    
    private readonly Random _random = new Random();
    
    public Subswarm(int particleCount, int quantumParticleCount, OptimizationFunction function, double w = 0.7, double c1 = 1.5, double c2 = 1.5)
    {
        Size = particleCount + quantumParticleCount;
        ParticleCount = particleCount;
        QuantumParticleCount = quantumParticleCount;
        
        _function = function;
        Particles = new List<Particle>(ParticleCount);
        QuantumParticles = new List<QuantumParticle>(QuantumParticleCount);
        
        W = w;
        C1 = c1;
        C2 = c2;
        
        for (int i = 0; i < ParticleCount; ++i)
        {
            Particle particle = new Particle(_function.D);
            Particles.Add(particle);
        }
        for (int i = 0; i < QuantumParticleCount; ++i)
        {
            QuantumParticle particle = new QuantumParticle(_function.D);
            QuantumParticles.Add(particle);
        }
    }
    
    public void Initialize(int iteration)
    {
        ResetGlobalBest();
        foreach (var particle in Particles)
        {
            particle.GenerateSolution(_function.Bounds);
            particle.Fitness = _function.CalculateDynamic(particle.Solution, iteration);
            
            particle.PersonalBest = particle.Solution.Clone();
            particle.PersonalBestFitness = particle.Fitness;
            
            UpdateGlobalBest(particle.Solution, particle.Fitness);
        }
        
        foreach (var particle in QuantumParticles)
        {
            particle.GenerateSolution(_function.Bounds);
            particle.Fitness = _function.CalculateDynamic(particle.Solution, iteration);
            
            particle.PersonalBest = particle.Solution.Clone();
            particle.PersonalBestFitness = particle.Fitness;
            
            UpdateGlobalBest(particle.Solution, particle.Fitness);
        }
    }

    public void UpdateParticles(int iteration)
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

        foreach (var particle in QuantumParticles)
        {
            particle.SetNewSolution(_function.Bounds, GlobalBest);
            particle.Fitness = _function.CalculateDynamic(particle.Solution, iteration);
            
            particle.UpdatePersonalBest(particle.Solution, particle.Fitness);
            
            UpdateGlobalBest(particle.PersonalBest, particle.PersonalBestFitness);
        }
    }

    public void ReevaluateParticleBests(int iteration)
    {
        foreach (var particle in Particles)
        {
            particle.PersonalBestFitness = _function.CalculateDynamic(particle.PersonalBest, iteration);
        }
        
        foreach (var particle in QuantumParticles)
        {
            particle.PersonalBestFitness = _function.CalculateDynamic(particle.PersonalBest, iteration);
        }
    }
    
    public double SwarmRadius()
    {
        double maxDist = 0;

        foreach (var particle in Particles)
        {
            double dist = Vector.EuclideanDistance(
                particle.Solution,
                GlobalBest);

            if (dist > maxDist)
                maxDist = dist;
        }

        return maxDist;
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
}