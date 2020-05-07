import numpy as np
from numpy import std
import os
import sys
import pandas as pd
from scipy.stats import ttest_rel, ttest_ind, pearsonr, ttest_1samp
from statistics import mean
from math import sqrt
from t_test_clustered_data import get_sorted_clusters, get_vectors, get_clusters, CLUSTERED_FILENAME_POSFIX, get_repo_name
from t_test_clustered_data import get_clusters


PUBLICATION_ID_COLUMN = "PublicationID"
TOOLS_COLUMN = "Tools"
TOOLS_SEPARATOR = ";"


def get_clustered_repositories(input_path):
    filenames = []
    repositories = []
    for root, dirpath, files in os.walk(input_path):
        for filename in files:
            if os.path.splitext(filename)[1] == ".csv" and \
            os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                filenames.append(os.path.join(root, filename))
                repositories.append(get_repo_name(filename))

    return filenames, repositories


def get_pub_tool_count(filename):
    """
    Returns the number of unique tools and publications in each
    cluster of the given repository filename. 
    """
    clusters = get_clusters(filename)

    pubs = {}
    tools = {}

    for k in clusters.groups:
        if k not in pubs:
            pubs[k] = {}
            tools[k] = {}
        for index, row in clusters.get_group(k).iterrows():
            pub_id = row.get(PUBLICATION_ID_COLUMN)
            if pub_id not in pubs[k]:
                pubs[k][pub_id] = 0

            tool_names = (row.get(TOOLS_COLUMN)).split(TOOLS_SEPARATOR)
            for name in tool_names:
                if name not in tools[k]:
                    tools[k][name] = 0

    cluster_pubs_count = {}
    for k in pubs:
        cluster_pubs_count[k] = len(pubs[k])
    
    cluster_tools_count = {}
    for k in tools:
        cluster_tools_count[k] = len(tools[k])

    return sum(cluster_pubs_count.values()), cluster_pubs_count, sum(cluster_tools_count.values()), cluster_tools_count



def run(input_path):
    filenames, repositories = get_clustered_repositories(input_path)
    for filename in filenames:
        c_pubs, ck_pubs, c_tools, ck_tools = get_pub_tool_count(filename)


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Missing input path.")
        exit()

    run(sys.argv[1])
