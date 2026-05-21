import os
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

ROOT = "root"

ALGORITHMS = ["MPSO", "GWO", "DGWO"]
FUNCTIONS = ["sphere", "rastrigin", "rosenbrock", "griewank"]
SHIFTS = ["linear", "teleport", "periodic"]

METRICS = ["fitness", "distance", "diversity"]


def load_curve(path):
    df = pd.read_csv(path)
    return df.iloc[:, 0].values


def make_dual_plot(curves, labels, title, ylabel, save_path):
    fig, axes = plt.subplots(1, 2, figsize=(12, 4))

    # -------------------
    # FULL VIEW (log scale for fitness only)
    # -------------------
    for curve, label in zip(curves, labels):
        axes[0].plot(curve, label=label)

    axes[0].set_title("Full view")
    axes[0].set_xlabel("Iteration")
    axes[0].set_ylabel(ylabel)
    axes[0].grid(True)

    axes[0].legend()

    # -------------------
    # ZOOMED VIEW (convergence region)
    # -------------------
    for curve, label in zip(curves, labels):
        axes[1].plot(curve, label=label)

    axes[1].set_title("Zoomed convergence")
    axes[1].set_xlabel("Iteration")
    axes[1].set_ylabel(ylabel)
    axes[1].grid(True)

    # super zoom
    axes[1].set_ylim(0, 20)

    plt.suptitle(title)
    plt.tight_layout()

    plt.savefig(save_path, dpi=300)
    plt.close()


# -------------------
# MAIN LOOP
# -------------------

for func in FUNCTIONS:
    for shift in SHIFTS:

        for metric in METRICS:

            curves = []
            labels = []

            for algo in ALGORITHMS:
                file_path = os.path.join(
                    ROOT,
                    algo,
                    func,
                    "analysis",
                    f"mean_{metric}_{shift}.csv"
                )

                if not os.path.exists(file_path):
                    continue

                curves.append(load_curve(file_path))
                labels.append(algo)

            if len(curves) == 0:
                continue

            save_dir = os.path.join(ROOT, "plots", func)
            os.makedirs(save_dir, exist_ok=True)

            save_path = os.path.join(
                save_dir,
                f"{metric}_{shift}_dual.png"
            )

            make_dual_plot(
                curves,
                labels,
                title=f"{func} - {shift} - {metric}",
                ylabel=metric.capitalize(),
                save_path=save_path
            )

print("Dual-view plotting complete.")