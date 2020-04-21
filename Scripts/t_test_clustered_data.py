import numpy as np
from numpy import std
import os
import sys
import pandas as pd
from scipy.stats import ttest_rel, ttest_ind, pearsonr, ttest_1samp
from statistics import mean
from math import sqrt


CLUSTERED_FILENAME_POSFIX = "_clustered"
CLUSTER_NAME_COLUMN_LABEL = "cluster_label"


def get_repo_name(filename):
    filename = os.path.basename(filename)
    return (os.path.splitext(filename)[0]).replace(CLUSTERED_FILENAME_POSFIX, "")


def ttest_by_cluster(root, filename):
    print("\t- Repository: {0}".format(get_repo_name(filename)))
    clusters = get_clusters(root, filename)
    for k in clusters.groups:
        tools = clusters.get_group(k)
        (cohen_d, cohen_d_interpretation), (t_statistic, pvalue) = paired_ttest(tools)
        print(f"\t\t- Cluster number:\t{k}")
        print(f"\t\t\t* Tools count:\t{len(tools)}")
        print_ttest_results(pvalue, t_statistic, cohen_d, cohen_d_interpretation, "\t\t\t")


def print_ttest_results(pvalue, t_statistic, cohen_d, cohen_d_interpretation, indentation="\t\t"):
    print(f"{indentation}* t-Statistic:\t{t_statistic}")
    print(f"{indentation}* p-value:\t{pvalue}")
    print(f"{indentation}* Cohen's d:\t{cohen_d}\t{cohen_d_interpretation}")


def paired_ttest(tools):
    citations, _, _, sums, avg_pre, avg_post, _ = get_vectors(tools)
    t_statistic, pvalue = ttest_rel(avg_pre, avg_post)
    return cohen_d(avg_pre, avg_post), (abs(t_statistic), pvalue)


def one_sample_ttest(x, population_mean):
    t_statistic, pvalue = ttest_1samp(x, population_mean)
    t_statistic = abs(t_statistic)
    d, d_interpretation = cohen_d(x, population_mean=population_mean)
    return t_statistic, pvalue, d, d_interpretation


def independent_ttest(x, y):
    t_statistic, pvalue = ttest_ind(x, y, equal_var=False)
    t_statistic = abs(t_statistic)
    d, d_interpretation = cohen_d(x, y)
    return t_statistic, pvalue, d, d_interpretation


def cohen_d(x, y=None, population_mean=0.0):
    if y:
        # Cohen's d is computed as explained in the following link:
        # https://stackoverflow.com/a/33002123/947889
        d = len(x) + len(y) - 2
        cohen_d = (mean(x) - mean(y)) / sqrt(((len(x) - 1) * std(x, ddof=1) ** 2 + (len(y) - 1) * std(y, ddof=1) ** 2) / d) 
        cohen_d = abs(cohen_d)
    else:
        cohen_d = (mean(x) - population_mean)/std(x, ddof=1)

    # This interpretation is based on the info available on Wikipedia:
    # https://en.wikipedia.org/wiki/Effect_size#Cohen.27s_d
    if cohen_d >= 0.00 and cohen_d < 0.10:
        msg = "Very small"
    if cohen_d >= 0.10 and cohen_d < 0.35:
        msg = "Small"
    if cohen_d >= 0.35 and cohen_d < 0.65:
        msg = "Medium"
    if cohen_d >= 0.65 and cohen_d < 0.90:
        msg = "Large"
    if cohen_d >= 0.90:
        msg = "Very large"

    return cohen_d, msg + " effect size"



def pre_post_columns(tools):
    """
    Returns all the column headers, and headers of columns containing 
    normalized citation counts belonging to when before and after a 
    tool was added to the repository.
    """
    column_headers = tools.columns.values.tolist()
    pre = []
    post = []
    for header in column_headers:
        try:
            v = float(header)
        except ValueError:
            continue

        if v < 0:
            pre.append(header)
        else:
            post.append(header)

    return column_headers, pre, post


def get_clusters(root, filename):
    """
    Returns a data frame grouped-by cluster name.
    
    :rtype:  pandas.core.groupby.generic.DataFrameGroupBy
    """
    input_df = pd.read_csv(os.path.join(root, filename), header=0, sep='\t')
    return input_df.groupby(CLUSTER_NAME_COLUMN_LABEL)


