"""

"""

from ..base import Base, CLUSTER_NAME_COLUMN_LABEL

import pandas as pd

class BaseStatistics(Base):
    """
    TODO
    """

    @staticmethod
    def get_clusters(filename):
        """
        Returns a data-frame grouped-by cluster name.

        :type  filename:    string
        :param filename:    Name of the file to be read.
    
        :rtype:     pandas.core.groupby.generic.DataFrameGroupBy
        :return:    A pandas data-frame grouped-by cluster name.
        """
        dataframe = pd.read_csv(filename, header=0, sep='\t')
        return dataframe.groupby(CLUSTER_NAME_COLUMN_LABEL)
