import matplotlib.pyplot as plt
import numpy as np
import os
import sys

from scipy.interpolate import make_interp_spline, BSpline

STEP_SIZE = 10
BACKGROND_COLOR = (0.9, 0.9, 0.9)
GRID_COLOR = (0.95, 0.95, 0.95)

def read_input(filename):
    before_x = []
    before_median = []
    before_lower_quartile = []
    before_upper_quartile = []
    before_max = []
    before_min = []
    after_x = []
    after_median = []
    after_lower_quartile = []
    after_upper_quartile = []
    after_max = []
    after_min = []    
    
    with open(filename) as f:
        lines = [line.rstrip() for line in f]
        for line in lines:
            columns = line.split("\t")
            x = float(columns[1])
            median = float(columns[3])
            lower_quartile = float(columns[2])
            upper_quartile = float(columns[4])
            max = float(columns[5])
            min = float(columns[6])

            if x < 0:
                before_x.append(x)
                before_median.append(median)
                before_lower_quartile.append(lower_quartile)
                before_upper_quartile.append(upper_quartile)
                before_max.append(max)
                before_min.append(min)
            elif x > 0:
                after_x.append(x)
                after_median.append(median)
                after_lower_quartile.append(lower_quartile)
                after_upper_quartile.append(upper_quartile)
                after_max.append(max)
                after_min.append(min)
            else:
                before_x.append(x)
                before_median.append(median)
                before_lower_quartile.append(lower_quartile)
                before_upper_quartile.append(upper_quartile)
                before_max.append(max)
                before_min.append(min)
                after_x.append(x)
                after_median.append(median)
                after_lower_quartile.append(lower_quartile)
                after_upper_quartile.append(upper_quartile)
                after_max.append(max)
                after_min.append(min)


    return \
        before_x, \
        before_median, \
        before_lower_quartile, \
        before_upper_quartile, \
        before_max, \
        before_min, \
        after_x, \
        after_median, \
        after_lower_quartile, \
        after_upper_quartile, \
        after_max, \
        after_min

def smooth(x, y):
    x = np.array(x)
    y = np.array(y)
    xnew = np.linspace(x.min(), x.max(), 50) 
    spl = make_interp_spline(x, y, k=2)
    y_smooth = spl(xnew)
    return xnew, y_smooth


def plot_smooth_line(ax, x, y, color, alpha=1, linestyle="-", label=None):
    s_x, s_y = smooth(x, y)
    ax.plot(s_x, s_y, c=color, alpha=alpha, linestyle=linestyle, label=label)


def plot_smooth_fill_between(ax, x, lower_y, upper_y, color):
    s_x, s_lower_y = smooth(x, lower_y)
    s_x, s_upper_y = smooth(x, upper_y)
    ax.fill_between(s_x, s_lower_y, s_upper_y, facecolor=color, alpha=0.3)


def plot(root, filename):
    before_x, before_median, before_lower_quartile, \
    before_upper_quartile, before_max, before_min, \
    after_x, after_median, after_lower_quartile, \
    after_upper_quartile, after_max, after_min = read_input(os.path.join(root, filename))

    fig, ax = plt.subplots()

    plot_smooth_line(ax, before_x, before_median, "red", label="Before adding to repository")
    plot_smooth_line(ax, before_x, before_max, color="red", linestyle='dashed')
    plot_smooth_line(ax, before_x, before_min, color="red", linestyle='dotted')
    plot_smooth_fill_between(ax, before_x, before_lower_quartile, before_upper_quartile, "red")

    plot_smooth_line(ax, after_x, after_median, "green", label="After adding to repository")
    plot_smooth_line(ax, after_x, after_max, color="green", linestyle='dashed')
    plot_smooth_line(ax, after_x, after_min, color="green", linestyle='dotted')
    plot_smooth_fill_between(ax, after_x, after_lower_quartile, after_upper_quartile, "green")

    start = -1
    end = 1.01
    stepsize = 0.4
    ax.xaxis.set_ticks(np.arange(start, end, stepsize))
    ax.set_xlabel("Date offset from adding to repository")

    ax.set_ylabel("Citations")

    ax.set_facecolor(BACKGROND_COLOR)
    ax.legend(loc="upper left")
    ax.grid(color=GRID_COLOR, linestyle='-', linewidth=1)
    ax.set_axisbelow(True)

    filename_without_extension = os.path.splitext(filename)[0]
    plt.title(filename_without_extension)

    plt.show()
    exit()

    image_file = os.path.join(root, filename_without_extension + '.png')
    if os.path.isfile(image_file):
        os.remove(image_file)
    plt.savefig(image_file)
    plt.close()


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Missing input path.")
        exit()

    inputPath = sys.argv[1]
    for root, dirpath, filenames in os.walk(inputPath):
        for filename in filenames:
            plot(root, filename)
            

