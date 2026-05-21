using GWO.Functions;
using GWO.Util;

namespace GWO.Optimizer.Populations.GWO;

public class WolfPopulationDynamicFinal
{
    public int Size;
    public List<Wolf> Wolves;

    private OptimizationFunction _function;

    public EliteWolves EliteWolves;

    public int ArchiveSize;
    public List<Wolf> RepresentativeArchive;

    private double JumpRate;
    private double aMax;
    private double aMin;

    private double SigmaInital = 1.0;
    private double SigmaFinal = 0;

    private readonly Random _random = new Random();
    
    public WolfPopulationDynamicFinal(int size, int archiveSize, OptimizationFunction function, 
        double jumpRate = 0.1,
        double aMin = 0, double aMax = 2)
    {
        Size = size;
        ArchiveSize = archiveSize;
        _function = function;
        JumpRate = jumpRate;
        this.aMin = aMin;
        this.aMax = aMax;
        
        Wolves = new List<Wolf>();
        RepresentativeArchive = new List<Wolf>(ArchiveSize);
        EliteWolves = new EliteWolves();
    }

    public void Initialize(int iteration)
    {
        EliteWolves.Reset();
        for(int i = 0;i<Size;++i)
        {
            Wolf wolf = new Wolf(_function.D);
            
            wolf.GenerateSolution(_function.Bounds);
            wolf.Fitness = _function.CalculateDynamic(wolf.Solution, iteration);
            
            Wolf oppositeWolf = new Wolf(_function.D);

            for (int j = 0; j < _function.D; ++j)
            {
                oppositeWolf.Solution.Set(j, _function.Bounds.Upper + _function.Bounds.Lower - wolf.Solution.Get(j));
            }
            
            oppositeWolf.Fitness = _function.CalculateDynamic(oppositeWolf.Solution, iteration);
            
            Wolves.Add(wolf);
            Wolves.Add(oppositeWolf);
        }
        
        Wolves.Sort((a, b) => a.Fitness.CompareTo(b.Fitness));
        Wolves.RemoveRange(Size, Size);
        
        EliteWolves.Insert(Wolves[0]);
        EliteWolves.Insert(Wolves[1]);
        EliteWolves.Insert(Wolves[2]);
        
        AddElitesToArchive();
    }

    public List<IterationResult> Process(int iterationNumber)
    {
        Initialize(0);

        List<IterationResult> results = new List<IterationResult>(iterationNumber);
        
        int T = iterationNumber;
        double a;
        double sigma;

        for (int t = 1; t <= T; ++t)
        {
            
            // DETECT CHANGE
            double checkFitness = _function.CalculateDynamic(RepresentativeArchive[0].Solution, t);
            
            if (Math.Abs(checkFitness - RepresentativeArchive[0].Fitness) > 1e-10)
            {
                ReevaluateArchive(t);
            }
            
            a = aParameter(t, T);
            sigma = sigmaParameter(t, T);
            
            UpdateWolfPositions(a, t, sigma);

            if (_random.NextDouble() < JumpRate)
            {
                for(int i = 0;i<Size;++i)
                {
                    Wolf oppositeWolf = new Wolf(_function.D);

                    for (int j = 0; j < _function.D; ++j)
                    {
                        oppositeWolf.Solution.Set(j, _function.Bounds.Upper + _function.Bounds.Lower - Wolves[i].Solution.Get(j));
                    }
            
                    oppositeWolf.Fitness = _function.CalculateDynamic(oppositeWolf.Solution, t);
                    
                    Wolves.Add(oppositeWolf);
                }
                
                Wolves.Sort((w1, w2) => w1.Fitness.CompareTo(w2.Fitness));
                Wolves.RemoveRange(Size, Size);
        
                EliteWolves.Reset();
                EliteWolves.Insert(Wolves[0]);
                EliteWolves.Insert(Wolves[1]);
                EliteWolves.Insert(Wolves[2]);
            }
            else
            {
                UpdateEliteWolves();
            }
            AddElitesToArchive();
            
            
            results.Add(new IterationResult(EliteWolves.Alpha().Fitness, a, CalculateDistanceToOptimum(t), CalculateDiversity()));
        }

        return results;
    }

    private void UpdateWolfPositions(double a, int iteration, double sigma)
    {
        foreach (var wolf in Wolves)
        {
            // REGULAR PROCESS
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
            
            
            // REPRESENTATIVE HUNTING
            RepresentativeArchive.Sort((w1, w2) => w1.Fitness.CompareTo(w2.Fitness));

            Vector xArchive = RepresentativeArchive[_random.Next(0, RepresentativeArchive.Count)].Solution;

            Vector xBest;
            if (RepresentativeArchive.Count < 5)
            {
                xBest = RepresentativeArchive[_random.Next(0, RepresentativeArchive.Count)].Solution;
            }
            else
            {
                xBest = RepresentativeArchive[_random.Next(0, 5)].Solution;
            }

            Vector xRandom1 = Wolves[_random.Next(0, Size)].Solution;
            Vector xRandom2 = Wolves[_random.Next(0, Size)].Solution;

            Vector xRh = xBest + CauchyDistribution(wolf.Solution, _random.NextDouble()) * (wolf.Solution - xArchive) +
                         sigma * (xRandom1 - xRandom2);


            double xFitness = _function.CalculateDynamic(x, iteration);
            double xRhFitness =  _function.CalculateDynamic(xRh, iteration);

            if (xFitness < xRhFitness)
            {
                wolf.Solution = x;
                wolf.Fitness = xFitness;
            }
            else
            {
                wolf.Solution = xRh;
                wolf.Fitness = xRhFitness;
            }
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

    private void AddElitesToArchive()
    {
        AddToArchive(EliteWolves.Alpha());
        AddToArchive(EliteWolves.Beta());
        AddToArchive(EliteWolves.Delta());
    }

    private void AddToArchive(Wolf wolf)
    {
        if (RepresentativeArchive.Count >= ArchiveSize)
        {
            RepresentativeArchive.RemoveAt(_random.Next(0, ArchiveSize));
        }
        
        Wolf newWolf = new Wolf(wolf.Dimension);
        newWolf.Solution = wolf.Solution.Clone();
        newWolf.Fitness = wolf.Fitness;
            
        RepresentativeArchive.Add(newWolf);
    }

    private void ReevaluateArchive(int iteration)
    {
        foreach (var wolf in RepresentativeArchive)
        {
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

    private double sigmaParameter(double t, double T)
    {
        return ((T - t) / T) * (SigmaInital - SigmaFinal) + SigmaFinal;
    }

    private Vector A(double a, Vector r1)
    {
        return 2 * a * r1 - a;
    }

    private Vector CauchyDistribution(Vector z, double r)
    {
        return z + 0.1 * Math.Tan(Math.PI * (r - 0.5));
    }
}