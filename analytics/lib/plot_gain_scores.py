"""
TODO: Add doc string.
"""

import os
import sys
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import matplotlib
import seaborn as sns
from t_test_clustered_data import get_sorted_clusters, get_clusters, CLUSTERED_FILENAME_POSFIX
from matplotlib.ticker import FormatStrFormatter, ScalarFormatter


GROWTH_COLUMN_HEADER = "GainScore"


def get_cluster_label(cluster_count, index):
    if cluster_count == 3:
        return ["Low-cited Publications", "Mid-cited Publications", "Highly-cited Publications"][index]
    elif cluster_count == 2:
        return ["Low-cited Publications", "Highly-cited Publications"][index]
    else:
        return f"Cluster {index+1}"


def get_growthes(tools, growth_column_header=GROWTH_COLUMN_HEADER):
    growthes = []
    for index, row in tools.iterrows():
        growthes.append(row.get(growth_column_header))
    return growthes


def plot(ax, filename, growthes, header=None, x_axis_label=None, y_axis_label=None, plot_density=False):
    histogram = ax.hist(growthes, density=plot_density, bins=24) # setting density to False will show count, and True will show probability.

    # Histogram is a two dimensional array; first dimension 
    # contains the counts/probabilities (y axis), and the 
    # second dimension contains the binned growths (x axis).
    values = histogram[0]

    # This is a very hacky solution! 
    # The objective is to determine if an array should be 
    # plotted in log scale or not, based on the values. 
    # For instance, if array contains only few values 
    # all ~=3, then if plotted in log scale, no ticks 
    # maybe generated for the y-axis. One way is to 
    # control minor h-grid lines, and one way is to 
    # ignore log scale all together. Here we cho0se 
    # the latter.
    values_with_no_zero = values.copy()
    values_with_no_zero = values_with_no_zero[values_with_no_zero != 0]
    contains_few_values = len(set(values_with_no_zero)) < 3

    if contains_few_values:
        ax.yaxis.set_major_formatter(ScalarFormatter())
    elif (contains_few_values or plot_density and max(values) > 0.1) or (not plot_density and max(values) < 10):
        ax.yaxis.set_major_formatter(ScalarFormatter())
        ax.yaxis.set_minor_formatter(ScalarFormatter())
    else:
        ax.set_yscale('log')
        if not plot_density:
            ax.yaxis.set_major_formatter(FormatStrFormatter('%d'))
    
    if header:
        ax.set_title(header)

    if x_axis_label:
        ax.set_xlabel(x_axis_label)

    if y_axis_label:
        ax.set_ylabel(y_axis_label)

def plot2(ax, growthes, header=None, x_axis_label=None, y_axis_label=None, plot_density=False, hist=True):
    kde = False if hist else True
    data = sns.distplot(growthes, bins=25, hist=hist, kde=kde, rug=True, ax=ax, kde_kws={"shade": True})

    if hist:
        values = [h.get_height() for h in data.patches]
        values = np.array(values)
    else:
        data = data[0].get_data()
        xvalues = data[0]
        yvalues = data[1]
        values = yvalues

    # This is a very hacky solution! 
    # The objective is to determine if an array should be 
    # plotted in log scale or not, based on the values. 
    # For instance, if array contains only few values 
    # all ~=3, then if plotted in log scale, no ticks 
    # maybe generated for the y-axis. One way is to 
    # control minor h-grid lines, and one way is to 
    # ignore log scale all together. Here we cho0se 
    # the latter.
    values_with_no_zero = values.copy()
    values_with_no_zero = values_with_no_zero[values_with_no_zero != 0]
    contains_few_values = len(set(values_with_no_zero)) < 3

    if contains_few_values:
        ax.yaxis.set_major_formatter(ScalarFormatter())
    elif (contains_few_values or plot_density and max(values) > 0.1) or (not plot_density and max(values) < 10):
        ax.yaxis.set_major_formatter(ScalarFormatter())
        ax.yaxis.set_minor_formatter(ScalarFormatter())
    else:
        ax.set_yscale('log')
        if not plot_density:
            ax.yaxis.set_major_formatter(FormatStrFormatter('%d'))

    if header:
        ax.set_title(header)

    if x_axis_label:
        ax.set_xlabel(x_axis_label)

    if y_axis_label:
        ax.set_ylabel(y_axis_label)



