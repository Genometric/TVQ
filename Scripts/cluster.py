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


def cluster(root, filename):
    print("\n>>> Processing file: {0}".format(filename))
    filename_without_extension = os.path.splitext(filename)[0]
    input_df = pd.read_csv(os.path.join(root, filename), header=0, sep='\t')
    
    # Remove the tool name column b/c it's a categorical column.
    df = input_df.drop("ToolName", 1)

    # Remove the ID column b/c it contains a unique record for every row.
    df = df.drop("ID", 1)

    # Perform hierarchical/agglomerative clustering and 
    # returns the hierarchical clustering encoded as a linkage matrix.
    # The `ward` linkage minimizes the variance of the clusters being merged.
    linkage_matrix = shc.linkage(df, method='ward')

    variance, dist_growth_acceleration, cluster_count, cut_distance = get_cluster_count(linkage_matrix, filename_without_extension)
    print("\t- Cluster Count:\t{0}".format(cluster_count))
    print("\t- Cluster Cut Height:\t{0}".format(cut_distance))

    # Apply cluster to data.
    # It is not ideal to re-cluster data; hence, a potential improvement would be to
    # rework this and avoid send clustering.
    model = AgglomerativeClustering(n_clusters=cluster_count, affinity='euclidean', linkage='ward')  
    cluster_labels = model.fit_predict(df)

    # Add cluster information to original data.
    input_df[CLUSTER_NAME_COLUMN_LABEL] = cluster_labels

    silhouette_score = sklearn.metrics.silhouette_score(df, cluster_labels)
    print("\t- Silhouette Score:\t{0}".format(silhouette_score))

    # Write the DataFrame to CSV. 
    clustered_filename = os.path.join(root, filename_without_extension + CLUSTERED_FILENAME_POSFIX + '.csv')
    if os.path.isfile(clustered_filename):
        os.remove(clustered_filename)
    input_df.to_csv(clustered_filename, sep='\t', encoding='utf-8', index=False)

    return linkage_matrix, cut_distance, silhouette_score, variance, dist_growth_acceleration


def get_cluster_count(Z, filename):
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

    return variance, dist_growth_acceleration, acceleration_rev.argmax() + 2, last_rev[acceleration_rev.argmax() + 1]


def set_plot_style():
    sns.set()
    sns.set_context("paper")
    sns.set_style("darkgrid")
    fig, axes = plt.subplots(nrows=4, ncols=2, figsize=(12, 16), dpi=300, gridspec_kw={'width_ratios': [2, 1]})  # , constrained_layout=True)
    plt.subplots_adjust(wspace=0.15, hspace=0.35)
    return fig, axes

def plot(ax, filename_without_extension, linkage_matrix, cut_distance, silhouette_score, variance, dist_growth_acceleration):
    col0 = ax[0]
    col1 = ax[1]

    # Plots the hierarchical clustering as a dendrogram.
    dend = shc.dendrogram(linkage_matrix, no_labels=True, orientation="right", ax=col0)
    col0.axvline(x=float(cut_distance), color='r', linestyle='--')

    # Plot to a PNG file.
    col0.set_title(filename_without_extension)
    col0.set_xlabel("Height")
    col0.grid(axis='x', which='major', color='w')

    # Plot the Elbow method's results.
    col1.plot(variance, label="Variance")
    col1.plot(dist_growth_acceleration, label="Distance growth acceleration")

    col1.set_title(filename)
    col1.set_xlabel("Number of clusters")
    col1.set_ylabel("Distortion")
    col1.legend(loc="upper right")

    # Show Y-Axis on the right side of the plot.
    #col1.yaxis.set_label_position("right")
    #col1.yaxis.tick_right()
    

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Missing input path.")
        exit()

    fig, ax = set_plot_style()
    inputPath = sys.argv[1]
    plot_row = 0
    for root, dirpath, filenames in os.walk(inputPath):
        for filename in filenames:
            if os.path.splitext(filename)[1] == ".csv" and \
               not os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                filename_without_extension = os.path.splitext(filename)[0]
                plot(ax[plot_row], filename_without_extension, *cluster(root, filename))
                plot_row += 1

    image_file = os.path.join(inputPath, 'plot.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file)
    plt.close()
