import os
import sys
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
from scipy.interpolate import make_interp_spline, BSpline
from t_test_clustered_data import get_sorted_clusters


# THIS SCRIPT IS EXPERIMENTAL.
# MOST METHODS OVERLAP WITH SIMILAR METHODS FROM OTHER SCRIPTS.


CLUSTERED_FILENAME_POSFIX = "_clustered"
CLUSTER_NAME_COLUMN_LABEL = "cluster_label"
CLUSTERING_STATS_REPORT_FILENAME = "clustering_stats.txt"

PRE = "pre"
POST = "post"

# A list of quartiles to be computed for each column of the 
# citations matrix (i.e., for each entry of normalized date).
QUARTILES = {0.25: {PRE: {"label": "25th percentile (pre)", "linestyle": "dashed", "color": "red"}, POST: {"label": "25th percentile (post)", "linestyle": "dashed", "color": "green"}},
             0.50: {PRE: {"label": "Median (pre)",          "linestyle": "solid",  "color": "red"}, POST: {"label": "Median (post)",          "linestyle": "solid",  "color": "green"}},
             0.75: {PRE: {"label": "75th percentile (pre)", "linestyle": "dotted", "color": "red"}, POST: {"label": "75th percentile (post)", "linestyle": "dotted", "color": "green"}}}

FILL_BETWEEN_PRE_COLOR = "red"
FILL_BETWEEN_POST_COLOR = "green"



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
    # A note on syntax: [*dictionary] unpacks the keys of the dictionary as a list.
    return citations.quantile([*QUARTILES])


def get_changes(citations):
    sum = 0.0
    changes = {}
    previous_col_mean = 0;

    _, pre_headers, post_headers = pre_post_columns(citations)

    for header in pre_headers[:-1] + post_headers:
        current_col_mean = citations[header].mean()
        change = current_col_mean - previous_col_mean
        sum += change
        changes[header] = change
        previous_col_mean = current_col_mean

    for k in changes:
        changes[k] = changes[k] / sum

    pre_max = post_max = 0
    for key, value in changes.items():
        if key < 0:
            pre_max = max(pre_max, value)
        else:
            post_max = max(post_max, value)

    return changes, pre_max, post_max


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


def plot(ax, filename, add_legend, quartiles, changes, header=None,
         x_axis_label=None, y_axis_label=None, secondary_y_axis_label=None):
    _, pre_x, post_x = pre_post_columns(quartiles)

    seco_axis_handles = []
    seco_axis_labels = []
    if changes:
        changes_x = pre_x[:-1] + post_x
        changes_y = list(changes.values())
        smooth_x, smooth_y = smooth(changes_x, changes_y)
        zeros_y = [0] * len(smooth_y)
        fill_color = ([230/255, 230/255, 239/255])
        series_color = ([140/255, 140/255, 140/255])
        secondary_ax = ax.twinx()
        secondary_ax.plot(smooth_x, smooth_y, label="Citation Count Change", color=series_color, alpha=0.4)
        secondary_ax.fill_between(smooth_x, zeros_y, smooth_y, facecolor=fill_color, alpha=0.5)
        secondary_ax.yaxis.label.set_color(series_color)
        secondary_ax.tick_params(axis='y', colors=series_color)
        seco_axis_handles, seco_axis_labels = secondary_ax.get_legend_handles_labels()
        if secondary_y_axis_label:
            secondary_ax.set_ylabel(secondary_y_axis_label)

    idxes = quartiles.index.sort_values()
    for idx in idxes:
        y = get_cols(quartiles, idx, pre_x)
        kwargs = QUARTILES[idx][PRE]
        plot_smooth_line(ax, pre_x, y, **kwargs)

        y = get_cols(quartiles, idx, post_x)
        kwargs = QUARTILES[idx][POST]
        plot_smooth_line(ax, post_x, y, **kwargs)
        
    y_min = get_cols(quartiles, min(idxes), pre_x)
    y_max = get_cols(quartiles, max(idxes), pre_x)
    plot_smooth_fill_between(ax, pre_x, y_min, y_max, FILL_BETWEEN_PRE_COLOR)

    y_min = get_cols(quartiles, min(idxes), post_x)
    y_max = get_cols(quartiles, max(idxes), post_x)
    plot_smooth_fill_between(ax, post_x, y_min, y_max, FILL_BETWEEN_POST_COLOR)

    if header:
        ax.set_title(header)

    start = -1
    end = 1.01
    stepsize = 0.4
    ax.xaxis.set_ticks(np.arange(start, end, stepsize))
    if x_axis_label:
        ax.set_xlabel(x_axis_label)

    if y_axis_label:
        ax.set_ylabel(y_axis_label)

    prim_axis_handles, prim_axis_labels = ax.get_legend_handles_labels()

    return prim_axis_handles + seco_axis_handles, prim_axis_labels + seco_axis_labels


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


