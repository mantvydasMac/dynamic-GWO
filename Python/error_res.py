import os
import pandas as pd
import numpy as np

ROOT = "root"

ALGORITHMS = ["MPSO", "GWO", "DGWO"]
FUNCTIONS = ["sphere", "rastrigin", "rosenbrock", "griewank"]
SHIFTS = ["linear", "teleport", "periodic"]

results = []

for algo in ALGORITHMS:
    for func in FUNCTIONS:
        folder = os.path.join(ROOT, algo, func)

        for shift in SHIFTS:
            file_path = os.path.join(folder, f"errors_{func}_{shift}.csv")

            if not os.path.exists(file_path):
                continue

            df = pd.read_csv(file_path)

            offline = df["OfflineError"].values
            tracking = df["TrackingError"].values

            results.append({
                "Algorithm": algo,
                "Function": func,
                "Shift": shift,

                "OfflineMean": np.mean(offline),
                "OfflineStd": np.std(offline),
                "OfflineMedian": np.median(offline),

                "TrackingMean": np.mean(tracking),
                "TrackingStd": np.std(tracking),
                "TrackingMedian": np.median(tracking),
            })

# convert to dataframe
summary_df = pd.DataFrame(results)

# save
output_path = os.path.join(ROOT, "error_summary.csv")
summary_df.to_csv(output_path, index=False)

print("Done. Saved to:", output_path)