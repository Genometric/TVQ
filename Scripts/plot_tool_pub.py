import numpy as np
from numpy import std
import os
import sys
import pandas as pd
from scipy.stats import ttest_rel, ttest_ind, pearsonr, ttest_1samp
from statistics import mean
from math import sqrt
from t_test_clustered_data import get_sorted_clusters, get_vectors, get_clusters, CLUSTERED_FILENAME_POSFIX, get_repo_name
from t_test_clustered_data import get_clusters
import seaborn as sns
import matplotlib.pyplot as plt
import matplotlib
from plot_pubs_in_clusters import get_color
import numpy as np
import matplotlib.pyplot as plt
from numpy.random import *
from plot_gain_scores import get_cluster_label


PUBLICATION_ID_COLUMN = "PublicationID"
TOOLS_COLUMN = "Tools"
TOOLS_SEPARATOR = ";"

# This list is defined so to minimize using very similar markers as much as possible.
MARKERS = ["o", "^", "x", "v", "1", "2", "3", "4", ">", "<", "*", "P", "+", "D", "X", "d"]


def get_marker(i):
    if i<len(MARKERS):
        return MARKERS[i]
    else:
        # TODO: there should be a better alternative.
        return "."


def get_clustered_repositories(input_path):
    filenames = []
    repositories = []
    for root, dirpath, files in os.walk(input_path):
        for filename in files:
            if os.path.splitext(filename)[1] == ".csv" and \
            os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                filenames.append(os.path.join(root, filename))
                repositories.append(get_repo_name(filename))

    return filenames, repositories


def get_pub_tool_count(filename):
    """
    Returns the number of unique tools and publications in each
    cluster of the given repository filename. 
    """
    clusters = get_clusters(filename)

    pubs = {}
    tools = {}

    for k in clusters.groups:
        if k not in pubs:
            pubs[k] = {}
            tools[k] = {}
        for index, row in clusters.get_group(k).iterrows():
            pub_id = row.get(PUBLICATION_ID_COLUMN)
            if pub_id not in pubs[k]:
                pubs[k][pub_id] = 0

            tool_names = (row.get(TOOLS_COLUMN)).split(TOOLS_SEPARATOR)
            for name in tool_names:
                if name not in tools[k]:
                    tools[k][name] = 0

    cluster_pubs_count = {}
    for k in pubs:
        cluster_pubs_count[k] = len(pubs[k])
    
    cluster_tools_count = {}
    for k in tools:
        cluster_tools_count[k] = len(tools[k])

    return sum(cluster_pubs_count.values()), cluster_pubs_count, sum(cluster_tools_count.values()), cluster_tools_count


def plot_clustered(input_path, filenames, repositories):
    fig, ax = set_plot_style(1, 1)
    i = 0
    max_x = 0
    max_y = 0
    repo_scatter = {}
    cluster_scatter = {}
    add_cluster_scatter = True
    for filename in filenames:
        add_repo_scatter = True
        c_pubs, ck_pubs, c_tools, ck_tools = get_pub_tool_count(filename)
        cluster_count = len(ck_pubs.keys())
        j = 0
        for k in ck_pubs:
            max_x = max(max_x, ck_pubs[k])
            max_y = max(max_y, ck_tools[k])
            scatter = ax.scatter(ck_pubs[k], ck_tools[k], marker=get_marker(j), color=get_color(i), alpha=0.5, s=80)

            if add_repo_scatter:
                repo_scatter[get_repo_name(filename)] = scatter
                add_repo_scatter = False

            if add_cluster_scatter:
                cluster_scatter[get_cluster_label(cluster_count, k)] = scatter
            j += 1

        add_cluster_scatter = False
        i += 1

    # The default range of plt when `s` is set in the `scatter` 
    # method does not keep all the points in the canvas; so their 
    # values are overridden.
    ax.set_ylim(bottom=0.5, top=max_y + (max_y * 0.5))
    ax.set_xlim(left=0.5, right=max_x + (max_x * 0.5))

    ax.set_yscale('log')
    ax.set_xscale('log')
    ax.yaxis.set_major_formatter(matplotlib.ticker.FormatStrFormatter('%d'))
    ax.xaxis.set_major_formatter(matplotlib.ticker.FormatStrFormatter('%d'))

    ax.set_xlabel("\nPublications Count")
    ax.set_ylabel("Tools Count\n")

    # It is required to add legend through `add_artist` for it not be overridden by the second legend.
    l1 = ax.legend(repo_scatter.values(), repo_scatter.keys(), scatterpoints=1, loc='lower right', ncol=2, title="Repositories")
    ax.add_artist(l1)
    l2 = ax.legend(cluster_scatter.values(), cluster_scatter.keys(), scatterpoints=1, loc='upper left', ncol=2, title="Clusters")

    image_file = os.path.join(input_path, 'plot_pub_tool_clustered.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()


def plot(input_path, filenames, repositories):
    fig, ax = set_plot_style(1, 1)
    i = 0
    max_x = 0
    max_y = 0
    repo_scatter = {}
    cluster_scatter = {}
    add_cluster_scatter = True
    xs = []
    ys = []
    for filename in filenames:
        add_repo_scatter = True
        c_pubs, _, c_tools, _ = get_pub_tool_count(filename)
        max_x = max(max_x, c_pubs)
        max_y = max(max_y, c_tools)
        xs.append(c_pubs)
        ys.append(c_tools)
        scatter = ax.scatter(c_pubs, c_tools, color=get_color(i), alpha=0.5, s=80)
        repo_scatter[get_repo_name(filename)] = scatter
        i += 1

    #for x,y in zip(xs,ys):
    #    plt.annotate(f"({x}, {y})",  # Label 
    #                 (x,y),
    #                 textcoords="offset points", # how to position the text
    #                 xytext=(0,10), # distance from text to points (x,y)
    #                 ha='center') # horizontal alignment can be left, right or center

    # The default range of plt when `s` is set in the `scatter` 
    # method does not keep all the points in the canvas; so their 
    # values are overridden.
    ax.set_ylim(bottom=128, top=max_y + (max_y * 0.5))
    ax.set_xlim(left=128, right=max_x + (max_x * 0.5))

    ax.set_xscale('log', basex=2)
    ax.set_yscale('log', basey=2)
    ax.yaxis.set_major_formatter(matplotlib.ticker.FormatStrFormatter('%d'))
    ax.xaxis.set_major_formatter(matplotlib.ticker.FormatStrFormatter('%d'))

    ax.set_xlabel("\nPublications Count")
    ax.set_ylabel("Tools Count\n")

    # It is required to add legend through `add_artist` for it not be overridden by the second legend.
    ax.legend(repo_scatter.values(), repo_scatter.keys(), scatterpoints=1, loc='upper left', ncol=2)
    #ax.add_artist(l1)
    #l2 = ax.legend(cluster_scatter.values(), cluster_scatter.keys(), scatterpoints=1, loc='upper left', ncol=2, title="Clusters")

    image_file = os.path.join(input_path, 'plot_pub_tool.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()


def set_plot_style(nrows, ncols, fig_height=5, fig_width=6):
    sns.set()
    sns.set_context("paper")
    sns.set_style("darkgrid")
    fig, axes = plt.subplots(nrows=nrows, ncols=ncols, figsize=(fig_width, fig_height), dpi=600)
    plt.subplots_adjust(wspace=0.2, hspace=0.2)
    return fig, axes


def run(input_path):
    filenames, repositories = get_clustered_repositories(input_path)
    plot(input_path, filenames, repositories)
    plot_clustered(input_path, filenames, repositories)


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Missing input path.")
        exit()

    run(sys.argv[1])