def get_vectors(tools):
    # columns: a list of all the column headers.
    # pre:  a list of headers of columns containing normalized citation counts BEFORE a tool was added to the repository.
    # post: a list of headers of columns containing normalized citation counts AFTER  a tool was added to the repository.
    columns, pre_headers, post_headers = pre_post_columns(tools)

    # A list of two-dimensional lists, first dimension is pre counts
    # and second dimension contains post citation counts.
    citations = []

    sums = []

    deltas = []

    # Lists contain citation counts before (pre) and after (post)
    # a tool was added to the repository.
    avg_pre = []
    avg_pst = []

    pre_citations = []
    post_citations = []
    for index, row in tools.iterrows():
        pre_vals = row.get(pre_headers).values.tolist()
        post_vals = row.get(post_headers).values.tolist()

        pre_citations.append(pre_vals)
        post_citations.append(post_vals)

        citations.append([pre_vals, post_vals])
        sums.append(np.sum(pre_vals + post_vals))
        avg_pre.append(np.average(pre_vals))
        avg_pst.append(np.average(post_vals))

        deltas.append(abs(np.average(post_vals) - np.average(pre_vals)))

    return citations, pre_citations, post_citations, sums, avg_pre, avg_pst, deltas


def get_sorted_clusters(clusters):
    agg_cluster_mapping = {}
    for k in clusters.groups:
        citations, _, _, _, _, _, _ = get_vectors(clusters.get_group(k))
        flattend = []
        for c in citations:
            flattend.append(c[0] + c[1])

        agg_cluster_mapping[np.average(flattend)] = k

    return sorted(agg_cluster_mapping), agg_cluster_mapping


def ttest_repository(input_filename, output_filename):
    print(f"\t- Repository: {get_repo_name(input_filename)}")
    tools = pd.read_csv(input_filename, header=0, sep='\t')
    (cohen_d, cohen_d_interpretation), (t_statistic, pvalue) = paired_ttest(tools)
    print_ttest_results(pvalue, t_statistic, cohen_d, cohen_d_interpretation, "\t\t")
    with open(output_filename, "a") as f:
        f.write(f"{get_repo_name(input_filename)}\t{t_statistic}\t{pvalue}\t{cohen_d}\t{cohen_d_interpretation}\n")


def ttest_repository_delta(input_filename, output_filename):
    print(f"\t- Repository: {get_repo_name(input_filename)}")
    tools = pd.read_csv(input_filename, header=0, sep='\t')
    _, _, _, _, _, _, delta = get_vectors(tools)
    t_statistic, pvalue, d, d_interpretation = one_sample_ttest(delta, 0.0)
    print_ttest_results(pvalue, t_statistic, d, d_interpretation, "\t\t")
    with open(output_filename, "a") as f:
        f.write(f"{get_repo_name(input_filename)}\t{t_statistic}\t{pvalue}\t{d}\t{d_interpretation}\n")


def ttest_repositories(repo_a_filename, repo_b_filename, output_filename):
    repo_a = get_repo_name(repo_a_filename)
    repo_b = get_repo_name(repo_b_filename)
    print(f"\t- Repositories: {repo_a} and {repo_b}")

    repo_a_tools = pd.read_csv(repo_a_filename, header=0, sep='\t')
    repo_b_tools = pd.read_csv(repo_b_filename, header=0, sep='\t')

    _, _, _, _, _, _, deltas_a = get_vectors(repo_a_tools)
    _, _, _, _, _, _, deltas_b = get_vectors(repo_b_tools)

    t_statistic, pvalue, d, d_interpretation = independent_ttest(deltas_a, deltas_b)
    print_ttest_results(pvalue, t_statistic, d, d_interpretation, "\t\t")

    with open(output_filename, "a") as f:
        f.write(f"{repo_a}\t{repo_b}\t{t_statistic}\t{pvalue}\t{d}\t{d_interpretation}\n")


