
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

    def __init__(self):
        input_path = None

    def run(self, input_path):
        """
        Executes a flow of computing t-test on
        files available from the input path.
        """
        self.input_path = input_path

        filenames = Base.get_files(input_path, include_clustered_files=True)

        self.ttest_avg_pre_post(filenames, os.path.join(input_path, "paired_ttest_avg_pre_post.txt"))
        self.ttest_delta(filenames, os.path.join(input_path, "one_sample_ttest.txt"))
        self.ttest_deltas(filenames, os.path.join(input_path, "ttest_repositories.txt"))

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

    def ttest_avg_pre_post(self, input_filenames, output_filename):
        with open(output_filename, "w") as f:
            f.write("Repository\tAverage Pre Citations\tAverage Post Citations\tGrowth\tt-Statistic\tp-value\tCohen's d\tInterpretation\n")

        for filename in input_filenames:
            repository = Base.get_repo_name(filename)
            publications = Base.get_publications(os.path.join(self.input_path, filename))

            d, d_interpretation, t_statistic, pvalue = BaseStatistics.ttest_avg_pre_post(publications)
            avg_pre, avg_post = self.get_avg_pre_post(publications)
            growth = ((avg_post - avg_pre) / avg_pre) * 100.0
            with open(output_filename, "a") as f:
                f.write(f"{repository}\t{avg_pre}\t{avg_post}\t{growth}%\t{t_statistic}\t{pvalue}\t{d}\t{d_interpretation}\n")

    def ttest_delta(self, input_filenames, output_filename):
        with open(output_filename, "w") as f:
            f.write("Repository\tAverage Pre Citations\tAverage Post Citations\tGrowth\tt-Statistic\tp-value\tCohen's d\tInterpretation\n")

        for filename in input_filenames:
            repository = Base.get_repo_name(filename)
            publications = Base.get_publications(os.path.join(self.input_path, filename))

            d, d_interpretation, t_statistic, pvalue = BaseStatistics.ttest_delta(publications)
            avg_pre, avg_post = self.get_avg_pre_post(publications)
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
                publications_a = Base.get_publications(os.path.join(self.input_path, input_filenames[i]))

                repository_b = Base.get_repo_name(input_filenames[j])
                publications_b = Base.get_publications(os.path.join(self.input_path, input_filenames[j]))

                d, d_interpretation, t_statistic, pvalue = BaseStatistics.ttest_deltas(publications_a, publications_b)

                with open(output_filename, "a") as f:
                    f.write(f"{repository_a}\t{repository_b}\t{t_statistic}\t{pvalue}\t{d}\t{d_interpretation}\n")

    def paired_ttest(self, tools):
        citations, _, _, sums, avg_pre, avg_post, _ = get_vectors(tools)
        t_statistic, pvalue = ttest_rel(avg_pre, avg_post)
        return cohen_d(avg_pre, avg_post), (abs(t_statistic), pvalue)

    # TDOO: move this method to BaseStatistics
    def get_avg_pre_post(self, publications):
        return mean(publications[SUM_PRE_CITATIONS_COLUMN_LABEL]), mean(publications[SUM_POST_CITATIONS_COLUMN_LABEL])

