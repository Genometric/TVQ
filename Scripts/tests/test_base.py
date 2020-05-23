import pytest
import os
import py

from lib.base import Base, CLUSTERED_FILENAME_POSFIX


CSV_FILES_COUNT = 3


class TestBase(object):

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
        only `aaa.csv` and `bbb.csv`:

        ├─── aaa.csv
        ├─── bbb.csv
        ├─── aaa_clustered.csv
        └─── ccc.txt

        :type  tmpdir:  string
        :param tmpdir:  The ‘tmpdir’ fixture is a py.path.local object
                        which will provide a temporary directory unique 
                        to the test invocation.
        """

        # Arrange
        for i in range(CSV_FILES_COUNT):
            filename = tmpdir.join(f"{i}.csv")
            filename.write("content")

        # Act
        files = Base.get_input_files(tmpdir)

        # Assert
        assert len(files) == CSV_FILES_COUNT
