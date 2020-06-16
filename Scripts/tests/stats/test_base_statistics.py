"""

"""

import pytest
import os
import py

from lib.base import Base, CLUSTERED_FILENAME_POSFIX, CLUSTER_NAME_COLUMN_LABEL
from lib.stats.base_statistics import BaseStatistics

from scipy.stats import ttest_rel, ttest_ind, pearsonr, ttest_1samp
from statistics import mean

from math import sqrt

from numpy import std

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

    def test_paired_ttest(self, test_publications):
        # Arrange
        input = test_publications[0]
        exp = test_publications[1]
        avg_pre = exp["avg_pre"]
        avg_post = exp["avg_post"]
        exp_t_statistic, exp_pvalue = ttest_rel(avg_pre, avg_post)

        # pooled standard deviation.
        pooled_sd = sqrt((pow(std(avg_post), 2) + pow(std(avg_pre), 2))/2.0)
        exp_cohens_d = (mean(avg_post) - mean(avg_pre)) / pooled_sd

        if exp_cohens_d >= 0.00 and exp_cohens_d < 0.10:
            msg = "Very small"
        if exp_cohens_d >= 0.10 and exp_cohens_d < 0.35:
            msg = "Small"
        if exp_cohens_d >= 0.35 and exp_cohens_d < 0.65:
            msg = "Medium"
        if exp_cohens_d >= 0.65 and exp_cohens_d < 0.90:
            msg = "Large"
        if exp_cohens_d >= 0.90:
            msg = "Very large"
        exp_interpretation = msg +  " effect size"

        # Act
        cohens_d, interpretation, t_statistic, pvalue = BaseStatistics.paired_ttest(input)

        # Assert
        assert pvalue == exp_pvalue
        assert cohens_d == exp_cohens_d
        assert t_statistic == abs(exp_t_statistic)
        assert interpretation == exp_interpretation

    def test_one_sample_ttest(self, test_publications):
        # Arrange
        input = test_publications[0]
        exp = test_publications[1]
        delta = exp["deltas"]
        exp_t_statistic, exp_pvalue = ttest_1samp(delta, 0.0)

        # Act
        cohens_d, interpretation, t_statistic, pvalue = BaseStatistics.one_sample_ttest(input)

        # Assert
        assert pvalue == exp_pvalue
        assert t_statistic == exp_t_statistic

