using GWO.Functions;
using GWO.Util;

namespace GWO.Optimizer.Populations.GWO;

public class Subpopulation
{
    public int Size;
    public List<Wolf> Wolves;

    private OptimizationFunction _function;

    public EliteWolves EliteWolves;

    public int APosition;
    public int ASpeed;
    
    public int ArchiveSize;
    public List<Wolf> RepresentativeArchive;

    private double SigmaInital = 0.7;
    private double SigmaFinal = 0;
    private double AInitial = 1.6;
    private double AFinal = 0.1;

    private readonly Random _random = new();
    
    public Subpopulation(int size, int archiveSize, OptimizationFunction function, int aSpeed = 100)
    {
        Size = size;
        ArchiveSize = archiveSize;
        _function = function;
        Wolves = new List<Wolf>();
        RepresentativeArchive = new List<Wolf>(ArchiveSize);
        EliteWolves = new EliteWolves();
        ASpeed = aSpeed;
        
        for (int i = 0; i < Size; ++i)
        {
            Wolf wolf = new Wolf(_function.D);
            Wolves.Add(wolf);
        }
    }
    
    public void Initialize(int iteration)
    {
        EliteWolves.Reset();
        RepresentativeArchive.Clear();
        
        foreach (var wolf in Wolves)
        {
            wolf.GenerateSolution(_function.Bounds);
            wolf.Fitness = _function.CalculateDynamic(wolf.Solution, iteration);
            EliteWolves.Insert(wolf);
        }

        AddElitesToArchive();
        APosition = 1;
    }
    
    public void UpdateWolfPositions(int iteration)
    {
        // DETECT CHANGE
        double checkFitness = _function.CalculateDynamic(RepresentativeArchive[0].Solution, iteration);
            
        if (Math.Abs(checkFitness - RepresentativeArchive[0].Fitness) > 1e-10)
        {
            ReevaluateArchive(iteration);
        }
        
        double a = aParameter(APosition, ASpeed);
        double sigma = sigmaParameter(APosition, ASpeed);
        
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

            Vector xRh = xBest + CauchyNoise(_function.D) * (wolf.Solution - xArchive) +
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
        
        UpdateEliteWolves();
        AddElitesToArchive();
        
        APosition++;
    }
    
    public double PopulationRadius()
    {
        double maxDist = 0;

        foreach (var wolf in Wolves)
        {
            double dist = Vector.EuclideanDistance(
                wolf.Solution,
                EliteWolves.Alpha().Solution);

            if (dist > maxDist)
                maxDist = dist;
        }

        return maxDist;
    }
    
    private void UpdateEliteWolves()
    {
        EliteWolves.Reset();
        foreach (var wolf in Wolves)
        {
            EliteWolves.Insert(wolf);
        }
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
    
    private double sigmaParameter(double t, double T)
    {
        return t > T ? SigmaFinal : ((T - t) / T) * (SigmaInital - SigmaFinal) + SigmaFinal;
    }

    private Vector CauchyNoise(int dimension)
    {
        Vector noise = new Vector(dimension);

        for (int i = 0; i < dimension; i++)
        {
            double r = _random.NextDouble();
            double cauchy = Math.Tan(Math.PI * (r - 0.5));

            noise.Set(i,  0.1 * cauchy);
        }

        return noise;
    }
    
    private double aParameter(double t, double T)
    {
        return t > T ? AFinal : AFinal + (AInitial - AFinal) * (1 - t / T);
    }
    
    private Vector A(double a, Vector r1)
    {
        return 2 * a * r1 - a;
    }
}