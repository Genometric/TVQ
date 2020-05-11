import os
import sys
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from t_test_clustered_data import CLUSTERED_FILENAME_POSFIX
from plot_gain_scores import get_growthes
from matplotlib.ticker import PercentFormatter, FormatStrFormatter


# When the histogram plots `density`, there should not 
# be any significant difference between `CitationGrowthOnNormalizedData`
# and `CitationGrowthOnInputData`.
GROWTH_COLUMN_HEADER = "CitationGrowthOnNormalizedData"

COLOR_PALETTES = {"Bioconda": "#3498db", "Bioconductor": "#feb308", "BioTools": "#34495e", "ToolShed": "#41aa33"}


def aggregate(input, min, max):
    # TODO: there must be some method (e.g., in numpy) to do this aggregation. 
    aggregated = []
    for item in input:
        if item < min:
            aggregated.append(min)
        elif item > max:
            aggregated.append(max)
        else:
            aggregated.append(item)
    return aggregated


def plot(ax, growthes, labels, colors, plot_density):
    counts, bins, patches = ax.hist(growthes, label=labels, density=plot_density, bins=10, rwidth=0.65,
                                    color = colors, align="left", histtype="bar")   # setting density to False will
                                                                                    # show count, and True will show
                                                                                    # probability.
    ax.set_yscale('log')
    ax.yaxis.set_major_formatter(FormatStrFormatter('%d'))
    ax.set_xticks(bins)
    ax.xaxis.set_major_formatter(PercentFormatter())
    ax.set_xlim([-50, 975])
    #ax.yaxis.set_major_formatter(mticker.ScalarFormatter()) # comment this when density=True
    ##ax.yaxis.set_minor_formatter(mticker.ScalarFormatter())


def set_plot_style(nrows, ncols, fig_height=4, fig_width=8):
    sns.set()
    sns.set_context("paper")
    sns.set_style("darkgrid")
    flatui = ["#9b59b6", "#3498db", "#e74c3c", "#34495e", "#2ecc71"]
    sns.palplot(sns.color_palette(flatui))
    fig, axes = plt.subplots(nrows=nrows, ncols=ncols, figsize=(fig_width, fig_height), dpi=600)
    plt.subplots_adjust(wspace=0.2, hspace=0.2)
    return fig, axes


def run(input_path, plot_density):
    files = []
    for root, dirpath, filenames in os.walk(input_path):
        for filename in filenames:
            if os.path.splitext(filename)[1] == ".csv" and \
            os.path.splitext(filename)[0].endswith(CLUSTERED_FILENAME_POSFIX):
                files.append(filename)

    x_axis_label = "\n Citation growth percentage"
    y_axis_label = "Probability \n" if plot_density else "Count \n"

    fig, ax = set_plot_style(1, 1)
    row_counter = -1
    growthes_dict = {}
    all_growthes = []
    labels = []
    colors = []
    for filename in files:
        print(f">>> Processing file: {filename}")
        row_counter += 1
        filename_without_extension = os.path.splitext(filename)[0]
        repository_name = filename_without_extension.replace(CLUSTERED_FILENAME_POSFIX, "")
        input_df = pd.read_csv(os.path.join(root, filename), header=0, sep='\t')
        all_growthes.append(aggregate(get_growthes(input_df, GROWTH_COLUMN_HEADER), -500, 1000))
        labels.append(repository_name)
        colors.append(COLOR_PALETTES[repository_name])
    
    plot(ax, all_growthes, labels, colors, plot_density)

    ax.set_xlabel(x_axis_label)
    ax.set_ylabel(y_axis_label)

    plt.legend(loc="upper right", ncol=4)

    image_file = os.path.join(input_path, 'percentage_of_growth.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file, bbox_inches='tight')
    plt.close()


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Missing input path.")
        exit()

    plot_density = False
    if len(sys.argv) == 3:
        plot_density = sys.argv[2] == "True"

    run(sys.argv[1], plot_density)

