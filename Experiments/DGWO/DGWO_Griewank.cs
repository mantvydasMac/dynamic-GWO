using GWO.Functions;
using GWO.Optimizer.Populations.GWO;
using GWO.Util;

namespace GWO.Experiments;

public class DGWO_Griewank
{
    public static void Run(string savePath)
    {
        int dimension = 10;
        int iterationNumber = 800;
        int populationSize = 40;

        string folderPath = "DGWO/";
        
        double amplitude = 120;
        double min = -600;
        double max = 600;

        void SaveErrorStats(string path, List<(int run, double offlineError, double trackingError)> stats)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("Run,OfflineError,TrackingError");

                foreach (var entry in stats)
                {
                    writer.WriteLine($"{entry.run},{entry.offlineError},{entry.trackingError}");
                }
            }
        }
        
        // SHIFT FUNCTIONS

        double ShiftPeriodic(int iteration)
        {
            double t = (double)iteration / iterationNumber;

            double cycles = 5.0; // number of full oscillations per run

            return amplitude * Math.Sin(2.0 * Math.PI * cycles * t);
        }

        double ShiftLinear(int iteration)
        {
            double t = (double)iteration / iterationNumber; // [0,1]
            return amplitude * (2.0 * t - 1.0); // [-amplitude, +amplitude]
        }

        double ShiftTeleport(int iteration)
        {
            double progress = (double)iteration / iterationNumber;

            double[] centers =
            {
                0.25, 0.75, 0.35, 0.60
            };

            double selected;

            if (progress < 0.25)
                selected = centers[0];
            else if (progress < 0.50)
                selected = centers[1];
            else if (progress < 0.75)
                selected = centers[2];
            else
                selected = centers[3];

            return min + selected * (max - min);
        }

        int runCount = 20;
        var (offlineError, trackingError, res) =
            (0.0, 0.0, new List<IterationResult>());

        
        var function = new Griewank(dimension, ShiftLinear);
        
        var population = new WolfPopulationDynamicMultiPopulation(function, 30, convergenceRadiusParameter: 0.04, startingSubpopulationCount: 1, excessSubpopulationCount: 1);
        
        List<(int run, double offlineError, double trackingError)> errorStats 
            = new List<(int, double, double)>();
       
        //SHIFT LINEAR ==================================================
        
        Console.WriteLine("== STARTED LINEAR");
        for (int i = 0; i < runCount; ++i)
        {
            (offlineError, trackingError, res) = population.Process(iterationNumber);
            ResultExporter.SaveResultsToCsv(res, savePath + folderPath + $"results_griewank_linear_{i+1}.csv");
            errorStats.Add((i + 1, offlineError, trackingError));
        }
        Console.WriteLine("++ FINISHED LINEAR");
        
        SaveErrorStats(savePath + folderPath + "errors_griewank_linear.csv", errorStats);
        
        //SHIFT TELEPORT ==================================================
        
        function.Shift = ShiftTeleport;
        errorStats.Clear();
        
        Console.WriteLine("== STARTED TELEPORT");
        for (int i = 0; i < runCount; ++i)
        {
            (offlineError, trackingError, res) = population.Process(iterationNumber);
            ResultExporter.SaveResultsToCsv(res, savePath + folderPath + $"results_griewank_teleport_{i+1}.csv");
            errorStats.Add((i + 1, offlineError, trackingError));
        }
        Console.WriteLine("++ FINISHED TELEPORT");

        SaveErrorStats(savePath + folderPath + "errors_griewank_teleport.csv", errorStats);
        
        //SHIFT PERIODIC ==================================================

        function.Shift = ShiftPeriodic;
        errorStats.Clear();
        
        Console.WriteLine("== STARTED PERIODIC");
        for (int i = 0; i < runCount; ++i)
        {
            (offlineError, trackingError, res) = population.Process(iterationNumber);
            ResultExporter.SaveResultsToCsv(res, savePath + folderPath + $"results_griewank_periodic_{i+1}.csv");
            errorStats.Add((i + 1, offlineError, trackingError));
        }
        Console.WriteLine("++ FINISHED PERIODIC");
        
        SaveErrorStats(savePath + folderPath + "errors_griewank_periodic.csv", errorStats);

    }
    
}