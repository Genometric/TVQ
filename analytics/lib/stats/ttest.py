
from .base_statistics import BaseStatistics

class TTest(BaseStatistics):

    def run(self, input_path):
        """
        Executes a pre-defined flow of computing t-test on
        files available from the input path.
        """
        filenames = self.get_files(input_path, include_clustered_files=True)

        repo_ttest_filename = os.path.join(root, "paired_ttest_avg_pre_post.txt")
        if os.path.isfile(repo_ttest_filename):
            os.remove(repo_ttest_filename)
        with open(repo_ttest_filename, "a") as f:
            f.write("Repository\tAverage Pre Citations\tAverage Post Citations\tGrowth\tt-Statistic\tp-value\tCohen's d\tInterpretation\n")

        for filename in filenames:
            ttest_repository(os.path.join(root, filename), repo_ttest_filename)
