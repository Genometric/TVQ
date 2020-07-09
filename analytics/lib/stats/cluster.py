"""

"""

from .base_statistics import BaseStatistics
from ..base import Base, CLUSTER_NAME_COLUMN_LABEL

import numpy as np
import os
import sys
import pandas as pd
import matplotlib.pyplot as plt
import scipy.cluster.hierarchy as shc
from sklearn.cluster import AgglomerativeClustering
import seaborn as sns
import sklearn
from matplotlib.lines import Line2D

class Cluster(BaseStatistics):
    
    def __init__(self):
        self.cluster_count = None

    def run(self, input_path, cluster_count=None):
        """
        Executes a flow of clustering publications
        in files available from the given input path.
        """
        self.cluster_count = cluster_count
        input_files = Base.get_files(input_path, include_clustered_files=True)

        for filename in input_files:
            clustered_publications = self._cluster(filename)

    def _cluster(self, filename):
        repo_name = Base.get_repo_name(filename)
        input_df = Base.get_publications(filename)
        citations, _, _, _, _, _, _ = Base.get_vectors(input_df)

        # Performs hierarchical/agglomerative clustering and 
        # returns the hierarchical clustering encoded as a linkage matrix.
        # The `ward` linkage minimizes the variance of the clusters being merged.
        linkage_matrix = shc.linkage(citations, method='ward')

        variance, dist_growth_acceleration, \
            auto_cluster_count, auto_cut_distance, \
            manual_cluster_count, manual_cut_distance = \
            self._get_cluster_count(linkage_matrix, self.cluster_count)

        _, auto_silhouette_score = self._get_silhouette_score(citations, auto_cluster_count)
        cluster_labels, manual_silhouette_score = self._get_silhouette_score(citations, manual_cluster_count)

        # Add cluster information to original data.
        input_df[CLUSTER_NAME_COLUMN_LABEL] = cluster_labels

        return input_df

    def _get_cluster_count(self, linkage_matrix, cluster_count):
        """
        TODO: some tweaks has been made to compute cluster count
        even when less than four publications are given. However, 
        its correctness is not checked! either double-check it,
        or raise an exception instead of attempting to determining 
        cluster count for less than four pubs.
        """
        # This method is implemented based on info available from the following link.
        # https://joernhees.de/blog/2015/08/26/scipy-hierarchical-clustering-and-dendrogram-tutorial/#Elbow-Method
        last = linkage_matrix[-10:, 2]
        last_rev = last[::-1]
        idxs = np.arange(1, len(last) + 1)
        variance = pd.DataFrame(last_rev, idxs)

        # n is the number of times values are differenced. 
        # If zero, the input is returned as-is.
        n = 0

        # This condition should be met when the size of
        # linkage matrix is larger than two, or when clustering
        # at least four publications. 
        # Therefore, this condition will be met most of
        # the time. Only when clustering less than three 
        # publications it is not met, which results in
        # setting `n=0` and not modifying `idxs`. 
        if len(last) > 2:
            n = 2
            idxs = idxs[:-2] + 1

        # 2nd derivative of the distances
        acceleration = np.diff(last, n)  
        acceleration_rev = acceleration[::-1]

        dist_growth_acceleration = pd.DataFrame(acceleration_rev, idxs)

        auto_index = int(acceleration_rev[1:].argmax()) + 3
        manual_index = auto_index if cluster_count is None else cluster_count

        if auto_index > len(last_rev):
            auto_index = len(last_rev)
        if manual_index > len(last_rev):
            manual_index = len(last_rev)

        return \
            variance, dist_growth_acceleration, \
            auto_index, float(last_rev[auto_index - 1]), \
            manual_index, float(last_rev[manual_index - 1])

    def _get_silhouette_score(self, data, cluster_count):
        # Apply cluster to data.
        # It is not ideal to re-cluster data; hence, a potential improvement would be to
        # rework this and avoid re-clustering.
        model = AgglomerativeClustering(n_clusters=cluster_count, affinity='euclidean', linkage='ward')
        cluster_labels = model.fit_predict(data)

        silhoutte_score = float("NaN")
        if cluster_count > 1:
            silhoutte_score = sklearn.metrics.silhouette_score(data, cluster_labels)

        return cluster_labels, silhoutte_score