def set_plot_style(nrows, ncols, wspace=0.25):
    sns.set()
    sns.set_context("paper")
    sns.set_style("darkgrid")
    fig, axes = plt.subplots(nrows=nrows, ncols=ncols, figsize=(3 * nrows, 4 * ncols), dpi=300, sharex=True)
    plt.subplots_adjust(wspace=wspace, hspace=0.07)
    return fig, axes


def run(input_path, plot_changes):
    files = []
    for root, dirpath, filenames in os.walk(input_path):
        for filename in filenames:
            if os.path.splitext(filename)[1] == ".csv" and \
            os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                files.append(filename)

    clusters = get_clusters(root, files[0])
    cluster_count = len(clusters.groups)

    fig, ax = set_plot_style(len(files), cluster_count, 0.25 if not plot_changes else 0.4)

    x_axis_label = "\n Date offset"
    y_axis_label = "Citations \n"
    row_counter = -1
    for filename in files:
        print(f">>> Processing file: {filename}")
        row_counter += 1
        filename_without_extension = os.path.splitext(filename)[0]
        repository_name = filename_without_extension.replace(CLUSTERED_FILENAME_POSFIX, "")
        clusters = get_clusters(root, filename)

        col_counter = -1
        keys, mappings = get_sorted_clusters(clusters)
        for i in range(0, len(keys)):
            print(f"\t- Processing cluster {i}")
            header = f"Cluster {i+1}"
            col_counter += 1
            citations, _, _, _, _ = get_vectors(clusters.get_group(mappings[keys[i]]))
            changes = None
            if plot_changes:
                changes, pre_max, post_max = get_changes(citations)
                print(f"\t\t* Pre-Max:\t{pre_max}")
                print(f"\t\t* Post-Max:\t{post_max}")
            quartiles = get_quartiles(citations)
            handles, labels = plot(
                ax[row_counter] if cluster_count == 1 else ax[row_counter][col_counter],
                filename_without_extension,
                True if col_counter == 4 else False,
                quartiles,
                changes,
                header=header if row_counter == 0 else None,
                x_axis_label=x_axis_label if row_counter == len(keys) else None,
                y_axis_label=f"{repository_name} \n \n {y_axis_label}" if col_counter == 0 else None,
                secondary_y_axis_label="\nDensity of Changes" if col_counter==len(keys)-1 else None)

    # The "magical" numbers of bbox_to_anchor are determined by trial-and-error.
    if plot_changes:
        fig.legend(handles, labels, loc='center', bbox_to_anchor=(0.480, 0.03), ncol=7, framealpha=0.0)
    else:
        fig.legend(handles, labels, loc='center', bbox_to_anchor=(0.454, 0.03), ncol=6, framealpha=0.0)

    image_file = os.path.join(input_path, 'clustered_citation_change.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Missing input path.")
        exit()

    plot_changes = False
    if len(sys.argv) == 3:
        plot_changes = sys.argv[2]

    run(sys.argv[1], plot_changes)
