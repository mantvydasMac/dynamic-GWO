import os
import numpy as np
import pandas as pd

ROOT = "root"

ALGORITHMS = ["MPSO", "GWO", "DGWO"]
FUNCTIONS = ["sphere", "rastrigin", "rosenbrock", "griewank"]
SHIFTS = ["linear", "teleport", "periodic"]

def load_run(file_path):
    return pd.read_csv(file_path)

def pad_and_stack(list_of_arrays):
    max_len = max(len(a) for a in list_of_arrays)

    padded = []
    for a in list_of_arrays:
        if len(a) < max_len:
            pad = np.full(max_len - len(a), np.nan)
            a = np.concatenate([a, pad])
        padded.append(a)

    return np.vstack(padded)

def mean_curve(matrix):
    return np.nanmean(matrix, axis=0)

for algo in ALGORITHMS:
    for func in FUNCTIONS:
        folder = os.path.join(ROOT, algo, func)

        for shift in SHIFTS:

            fitness_runs = []
            distance_runs = []
            diversity_runs = []

            for run in range(1, 21):
                file_path = os.path.join(folder, f"results_{func}_{shift}_{run}.csv")

                if not os.path.exists(file_path):
                    continue

                df = load_run(file_path)

                fitness_runs.append(df["Fitness"].values)
                distance_runs.append(df["Distance"].values)
                diversity_runs.append(df["Diversity"].values)

            if len(fitness_runs) == 0:
                continue

            fitness_mat = pad_and_stack(fitness_runs)
            distance_mat = pad_and_stack(distance_runs)
            diversity_mat = pad_and_stack(diversity_runs)

            mean_fitness = mean_curve(fitness_mat)
            mean_distance = mean_curve(distance_mat)
            mean_diversity = mean_curve(diversity_mat)

            out_dir = os.path.join(folder, "analysis")
            os.makedirs(out_dir, exist_ok=True)

            pd.DataFrame({"MeanFitness": mean_fitness}).to_csv(
                os.path.join(out_dir, f"mean_fitness_{shift}.csv"),
                index=False
            )

            pd.DataFrame({"MeanDistance": mean_distance}).to_csv(
                os.path.join(out_dir, f"mean_distance_{shift}.csv"),
                index=False
            )

            pd.DataFrame({"MeanDiversity": mean_diversity}).to_csv(
                os.path.join(out_dir, f"mean_diversity_{shift}.csv"),
                index=False
            )

print("Done.")