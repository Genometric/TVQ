"""

"""

import os
from ..base_test_case import BaseTestCase
from lib.stats.ttest import TTest


class TestTTest(BaseTestCase):
    """
    """
    
    def test_if_all_expected_files_written(self, tmp_clustered_files):
        tmpdir = tmp_clustered_files[0]
        repos = tmp_clustered_files[1]
        
        ttest = TTest(tmpdir)
        ttest.run()

    def test_ttest_delta(self, tmp_clustered_files):
        # Arrange
        tmpdir = tmp_clustered_files[0]
        input_files = [x[0] for x in tmp_clustered_files[1]]
        output_file = os.path.join(tmpdir, "test_ttest_delta_output.csv")

        # Pre-act Assert
        # The output file should not exist before the test runs.
        assert os.path.exists(output_file) == False
        
        # Act
        ttest = TTest(tmpdir)
        ttest.ttest_delta(input_files, output_file)

        # Assert
        assert os.path.exists(output_file)
        