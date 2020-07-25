"""

"""

from .base_statistics import BaseStatistics
from ..base import Base, CLUSTER_NAME_COLUMN_LABEL, CLUSTERED_FILENAME_POSFIX
from ..plots.base_plot import BasePlot

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

class Cluster(BaseStatistics, BasePlot):
    
    def __init__(self):
        self.input_path = None
        self.cluster_count = None
        self.clustering_stats_filename = "clustering_stats.txt"

    def run(self, input_path, cluster_count=None):
        """
        Executes a flow of clustering publications
        in files available from the given input path.
        """
        self.input_path = input_path
        self.cluster_count = cluster_count

        # Creates a file for clustering stats,
        # and initializes it with column headers.
        self._write_cluster_stats()

        # Setup plotting.
        fig, ax = self.set_plot_style(nrows=4, ncols=2, height=12, width=16, dpi=300)
        plot_row = 0
        col_counter = 0

        input_files = Base.get_files(self.input_path, include_clustered_files=True)
        for filename in input_files:
            repo_name = Base.get_repo_name(filename)
            self._cluster(filename)
            self._write_cluster_stats(repo_name)
            self.clustered_publications = self._sort_clusters(self.clustered_publications)
            clustered_repo_filename = os.path.join(self.input_path, repo_name + CLUSTERED_FILENAME_POSFIX + '.csv')
            if os.path.isfile(clustered_repo_filename):
                os.remove(clustered_repo_filename)
            self.clustered_publications.to_csv(clustered_repo_filename, sep='\t', encoding='utf-8', index=False)

            col_counter += 1
            self._plot(ax=ax[plot_row], repository_name=repo_name, add_legend=True if col_counter == 4 else False, )

    def _cluster(self, filename):
        repo_name = Base.get_repo_name(filename)
        input_df = Base.get_publications(filename)
        citations, _, _, _, _, _, _ = Base.get_vectors(input_df)

        # Performs hierarchical/agglomerative clustering and 
        # returns the hierarchical clustering encoded as a linkage matrix.
        # The `ward` linkage minimizes the variance of the clusters being merged.
        self.linkage_matrix = shc.linkage(citations, method='ward')

        self._get_cluster_count(self.linkage_matrix, self.cluster_count)

        _, self.auto_silhouette_score = self._get_silhouette_score(citations, self.auto_cluster_count)
        cluster_labels, self.manual_silhouette_score = self._get_silhouette_score(citations, self.manual_cluster_count)

        # Add cluster information to original data.
        input_df[CLUSTER_NAME_COLUMN_LABEL] = cluster_labels

        self.clustered_publications = input_df

    def _get_cluster_count(self, cluster_count):
        """
        TODO: some tweaks has been made to compute cluster count
        when less than four publications are given. However, 
        its correctness is not checked! either double-check it,
        or raise an exception instead of attempting to determining 
        cluster count for less than four pubs.
        """
        # This method is implemented based on info available from the following link.
        # https://joernhees.de/blog/2015/08/26/scipy-hierarchical-clustering-and-dendrogram-tutorial/#Elbow-Method
        last = self.linkage_matrix[-10:, 2]
        last_rev = last[::-1]
        idxs = np.arange(1, len(last) + 1)
        self.variance = pd.DataFrame(last_rev, idxs)

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

        self.dist_growth_acceleration = pd.DataFrame(acceleration_rev, idxs)

        self.auto_cluster_count = int(acceleration_rev[1:].argmax()) + 3
        self.manual_cluster_count = self.auto_cluster_count if cluster_count is None else cluster_count

        if self.auto_cluster_count > len(last_rev):
            self.auto_cluster_count = len(last_rev)
        if self.manual_cluster_count > len(last_rev):
            self.manual_cluster_count = len(last_rev)

        self.auto_cut_distance = float(last_rev[self.auto_cluster_count - 1])
        self.manual_cut_distance = float(last_rev[self.manual_cluster_count - 1])

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

    def _sort_clusters(self, clustered_pubs):
        """
        Sort cluster labels based on the mean value of tools in each cluster.
        For instance, a group of tools might be clustered as cluster `0` and 
        another group as cluster `1`. If the mean of the second group is less
        than the mean of the first group, then the following code will update 
        cluster labels of the tools so that all clustered as cluster `0` are 
        clustered as cluster `1`, and those clustered as `1` are clustered as 
        cluster `0`.
        """
        mappings = {}
        _, mean_cluster_num_mappings, sorted_keys = Base.get_sorted_clusters(clustered_pubs.groupby(CLUSTER_NAME_COLUMN_LABEL))
        for i in range(0, len(sorted_keys)):
            mappings[mean_cluster_num_mappings[sorted_keys[i]]] = i
        clustered_pubs[CLUSTER_NAME_COLUMN_LABEL] = clustered_pubs[CLUSTER_NAME_COLUMN_LABEL].map(mappings)

        return clustered_pubs

    def _write_cluster_stats(self, repo_name=None):
        """
        Writes clustering status to a file. 

        :type   stats:  string
        :param  stats:  Serialized clustering statistics to a single string.
                        If not given, this method creates a statistics file
                        and initialize it with column headers.
                        If given, the stats are written to clustering stats file.
        """
        filename = os.path.join(self.input_path, self.clustering_stats_filename)
        if repo_name:
            with open(filename, "a") as f:
                f.write(\
                    f"{repo_name}\t" \
                    f"{self.auto_cluster_count}\t" \
                    f"{self.auto_cut_distance}\t" \
                    f"{auto_silhouette_score}\t" \
                    f"{self.manual_cluster_count}\t" \
                    f"{self.manual_cut_distance}\t" \
                    f"{self.manual_silhouette_score}\n")
        else:
            with open(filename, "w") as f:
                f.write(
                    "Filename\t" \
                    "Auto-determined Cluster Count\t" \
                    "Auto-determined Dendrogram Cut Height\t" \
                    "Auto-determined Cluster Silhouette Score\t" \
                    "Manually-set Cluster Count\t" \
                    "Manually-set Dendrogram Cut Height\t" \
                    "Manually-set Cluster Silhouette Score\n")

    def _plot(self, ax, repository_name, add_legend):
        col0 = ax[0]
        col1 = ax[1]

        auto_cut_color = "orange"
        manu_cut_color = "orange"

        auto_cut_line_style = "dotted"
        manu_cut_line_style = "dashed"

        linewidth = 1.5

        # Plots the hierarchical clustering as a dendrogram.
        dend = shc.dendrogram(self.linkage_matrix, no_labels=True, orientation="right", ax=col0)  #, truncate_mode="level")
        col0.axvline(x=self.auto_cut_distance, color=auto_cut_color, linewidth=linewidth, linestyle=auto_cut_line_style)
        col0.axvline(x=self.manual_cut_distance, color=manu_cut_color, linewidth=linewidth, linestyle=manu_cut_line_style)

        # Plot to a PNG file.
        col0.set_title(repository_name)
        col0.set_xlabel("Height")
        col0.grid(axis='x', which='major', color='w')

        col0.text(\
            0.82, 0.1, \
            "Silhouette Score={:.4f}".format(manual_silhouette_score), \
            horizontalalignment='center', \
            verticalalignment='center', \
            transform=col0.transAxes)

        # Plot the Elbow method's results.
        col1.plot(self.variance, label="Variance", marker='o', color='green')
        col1.plot(self.dist_growth_acceleration, label="Distance growth acceleration", marker="x", color="blue")

        col1.set_title(repository_name)
        col1.set_xlabel("Number of clusters")
        col1.set_ylabel("Distortion")

        if add_legend:
            col1.legend(loc='center', bbox_to_anchor=(0.5, -0.3), framealpha=0.0, fancybox=True)

            lines = [\
                Line2D([0], [0], color=auto_cut_color, linewidth=linewidth, linestyle=auto_cut_line_style), \
                Line2D([0], [0], color=manu_cut_color, linewidth=linewidth, linestyle=manu_cut_line_style)]

            labels = ['Auto-determined cut height', 'Manually-set cut height']
            col0.legend(lines, labels, loc='center', bbox_to_anchor=(0.5, -0.3), framealpha=0.0, fancybox=True)

        col1.axvline(x=self.auto_cluster_count, color=auto_cut_color, linewidth=1.5, linestyle=auto_cut_line_style)
        col1.axvline(x=self.manual_cluster_count, color=manu_cut_color, linewidth=1.5, linestyle=manu_cut_line_style)
