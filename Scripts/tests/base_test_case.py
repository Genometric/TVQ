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
        i = 2

        # The _exclusive_ index of the last column containing citations
        # before a tool was added to the repository. And
        # the _inclusive_ index of the first column containing citations
        # before a tool was added to the repository.
        j = 4

        # The _exclusive_ index of the last column containing citations
        # after a tool was added to the repository.
        k = 7

        pubs_a = []
        pubs_b = []

        # Note, the last column is used to assert if only the contiguous 
        # numerical headers are considered as those containing citations.
        # Therefore, this column should not be read as containing citations.

        header =      ["id", "name", "-1.0", "-0.5", "0.0", "0.5", "1.0", CLUSTER_NAME_COLUMN_LABEL, "1.1"]
        pubs_a.append(["01", "p1_1", 0.0000, 0.1000, 0.200, 0.300, 0.400, 1, 123])
        pubs_a.append(["02", "p1_2", 0.1200, 0.2000, 0.300, 0.400, 0.500, 1, 456])
        pubs_a.append(["03", "p1_3", 1.0000, 2.0000, 3.000, 4.444, 5.000, 2, 789])

        pubs_b.append(["01", "p2_1", 0.1111, 0.2222, 0.333, 0.444, 0.555, 1, 888])
        pubs_b.append(["02", "p2_2", 0.0123, 0.0456, 0.078, 0.099, 0.100, 1, 888])
        pubs_b.append(["03", "p2_3", 2.0000, 3.0000, 4.000, 5.000, 6.000, 2, 888])

        pubs_1 = pd.DataFrame(pubs_a, columns=header)
        pubs_2 = pd.DataFrame(pubs_b, columns=header)

        # The cast from a numpy array to python list in the following
        # is used to avoid list comparison issues when comparing a list
        # with a numpy array.
        def get_expected_values(pubs):
            return {
                "citations":[x[i:k].tolist() for x in pubs],
                "pre":      [x[i:j].tolist() for x in pubs],
                "post":     [x[j:k].tolist() for x in pubs],
                "sums":     [sum(x[i:k]) for x in pubs],
                "avg_pre":  [average(x[i:j].tolist()) for x in pubs],
                "avg_post": [average(x[j:k].tolist()) for x in pubs],
                "deltas":   [max(x[j:k]) - max(x[i:j]) for x in pubs]}

        # Each tuple in the following list is separate input and 
        # expected value for a test. Hence, two tuples will cause 
        # a test to run twice, once for the first tuple, and once
        # for the second tuple.
        return [
            (pubs_1, get_expected_values(pubs_1.values)), 
            (pubs_2, get_expected_values(pubs_2.values))]

    @pytest.fixture(params=get_test_publications(), scope="session")
    def test_publications(self, request):
        return request.param

    @pytest.fixture(scope="session")
    def tmp_clustered_files(self, tmpdir_factory):
        """
        Use this method to create temporary test files. 
        For this, first create a tmp path as:

            tmpdir = tmpdir_factory.mktemp("clustered_files")

        then create as many files needed inside that path. 
        These files will be available for all the test
        methods of this class. 
        """
        pass

    @pytest.fixture(scope="session")
    def clustered_files(self, tmpdir_factory):
        """
        Gets persisted test files in the `test-data` path.
        """

        # The goal is to get the absolute path to the 
        # directory that contains test data. So first
        # it gets the absolute path to the project root 
        # and then appends `test-data` to it. 
        input_path = os.path.join(os.path.abspath(os.curdir), "test-data")
        
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
        Asserts if two lists are equal, returns true if they, false otherwise.
        """
        return all([x == y for x,y in zip(l1, l2)])
