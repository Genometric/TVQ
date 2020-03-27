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


def cluster(root, filename):
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

    cluster_count, cut_distance = get_cluster_count(linkage_matrix, filename_without_extension)
    print(cluster_count)
    print(cut_distance)

    # Plots the hierarchical clustering as a dendrogram.
    plt.figure(figsize=(10, 7))
    dend = shc.dendrogram(linkage_matrix)
    plt.axhline(y=float(cut_distance), color='r', linestyle='--')
    #plt.show()

    # Plot to a PNG file.
    plt.title(filename_without_extension)
    image_file = os.path.join(root, filename_without_extension + '_dendrogram.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file)
    plt.close()

    # Apply cluster to data.
    # It is not ideal to re-cluster data; hence, a potential improvement would be to
    # rework this and avoid send clustering.
    model = AgglomerativeClustering(n_clusters=cluster_count, affinity='euclidean', linkage='ward')  
    cluster_labels = model.fit_predict(df)
    df["cluster_label"] = cluster_labels


def get_cluster_count(Z, filename):
    last = Z[-10:, 2]
    last_rev = last[::-1]
    idxs = np.arange(1, len(last) + 1)
    plt.plot(idxs, last_rev)

    # 2nd derivative of the distances
    acceleration = np.diff(last, 2)  
    acceleration_rev = acceleration[::-1]
    plt.plot(idxs[:-2] + 1, acceleration_rev)

    # Plot to a PNG file.
    plt.title(filename)
    image_file = os.path.join(root, filename + '_elbow.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file)
    plt.close()

    return acceleration_rev.argmax() + 2, last_rev[acceleration_rev.argmax() + 1]
    

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Missing input path.")
        exit()

    inputPath = sys.argv[1]
    for root, dirpath, filenames in os.walk(inputPath):
        for filename in filenames:
            if os.path.splitext(filename)[1] == ".csv":
                cluster(root, filename)
