"""

"""

import pytest
import os
import py

from lib.base import Base, CLUSTERED_FILENAME_POSFIX
from lib.stats.base_statistics import BaseStatistics

from ..base_test_case import BaseTestCase

# Is the number of clusters in the test data.
TEST_DATA_CLUSTERS = 6


class TestBaseStatistics(BaseTestCase):
    """
    TODO
    """

    def test_get_clusters(self, clustered_files):
        """
        clustered_files is set using clustered_files fixture from TestsBase.

        TODO: modify test so that it runs for every file in clustered files. 
        """
        # Arrange
        filename = clustered_files[0]
        
        # Act
        clusters = BaseStatistics.get_clusters(filename)

        # Assert
        assert len(clusters.groups) == TEST_DATA_CLUSTERS


