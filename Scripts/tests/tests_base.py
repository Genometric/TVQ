"""

"""

import pytest
import os
import py
import pandas as pd

from lib.base import Base, CLUSTERED_FILENAME_POSFIX
from lib.stats.base_statistics import BaseStatistics


class TestsBase(object):

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
    def publications(self, clustered_files):
        """

        """
        publications = []
        for filename in clustered_files:
            publications.append(pd.read_csv(filename, header=0, sep='\t'))
        return publications
