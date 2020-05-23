import os


CLUSTERED_FILENAME_POSFIX = "_clustered"
CLUSTER_NAME_COLUMN_LABEL = "cluster_label"


class Base(object):
    """
    Base class containing common functionality to be used
    by the derived types. 
    """

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


    def get_input_files(input_path):
        """

        """
        files = []
        for root, dirpath, filenames in os.walk(input_path):
            for filename in filenames:
                if os.path.splitext(filename)[1] == ".csv" and \
                   not os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                    files.append(os.path.join(root, filename))
        return files

