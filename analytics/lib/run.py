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

def add_input_arg(parser):
    # By default the value of arguments are stored in a variable with the same name
    # as the argument; e.g., the variable name is `args.name` for the following 
    # argument. This can be changed using the `dest` argument.
    parser.add_argument("input", type=dir_path, help="Path to directory containing input data.")
    return parser


def add_cluster_count_arg(parser):
    parser.add_argument("-c", "--cluster_count", type=int, 
                        help="Groups data in the given number of clusters. " +
                        "If not provided, the cluster count is determined " +
                        "automatically using the Elbow method.")
    return parser


def add_cluster_source(parser):
    parser.add_argument("-s", "--source", type=str, default="citations",
                        help="Sets the cluster source.")
    return parser


def add_plot_changes(parser):
    parser.add_argument("-g", "--plot_changes", action="store_true", 
                        help="If set, plots changes on clustered citation counts. " +
                        "By default the changes are not plotted.")
    return parser


def add_plot_density(parser):
    parser.add_argument("-d", "--plot_density", action="store_true", 
                        help="If set, plots probability instead of absolute values. Default is False.")
    return parser


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description="A command-line interface to the scripts implemented " + 
                    "for analyzing the TVQ-generated citation data.")

    subparsers = parser.add_subparsers(help="Commands", dest="command")

    EXE_ALL_CMD = "exe-all"
    CLUSTER_CMD = "cluster"
    PLT_CLS_CMD = "plot-cluster"
    TTEST_CMD = "t-test"
    G_HIST_CMD = "growth-hist"
    GAIN_SC_CMD = "gain-score"
    PUBS_IN_C_CMD = "pubs-in-clusters"
    TOOL_PUB_CMD = "tool-pub"
    C_DIST_CMD = "citation-dist"

    exe_all = subparsers.add_parser(EXE_ALL_CMD, help="Executes all the scripts in a predefined order.")
    exe_all = add_input_arg(exe_all)
    exe_all = add_cluster_count_arg(exe_all)
    exe_all = add_cluster_source(exe_all)
    exe_all = add_plot_changes(exe_all)
    exe_all = add_plot_density(exe_all)

    cluster_parser = subparsers.add_parser(CLUSTER_CMD, help="Clusters publications in repositories stored in CSV files in given input path.")
    cluster_parser = add_input_arg(cluster_parser)
    cluster_parser = add_cluster_count_arg(cluster_parser)
    cluster_parser = add_cluster_source(cluster_parser)

    plot_cluster_parser = subparsers.add_parser(PLT_CLS_CMD, help="Plot clusters and citation counts in quartiles.")
    plot_cluster_parser = add_input_arg(plot_cluster_parser)
    plot_cluster_parser = add_plot_changes(plot_cluster_parser)

    ttest_parser = subparsers.add_parser(TTEST_CMD, help="Performs t-test on the citation count of publications.")
    ttest_parser = add_input_arg(plot_cluster_parser)

    growth_histogram_parser = subparsers.add_parser(G_HIST_CMD, help="Plots a histogram of the citation count growth.")
    growth_histogram_parser = add_input_arg(growth_histogram_parser)
    growth_histogram_parser = add_plot_density(growth_histogram_parser)

    gain_score_parser = subparsers.add_parser(GAIN_SC_CMD, help="Plots gain score.")
    gain_score_parser = add_input_arg(gain_score_parser)
    gain_score_parser = add_plot_density(gain_score_parser)

    pubs_in_clusters_parser = subparsers.add_parser(PUBS_IN_C_CMD, help="Plots publications in clusters.")
    pubs_in_clusters_parser = add_input_arg(pubs_in_clusters_parser)

    tool_pub_parser = subparsers.add_parser(TOOL_PUB_CMD, help="Plots tools in publications.")
    tool_pub_parser = add_input_arg(tool_pub_parser)

    plot_citation_dist_parser = subparsers.add_parser(C_DIST_CMD, help="Plots citations distribution.")
    plot_citation_dist_parser = add_input_arg(plot_citation_dist_parser)
    plot_citation_dist_parser = add_plot_density(plot_citation_dist_parser)

    args = parser.parse_args()

    if args.command == EXE_ALL_CMD:
        input_path = args.input
        plot_changes = args.plot_changes
        cluster_count = args.cluster_count
        plot_density = args.plot_density
        cluster_source = args.source

        cluster.run(input_path, cluster_count, cluster_source)
        plot_cluster_quartiles.run(input_path, plot_changes)
        t_test_clustered_data.run(input_path)
        citation_growth_histogram.run(input_path, plot_density)
        plot_gain_scores.run(input_path, plot_density)
        plot_pubs_in_clusters.run(input_path)
        plot_tool_pub.run(input_path)
        plot_citations_distribution.run(input_path, plot_density)

    elif args.command == CLUSTER_CMD:
        cluster.run(args.input, args.cluster_count, args.source)

    elif args.command == PLT_CLS_CMD:
        plot_cluster_quartiles.run(args.input, args.plot_changes)

    elif args.command == TTEST_CMD:
        t_test_clustered_data.run(args.input)

    elif args.command == G_HIST_CMD:
        citation_growth_histogram.run(args.input, args.plot_density)

    elif args.command == GAIN_SC_CMD:
        plot_gain_scores.run(args.input, args.plot_density)

    elif args.command == PUBS_IN_C_CMD:
        plot_pubs_in_clusters.run(args.input)

    elif args.command == TOOL_PUB_CMD:
        plot_tool_pub.run(args.input)

    elif args.command == C_DIST_CMD:
        plot_citations_distribution.run(args.input, args.plot_density)
