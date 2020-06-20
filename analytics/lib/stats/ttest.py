
import os

from .base_statistics import BaseStatistics
from ..base import Base

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
