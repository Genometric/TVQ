"""

"""

import os
from ..base_test_case import BaseTestCase
from lib.stats.cluster import Cluster
from lib.base import Base
import math

class TestCluster(BaseTestCase):
    """
    """

    def test_if_all_expected_files_written(self, tmp_clustered_files):
        # Arrange
        tmpdir = tmp_clustered_files[0]
        repos = tmp_clustered_files[1]

        # Act
        Cluster().run(tmpdir)

        # Assert