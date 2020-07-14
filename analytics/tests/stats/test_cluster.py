"""

"""

import os
from ..base_test_case import BaseTestCase
from lib.stats.cluster import Cluster
from lib.base import Base, CLUSTER_NAME_COLUMN_LABEL
import math
from os import listdir

class TestCluster(BaseTestCase):
    """
    """

    def test_if_all_expected_files_written(self, tmp_clustered_files):
        # Arrange
        tmpdir = tmp_clustered_files[0]
        repos = tmp_clustered_files[1]
        clustered_files = len(repos)

        # Act
        Cluster().run(tmpdir)

        # Assert
        # There should be one file for each repository, and
        # one another file that contains the clustering stats.
        assert len(listdir(tmpdir)) == clustered_files + 1

    def test_cluster_numbers(self, tmp_clustered_files):
        # Arrange
        tmpdir = tmp_clustered_files[0]
        repos = tmp_clustered_files[1]
        test_pubs = BaseTestCase.get_test_publications()

        # Act
        Cluster().run(tmpdir)
        files = Base.get_files(tmpdir, include_clustered_files=True)

        # Assert
        # TODO: This assertion is anti-pattern; must be re-implemented in a much better way.
        for file in files:
            publications = Base.get_publications(file)
            
            checked = False
            for idx, row in publications.iterrows():
                for test_pub in test_pubs:
                    for idx2, row2 in test_pub[0].iterrows():
                        if row.get("Tools") == row2.get("Tools"):
                            assert row.get(CLUSTER_NAME_COLUMN_LABEL) == row2.get(CLUSTER_NAME_COLUMN_LABEL)
                            checked = True
            
            assert checked == True
