"""

"""

import pytest
import os
import py
import pandas as pd
import numpy as np
from numpy import average
from numpy import max

from lib.base import Base, CLUSTERED_FILENAME_POSFIX, CLUSTER_NAME_COLUMN_LABEL
from lib.stats.base_statistics import BaseStatistics


class BaseTestCase(object):

    def get_test_publications():
        """
        Returns two tuples of test publications, each tuple contains 
        input and expected values.
        """

        # The _inclusive_ index of the first column containing citations 
        # before a tool was added to the repository.
        i = 8

        # The _exclusive_ index of the last column containing citations
        # before a tool was added to the repository. And
        # the _inclusive_ index of the first column containing citations
        # before a tool was added to the repository.
        j = 10

        # The _exclusive_ index of the last column containing citations
        # after a tool was added to the repository.
        k = 13

        pubs_a = []
        pubs_b = []

        # Few notes:
        # - The last column is used to assert if only the contiguous 
        #   numerical headers are considered as the columns containing citations.
        #   Therefore, this column should not be read as a column that contains citations.
        # - Few variables are used in the following for column headers, the 
        #   main motivation was to make the table more visually intuitive.
        # - All the values in the table are semi-random; i.e., they are
        #   randomly generated so to follow the logic of each column.
        # - The cluster labels assigned to each publication are determined
        #   using hierarchical clustering with single linkage and Euclidean 
        #   distance.

        tids = "ToolIDs"
        g = "GainScore"
        ginp = "CitationGrowthOnInputData"
        gnor = "CitationGrowthOnNormalizedData"
        sp = "SumPreRawCitations"
        so = "SumPostRawCitations"
        c = CLUSTER_NAME_COLUMN_LABEL

        header =      ["id", "Tools", tids,    g,    ginp, gnor, sp, so, "-1.0", "-0.5", "0.0", "0.5", "1.0", c, "1.1"]
        pubs_a.append(["01", "t1",    "11",    1.04, 11.1, 43.9, 12, 34, 0.0000, 0.1000, 0.200, 0.300, 0.400, 0, 12345])
        pubs_a.append(["02", "t2;t3", "12;13", 5.71, 10.9, 23.2, 11, 44, 1.0000, 2.0000, 3.000, 4.444, 5.000, 1, 45678])
        pubs_a.append(["03", "t4",    "14",    79.0, 99.9, 33.2, 56, 98, 0.1200, 0.2000, 0.300, 0.400, 0.500, 0, 78900])

        pubs_b.append(["01", "t5;t6", "25;26", 2.01, 1.21, 1.33, 23, 33, 0.1111, 0.2222, 0.333, 0.444, 0.555, 0, 88800])
        pubs_b.append(["02", "t7",    "27",    2.99, 2.33, 3.44, 12, 23, 0.0123, 0.0456, 0.078, 0.099, 0.100, 0, 88800])
        pubs_b.append(["03", "t8",    "28",    8.99, 9.88, 2.33, 33, 44, 2.0000, 3.0000, 4.000, 5.000, 6.000, 1, 88800])

        pubs_1 = pd.DataFrame(pubs_a, columns=header)
        pubs_2 = pd.DataFrame(pubs_b, columns=header)

        # The cast from a numpy array to python list in the following
        # is used to avoid list comparison issues when comparing a list
        # with a numpy array.
        def get_expected_values(pubs):
            # The following lines create a dictionary (d) where the 
            # key is the cluster number, and the value is the average 
            # of all the citation counts of all the publications in
            # that cluster.
            g_pubs = pubs.groupby(CLUSTER_NAME_COLUMN_LABEL)
            d = {x:
                 # The average of the average of citations.
                 average(
                     # The average of citations.
                     [average(r.get(header[i:k]).values.tolist()) 
                     for _, r in g_pubs.get_group(x).iterrows()])
                for x in g_pubs.groups}

            pubs = pubs.values
            return {
                "citations":    [x[i:k].tolist() for x in pubs],
                "pre":          [x[i:j].tolist() for x in pubs],
                "post":         [x[j:k].tolist() for x in pubs],
                "sums":         [sum(x[i:k]) for x in pubs],
                "avg_pre":      [average(x[i:j].tolist()) for x in pubs],
                "avg_post":     [average(x[j:k].tolist()) for x in pubs],
                "deltas":       [max(x[j:k]) - max(x[i:j]) for x in pubs],
                "cluster_avg":  d,
                "avg_sum_pre":  average([x[6] for x in pubs]),
                "avg_sum_post": average([x[7] for x in pubs])}

        # Each tuple in the following list is separate input and 
        # expected value for a test. Hence, two tuples will cause 
        # a test to run twice, once for the first tuple, and once
        # for the second tuple.
        return \
            (pubs_1, get_expected_values(pubs_1)),\
            (pubs_2, get_expected_values(pubs_2))

    @pytest.fixture(params=get_test_publications(), scope="session")
    def test_publications(self, request):
        return request.param

    @pytest.fixture(scope="session")
    def tmp_files(self, tmpdir_factory):
        """

        """
        test_pubs = BaseTestCase.get_test_publications()
        for repo in test_pubs:
            repo[0] = repo[0].drop(CLUSTER_NAME_COLUMN_LABEL, 1)

        tmpdir, filenames = self._write_tmp_files(test_pubs, tmpdir_factory)
        return tmpdir, filenames

    @pytest.fixture(scope="session")
    def tmp_clustered_files(self, tmpdir_factory):
        """
        Returns absolute path to files that contain publications as
        defined in `get_test_publications`. 

        Use this method to create temporary test files. 
        """
        test_pubs = BaseTestCase.get_test_publications()
        tmpdir, filenames = self._write_tmp_files(test_pubs, tmpdir_factory)
        return tmpdir, filenames

    def _write_tmp_files(self, publications, tmpdir_factory):
        tmpdir = tmpdir_factory.mktemp("clustered_files")

        c = 0
        filenames = []
        for repo in publications:
            c += 1
            filename = os.path.join(tmpdir, f"repo_{c}{CLUSTERED_FILENAME_POSFIX}.csv")
            repo[0].to_csv(filename, sep="\t", encoding='utf-8')
            filenames.append({"filename": filename, "exp_values": repo[1]})

        return tmpdir, filenames

    @pytest.fixture(scope="session")
    def clustered_files(self, tmpdir_factory):
        """
        Gets persisted test files in the `test_data` path.
        """

        # The goal is to get the absolute path to the 
        # directory that contains test data. So first
        # it gets the absolute path to the project root 
        # and then appends `test_data` to it. 
        input_path = os.path.join(os.path.abspath(os.getcwd()), "analytics", "test_data", "")
        
        filenames = []
        # TODO: replace this with the method from base that returns clustered files.
        for root, dirpath, files in os.walk(input_path):
            for filename in files:
                if os.path.splitext(filename)[1] == ".csv" and \
                        os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                    filenames.append(os.path.join(input_path, filename))
        return filenames

    @pytest.fixture(scope="session")
    def test_publications_from_files(self, clustered_files):
        """

        """
        publications = []
        for filename in clustered_files:
            publications.append(pd.read_csv(filename, header=0, sep='\t'))
        return publications

    @staticmethod
    def assert_lists_equal(l1, l2):
        """
        Asserts if two lists are **almost** equal, with relative 
        tolerance of 1e-5 and absolute tolerance of 1e-8. 
        """
        return np.allclose(l1, l2)

    @staticmethod
    def assert_str_list_equal(l1, l2):
        for i in range(len(l1)):
            if l1[i] != l2[i]:
                return False
        return True
