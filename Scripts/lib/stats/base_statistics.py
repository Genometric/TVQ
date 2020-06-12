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
    def get_mean_of_raw_citations(citations, pre=True):
        col = SUM_PRE_CITATIONS_COLUMN_LABEL if pre else SUM_POST_CITATIONS_COLUMN_LABEL
        return mean(citations[col])
