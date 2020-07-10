"""
TODO: Add doc string.
"""

import os
import sys
import cluster
import plot_cluster_quartiles
import t_test_clustered_data
import citation_growth_histogram
import plot_gain_scores
import plot_pubs_in_clusters
import plot_tool_pub
import plot_citations_distribution

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Missing input path.")
        exit()

    input_path = sys.argv[1]
    plot_changes = False
    if len(sys.argv) >= 3:
        plot_changes = sys.argv[2] == "True"

    if len(sys.argv) >= 4:
        cluster_count = int(sys.argv[3])
    else:
        cluster_count = None

    plot_density = False
    if len(sys.argv) >= 5:
        plot_density = sys.argv[4] == "True"

    if len(sys.argv) >= 6:
        cluster_source = sys.argv[5]
    else:
        cluster_source = "citations"

    cluster.run(input_path, cluster_count, cluster_source)
    plot_cluster_quartiles.run(input_path, plot_changes)
    t_test_clustered_data.run(input_path)
    citation_growth_histogram.run(input_path, plot_density)
    plot_gain_scores.run(input_path, plot_density)
    plot_pubs_in_clusters.run(input_path)
    plot_tool_pub.run(input_path)
    plot_citations_distribution.run(input_path, plot_density)
