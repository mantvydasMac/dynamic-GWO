using GWO.Functions;
using GWO.Util;

namespace GWO.Optimizer.Populations.GWO;

public class WolfPopulationDynamicMultiPopulation
{
    public int SubpopulationSize;
    public int SubpopulationArchiveSize;
    
    public int StartingSubpopulationCount;
    public int ExcessSubpopulationCount;

    public double ConvergenceRadius;

    private OptimizationFunction _function;

    public List<Subpopulation> Subpopulations = new();

    private readonly Random _random = new();
    
    public WolfPopulationDynamicMultiPopulation(OptimizationFunction function,
        int subpopulationSize = 30,
        int subpopulationArchiveSize = 8,
        int startingSubpopulationCount = 1,
        int excessSubpopulationCount = 1, 
        double convergenceRadiusParameter = 0.05)
    {
        
        SubpopulationSize = subpopulationSize;
        SubpopulationArchiveSize = subpopulationArchiveSize;
        _function = function;
        StartingSubpopulationCount = startingSubpopulationCount;
        ExcessSubpopulationCount = excessSubpopulationCount;
        ConvergenceRadius = SearchSpaceDiagonal() * convergenceRadiusParameter;

    }

    private void Initialize(int iteration)
    {
        Subpopulations.Clear();
        for (int i = 0; i < StartingSubpopulationCount; ++i)
        {
            Subpopulation p = new Subpopulation(SubpopulationSize, SubpopulationArchiveSize, _function);
            p.Initialize(iteration);
            
            Subpopulations.Add(p);
        }
    }

    public (double, double, List<IterationResult>) Process(int iterationNumber)
    {
        Initialize(0);

        List<IterationResult> results = new List<IterationResult>(iterationNumber);

        double offlineError = 0;
        double trackingError = 0;
        
        int T = iterationNumber;

        for (int t = 1; t <= T; ++t)
        {
            
            var (Mfree, worstIndex) = NumberOfFreeSubpopulations();
            if (Mfree == 0)
            {
                Subpopulation p = new Subpopulation(SubpopulationSize, SubpopulationArchiveSize, _function);
                p.Initialize(t);
            
                Subpopulations.Add(p);
            }
            else if (Mfree > ExcessSubpopulationCount)
            {
                Subpopulations.RemoveAt(worstIndex);
            }
            
            
            foreach (var p in Subpopulations)
            {
                p.UpdateWolfPositions(t);
            }

            Wolf bestIterationWolf = GetBestWolf();
            double distanceToOptimum = CalculateDistanceToOptimum(bestIterationWolf.Solution, t);

            offlineError += bestIterationWolf.Fitness;
            trackingError += distanceToOptimum;

            results.Add(new IterationResult(bestIterationWolf.Fitness, Subpopulations.Count, distanceToOptimum, CalculateDiversity()));

        }

        return (offlineError/T, trackingError/T, results);
    }

    public Wolf GetBestWolf()
    {
        Wolf best = Wolf.DefaultWolf();

        foreach (var p in Subpopulations)
        {
            if(p.EliteWolves.Alpha().Fitness < best.Fitness)
            {
                best = p.EliteWolves.Alpha();
            }
        }

        return best;
    }
    
    private (int, int) NumberOfFreeSubpopulations()
    {
        double worstFitness = 0;
        int worstIndex = -1;
        
        int num = 0;
        
        for(int i = 0;i<Subpopulations.Count;++i)
        {
            
            var p = Subpopulations[i];
            double radius = p.PopulationRadius();
            
            if (radius > ConvergenceRadius)
            {
                num++;
                if (p.EliteWolves.Alpha().Fitness > worstFitness)
                {
                    worstFitness = p.EliteWolves.Alpha().Fitness;
                    worstIndex = i;
                }
            }
        }
        return (num, worstIndex);
    }

    private double CalculateDiversity()
    {
        Vector mean = new Vector(_function.D);
        double count = Subpopulations.Count * SubpopulationSize;

        foreach (var p in Subpopulations)
        {
            foreach (var w in p.Wolves)
            {
                mean += w.Solution;
            }
        }

        mean /= count;

        double sum = 0;

        foreach (var p in Subpopulations)
        {
            foreach (var w in p.Wolves)
            {
                sum += Vector.EuclideanDistance(w.Solution, mean);
            }
        }

        return sum / count;
    }
    
    private double CalculateDistanceToOptimum(Vector solution, int iteration)
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