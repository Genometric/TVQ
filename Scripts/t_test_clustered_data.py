import numpy as np
import os
import sys
import pandas as pd
from scipy.stats import ttest_rel


CLUSTERED_FILENAME_POSFIX = "_clustered"
CLUSTER_NAME_COLUMN_LABEL = "cluster_label"


def ttest_by_cluster(root, filename):
    print("\n>>> Processing file: {0}".format(filename))
    clusters = get_clusters(root, filename)
    for k in clusters.groups:
        t_statistic, pvalue = ttest(k, clusters.get_group(k))
        print(t_statistic)
        print(pvalue)


def ttest(cluster_label, tools):
    # columns: a list of all the column headers.
    # pre:  a list of headers of columns containing normalized citation counts BEFORE a tool was added to the repository.
    # post: a list of headers of columns containing normalized citation counts AFTER  a tool was added to the repository.
    columns, pre, post = pre_post_columns(tools)

    avg_pre = []
    avg_pst = []
    for index, row in tools.iterrows():
        avg_pre.append(np.average(row.get(pre).values.tolist()))
        avg_pst.append(np.average(row.get(post).values.tolist()))

    return ttest_rel(avg_pre, avg_pst)
    


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
