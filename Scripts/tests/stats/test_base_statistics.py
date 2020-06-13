"""

"""

import pytest
import os
import py

from lib.base import Base, CLUSTERED_FILENAME_POSFIX, CLUSTER_NAME_COLUMN_LABEL
from lib.stats.base_statistics import BaseStatistics

from ..base_test_case import BaseTestCase


class TestBaseStatistics(BaseTestCase):
    """
    TODO
    """
    
    def test_get_mean_of_raw_citations(self, test_publications):
        # Arrange
        input = test_publications[0]
        expected_pre = test_publications[1]["avg_sum_pre"]
        expected_pst = test_publications[1]["avg_sum_post"]

        # Act
        pre = BaseStatistics.get_mean_of_raw_citations(input, True)
        pst = BaseStatistics.get_mean_of_raw_citations(input, False)
        
        # Assert
        assert pre == expected_pre
        assert pst == expected_pst
