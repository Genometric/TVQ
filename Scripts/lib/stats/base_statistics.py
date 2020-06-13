"""

"""

from ..base import Base, CLUSTER_NAME_COLUMN_LABEL

import pandas as pd

from statistics import mean

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
