using GWO.Functions;
using GWO.Util;

namespace GWO.Optimizer;

public class WolfPopulationDynamicClampedA
{
    public int Size;
    public List<Wolf> Wolves;

    private OptimizationFunction _function;

    public EliteWolves EliteWolves;

    private double aMax;
    private double aMin;

    public WolfPopulationDynamicClampedA(int size, OptimizationFunction function, double aMin = 0, double aMax = 2)
    {
        Size = size;
        _function = function;
        Wolves = new List<Wolf>(Size);
        EliteWolves = new EliteWolves();

        this.aMin = aMin;
        this.aMax = aMax;
        
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

    public List<IterationResult> Process(int iterationNumber)
    {
        Initialize();

        List<IterationResult> results = new List<IterationResult>(iterationNumber);
        
        int T = iterationNumber;
        double a;

        for (int t = 1; t <= T; ++t)
        {
            a = aParameter(t, T);
            
            UpdateWolfPositions(a, t);
            UpdateEliteWolves();
            
            results.Add(new IterationResult(EliteWolves.Alpha().Fitness, a, 0));
        }

        return results;
    }

    private void UpdateWolfPositions(double a, int iteration)
    {
        Random random = new Random();
        
        foreach (var wolf in Wolves)
        {
            Vector dAlpha =
                (RandomDouble.GetVector(random, _function.D, new Bounds(0, 2)) * EliteWolves.Alpha().Solution -
                 wolf.Solution).Abs();
            Vector dBeta =
                (RandomDouble.GetVector(random, _function.D, new Bounds(0, 2)) * EliteWolves.Beta().Solution -
                 wolf.Solution).Abs();
            Vector dDelta =
                (RandomDouble.GetVector(random, _function.D, new Bounds(0, 2)) * EliteWolves.Delta().Solution -
                 wolf.Solution).Abs();

            Vector x1 = EliteWolves.Alpha().Solution -
                        A(a, RandomDouble.GetVector(random, _function.D, new Bounds(0, 1))) * dAlpha;
            Vector x2 = EliteWolves.Beta().Solution -
                        A(a, RandomDouble.GetVector(random, _function.D, new Bounds(0, 1))) * dBeta;
            Vector x3 = EliteWolves.Delta().Solution -
                        A(a, RandomDouble.GetVector(random, _function.D, new Bounds(0, 1))) * dDelta;

            Vector x = (x1 + x2 + x3) / 3;

            wolf.Solution = x;
            wolf.Fitness = _function.CalculateDynamic(wolf.Solution, iteration);
        }
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
        return aMin + (aMax - aMin) * (1 - t / T);
    }

    private Vector A(double a, Vector r1)
    {
        return 2 * a * r1 - a;
    }
}