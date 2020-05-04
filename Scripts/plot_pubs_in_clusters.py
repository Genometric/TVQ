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
import matplotlib.ticker as mticker
from t_test_clustered_data import get_sorted_clusters, get_vectors, get_clusters, CLUSTERED_FILENAME_POSFIX


def set_plot_style(nrows, ncols, fig_height=12, fig_width=12):
    sns.set()
    sns.set_context("paper")
    sns.set_style("darkgrid")
    fig, axes = plt.subplots(nrows=nrows, ncols=ncols, figsize=(fig_width, fig_height), dpi=300)
    plt.subplots_adjust(wspace=0.2, hspace=0.2)
    return fig, axes


def run(input_path):
    counts = {}
    for root, dirpath, filenames in os.walk(input_path):
        for filename in filenames:
            if os.path.splitext(filename)[1] == ".csv" and \
            os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                clusters = get_clusters(root, filename)
                for k in clusters.groups:
                    if k not in counts:
                        counts[k] = {}
                    counts[k][filename] = len(clusters.groups[k])

    fig, ax = set_plot_style(1,1)
    width = 0.27
    series = []
    x = list(range(len(counts) + 1))
    i = 0
    for cluster in counts:
        y = []
        for repo in counts[cluster]:
            y.append(counts[cluster][repo])
        series.append(ax.bar([j + (width * i) for j in x], y, width, color='r'))
        i += 1
    
    image_file = os.path.join(input_path, 'counts.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Missing input path.")
        exit()

    run(sys.argv[1])