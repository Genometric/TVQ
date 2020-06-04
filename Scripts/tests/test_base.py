import pytest
import os
import py
import numpy as np

from lib.base import Base, CLUSTERED_FILENAME_POSFIX

from .base_test_case import BaseTestCase


CSV_FILES_COUNT = 3

# The number of publications in the input test files.
PUBLICATION_COUNT = 25

# The number of columns in the test files that 
# represent citation counts before a tool was 
# added to the repository---excluding the year
# the tool was added (i.e., the 0 normalized date).
PRE_COL_COUNT = 10

# The number of columns in the tests files that
# represent citation counts after a tool was
# added to the repository---including the year
# the tool was added (i.e., the 0 normalized date).
POST_COL_COUNT = 11


class TestBase(BaseTestCase):

    @pytest.mark.parametrize(
        "input,expected", 
        [("name.csv", "name"),
         ("./a/path/name.csv", "name"),
         (f"./a/path/name{CLUSTERED_FILENAME_POSFIX}.csv", "name")])
    def test_get_repo_name(self, input, expected):
        """
        Asserts if the `get_repo_name` extracts repository name 
        from a given filename.
        """

        # Arrange, Act, Assert
        assert expected == Base.get_repo_name(input)


    def test_get_input_files(self, tmpdir):
        """
        Asserts if the `get_input_files` method reads only **input**
        (i.e., those without the cluster postfix) CSV files the given path.

        For instance, from a directory as the following, it should read 
        only `file_1.csv` and `file_2.csv`:

        ├─── file_1.csv
        ├─── file_2.csv
        ├─── file_1_clustered.csv
        └─── file_3.txt

        :type  tmpdir:  string
        :param tmpdir:  The ‘tmpdir’ fixture is a py.path.local object
                        which will provide a temporary directory unique 
                        to the test invocation.
        """

        # Arrange
        x = "content"
        for i in range(CSV_FILES_COUNT):
            tmpdir.join(f"file_{i}.csv").write(x)
        tmpdir.join(f"file_{i}{CLUSTERED_FILENAME_POSFIX}.csv").write(x)
        tmpdir.join(f"file_n.txt").write(x)

        # Act
        files = Base.get_input_files(tmpdir)

        # Assert
        assert len(files) == CSV_FILES_COUNT


    def test_get_input_files(self, tmpdir):
        """
        TODO: ... 
        """
        pass

    def test_get_citations(self, publications):
        """
        TODO:
        """
        # TODO: fix this so this test runs separately on different pubs. 
        for pub in publications:
            pre, post = Base.get_citations_headers(pub)

            assert len(pre) == PRE_COL_COUNT
            assert len(post) == POST_COL_COUNT

    def test_get_vectors(self, publications):
        # TODO: this test should be extended to work for every item in publications separately. 
        pub = publications[0]
        citations, pre_citations, post_citations, sums, avg_pre, avg_pst, deltas = Base.get_vectors(pub)

        # TODO: is there any better way asserting results?
        citations = np.array(citations)
        assert citations.shape == (PUBLICATION_COUNT, 2)
        assert citations[0][0][0] == 1.16691
        assert citations[0][1][2] == 19.37893
        assert np.average(citations[9][1]) == 57.985079999999996

        pre_citations = np.array(pre_citations)
        assert citations.shape == (PUBLICATION_COUNT, 2)
        assert pre_citations[1][2] == 7.68509
        assert np.average(pre_citations[5][1]) == 5.79521

