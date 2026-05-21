namespace GWO.Util;

public struct IterationResult
{
    public double Fitness;
    public double aParameter;
    public double DistanceToOptimum;
    public double Diversity;

    public IterationResult(double fitness = 0, double a = 0, double distance = 0, double diversity = 0)
    {
        Fitness = fitness;
        aParameter = a;
        DistanceToOptimum = distance;
        Diversity = diversity;
    }
}


public static class ResultExporter
{
    public static void SaveResultsToCsv(List<IterationResult> results, string path)
    {
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("Iteration,Fitness,AParameter,Distance,Diversity");

            for (int i = 0; i < results.Count; i++)
            {
                writer.WriteLine(
                    $"{i + 1}," +
                    $"{results[i].Fitness}," +
                    $"{results[i].aParameter}," +
                    $"{results[i].DistanceToOptimum}," +
                    $"{results[i].Diversity}"
                );
            }
        }
    }
}