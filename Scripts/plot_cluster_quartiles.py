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
from scipy.interpolate import make_interp_spline, BSpline


# THIS SCRIPT IS EXPERIMENTAL.
# MOST METHODS OVERLAP WITH SIMILAR METHODS FROM OTHER SCRIPTS.


CLUSTERED_FILENAME_POSFIX = "_clustered"
CLUSTER_NAME_COLUMN_LABEL = "cluster_label"
CLUSTERING_STATS_REPORT_FILENAME = "clustering_stats.txt"

# A list of quartiles to be computed for each column of the 
# citations matrix (i.e., for each entry of normalized date).
QUARTILES = [0.25, 0.5, 0.75]


def get_clusters(root, filename):
    """
    Returns a data frame grouped-by cluster name.
    
    :rtype:  pandas.core.groupby.generic.DataFrameGroupBy
    """
    input_df = pd.read_csv(os.path.join(root, filename), header=0, sep='\t')
    return input_df.groupby(CLUSTER_NAME_COLUMN_LABEL)


def get_quartiles(citations):
    # A data frame whose columns are set to the entries of normalized date,
    # and rows containing their quartiles. The quartile is given as the index 
    # of the row. 
    return citations.quantile(QUARTILES)


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


def get_cols(dataframe, row, cols):
    y = []
    for x in cols:
        y.append(dataframe.at[row, x])
    return y


def plot(ax, filename, add_legend, quartiles):
    _, pre_x, post_x = pre_post_columns(quartiles)

    idxes = quartiles.index.sort_values()
    median = np.median(idxes)
    for idx in idxes:
        linestyle = "dashed" if idx == median else "dotted"
        y = get_cols(quartiles, idx, pre_x)
        plot_smooth_line(ax, pre_x, y, "red", label="Before adding to repository", linestyle=linestyle)

        y = get_cols(quartiles, idx, post_x)
        plot_smooth_line(ax, post_x, y, "green", label="Before adding to repository", linestyle=linestyle)
        
    y_min = get_cols(quartiles, min(idxes), pre_x)
    y_max = get_cols(quartiles, max(idxes), pre_x)
    plot_smooth_fill_between(ax, pre_x, y_min, y_max, "red")

    y_min = get_cols(quartiles, min(idxes), post_x)
    y_max = get_cols(quartiles, max(idxes), post_x)
    plot_smooth_fill_between(ax, post_x, y_min, y_max, "green")


    #plot_smooth_line(ax, pre_x, before_median, "red", label="Before adding to repository")
    #plot_smooth_line(ax, pre_x, before_max, color="red", linestyle='dashed')
    #plot_smooth_line(ax, pre_x, before_min, color="red", linestyle='dotted')
    #plot_smooth_fill_between(ax, pre_x, before_lower_quartile, before_upper_quartile, "red")

    #plot_smooth_line(ax, post_x, after_median, "green", label="After adding to repository")
    #plot_smooth_line(ax, post_x, after_max, color="green", linestyle='dashed')
    #plot_smooth_line(ax, post_x, after_min, color="green", linestyle='dotted')
    #plot_smooth_fill_between(ax, post_x, after_lower_quartile, after_upper_quartile, "green")

    #start = -1
    #end = 1.01
    #stepsize = 0.4
    #ax.xaxis.set_ticks(np.arange(start, end, stepsize))
    #ax.set_xlabel("Date offset from adding to repository")

    #ax.set_ylabel("Citations")

    #ax.set_facecolor(BACKGROND_COLOR)
    #ax.legend(loc="upper left")
    #ax.grid(color=GRID_COLOR, linestyle='-', linewidth=1)
    #ax.set_axisbelow(True)

    #filename_without_extension = os.path.splitext(filename)[0]
    #plt.title(filename_without_extension)

    #image_file = os.path.join(root, filename_without_extension + '.png')
    #if os.path.isfile(image_file):
    #    os.remove(image_file)
    #plt.savefig(image_file)
    #plt.close()


def pre_post_columns(tools):
    """
    Returns all the column headers, and headers of columns containing 
    normalized citation counts belonging to when before and after a 
    tool was added to the repository.
    """
    column_headers = tools.columns.values.tolist()
    pre = []
    post = []
    for header in column_headers:
        try:
            v = float(header)
        except ValueError:
            continue

        if v == 0:
            pre.append(header)
            post.append(header)
        elif v < 0:
            pre.append(header)
        else:
            post.append(header)

    return column_headers, pre, post


def get_vectors(tools):
    # columns: a list of all the column headers.
    # pre:  a list of headers of columns containing normalized citation counts BEFORE a tool was added to the repository.
    # post: a list of headers of columns containing normalized citation counts AFTER  a tool was added to the repository.
    columns, pre_headers, post_headers = pre_post_columns(tools)

    # A list of two-dimensional lists, first dimension is pre counts
    # and second dimension contains post citation counts.
    pre_post_citations = []

    citations = pd.DataFrame(columns=tools.columns)

    sums = []

    # Lists contain citation counts before (pre) and after (post)
    # a tool was added to the repository.
    avg_pre = []
    avg_pst = []

    for index, row in tools.iterrows():
        citations = citations.append(row, ignore_index=True)

        pre_vals = row.get(pre_headers).values.tolist()
        post_vals = row.get(post_headers).values.tolist()

        pre_post_citations.append([pre_vals, post_vals])
        sums.append(np.sum(pre_vals + post_vals))
        avg_pre.append(np.average(pre_vals))
        avg_pst.append(np.average(post_vals))

    # Converts the numeric column headers that are 
    # represented as strings.
    indexes = []
    for col in citations.columns:
        try:
            indexes.append(float(col))
        except ValueError:
            indexes.append(col)
    citations.columns = indexes

    return citations, pre_post_citations, sums, avg_pre, avg_pst


def set_plot_style(nrows, ncols):
    sns.set()
    sns.set_context("paper")
    sns.set_style("darkgrid")
    fig, axes = plt.subplots(nrows=nrows, ncols=ncols, figsize=(12, 16), dpi=300)  #, gridspec_kw={'width_ratios': [2, 1]})  # , constrained_layout=True)
    plt.subplots_adjust(wspace=0.15, hspace=0.35)
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

    fig, ax = set_plot_style(len(files), 3)

    row_counter = -1
    for filename in files:
        row_counter += 1
        filename_without_extension = os.path.splitext(filename)[0]
        clusters = get_clusters(root, filename)

        col_counter = -1
        for k in clusters.groups:
            col_counter += 1
            citations, _, _, _, _ = get_vectors(clusters.get_group(k))
            quartiles = get_quartiles(citations)
            plot(ax[row_counter][col_counter], filename_without_extension, True if col_counter == 4 else False, quartiles)

    image_file = os.path.join(inputPath, 'plot.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()
