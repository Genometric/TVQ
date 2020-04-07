import numpy as np
import os
import sys
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import scipy.cluster.hierarchy as shc
from sklearn.cluster import AgglomerativeClustering
import seaborn as sns
import itertools
from scipy.spatial.distance import cdist 
import sklearn


CLUSTERED_FILENAME_POSFIX = "_clustered"
CLUSTER_NAME_COLUMN_LABEL = "cluster_label"
CLUSTERING_STATS_REPORT_FILENAME = "clustering_stats.txt"


def get_clusters(root, filename):
    """
    Returns a data frame grouped-by cluster name.
    
    :rtype:  pandas.core.groupby.generic.DataFrameGroupBy
    """
    input_df = pd.read_csv(os.path.join(root, filename), header=0, sep='\t')
    return input_df.groupby(CLUSTER_NAME_COLUMN_LABEL)


def get_quartiles(cluster):
    pass


def smooth(x, y):
    x = np.array(x)
    y = np.array(y)
    xnew = np.linspace(x.min(), x.max(), 50) 
    spl = make_interp_spline(x, y, k=2)
    y_smooth = spl(xnew)
    return xnew, y_smooth


def plot_smooth_line(ax, x, y, color, alpha=1, linestyle="-", label=None):
    s_x, s_y = smooth(x, y)
    ax.plot(s_x, s_y, c=color, alpha=alpha, linestyle=linestyle, label=label)


def plot_smooth_fill_between(ax, x, lower_y, upper_y, color):
    s_x, s_lower_y = smooth(x, lower_y)
    s_x, s_upper_y = smooth(x, upper_y)
    ax.fill_between(s_x, s_lower_y, s_upper_y, facecolor=color, alpha=0.3)


def plot(root, filename):
    before_x, before_median, before_lower_quartile, \
    before_upper_quartile, before_max, before_min, \
    after_x, after_median, after_lower_quartile, \
    after_upper_quartile, after_max, after_min = read_input(os.path.join(root, filename))

    fig, ax = plt.subplots()

    plot_smooth_line(ax, before_x, before_median, "red", label="Before adding to repository")
    plot_smooth_line(ax, before_x, before_max, color="red", linestyle='dashed')
    plot_smooth_line(ax, before_x, before_min, color="red", linestyle='dotted')
    plot_smooth_fill_between(ax, before_x, before_lower_quartile, before_upper_quartile, "red")

    plot_smooth_line(ax, after_x, after_median, "green", label="After adding to repository")
    plot_smooth_line(ax, after_x, after_max, color="green", linestyle='dashed')
    plot_smooth_line(ax, after_x, after_min, color="green", linestyle='dotted')
    plot_smooth_fill_between(ax, after_x, after_lower_quartile, after_upper_quartile, "green")

    start = -1
    end = 1.01
    stepsize = 0.4
    ax.xaxis.set_ticks(np.arange(start, end, stepsize))
    ax.set_xlabel("Date offset from adding to repository")

    ax.set_ylabel("Citations")

    ax.set_facecolor(BACKGROND_COLOR)
    ax.legend(loc="upper left")
    ax.grid(color=GRID_COLOR, linestyle='-', linewidth=1)
    ax.set_axisbelow(True)

    filename_without_extension = os.path.splitext(filename)[0]
    plt.title(filename_without_extension)

    image_file = os.path.join(root, filename_without_extension + '.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file)
    plt.close()


def set_plot_style():
    sns.set()
    sns.set_context("paper")
    sns.set_style("darkgrid")
    fig, axes = plt.subplots(nrows=4, ncols=2, figsize=(12, 16), dpi=300, gridspec_kw={'width_ratios': [2, 1]})  # , constrained_layout=True)
    plt.subplots_adjust(wspace=0.15, hspace=0.35)
    return fig, axes


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Missing input path.")
        exit()

    inputPath = sys.argv[1]

    set_plot_style()
    col_counter = 0
    for root, dirpath, filenames in os.walk(inputPath):
        for filename in filenames:
            if os.path.splitext(filename)[1] == ".csv" and \
               not os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                col_counter += 1
                filename_without_extension = os.path.splitext(filename)[0]
                clusters = get_clusters(root, filename)
                print(clusters)

                plot(ax[plot_row], filename_without_extension, True if col_counter == 4 else False, *cluster(root, filename, cluster_count))
                plot_row += 1