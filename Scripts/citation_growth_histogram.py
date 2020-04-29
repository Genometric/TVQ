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


GROWTH_COLUMN_HEADER = "CitationGrowthOnInputData"


def plot(ax, growthes, labels):
    ax.hist(growthes, label=labels, density=True, bins=24) # setting density to False will show count, and True will show probability.
    ax.set_yscale('log')
    #ax.yaxis.set_major_formatter(mticker.ScalarFormatter()) # comment this when density=True
    ##ax.yaxis.set_minor_formatter(mticker.ScalarFormatter())


def set_plot_style(nrows, ncols, fig_height=6, fig_width=8):
    sns.set()
    sns.set_context("paper")
    sns.set_style("darkgrid")
    fig, axes = plt.subplots(nrows=nrows, ncols=ncols, figsize=(fig_width, fig_height), dpi=300)
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

    x_axis_label = "\n Citation Growth Percentage"
    y_axis_label = "Probability \n"

    fig, ax = set_plot_style(1, 1)
    row_counter = -1
    all_growthes = []
    labels = []
    for filename in files:
        print(f">>> Processing file: {filename}")
        row_counter += 1
        filename_without_extension = os.path.splitext(filename)[0]
        repository_name = filename_without_extension.replace(CLUSTERED_FILENAME_POSFIX, "")
        input_df = pd.read_csv(os.path.join(root, filename), header=0, sep='\t')
        all_growthes.append(get_growthes(input_df))
        labels.append(repository_name)
    
    plot(ax, all_growthes, labels)

    ax.set_xlabel(x_axis_label)
    ax.set_ylabel(y_axis_label)

    #handles, labels = ax.get_legend_handles_labels()
    plt.legend(loc="upper left")
    #fig.legend(handles, labels, loc='center')

    image_file = os.path.join(inputPath, 'percentage_of_growth.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()
