using GWO.Functions;
using GWO.Util;

namespace GWO.Optimizer;

public class WolfPopulationDynamicAParameter
{
    public int Size;
    public List<Wolf> Wolves;

    private OptimizationFunction _function;

    public EliteWolves EliteWolves;

    private double ConvergenceSpeed;

    public WolfPopulationDynamicAParameter(int size, OptimizationFunction function, int convergenceSpeed = 200)
    {
        Size = size;
        _function = function;
        Wolves = new List<Wolf>(Size);
        EliteWolves = new EliteWolves();
        ConvergenceSpeed = convergenceSpeed;
        
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

        double prevFitness = EliteWolves.Alpha().Fitness;
        
        List<IterationResult> results = new List<IterationResult>(iterationNumber);
        
        int T = iterationNumber;
        double a;

        int convPos = 1;
        for (int t = 1; t <= T; ++t)
        {
            a = aParameter(convPos);
            
            UpdateWolfPositions(a, t);
            UpdateEliteWolves();
            
            results.Add(new IterationResult(EliteWolves.Alpha().Fitness, a, 0));

            if (EliteWolves.Alpha().Fitness - prevFitness > 0)
            {
                convPos = AdjustConvergencePosition(EliteWolves.Alpha().Fitness - prevFitness, convPos);
            }

            prevFitness = EliteWolves.Alpha().Fitness;
            
            convPos++;
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

    private int AdjustConvergencePosition(double fitnessDelta, int convergencePosition)
    {
        double d = 10;
        int shift = (int) AdjustmentFormula(fitnessDelta);
        int newPosition = convergencePosition - shift;

        return newPosition < 0 ? 0 : newPosition;
    }

    private double AdjustmentFormula(double fitnessDelta)
    {
        double res = 0.7 * (fitnessDelta - 10);
        return res > 0 ? res : 0;
    }
    private double aParameter(double convergencePosition)
    {
        double a = 2 - convergencePosition * (2 / ConvergenceSpeed);
        return a >= 0 ? a : 0;
    }

    private Vector A(double a, Vector r1)
    {
        return 2 * a * r1 - a;
    }
}