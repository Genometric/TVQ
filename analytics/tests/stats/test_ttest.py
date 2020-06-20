"""

"""

from ..base_test_case import BaseTestCase
from lib.stats.ttest import TTest


class TestTTest(BaseTestCase):
    """
    """
    
    def test_if_all_expected_files_written(self, tmp_clustered_files):
        tmpdir = tmp_clustered_files[0]
        repos = tmp_clustered_files[1]
        
        ttest = TTest()
        ttest.run(tmpdir)

        