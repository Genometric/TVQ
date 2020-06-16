"""

"""

from ..base import Base, CLUSTER_NAME_COLUMN_LABEL

import pandas as pd

from statistics import mean

from scipy.stats import ttest_rel, ttest_ind, pearsonr, ttest_1samp
from math import sqrt
from numpy import std

SUM_PRE_CITATIONS_COLUMN_LABEL = "SumPreRawCitations"
SUM_POST_CITATIONS_COLUMN_LABEL = "SumPostRawCitations"

class BaseStatistics(Base):
    """
    TODO
    """
    
    @staticmethod
    def get_mean_of_raw_citations(publications, pre=True):
        """
        Computes the mean of citation counts of all the publications
        before or after (the `pre` argument) they were added to the repository. 

        :type   publications:   pandas.core.frame.DataFrame
        :param  publications:   A dataframe containing the publications.

        :type   pre:    boolean
        :param  pre:    If set to true   or false, respectively returns the mean of  
                        citations before or after the tools were added to the repository.

        :rtype:     float
        :return:    The mean of citations.
        """
        col = SUM_PRE_CITATIONS_COLUMN_LABEL if pre else SUM_POST_CITATIONS_COLUMN_LABEL
        return mean(publications[col])

    @staticmethod
    def paired_ttest(publications):
        """
        Calculates the t-test on average citations before and after tools were
        added to the repository; it assumes the populations are related.

        :type   publications:   pandas.core.frame.DataFrame
        :param  publications:   A dataframe containing the publications. 
        
        :return returns Cohen's d, it's interpretation, t-statistic of the t-test and it's p-value.
        """
        citations, _, _, sums, avg_pre, avg_post, _ = Base.get_vectors(publications)
        t_statistic, pvalue = ttest_rel(avg_pre, avg_post)
        return (BaseStatistics.cohen_d(avg_pre, avg_post)) + (abs(t_statistic), pvalue)

    @staticmethod
    def one_sample_ttest(publications, population_mean=0.0):
        _, _, _, _, _, _, delta = Base.get_vectors(publications)
        t_statistic, pvalue = ttest_1samp(x, population_mean)
        return (BaseStatistics.cohen_d(delta, population_mean=population_mean)) + (abs(t_statistic), pvalue)

    @staticmethod
    def cohen_d(x, y=None, population_mean=0.0):
        if len(x) < 2 or (y and len(y) < 2):
            return float('NaN'), "NaN"

        if y:
            # Cohen's d is computed as explained in the following link:
            # https://stackoverflow.com/a/33002123/947889
            d = len(x) + len(y) - 2
            cohen_d = (mean(x) - mean(y)) / sqrt(((len(x) - 1) * std(x) ** 2 + (len(y) - 1) * std(y) ** 2) / d)
        else:
            cohen_d = (mean(x) - population_mean) / std(x)

        cohen_d = abs(cohen_d)

        # This interpretation is based on the info available on Wikipedia:
        # https://en.wikipedia.org/wiki/Effect_size#Cohen.27s_d
        if cohen_d >= 0.00 and cohen_d < 0.10:
            msg = "Very small"
        if cohen_d >= 0.10 and cohen_d < 0.35:
            msg = "Small"
        if cohen_d >= 0.35 and cohen_d < 0.65:
            msg = "Medium"
        if cohen_d >= 0.65 and cohen_d < 0.90:
            msg = "Large"
        if cohen_d >= 0.90:
            msg = "Very large"

        return cohen_d, msg + " effect size"

