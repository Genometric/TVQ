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

    def _get_cohens_d_and_interpretation(self, x, y=None, theoretical_mean=0.0):
        """
        Computes Cohen's d on one or two groups.

        This method is manually tested against online Cohen's d calculator.

        :type  x:  list
        :param x:  First group. 

        :type  y:  list
        :param y:  Second group (optional).

        :type  theoretical_mean:  float
        :param theoretical_mean:  Population mean before treatment. Used when y is not given. 

        :return returns Cohen's d, it's interpretation, t-statistic of the t-test and it's p-value.
        """
        if y:
            # pooled standard deviation.
            pooled_std = sqrt((pow(std(y), 2) + pow(std(x), 2))/2.0)
            d = (mean(y) - mean(x)) / pooled_std
        else:
            d = (mean(x) - theoretical_mean) / std(x)

        if d >= 0.00 and d < 0.10:
            msg = "Very small"
        if d >= 0.10 and d < 0.35:
            msg = "Small"
        if d >= 0.35 and d < 0.65:
            msg = "Medium"
        if d >= 0.65 and d < 0.90:
            msg = "Large"
        if d >= 0.90:
            msg = "Very large"

        return d, msg + " effect size"

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

    def test_ttest_avg_pre_post(self, test_publications):
        # Arrange
        input = test_publications[0]
        exp = test_publications[1]
        avg_pre = exp["avg_pre"]
        avg_post = exp["avg_post"]
        exp_t_statistic, exp_pvalue = ttest_rel(avg_pre, avg_post)
        exp_cohens_d, exp_interpretation = self._get_cohens_d_and_interpretation(avg_pre, avg_post)

        # Act
        cohens_d, interpretation, t_statistic, pvalue = BaseStatistics.ttest_avg_pre_post(input)

        # Assert
        assert pvalue == exp_pvalue
        assert t_statistic == abs(exp_t_statistic)
        assert cohens_d == exp_cohens_d
        assert interpretation == exp_interpretation

    def test_ttest_delta(self, test_publications):
        # Arrange
        input = test_publications[0]
        exp = test_publications[1]
        delta = exp["deltas"]
        theoretical_mean=0.0
        exp_t_statistic, exp_pvalue = ttest_1samp(delta, theoretical_mean)
        exp_cohens_d, exp_interpretation = self._get_cohens_d_and_interpretation(delta, theoretical_mean)

        # Act
        cohens_d, interpretation, t_statistic, pvalue = BaseStatistics.ttest_delta(input)

        # Assert
        assert pvalue == exp_pvalue
        assert t_statistic == exp_t_statistic
        assert cohens_d == exp_cohens_d
        assert interpretation == exp_interpretation
