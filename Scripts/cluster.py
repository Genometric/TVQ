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

    cluster_count, cut_distance = get_cluster_count(linkage_matrix, filename_without_extension)
    print("\t- Cluster Count:\t{0}".format(cluster_count))
    print("\t- Cluster Cut Height:\t{0}".format(cut_distance))

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

    # Add cluster information to original data.
    input_df["cluster_label"] = cluster_labels

    silhouette_score = sklearn.metrics.silhouette_score(df, cluster_labels)
    print("\t- Silhouette Score:\t{0}".format(silhouette_score))


    # Write the DataFrame to CSV. 
    clustered_filename = os.path.join(root, filename_without_extension + CLUSTERED_FILENAME_POSFIX + '.csv')
    if os.path.isfile(clustered_filename):
        os.remove(clustered_filename)
    input_df.to_csv(clustered_filename, sep='\t', encoding='utf-8', index=False)


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
            if os.path.splitext(filename)[1] == ".csv" and \
               not os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                cluster(root, filename)
