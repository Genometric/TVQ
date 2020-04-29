import numpy as np
import os
import sys
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import scipy.cluster.hierarchy as shc
import scipy.stats as st
from sklearn.cluster import AgglomerativeClustering
import seaborn as sns
import itertools
from scipy.spatial.distance import cdist 
import sklearn
from scipy.interpolate import make_interp_spline, BSpline
from t_test_clustered_data import get_sorted_clusters, get_vectors, get_clusters, CLUSTERED_FILENAME_POSFIX
from plot_gain_scores import get_growthes
import matplotlib.ticker as mticker
from matplotlib.ticker import FormatStrFormatter, PercentFormatter


GROWTH_COLUMN_HEADER = "CitationGrowthOnInputData"

COLOR_PALETTES = {"Bioconda": "#3498db", "Bioconductor": "#feb308", "BioTools": "#34495e", "ToolShed": "#41aa33"}


def aggregate(input, min, max):
    # TODO: there must be some method (e.g., in numpy) to do this aggregation. 
    aggregated = []
    for item in input:
        if item < min:
            aggregated.append(min)
        elif item > max:
            aggregated.append(max)
        else:
            aggregated.append(item)
    return aggregated


def plot(ax, growthes, labels, colors):
    counts, bins, patches = ax.hist(growthes, label=labels, density=True, bins=12, rwidth=0.65, color = colors, align="left", histtype="bar") # setting density to False will show count, and True will show probability.
    ax.set_yscale('log')
    ax.set_xticks(bins)
    ax.xaxis.set_major_formatter(PercentFormatter())
    ax.set_xlim([-575, 975])
    #ax.yaxis.set_major_formatter(mticker.ScalarFormatter()) # comment this when density=True
    ##ax.yaxis.set_minor_formatter(mticker.ScalarFormatter())


def set_plot_style(nrows, ncols, fig_height=4, fig_width=8):
    sns.set()
    sns.set_context("paper")
    sns.set_style("darkgrid")
    flatui = ["#9b59b6", "#3498db", "#e74c3c", "#34495e", "#2ecc71"]
    sns.palplot(sns.color_palette(flatui))
    fig, axes = plt.subplots(nrows=nrows, ncols=ncols, figsize=(fig_width, fig_height), dpi=600)
    plt.subplots_adjust(wspace=0.2, hspace=0.2)
    return fig, axes


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Missing input path.")
        exit()

    inputPath = sys.argv[1]

    files = []
    for root, dirpath, filenames in os.walk(inputPath):
        for filename in filenames:
            if os.path.splitext(filename)[1] == ".csv" and \
            os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                files.append(filename)

    x_axis_label = "\n Citation growth percentage"
    y_axis_label = "Probability \n"

    fig, ax = set_plot_style(1, 1)
    row_counter = -1
    growthes_dict = {}
    all_growthes = []
    labels = []
    colors = []
    for filename in files:
        print(f">>> Processing file: {filename}")
        row_counter += 1
        filename_without_extension = os.path.splitext(filename)[0]
        repository_name = filename_without_extension.replace(CLUSTERED_FILENAME_POSFIX, "")
        input_df = pd.read_csv(os.path.join(root, filename), header=0, sep='\t')
        all_growthes.append(aggregate(get_growthes(input_df), -500, 1000))
        labels.append(repository_name)
        colors.append(COLOR_PALETTES[repository_name])
    
    plot(ax, all_growthes, labels, colors)

    ax.set_xlabel(x_axis_label)
    ax.set_ylabel(y_axis_label)

    plt.legend(loc="upper right", ncol=2)

    image_file = os.path.join(inputPath, 'percentage_of_growth.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()