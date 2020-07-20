"""
TODO: Add doc string.
"""

import argparse
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


def dir_path(path):
    if os.path.isdir(path):
        return path
    else:
        raise argparse.ArgumentTypeError(f"Invalid path: {path}")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description="A command-line interface to the scripts implemented " + 
                    "for analyzing the TVQ-generated citation data.")

    subparsers = parser.add_subparsers(help="Commands", dest="command")

    EXE_ALL_CMD = "exe_all"

    exe_all = subparsers.add_parser(EXE_ALL_CMD, help="Executes all the scripts in a predefined order.")

    # By default the value of arguments are stored in a variable with the same name
    # as the argument; e.g., the variable name is `args.name` for the following 
    # argument. This can be changed using the `dest` argument.
    exe_all.add_argument("input", type=dir_path, help="Path to directory containing input data.")
    exe_all.add_argument("-c", "--cluster_count", type=int, 
                         help="Groups data in the given number of clusters. " +
                         "If not provided, the cluster count is determined " +
                         "automatically using the Elbow method.")

    args = parser.parse_args()

    if args.command == EXE_ALL_CMD:
        input_path = args.input
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
