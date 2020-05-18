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
from t_test_clustered_data import get_sorted_clusters, get_vectors, get_clusters, CLUSTERED_FILENAME_POSFIX, get_repo_name
from plot_gain_scores import get_cluster_label
import random
from matplotlib.ticker import FormatStrFormatter


COLOR_PALETTE = ["#3498db", "#feb308", "#34495e", "#41aa33"]


def get_color(i):
    if i<len(COLOR_PALETTE):
        return COLOR_PALETTE[i]
    else:
        color = "ffffff"
        while color == "ffffff":
            color = ''.join([random.choice('0123456789ABCDEF') for i in range(6)])
        return "#" + color


def set_plot_style(nrows, ncols, fig_height=6, fig_width=7):
    sns.set()
    sns.set_context("paper")
    sns.set_style("darkgrid")
    fig, axes = plt.subplots(nrows=nrows, ncols=ncols, figsize=(fig_width, fig_height), dpi=600)
    plt.subplots_adjust(wspace=0.2, hspace=0.2)
    return fig, axes


def get_pubs_count(input_path):
    counts = {}
    repos = []
    cluster_count = 0
    for root, dirpath, filenames in os.walk(input_path):
        for filename in filenames:
            if os.path.splitext(filename)[1] == ".csv" and \
            os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                repos.append(get_repo_name(filename))
                clusters = get_clusters(os.path.join(root, filename))
                cluster_count = len(clusters.groups)
                for k in clusters.groups:
                    if k not in counts:
                        counts[k] = {}
                    counts[k][filename] = len(clusters.groups[k])

    return counts, repos, cluster_count


def run(input_path):
    pubs, repos, cluster_count = get_pubs_count(input_path)

    fig, ax = set_plot_style(1, 1)
    offset = 0.75 / cluster_count
    series = []
    
    i = 0
    for cluster in pubs:
        y = []
        for repo in pubs[cluster]:
            y.append(pubs[cluster][repo])
        series.append(ax.bar([j + (offset * i) for j in list(range(len(repos)))], y, offset, color=get_color(i)))
        i += 1
    
    ax.set_ylabel('Count')
    ax.set_yscale('log')
    ax.yaxis.set_major_formatter(FormatStrFormatter('%d'))

    x = list(range(len(repos)))
    ax.set_xticks([i + offset for i in x])
    ax.set_xticklabels(repos)

    # Show only horizontal grid lines.
    ax.grid(axis='x', which='major')
    ax.legend(series, (get_cluster_label(len(pubs.keys()), i) for i in pubs.keys()))

    for rect in series:
        autolabel(ax, rect)

    image_file = os.path.join(input_path, 'num_pubs_in_clusters.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()


def autolabel(ax, rects):
    # This method is based on the docs of pyplot.
    """Attach a text label above each bar in *rects*, displaying its height."""
    for rect in rects:
        height = rect.get_height()
        ax.annotate('{}'.format(height),
                    xy=(rect.get_x() + rect.get_width() / 2, height),
                    xytext=(0, 3),  # 3 points vertical offset
                    textcoords="offset points",
                    ha='center', va='bottom')


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Missing input path.")
        exit()

    run(sys.argv[1])
