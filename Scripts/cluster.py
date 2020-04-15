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
from matplotlib.lines import Line2D
from t_test_clustered_data import get_sorted_clusters


CLUSTERED_FILENAME_POSFIX = "_clustered"
CLUSTER_NAME_COLUMN_LABEL = "cluster_label"
CLUSTERING_STATS_REPORT_FILENAME = "clustering_stats.txt"


def get_silhouette_score(df, cluster_count):
    # Apply cluster to data.
    # It is not ideal to re-cluster data; hence, a potential improvement would be to
    # rework this and avoid send clustering.
    model = AgglomerativeClustering(n_clusters=cluster_count, affinity='euclidean', linkage='ward')  
    cluster_labels = model.fit_predict(df)

    return cluster_labels, sklearn.metrics.silhouette_score(df, cluster_labels)


def cluster(root, filename, cluster_count):
    repo_name = os.path.splitext(filename)[0]
    print(">>> Clustering repository: {0}".format(repo_name))
    input_df = pd.read_csv(os.path.join(root, filename), header=0, sep='\t')
    
    # Remove the tool name column b/c it's a categorical column.
    df = input_df.drop("ToolName", 1)

    # Remove the ID column b/c it contains a unique record for every row.
    df = df.drop("ID", 1)

    # Remove the GainScore column because its values should not be considered for clustering.
    df = df.drop("GainScore", 1)

    # Perform hierarchical/agglomerative clustering and 
    # returns the hierarchical clustering encoded as a linkage matrix.
    # The `ward` linkage minimizes the variance of the clusters being merged.
    linkage_matrix = shc.linkage(df, method='ward')

    variance, dist_growth_acceleration, auto_cluster_count, auto_cut_distance, manual_cluster_count, manual_cut_distance = get_cluster_count(linkage_matrix, repo_name, cluster_count)

    _, auto_silhouette_score = get_silhouette_score(df, auto_cluster_count)
    cluster_labels, manual_silhouette_score = get_silhouette_score(df, manual_cluster_count)

    # Add cluster information to original data.
    input_df[CLUSTER_NAME_COLUMN_LABEL] = cluster_labels

    # Sort cluster labels based on the mean value of tools in each cluster.
    # For instance, a group of tools might be clustered as cluster `0` and 
    # another group as cluster `1`. If the mean of the second group is less
    # than the mean of the first group, then the following code will update 
    # cluster labels of the tools so that all clustered as cluster `0` are 
    # clustered as cluster `1`, and those clustered as `1` are clustered as 
    # cluster `0`.
    mappings = {}
    sorted_keys, mean_cluster_num_mappings = get_sorted_clusters(input_df.groupby(CLUSTER_NAME_COLUMN_LABEL))
    for i in range(0, len(sorted_keys)):
        mappings[mean_cluster_num_mappings[sorted_keys[i]]] = i
    input_df[CLUSTER_NAME_COLUMN_LABEL] = input_df[CLUSTER_NAME_COLUMN_LABEL].map(mappings)
    
    # Write the DataFrame to CSV. 
    clustered_filename = os.path.join(root, repo_name + CLUSTERED_FILENAME_POSFIX + '.csv')
    if os.path.isfile(clustered_filename):
        os.remove(clustered_filename)
    input_df.to_csv(clustered_filename, sep='\t', encoding='utf-8', index=False)

    with open(os.path.join(root, CLUSTERING_STATS_REPORT_FILENAME), "a") as f:
        f.write(f"{repo_name}\t{auto_cluster_count}\t{auto_cut_distance}\t{auto_silhouette_score}\t{manual_cluster_count}\t{manual_cut_distance}\t{manual_silhouette_score}\n")

    return linkage_matrix, auto_cut_distance, auto_cluster_count, auto_silhouette_score, manual_cut_distance, manual_cluster_count, manual_silhouette_score, variance, dist_growth_acceleration


def get_cluster_count(Z, filename, cluster_count):
    # This method is implemented based on info available from the following link.
    # https://joernhees.de/blog/2015/08/26/scipy-hierarchical-clustering-and-dendrogram-tutorial/#Elbow-Method
    last = Z[-10:, 2]
    last_rev = last[::-1]
    idxs = np.arange(1, len(last) + 1)
    variance = pd.DataFrame(last_rev, idxs)

    # 2nd derivative of the distances
    acceleration = np.diff(last, 2)  
    acceleration_rev = acceleration[::-1]
    dist_growth_acceleration = pd.DataFrame(acceleration_rev, idxs[:-2] + 1)

    auto_index = int(acceleration_rev[1:].argmax()) + 3
    manual_index = auto_index if cluster_count is None else cluster_count
    return variance, dist_growth_acceleration, auto_index, float(last_rev[auto_index - 1]), manual_index, float(last_rev[manual_index - 1])


