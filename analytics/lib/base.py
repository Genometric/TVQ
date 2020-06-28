"""
TODO: Add doc string.
"""

import os
import numpy as np
import pandas as pd
from numpy import average


CLUSTERED_FILENAME_POSFIX = "_clustered"
CLUSTER_NAME_COLUMN_LABEL = "cluster_label"


class Base(object):
    """
    Base class containing common functionality to be used
    by the derived types. 
    """

    def run(self, input_path):
        raise NotImplementedError()

    def get_repo_name(filename):
        """
        Extracts repository name from the given filename.

        :type  filename:    string
        :param filename:    The filename from which the repository 
                            name should be extracted.

        :rtype:     string
        :return:    Repository name.
        """
        filename = os.path.basename(filename)
        return (os.path.splitext(filename)[0]).replace(CLUSTERED_FILENAME_POSFIX, "")

    def get_files(path, extension="csv", include_clustered_files=False):
        """
        Gets a list of absolute paths to files with given `extension` in the given `path`.

        :type   path:   string
        :param  path:   The path in which to search for the files.

        :type   extension:  string
        :param  extension:  Sets the extension of the files to search for in the given path.

        :type   include_clustered_files:    boolean
        :param  include_clustered_files:    If set to True, it will return only the files 
                                            with given extension whose filename ends with 
                                            `CLUSTERED_FILENAME_POSFIX`, otherwise if set
                                            to False (default).

        :rtype:     list<string>
        :return:    A list of absolute paths to files in the given path that match given criteria. 
        """
        files = []
        for root, dirpath, filenames in os.walk(path):
            for filename in filenames:
                if os.path.splitext(filename)[1] == ".csv":
                    is_clustered_file = \
                        os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX)

                    if (include_clustered_files and is_clustered_file) or \
                       (not include_clustered_files and not is_clustered_file):
                        files.append(os.path.join(root, filename))
        return files

    @staticmethod
    def get_publications(filename):
        """
        Reads publications from file with the given filename.

        :type   filename:   string
        :param  filename:   The name of the file from which publications should be read from.

        :rtype:     pandas.core.frame.DataFrame
        :return:    A dataframe that contains publications read from the given file.
        """
        return pd.read_csv(filename, header=0, sep='\t')

    @staticmethod
    def get_clusters(filename):
        """
        Returns a data-frame grouped-by cluster name.

        :type  filename:    string
        :param filename:    Name of the file to be read.
    
        :rtype:     pandas.core.groupby.generic.DataFrameGroupBy
        :return:    A pandas data-frame grouped-by cluster name.
        """
        dataframe = Base.get_publications(filename)
        return dataframe.groupby(CLUSTER_NAME_COLUMN_LABEL)

    def get_citations_headers(publications):
        """
        Extracts the headers of columns containing citations of publications 
        from the given data frame.

        This method assumes the consecutive columns with numerical headers
        (starting from the first numerical header to the next non-numerical header)
        contain the citation count of publications. The negative and positive
        numerical headers are assumed to be containing citations belong to 
        before and after the tool was added to the repository, respectively.

        :type   publications:   pandas.core.frame.DataFrame
        :param  publications:   The dataframe from which to extract citations count.

        :returns:
            - list<string> pre:     The headers of columns containing citation counts 
                                    before the tool was added to the repository.
            - list<string> post:    The headers of columns containing citation counts 
                                    after the tool was added to the repository.
        """
        headers = publications.columns.values.tolist()
        pre = []
        post = []
        s = False
        for header in headers:
            try:
                v = float(header)
            except ValueError:
                if s: break
                else: continue

            s = True
            if v < 0:
                pre.append(header)
            else:
                post.append(header)

        return pre, post

    def get_vectors(publications, citations_per_year=False):
        """

        :type   publications:   pandas.core.frame.DataFrame
        :param  publications:   The dataframe from which to extract citations vectors.

        :returns:
        """
        pre_headers, post_headers = Base.get_citations_headers(publications)

        # A list of two-dimensional lists, first dimension is pre counts
        # and second dimension contains post citation counts.
        citations = []

        sums = []

        deltas = []

        # Lists contain citation counts before (pre) and after (post)
        # a tool was added to the repository.
        avg_pre = []
        avg_pst = []

        pre_citations = []
        post_citations = []
        for index, row in publications.iterrows():
            pre_vals = row.get(pre_headers).values.tolist()
            post_vals = row.get(post_headers).values.tolist()

            pre_citations.append(pre_vals)
            post_citations.append(post_vals)

            citations.append(pre_vals + post_vals)
            sums.append(np.sum(pre_vals + post_vals))
            avg_pre.append(np.average(pre_vals))
            avg_pst.append(np.average(post_vals))

            if citations_per_year:
                deltas.append(abs(np.average(post_vals) - np.average(pre_vals)))
            else:
                deltas.append(abs(np.max(post_vals) - np.max(pre_vals)))

        return citations, pre_citations, post_citations, sums, avg_pre, avg_pst, deltas

    def get_sorted_clusters(publications):
        """
        Computes the average of all the citation counts of all the publications in every cluster.

        :type   publications:   pandas.core.groupby.generic.DataFrameGroupBy
        :param  publications:   A dataframe grouped by clusters. 

        :returns:
            -   list<float> mapping:    A sorted list of citation count average.

            -   dictionary  cluster_avg_mapping: 
                                        A dictionary where keys are the cluster numbers
                                        and values are the average of citation count of 
                                        publications in that cluster.

            -   dictionary  avg_cluster_mapping: 
                                        A dictionary where keys are the average of 
                                        citation count of publications in a cluster
                                        which is given by the value of that entry.
        """
        cluster_avg_mapping = {}
        avg_cluster_mapping = {}
        for k in publications.groups:
            citations, _, _, _, _, _, _ = Base.get_vectors(publications.get_group(k))
            avg = average(citations)
            cluster_avg_mapping[k] = avg
            avg_cluster_mapping[avg] = k
        
        sorted_avg = sorted(cluster_avg_mapping.values())

        return cluster_avg_mapping, avg_cluster_mapping, sorted_avg
