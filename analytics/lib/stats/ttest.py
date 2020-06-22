
import os

from .base_statistics import BaseStatistics
from ..base import Base

import numpy as np
from numpy import std
import os
import sys
import pandas as pd
from scipy.stats import ttest_rel, ttest_ind, pearsonr, ttest_1samp
from statistics import mean
from math import sqrt

class TTest(BaseStatistics):

    def run(self, input_path):
        """
        Executes a pre-defined flow of computing t-test on
        files available from the input path.
        """
        filenames = Base.get_files(input_path, include_clustered_files=True)

        repo_ttest_filename = os.path.join(input_path, "paired_ttest_avg_pre_post.txt")
        if os.path.isfile(repo_ttest_filename):
            os.remove(repo_ttest_filename)
        with open(repo_ttest_filename, "a") as f:
            f.write("Repository\tAverage Pre Citations\tAverage Post Citations\tGrowth\tt-Statistic\tp-value\tCohen's d\tInterpretation\n")

        for filename in filenames:
            repository = Base.get_repo_name(filename)
            publications = Base.get_publications(os.path.join(input_path, filename))
            self.ttest_repository(repository, publications, repo_ttest_filename)

        print("\n>>> Performing t-test on citations delta (post - pre) for the null hypothesis that the mean equals zero.")
        one_sample_ttest_filename = os.path.join(input_path, "one_sample_ttest.txt")
        if os.path.isfile(one_sample_ttest_filename):
            os.remove(one_sample_ttest_filename)
        with open(one_sample_ttest_filename, "a") as f:
            f.write("Repository\tAverage Pre Citations\tAverage Post Citations\tGrowth\tt-Statistic\tp-value\tCohen's d\tInterpretation\n")
        for filename in filenames:
            ttest_repository_delta(os.path.join(input_path, filename), one_sample_ttest_filename)

        print(f"\n>>> Performing Welch's t-test for the null hypothesis that the two repositories have identical average values of pre-post delta, NOT assuming equal population variance.")
        repos_ttest_filename = os.path.join(input_path, "ttest_repositories.txt")
        if os.path.isfile(repos_ttest_filename):
            os.remove(repos_ttest_filename)
        with open(repos_ttest_filename, "a") as f:
            f.write("Repository A\tRepository B\tt-Statistic\tp-value\tCohen's d\tInterpretation\n")

        for i in range(0, len(filenames)-1):
            for j in range(i+1, len(filenames)):
                ttest_repositories(os.path.join(input_path, filenames[i]), os.path.join(input_path, filenames[j]), repos_ttest_filename)

        print("\n>>> Performing t-test on pre and post citations of tools in different clusters for the null hypothesis that the two have identical average values.")
        for filename in filenames:
            ttest_by_cluster(root, filename)

        print(f"\n>>> Performing Welch's t-test for the null hypothesis that the two independent relative clusters of two repositories have identical average (expected) values NOT assuming equal population variance.")
        tcc_filename = os.path.join(input_path, 'ttest_corresponding_clusters.txt')
        if os.path.isfile(tcc_filename):
            os.remove(tcc_filename)

        # Add column header. 
        with open(tcc_filename, "a") as f:
            f.write(
                f"Repo A\t"
                f"Repo B\t"
                f"Repo A Cluster Number\t"
                f"Repo B Cluster Number\t"
                f"Average Citation Count in Repo A Cluster\t"
                f"Average Citation Count in Repo B Cluster\t"
                f"t Statistic\t"
                f"p-value\t"
                f"Cohen's d\tC"
                f"ohen's d Interpretation\n")

        # Iterate through all the permutations of repositories,
        # and compute t-test between corresponding clusters.
        for i in range(0, len(filenames)-1):
            for j in range(i+1, len(filenames)):
                ttest_corresponding_clusters(input_path, filenames[i], filenames[j], tcc_filename)

    def ttest_repository(self, repository, publications, output_filename):
        d, d_interpretation, t_statistic, pvalue = BaseStatistics.ttest_avg_pre_post(publications)
        avg_pre, avg_post = get_avg_pre_post(publications)
        print_ttest_results(pvalue, t_statistic, d, d_interpretation, "\t\t")
        growth = ((avg_post - avg_pre) / avg_pre) * 100.0
        with open(output_filename, "a") as f:
            f.write(f"{repository}\t{avg_pre}\t{avg_post}\t{growth}%\t{t_statistic}\t{pvalue}\t{d}\t{d_interpretation}\n")

    def paired_ttest(self, tools):
        citations, _, _, sums, avg_pre, avg_post, _ = get_vectors(tools)
        t_statistic, pvalue = ttest_rel(avg_pre, avg_post)
        return cohen_d(avg_pre, avg_post), (abs(t_statistic), pvalue)

