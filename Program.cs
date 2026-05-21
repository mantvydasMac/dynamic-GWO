using GWO.Experiments.GWO;


string projectPath = Directory
    .GetParent(AppDomain.CurrentDomain.BaseDirectory)
    .Parent
    .Parent
    .Parent
    .FullName;

string savePath = Path.Combine(projectPath, "Python/");



MPSO_Griewank.Run(savePath);



