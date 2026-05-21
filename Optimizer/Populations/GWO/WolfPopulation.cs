using GWO.Functions;
using GWO.Util;

namespace GWO.Optimizer;

public class WolfPopulation
{
    public int Size;
    public List<Wolf> Wolves;

    private OptimizationFunction _function;

    public EliteWolves EliteWolves;

    public WolfPopulation(int size, OptimizationFunction function)
    {
        Size = size;
        _function = function;
        Wolves = new List<Wolf>(Size);
        EliteWolves = new EliteWolves();
    }

    public void Initialize()
    {
        for (int i = 0; i < Size; ++i)
        {
            Wolf wolf = new Wolf(_function.D);
            wolf.GenerateSolution(_function.Bounds);
            wolf.Fitness = _function.CalculateDynamic(wolf.Solution, 0);
            Wolves.Add(wolf);
            EliteWolves.Insert(wolf);
        }
    }

    public double Process(int iterationNumber)
    {
        int T = iterationNumber;
        double a;

        for (int t = 1; t <= T; ++t)
        {
            a = aParameter(t, T);
            
            UpdateWolfPositions(a);
            UpdateEliteWolves();
            
            Console.WriteLine($"t = {t} | Fitness: {EliteWolves.Alpha().Fitness}");
        }

        return 0;
    }

    public void UpdateWolfPositions(double a)
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
            wolf.Fitness = _function.CalculateDynamic(wolf.Solution, 0);
        }
    }

    public void UpdateEliteWolves()
    {
        EliteWolves.Reset();
        foreach (var wolf in Wolves)
        {
            EliteWolves.Insert(wolf);
        }
    }

    public double aParameter(double t, double T)
    {
        return 2 - t * (2 / T);
    }

    public Vector A(double a, Vector r1)
    {
        return 2 * a * r1 - a;
    }
}