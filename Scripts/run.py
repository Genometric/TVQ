import os
import sys
import cluster
import plot_cluster_quartiles
import t_test_clustered_data
import citation_growth_histogram
import plot_gain_scores
import plot_pubs_in_clusters

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Missing input path.")
        exit()

    input_path = sys.argv[1]

    plot_changes = False
    if len(sys.argv) == 3:
        plot_changes = sys.argv[2]

    if len(sys.argv) == 4:
        cluster_count = int(sys.argv[3])
    else:
        cluster_count = None

    cluster.run(input_path, cluster_count)
    plot_cluster_quartiles.run(input_path, plot_changes)
    t_test_clustered_data.run(input_path)
    citation_growth_histogram.run(input_path)
    plot_gain_scores.run(input_path)
    plot_pubs_in_clusters.run(input_path)