def set_plot_style(nrows, ncols, fig_height=12, fig_width=12):
    sns.set()
    sns.set_context("paper")
    sns.set_style("darkgrid")
    fig, axes = plt.subplots(nrows=nrows, ncols=ncols, figsize=(fig_width, fig_height), dpi=300)
    plt.subplots_adjust(wspace=0.2, hspace=0.2)
    return fig, axes


def run(input_path, plot_density):
    files = []
    for root, dirpath, filenames in os.walk(input_path):
        for filename in filenames:
            if os.path.splitext(filename)[1] == ".csv" and \
            os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                files.append(filename)

    x_axis_label = "\n Citation Growth"
    y_axis_label = ("Probability\n" if plot_density else "Count\n")

    clusters = get_clusters(os.path.join(root, files[0]))
    cluster_count = len(clusters.groups)

    fig, ax = set_plot_style(len(files), cluster_count)
    row_counter = -1
    for filename in files:
        print(f">>> Processing file: {filename}")
        row_counter += 1
        filename_without_extension = os.path.splitext(filename)[0]
        repository_name = filename_without_extension.replace(CLUSTERED_FILENAME_POSFIX, "")
        clusters = get_clusters(os.path.join(root, filename))

        col_counter = -1
        keys, mappings = get_sorted_clusters(clusters)
        for i in range(0, len(keys)):
            print(f"\t- Processing cluster {i}")
            header = get_cluster_label(cluster_count, i)
            col_counter += 1
            growthes = get_growthes(clusters.get_group(mappings[keys[i]]))
            plot(
                ax[row_counter] if cluster_count == 1 else ax[row_counter][col_counter],
                filename_without_extension,
                growthes,
                header=header if row_counter == 0 else None,
                x_axis_label=x_axis_label if row_counter == len(keys) else None,
                y_axis_label=f"{repository_name} \n \n {y_axis_label}" if col_counter == 0 else None,
                plot_density=plot_density)
    
    last_ax = ax[row_counter] if cluster_count == 1 else ax[row_counter][col_counter]
    handles, labels = last_ax.get_legend_handles_labels()

    image_file = os.path.join(input_path, 'gain_scores_clustered.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()

    fig, ax = set_plot_style(1, len(files), fig_height=3, fig_width=16)
    col_counter = -1
    for filename in files:
        print(f">>> Processing file: {filename}")
        col_counter += 1
        filename_without_extension = os.path.splitext(filename)[0]
        repository_name = filename_without_extension.replace(CLUSTERED_FILENAME_POSFIX, "")

        tools = pd.read_csv(os.path.join(root, filename), header=0, sep='\t')
        growthes = get_growthes(tools)
        plot(
            ax[col_counter],
            filename_without_extension,
            growthes,
            header=repository_name,
            x_axis_label=x_axis_label,
            y_axis_label=f"\n {y_axis_label}" if col_counter == 0 else None,
            plot_density=plot_density)
        
    handles, labels = ax[col_counter].get_legend_handles_labels()

    image_file = os.path.join(input_path, 'gain_scores.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()

    fig, ax = set_plot_style(1, len(files), fig_height=3, fig_width=16)
    col_counter = -1
    for filename in files:
        col_counter += 1
        filename_without_extension = os.path.splitext(filename)[0]
        repository_name = filename_without_extension.replace(CLUSTERED_FILENAME_POSFIX, "")

        tools = pd.read_csv(os.path.join(root, filename), header=0, sep='\t')
        growthes = get_growthes(tools)
        plot2(
            ax[col_counter],
            growthes,
            header=repository_name,
            x_axis_label=x_axis_label,
            y_axis_label=f"\n {y_axis_label}" if col_counter == 0 else None,
            plot_density=plot_density)

    handles, labels = ax[col_counter].get_legend_handles_labels()

    image_file = os.path.join(input_path, 'gain_scores_sns.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()
