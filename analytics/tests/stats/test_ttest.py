"""

"""

import os
from ..base_test_case import BaseTestCase
from lib.stats.ttest import TTest
from lib.base import Base


class TestTTest(BaseTestCase):
    """
    """

    def test_if_all_expected_files_written(self, tmp_clustered_files):
        # Arrange
        tmpdir = tmp_clustered_files[0]
        repos = tmp_clustered_files[1]

        # Act
        TTest().run(tmpdir)

        # Assert

    def test_ttest_delta(self, tmp_clustered_files):
        # Arrange
        tmpdir, inputs = tmp_clustered_files

        input_files = [x["filename"] for x in inputs]
        exp_avg_pre = [x["exp_values"]["avg_sum_pre"] for x in inputs]
        exp_avg_post = [x["exp_values"]["avg_sum_post"] for x in inputs]
        output_file = os.path.join(tmpdir, "test_ttest_delta_output.csv")

        # Pre-act Assert
        # The output file should not exist before the test runs.
        assert os.path.exists(output_file) == False

        # Act
        TTest().ttest_delta(input_files, output_file)

        # Assert
        assert os.path.exists(output_file)

        output_info = Base.get_publications(output_file)

        # Check if files have header
        assert BaseTestCase.assert_str_list_equal(list(output_info), TTest.TTEST_HEADER)

        assert BaseTestCase.assert_lists_equal(output_info["Average Pre Citations"], exp_avg_pre)

        # ... TODO: more assertion ... 
        # ... (1) check if files have header
        # ... (2) check the value of avg post citations
        # ... (3) check t-statistic and other columns; no need to check 
        #         for actual values, only check if the values exist and 
        #         are not 0.0 or NaN.
