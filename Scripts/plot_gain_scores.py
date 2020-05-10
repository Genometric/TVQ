import os
import sys
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from t_test_clustered_data import get_sorted_clusters, get_clusters, CLUSTERED_FILENAME_POSFIX


GROWTH_COLUMN_HEADER = "GainScore"


def get_cluster_label(cluster_count, index):
    if cluster_count == 3:
        return ["Low-cited Publications", "Mid-cited Publications", "Highly-cited Publications"][index]
    elif cluster_count == 2:
        return ["Low-cited Publications", "Highly-cited Publications"][index]
    else:
        return f"Cluster {i+1}"


def get_growthes(tools, growth_column_header=GROWTH_COLUMN_HEADER):
    growthes = []
    for index, row in tools.iterrows():
        growthes.append(row.get(growth_column_header))
    return growthes


def plot(ax, filename, growthes, header=None, x_axis_label=None, y_axis_label=None):
    ax.hist(growthes, density=True, bins=24) # setting density to False will show count, and True will show probability.
    ax.set_yscale('log')
    #ax.yaxis.set_major_formatter(mticker.ScalarFormatter()) # comment this when density=True
    ##ax.yaxis.set_minor_formatter(mticker.ScalarFormatter())

    if header:
        ax.set_title(header)

    if x_axis_label:
        ax.set_xlabel(x_axis_label)

    if y_axis_label:
        ax.set_ylabel(y_axis_label)


def set_plot_style(nrows, ncols, fig_height=12, fig_width=12):
    sns.set()
    sns.set_context("paper")
    sns.set_style("darkgrid")
    fig, axes = plt.subplots(nrows=nrows, ncols=ncols, figsize=(fig_width, fig_height), dpi=300)
    plt.subplots_adjust(wspace=0.2, hspace=0.2)
    return fig, axes


def run(input_path):
    files = []
    for root, dirpath, filenames in os.walk(input_path):
        for filename in filenames:
            if os.path.splitext(filename)[1] == ".csv" and \
            os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                files.append(filename)

    x_axis_label = "\n Citation Growth"
    y_axis_label = "Probability \n"

    clusters = get_clusters(os.path.join(root, files[0]))
    cluster_count = len(clusters.groups)

    fig, ax = set_plot_style(len(files), cluster_count)
    row_counter = -1
    for filename in files:
        print(f">>> Processing file: {filename}")
        row_counter += 1
        filename_without_extension = os.path.splitext(filename)[0]
        repository_name = filename_without_extension.replace(CLUSTERED_FILENAME_POSFIX, "")
        clusters = get_clusters(os.path.join(root, filename))

        col_counter = -1
        keys, mappings = get_sorted_clusters(clusters)
        for i in range(0, len(keys)):
            print(f"\t- Processing cluster {i}")
            header = get_cluster_label(cluster_count, i)
            col_counter += 1
            growthes = get_growthes(clusters.get_group(mappings[keys[i]]))
            plot(
                ax[row_counter] if cluster_count == 1 else ax[row_counter][col_counter],
                filename_without_extension,
                growthes,
                header=header if row_counter == 0 else None,
                x_axis_label=x_axis_label if row_counter == len(keys) else None,
                y_axis_label=f"{repository_name} \n \n {y_axis_label}" if col_counter == 0 else None)
    
    last_ax = ax[row_counter] if cluster_count == 1 else ax[row_counter][col_counter]
    handles, labels = last_ax.get_legend_handles_labels()

    image_file = os.path.join(input_path, 'gain_scores_clustered.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()

    fig, ax = set_plot_style(1, len(files), fig_height=3, fig_width=16)
    col_counter = -1
    for filename in files:
        print(f">>> Processing file: {filename}")
        col_counter += 1
        filename_without_extension = os.path.splitext(filename)[0]
        repository_name = filename_without_extension.replace(CLUSTERED_FILENAME_POSFIX, "")

        tools = pd.read_csv(os.path.join(root, filename), header=0, sep='\t')
        growthes = get_growthes(tools)
        plot(
            ax[col_counter],
            filename_without_extension,
            growthes,
            header=repository_name,
            x_axis_label=x_axis_label,
            y_axis_label=f"\n {y_axis_label}" if col_counter == 0 else None)
        
    handles, labels = ax[col_counter].get_legend_handles_labels()

    image_file = os.path.join(input_path, 'gain_scores.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Missing input path.")
        exit()

    run(sys.argv[1])