def ttest_corresponding_clusters(root, filename_a, filename_b, output_filename):
    repo_a = get_repo_name(filename_a)
    repo_b = get_repo_name(filename_b)
    print(f"\t- Repositories: {repo_a} and {repo_b}")

    clusters_a = get_clusters(root, filename_a)
    clusters_b = get_clusters(root, filename_b)
    sorted_keys_a, agg_cluster_mapping_a = get_sorted_clusters(clusters_a)
    sorted_keys_b, agg_cluster_mapping_b = get_sorted_clusters(clusters_b)    

    with open(output_filename, "a") as f:
        for i in range(0, len(sorted_keys_a)):
            cluster_a_num = agg_cluster_mapping_a[sorted_keys_a[i]]
            cluster_b_num = agg_cluster_mapping_b[sorted_keys_b[i]]
            _, _, _, sums_a, _, _, _ = get_vectors(clusters_a.get_group(cluster_a_num))
            _, _, _, sums_b, _, _, _ = get_vectors(clusters_b.get_group(cluster_b_num))

            t_statistic, pvalue, d, d_interpretation = independent_ttest(sums_a, sums_b)
            print_ttest_results(pvalue, t_statistic, d, d_interpretation, "\t\t")

            f.write(f"{repo_a}\t{repo_b}\t{i}\t{i}\t{sorted_keys_a[i]}\t{sorted_keys_b[i]}\t{t_statistic}\t{pvalue}\t{d}\t{d_interpretation}\n")


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Missing input path.")
        exit()

    inputPath = sys.argv[1]

    filenames = []
    for root, dirpath, files in os.walk(inputPath):
        for filename in files:
            if os.path.splitext(filename)[1] == ".csv" and \
               os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                filenames.append(filename)

    print("\n>>> Performing t-test on pre and post citations for the null hypothesis that the two have identical average values.")
    repo_ttest_filename = os.path.join(root, "paired_ttest_avg_pre_post.txt")
    if os.path.isfile(repo_ttest_filename):
        os.remove(repo_ttest_filename)
    with open(repo_ttest_filename, "a") as f:
        f.write("Repository\tt-Statistic\tp-value\tCohen's d\tInterpretation\n")

    for filename in filenames:
        ttest_repository(os.path.join(root, filename), repo_ttest_filename)

    print("\n>>> Performing t-test on citations delta (post - pre) for the null hypothesis that the mean equals zero.")
    one_sample_ttest_filename = os.path.join(root, "one_sample_ttest.txt")
    if os.path.isfile(one_sample_ttest_filename):
        os.remove(one_sample_ttest_filename)
    with open(one_sample_ttest_filename, "a") as f:
        f.write("Repository\tt-Statistic\tp-value\tCohen's d\tInterpretation\n")
    for filename in filenames:
        ttest_repository_delta(os.path.join(root, filename), one_sample_ttest_filename)

    print(f"\n>>> Performing Welch's t-test for the null hypothesis that the two repositories have identical average values of pre-post delta, NOT assuming equal population variance.")
    repos_ttest_filename = os.path.join(root, "ttest_repositories.txt")
    if os.path.isfile(repos_ttest_filename):
        os.remove(repos_ttest_filename)
    with open(repos_ttest_filename, "a") as f:
        f.write("Repository A\tRepository B\tt-Statistic\tp-value\tCohen's d\tInterpretation\n")

    for i in range(0, len(filenames)-1):
        for j in range(i+1, len(filenames)):
            ttest_repositories(os.path.join(root, filenames[i]), os.path.join(root, filenames[j]), repos_ttest_filename)

    print("\n>>> Performing t-test on pre and post citations of tools in different clusters for the null hypothesis that the two have identical average values.")
    for filename in filenames:
        ttest_by_cluster(root, filename)

    print(f"\n>>> Performing Welch's t-test for the null hypothesis that the two independent relative clusters of two repositories have identical average (expected) values NOT assuming equal population variance.")
    tcc_filename = os.path.join(inputPath, 'ttest_corresponding_clusters.txt')
    if os.path.isfile(tcc_filename):
        os.remove(tcc_filename)

    # Add column header. 
    with open(tcc_filename, "a") as f:
        f.write(f"Repo A\tRepo B\tRepo A Cluster Number\tRepo B Cluster Number\tAverage Citation Count in Repo A Cluster\tAverage Citation Count in Repo B Cluster\tt Statistic\tp-value\tCohen's d\tCohen's d Interpretation\n")

    # Iterate through all the permutations of repositories,
    # and compute t-test between corresponding clusters.
    for i in range(0, len(filenames)-1):
        for j in range(i+1, len(filenames)):
            ttest_corresponding_clusters(root, filenames[i], filenames[j], tcc_filename)
