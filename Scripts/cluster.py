import numpy as np
import os
import sys
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import scipy.cluster.hierarchy as shc
from sklearn.cluster import AgglomerativeClustering
import seaborn as sns


def cluster(root, filename):
    input_df = pd.read_csv(os.path.join(root, filename), header=0, sep='\t')
    
    # Remove the tool name column b/c it's a categorical column.
    df = input_df.drop("ToolName", 1)

    # Remove the ID column b/c it contains a unique record for every row.
    df = df.drop("ID", 1)

    # Perform hierarchical/agglomerative clustering and 
    # returns the hierarchical clustering encoded as a linkage matrix.
    # The `ward` linkage minimizes the variance of the clusters being merged.
    linkage_matrix = shc.linkage(df, method='ward')

    # Plots the hierarchical clustering as a dendrogram.
    plt.figure(figsize=(10, 7))  
    plt.title(filename)  
    dend = shc.dendrogram(linkage_matrix)
    plt.axhline(y=6, color='r', linestyle='--')
    plt.show()

    
    cluster = AgglomerativeClustering(n_clusters=2, affinity='euclidean', linkage='ward')  
    hc = cluster.fit(df)
    df["clusters"] = hc.labels_


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Missing input path.")
        exit()

    inputPath = sys.argv[1]
    for root, dirpath, filenames in os.walk(inputPath):
        for filename in filenames:
            cluster(root, filename)
