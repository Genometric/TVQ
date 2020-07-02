
import os

from .base_statistics import BaseStatistics, SUM_PRE_CITATIONS_COLUMN_LABEL, SUM_POST_CITATIONS_COLUMN_LABEL
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
    TTEST_HEADER = ["Repository", "Average Pre Citations", "Average Post Citations", "Growth", "t-Statistic", "p-value", "Cohen's d", "Interpretation"]
    def __init__(self):
        pass

    def run(self, input_path):
        """
        Executes a flow of computing t-test on
        files available from the input path.
        """
        filenames = Base.get_files(input_path, include_clustered_files=True)

        self.ttest_avg_pre_post(filenames, os.path.join(input_path, "paired_ttest_avg_pre_post.txt"))
        self.ttest_delta(filenames, os.path.join(input_path, "one_sample_ttest.txt"))
        self.ttest_deltas(filenames, os.path.join(input_path, "ttest_repositories.txt"))
        self.ttest_corresponding_clusters(filenames, os.path.join(input_path, 'ttest_corresponding_clusters.txt'))

    def ttest_avg_pre_post(self, input_filenames, output_filename):
        with open(output_filename, "w") as f:
            f.write("Repository\tAverage Pre Citations\tAverage Post Citations\tGrowth\tt-Statistic\tp-value\tCohen's d\tInterpretation\n")

        for filename in input_filenames:
            repository = Base.get_repo_name(filename)
            publications = Base.get_publications(filename)

            d, d_interpretation, t_statistic, pvalue = BaseStatistics.ttest_avg_pre_post(publications)
            avg_pre = BaseStatistics.get_mean_of_raw_citations(publications, True)
            avg_post = BaseStatistics.get_mean_of_raw_citations(publications, False)
            growth = ((avg_post - avg_pre) / avg_pre) * 100.0
            with open(output_filename, "a") as f:
                f.write(f"{repository}\t{avg_pre}\t{avg_post}\t{growth}%\t{t_statistic}\t{pvalue}\t{d}\t{d_interpretation}\n")

    def ttest_delta(self, input_filenames, output_filename):
        with open(output_filename, "w") as f:
            f.write("\t".join(TTest.TTEST_HEADER) + "\n")

        for filename in input_filenames:
            repository = Base.get_repo_name(filename)
            publications = Base.get_publications(filename)

            d, d_interpretation, t_statistic, pvalue = BaseStatistics.ttest_delta(publications)
            avg_pre = BaseStatistics.get_mean_of_raw_citations(publications, True)
            avg_post = BaseStatistics.get_mean_of_raw_citations(publications, False)
            growth = ((avg_post - avg_pre) / avg_pre) * 100.0
            with open(output_filename, "a") as f:
                f.write(f"{repository}\t{avg_pre}\t{avg_post}\t{growth}%\t{t_statistic}\t{pvalue}\t{d}\t{d_interpretation}\n")

    def ttest_deltas(self, input_filenames, output_filename):
        """
        Performing Welch's t-test for the null hypothesis that the two 
        repositories have identical average values of pre-post delta, 
        NOT assuming equal population variance.
        """
        with open(output_filename, "w") as f:
            f.write("Repository A\tRepository B\tt-Statistic\tp-value\tCohen's d\tInterpretation\n")

        for i in range(0, len(input_filenames)-1):
            for j in range(i+1, len(input_filenames)):
                repository_a = Base.get_repo_name(input_filenames[i])
                publications_a = Base.get_publications(input_filenames[i])

                repository_b = Base.get_repo_name(input_filenames[j])
                publications_b = Base.get_publications(input_filenames[j])

                d, d_interpretation, t_statistic, pvalue = BaseStatistics.ttest_deltas(publications_a, publications_b)

                with open(output_filename, "a") as f:
                    f.write(f"{repository_a}\t{repository_b}\t{t_statistic}\t{pvalue}\t{d}\t{d_interpretation}\n")

    def ttest_corresponding_clusters(self, input_filenames, output_filename):
        """
        Performing Welch's t-test for the null hypothesis that the two 
        independent relative clusters of two repositories have identical 
        average (expected) values NOT assuming equal population variance.
        """

        # Add column header. 
        with open(output_filename, "a") as f:
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
        for i in range(0, len(input_filenames)-1):
            for j in range(i+1, len(input_filenames)):
                file_a = input_filenames[i]
                file_b = input_filenames[j]

                repo_a = Base.get_repo_name(file_a)
                repo_b = Base.get_repo_name(file_b)

                clusters_a = Base.get_clusters(file_a)
                clusters_b = Base.get_clusters(file_b)
                _, mapping_a, sorted_avg_a = Base.get_sorted_clusters(clusters_a)
                _, mapping_b, sorted_avg_b = Base.get_sorted_clusters(clusters_b)

                with open(output_filename, "a") as f:
                    for i in range(0, len(sorted_avg_a)):
                        cluster_a_num = mapping_a[sorted_avg_a[i]]
                        cluster_b_num = mapping_b[sorted_avg_b[i]]

                        d, d_interpretation, t_statistic, pvalue =\
                            BaseStatistics.ttest_total_citations(
                                clusters_a.get_group(cluster_a_num), 
                                clusters_b.get_group(cluster_b_num))

                        f.write(
                            f"{repo_a}\t"
                            f"{repo_b}\t"
                            f"{i}\t"
                            f"{i}\t"
                            f"{sorted_avg_a[i]}\t"
                            f"{sorted_avg_b[i]}\t"
                            f"{t_statistic}\t"
                            f"{pvalue}\t"
                            f"{d}\t"
                            f"{d_interpretation}\n")
