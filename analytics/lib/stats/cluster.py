"""

"""

from .base_statistics import BaseStatistics
from ..base import Base

class Cluster(BaseStatistics):
    
    def __init__(self):
        self.cluster_count = None

    def run(self, input_path, cluster_count=None):
        """
        Executes a flow of clustering publications
        in files available from the given input path.
        """
        self.cluster_count = cluster_count
        input_files = Base.get_files(input_path, include_clustered_files=False)

        for filename in input_files:
            self._cluster(filename)

    def _cluster(self, filename):
        pass