"""

"""

import os
from ..base_test_case import BaseTestCase
from lib.stats.cluster import Cluster
from lib.base import Base
import math
from os import listdir

class TestCluster(BaseTestCase):
    """
    """

    def test_if_all_expected_files_written(self, tmp_clustered_files):
        # Arrange
        tmpdir = tmp_clustered_files[0]
        repos = tmp_clustered_files[1]
        clustered_files = len(repos)

        # Act
        Cluster().run(tmpdir)

        # Assert
        # There should be one file for each repository, and
        # one another file that contains the clustering stats.
        assert len(listdir(tmpdir)) == clustered_files + 1
