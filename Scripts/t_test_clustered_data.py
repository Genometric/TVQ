import numpy as np
from numpy import std
import os
import sys
import pandas as pd
from scipy.stats import ttest_rel, pearsonr
from statistics import mean
from math import sqrt


CLUSTERED_FILENAME_POSFIX = "_clustered"
CLUSTER_NAME_COLUMN_LABEL = "cluster_label"


def ttest_by_cluster(root, filename):
    print("\n>>> Processing file: {0}".format(filename))
    clusters = get_clusters(root, filename)
    for k in clusters.groups:
        (cohen_d, cohen_d_interpretation), (t_statistic, pvalue) = ttest(k, clusters.get_group(k))
        print(f"\t- Cluster number:\t{k}")
        print(f"\t\t* t-Statistic:\t{t_statistic}")
        print(f"\t\t* p-value:\t{pvalue}")
        print(f"\t\t* Cohen's d:\t{cohen_d}\t{cohen_d_interpretation}")


def ttest(cluster_label, tools):
    # columns: a list of all the column headers.
    # pre:  a list of headers of columns containing normalized citation counts BEFORE a tool was added to the repository.
    # post: a list of headers of columns containing normalized citation counts AFTER  a tool was added to the repository.
    columns, pre_headers, post_headers = pre_post_columns(tools)

    # Lists contain citation counts before (pre) and after (post) a tool was added to the repository.
    pre = []
    post = []
    avg_pre = []
    avg_pst = []
    for index, row in tools.iterrows():
        pre_vals = row.get(pre_headers).values.tolist()
        post_vals = row.get(post_headers).values.tolist()

        pre.append(pre_vals)
        post.append(post_vals)
        avg_pre.append(np.average(pre_vals))
        avg_pst.append(np.average(post_vals))

    return cohen_d(pre_vals,post_vals), ttest_rel(avg_pre, avg_pst)


def cohen_d(x,y):
    # Cohen's d is computed as explained in the following link:
    # https://stackoverflow.com/a/33002123/947889
    d = len(x) + len(y) - 2
    cohen_d = (mean(x) - mean(y)) / sqrt(((len(x) - 1) * std(x, ddof=1) ** 2 + (len(y) - 1) * std(y, ddof=1) ** 2) / d) 
    cohen_d = abs(cohen_d)

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

    return cohen_d, msg + " effect size."



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


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Missing input path.")
        exit()

    inputPath = sys.argv[1]
    for root, dirpath, filenames in os.walk(inputPath):
        for filename in filenames:
            if os.path.splitext(filename)[1] == ".csv" and \
               os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                ttest_by_cluster(root, filename)