def set_plot_style():
    sns.set()
    sns.set_context("paper")
    sns.set_style("darkgrid")
    fig, axes = plt.subplots(nrows=4, ncols=2, figsize=(12, 16), dpi=300, gridspec_kw={'width_ratios': [2, 1]})  # , constrained_layout=True)
    plt.subplots_adjust(wspace=0.15, hspace=0.35)
    return fig, axes


def plot(ax, filename_without_extension, add_legend, linkage_matrix, auto_cut_distance, auto_cluster_count, auto_silhouette_score, manual_cut_distance, manual_cluster_count, manual_silhouette_score, variance, dist_growth_acceleration):
    col0 = ax[0]
    col1 = ax[1]

    auto_cut_color = "orange"
    manu_cut_color = "orange"

    auto_cut_line_style = "dotted"
    manu_cut_line_style = "dashed"

    linewidth = 1.5

    # Plots the hierarchical clustering as a dendrogram.
    dend = shc.dendrogram(linkage_matrix, no_labels=True, orientation="right", ax=col0)
    col0.axvline(x=auto_cut_distance, color=auto_cut_color, linewidth=linewidth, linestyle=auto_cut_line_style)
    col0.axvline(x=manual_cut_distance, color=manu_cut_color, linewidth=linewidth, linestyle=manu_cut_line_style)

    # Plot to a PNG file.
    col0.set_title(filename_without_extension)
    col0.set_xlabel("Height")
    col0.grid(axis='x', which='major', color='w')

    col0.text(0.82, 0.1, "Silhouette Score={:.4f}".format(manual_silhouette_score), horizontalalignment='center', verticalalignment='center', transform=col0.transAxes)

    # Plot the Elbow method's results.
    col1.plot(variance, label="Variance", marker='o', color='green')
    col1.plot(dist_growth_acceleration, label="Distance growth acceleration", marker="x", color="blue")

    col1.set_title(filename_without_extension)
    col1.set_xlabel("Number of clusters")
    col1.set_ylabel("Distortion")

    if add_legend:
        col1.legend(loc='center', bbox_to_anchor=(0.5, -0.3), framealpha=0.0, fancybox=True)

        lines = [Line2D([0], [0], color=auto_cut_color, linewidth=linewidth, linestyle=auto_cut_line_style), Line2D([0], [0], color=manu_cut_color, linewidth=linewidth, linestyle=manu_cut_line_style)]
        labels = ['Auto-determined cut height', 'Manually-set cut height']
        col0.legend(lines, labels, loc='center', bbox_to_anchor=(0.5, -0.3), framealpha=0.0, fancybox=True)

    col1.axvline(x=auto_cluster_count, color=auto_cut_color, linewidth=1.5, linestyle=auto_cut_line_style)
    col1.axvline(x=manual_cluster_count, color=manu_cut_color, linewidth=1.5, linestyle=manu_cut_line_style)
    

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Missing input path.")
        exit()

    if len(sys.argv) == 3:
        cluster_count = int(sys.argv[2])
    else:
        cluster_count = None

    fig, ax = set_plot_style()
    inputPath = sys.argv[1]
    plot_row = 0
    col_counter = 0

    cluster_ststs_filename = os.path.join(inputPath, CLUSTERING_STATS_REPORT_FILENAME)
    if os.path.isfile(cluster_ststs_filename):
        os.remove(cluster_ststs_filename)
    # Write column's headers.
    with open(cluster_ststs_filename, "a") as f:
        f.write("Filename\tAuto-determined Cluster Count\tAuto-determined Dendrogram Cut Height\tAuto-determined Cluster Silhouette Score\tManually-set Cluster Count\tManually-set Dendrogram Cut Height\tManually-set Cluster Silhouette Score\n")

    for root, dirpath, filenames in os.walk(inputPath):
        for filename in filenames:
            if os.path.splitext(filename)[1] == ".csv" and \
               not os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                col_counter += 1
                filename_without_extension = os.path.splitext(filename)[0]
                plot(ax[plot_row], filename_without_extension, True if col_counter == 4 else False, *cluster(root, filename, cluster_count))
                plot_row += 1

    image_file = os.path.join(inputPath, 'dendrogram-and-elbow.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()
