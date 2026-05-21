using GWO.Functions;
using GWO.Util;

namespace GWO.Optimizer.Populations.GWO;

public class WolfPopulationDynamic
{
    public int Size;
    public List<Wolf> Wolves;

    private OptimizationFunction _function;

    public EliteWolves EliteWolves;

    private readonly Random _random = new Random();
    
    public WolfPopulationDynamic(int size, OptimizationFunction function)
    {
        Size = size;
        _function = function;
        Wolves = new List<Wolf>(Size);
        EliteWolves = new EliteWolves();
        
        for (int i = 0; i < Size; ++i)
        {
            Wolf wolf = new Wolf(_function.D);
            Wolves.Add(wolf);
        }
    }

    private void Initialize()
    {
        EliteWolves.Reset();
        foreach (var wolf in Wolves)
        {
            wolf.GenerateSolution(_function.Bounds);
            wolf.Fitness = _function.CalculateDynamic(wolf.Solution, 0);
            EliteWolves.Insert(wolf);
        }
    }

    public (double, double, List<IterationResult>) Process(int iterationNumber)
    {
        Initialize();

        List<IterationResult> results = new List<IterationResult>(iterationNumber);
        
        int T = iterationNumber;
        double a;

        double offlineError = 0;
        double trackingError = 0;

        for (int t = 1; t <= T; ++t)
        {
            a = aParameter(t, T);
            
            UpdateWolfPositions(a, t);
            UpdateEliteWolves();


            double distanceToOptimum = CalculateDistanceToOptimum(t);
            offlineError += EliteWolves.Alpha().Fitness;
            trackingError += distanceToOptimum;
            
            results.Add(new IterationResult(EliteWolves.Alpha().Fitness, a, distanceToOptimum, CalculateDiversity()));
        }

        return (offlineError/T, trackingError/T, results);
    }

    private void UpdateWolfPositions(double a, int iteration)
    {
        
        foreach (var wolf in Wolves)
        {
            Vector dAlpha =
                (RandomDouble.GetVector(_random, _function.D, new Bounds(0, 2)) * EliteWolves.Alpha().Solution -
                 wolf.Solution).Abs();
            Vector dBeta =
                (RandomDouble.GetVector(_random, _function.D, new Bounds(0, 2)) * EliteWolves.Beta().Solution -
                 wolf.Solution).Abs();
            Vector dDelta =
                (RandomDouble.GetVector(_random, _function.D, new Bounds(0, 2)) * EliteWolves.Delta().Solution -
                 wolf.Solution).Abs();

            Vector x1 = EliteWolves.Alpha().Solution -
                        A(a, RandomDouble.GetVector(_random, _function.D, new Bounds(0, 1))) * dAlpha;
            Vector x2 = EliteWolves.Beta().Solution -
                        A(a, RandomDouble.GetVector(_random, _function.D, new Bounds(0, 1))) * dBeta;
            Vector x3 = EliteWolves.Delta().Solution -
                        A(a, RandomDouble.GetVector(_random, _function.D, new Bounds(0, 1))) * dDelta;

            Vector x = (x1 + x2 + x3) / 3;

            wolf.Solution = x;
            wolf.Fitness = _function.CalculateDynamic(wolf.Solution, iteration);
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

            foreach (var wolf in Wolves)
            {
                sum += wolf.Solution.Get(i);
            }
            
            centroid.Set(i, sum/Size);
        }

        
        double diversity = 0;

        double distance;
        foreach (var wolf in Wolves)
        {
            distance = 0;

            for (int i = 0; i < dimension; i++)
            {
                distance += Math.Pow(
                    wolf.Solution.Get(i) - centroid.Get(i),
                    2);
            }

            diversity += Math.Sqrt(distance);
        }

        return diversity / Size;
    }

    private double CalculateDistanceToOptimum(int iteration)
    {
        var alpha = EliteWolves.Alpha();
        double distance = 0;
        double shift = _function.Shift(iteration);
            
        for (int i = 0; i < _function.D; ++i)
        {
            double optimum = _function.Optimum.Get(i) + shift;
                
            distance += Math.Pow(alpha.Solution.Get(i) - optimum, 2);
        }

        return Math.Sqrt(distance);
    }
    
    private void UpdateEliteWolves()
    {
        EliteWolves.Reset();
        foreach (var wolf in Wolves)
        {
            EliteWolves.Insert(wolf);
        }
    }

    private double aParameter(double t, double T)
    {
        return 2 - t * (2 / T);
    }

    private Vector A(double a, Vector r1)
    {
        return 2 * a * r1 - a;
    }
}