using System.Runtime.InteropServices;
using GWO.Functions;
using GWO.Util;

namespace GWO.Optimizer.Populations.PSO;

public class MPSO
{
    private int SwarmCount;
    private int ExcessSwarmCount;
    private int SubswarmParticleCount;
    private int SubswarmQuantumParticleCount;

    private double ExclusionRadius;
    public double ConvergenceRadius;
    
    private OptimizationFunction _function;

    private List<Subswarm> Swarms = new List<Subswarm>();

    public int NumberOfSwarms()
    {
        return Swarms.Count;
    }

    public MPSO(OptimizationFunction function, 
        int startingSwarmCount = 1, 
        int excessSwarmCount = 1, 
        int subswarmParticleCount = 15, 
        int subswarmQuantumParticleCount = 8, 
        double exclusionRadius = 0.5,
        double convergenceRadiusParameter = 0.01
        )
    {
        _function = function;
        
        SwarmCount = startingSwarmCount;
        ExcessSwarmCount = excessSwarmCount;
        SubswarmParticleCount = subswarmParticleCount;
        SubswarmQuantumParticleCount = subswarmQuantumParticleCount;
        ExclusionRadius = exclusionRadius;
        ConvergenceRadius = SearchSpaceDiagonal() * convergenceRadiusParameter;
    }

    private void Initialize()
    {
        for (int i = 0; i < SwarmCount; ++i)
        {
            Subswarm swarm = new Subswarm(SubswarmParticleCount, SubswarmQuantumParticleCount, _function);
            swarm.Initialize(0);
            
            Swarms.Add(swarm);
        }
    }
    
    public (double, double, List<IterationResult>) Process(int iterationNumber)
    {
        // Initialization
        Initialize();
        
        List<IterationResult> results = new List<IterationResult>(iterationNumber);
        
        int T = iterationNumber;
        
        double offlineError = 0;
        double trackingError = 0;

        for (int t = 1; t <= T; ++t)
        {
            
            // ADAPT NUMBER OF SWARMS
            var (Mfree, worstIndex) = NumberOfFreeSwarms();
            if (Mfree == 0)
            {
                Subswarm swarm = new Subswarm(SubswarmParticleCount, SubswarmQuantumParticleCount, _function);
                swarm.Initialize(t);
            
                Swarms.Add(swarm);
            }
            else if (Mfree > ExcessSwarmCount)
            {
                Swarms.RemoveAt(worstIndex);
            }
            int l = 0;
            foreach (var swarm in Swarms)
            {
                l++;
                var fitness = _function.CalculateDynamic(swarm.GlobalBest, t);

                // DETECT CHANGE
                if (Math.Abs(fitness - swarm.GlobalBestFitness) > 1e-10)
                {
                    swarm.ReevaluateParticleBests(t);
                    swarm.GlobalBestFitness = fitness;
                }

                // UPDATE PARTICLES
                swarm.UpdateParticles(t);
                
                // EXCLUSION
                foreach (var exclSwarm in Swarms)
                {
                    if(swarm == exclSwarm) continue;

                    if (Vector.EuclideanDistance(swarm.GlobalBest, exclSwarm.GlobalBest) < ExclusionRadius)
                    {
                        if (swarm.GlobalBestFitness <= exclSwarm.GlobalBestFitness)
                        {
                            swarm.Initialize(t);
                        }
                        else
                        {
                            exclSwarm.Initialize(t);
                        }
                    }
                }
            }
            
            double bestIterationFitness = Double.PositiveInfinity;
            Vector bestIterationSolution = new Vector(_function.D);
                
            foreach (var swarm in Swarms)
            {
                if (swarm.GlobalBestFitness < bestIterationFitness)
                {
                    bestIterationFitness = swarm.GlobalBestFitness;
                    bestIterationSolution = swarm.GlobalBest;
                }
            }

            double distanceToOptimum = CalculateDistanceToOptimum(t, bestIterationSolution);

            offlineError += bestIterationFitness;
            trackingError += distanceToOptimum;
            
            results.Add(new IterationResult(bestIterationFitness, Swarms.Count, distanceToOptimum, CalculateDiversity() ));
        }

        return (offlineError/T, trackingError/T, results);
    }
    
    private double CalculateDiversity()
    {
        Vector mean = new Vector(_function.D);
        double count = Swarms.Count * SubswarmParticleCount;

        foreach (var s in Swarms)
        {
            foreach (var p in s.Particles)
            {
                mean += p.Solution;
            }
        }

        mean /= count;

        double sum = 0;

        foreach (var s in Swarms)
        {
            foreach (var p in s.Particles)
            {
                sum += Vector.EuclideanDistance(p.Solution, mean);
            }
        }

        return sum / count;
    }

    private double CalculateDistanceToOptimum(int iteration, Vector solution)
    {
        double distance = 0;
        double shift = _function.Shift(iteration);
            
        for (int i = 0; i < _function.D; ++i)
        {
            double optimum = _function.Optimum.Get(i) + shift;
                
            distance += Math.Pow(solution.Get(i) - optimum, 2);
        }

        return Math.Sqrt(distance);
    }
    
    private (int, int) NumberOfFreeSwarms()
    {
        double worstFitness = 0;
        int worstIndex = -1;
        
        int num = 0;
        
        for(int i = 0;i<Swarms.Count;++i)
        {
            var swarm = Swarms[i];
            
            double swarmRadius = swarm.SwarmRadius();
            
            if (swarmRadius > ConvergenceRadius)
            {
                num++;
                if (swarm.GlobalBestFitness > worstFitness)
                {
                    worstFitness = swarm.GlobalBestFitness;
                    worstIndex = i;
                }
            }
        }
        return (num, worstIndex);
    }

    private double SearchSpaceDiagonal()
    {
        double sum = 0;
        for (int i = 0; i < _function.D; ++i)
        {
            sum += Math.Pow(_function.Bounds.Upper - _function.Bounds.Lower, 2);
        }

        return Math.Sqrt(sum);
    }
}