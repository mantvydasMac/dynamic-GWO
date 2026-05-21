import os
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

ROOT = "root"

ALGORITHMS = ["MPSO", "GWO", "DGWO"]
FUNCTIONS = ["sphere", "rastrigin", "rosenbrock", "griewank"]
SHIFTS = ["linear", "teleport", "periodic"]


def plot_error_metric(values, labels, title, ylabel, save_path):
    plt.figure(figsize=(8, 5))

    x = np.arange(len(labels))

    plt.bar(x, values)

    plt.xticks(x, labels)
    plt.ylabel(ylabel)
    plt.title(title)
    plt.grid(axis='y')

    plt.tight_layout()
    plt.savefig(save_path, dpi=300)
    plt.close()


for func in FUNCTIONS:
    for shift in SHIFTS:

        offline_means = []
        tracking_means = []

        labels = []

        for algo in ALGORITHMS:

            file_path = os.path.join(
                ROOT,
                algo,
                func,
                f"errors_{func}_{shift}.csv"
            )

            if not os.path.exists(file_path):
                continue

            df = pd.read_csv(file_path)

            offline_mean = df["OfflineError"].mean()
            tracking_mean = df["TrackingError"].mean()

            offline_means.append(offline_mean)
            tracking_means.append(tracking_mean)

            labels.append(algo)

        if len(labels) == 0:
            continue

        save_dir = os.path.join(ROOT, "plots", "errors", func)
        os.makedirs(save_dir, exist_ok=True)

        # Offline error plot
        plot_error_metric(
            offline_means,
            labels,
            title=f"{func} - {shift} - Offline Error",
            ylabel="Offline Error",
            save_path=os.path.join(
                save_dir,
                f"offline_error_{shift}.png"
            )
        )

        # Tracking error plot
        plot_error_metric(
            tracking_means,
            labels,
            title=f"{func} - {shift} - Tracking Error",
            ylabel="Tracking Error",
            save_path=os.path.join(
                save_dir,
                f"tracking_error_{shift}.png"
            )
        )

print("Error plotting complete.")