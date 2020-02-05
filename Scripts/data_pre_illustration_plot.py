import matplotlib.pyplot as plt
import numpy as np
import os
import sys

from matplotlib.pyplot import figure
from matplotlib.ticker import MaxNLocator


B_FOREGROUND_COLOR = "purple"
A_FOREGROUND_COLOR = "green"
BACKGROND_COLOR = (0.9, 0.9, 0.9)
GRID_COLOR = (0.8, 0.8, 0.8)
LINE_STYLE = "-"
X_AXIS_BIN_COUNT = 6


t1_b_x = [2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009, 2010, 2011, 2012, 2013, 2014]
t1_b_y = [0, 10, 16, 22, 28, 32, 48, 59, 71, 82, 93, 112, 123]
t1_a_x = [2015, 2016, 2017, 2018, 2019, 2020]
t1_a_y = [230, 390, 412, 600, 690, 730]


t2_b_x = [2015, 2016, 2017, 2018]
t2_b_y = [90, 120, 150, 200]
t2_a_x = [2019, 2020]
t2_a_y = [320, 400]


def normalize_by_vector_length(x, y):
    # x: the vector to use for normalization.
    # y: the vector to be normalized.
    x = np.array(x)
    length = x.max() - x.min()
    return np.array([item / length for item in y])


def normalize_min_max(v, is_before=True):
    v = np.array(v)
    min = float(v.min())
    max = float(v.max())
    if is_before:
        return np.array([(-1) * ((x - max) / (min - max)) for x in v])
    else:
        return np.array([(x - min) / (max - min) for x in v])
    
    

def generate_plot(ax, title, xlabel, ylabel, b_x, b_y, a_x, a_y, integer_x_axix=False, before_marker = "o", after_marker = "o"):
    # This addes the last item of "before" to 
    # the begining of "after" vector, hence 
    # connecting them.
    ## a_x = np.insert(a_x, 0, b_x[-1])
    ## a_y = np.insert(a_y, 0, b_y[-1])
    
    ax.plot(b_x, b_y, c=B_FOREGROUND_COLOR, marker=before_marker)
    ax.plot(a_x, a_y, c=A_FOREGROUND_COLOR, marker=after_marker)
    ax.set_xlabel(xlabel)
    ax.set_ylabel(ylabel)
    ax.set_title(title)
    
    ax.set_facecolor(BACKGROND_COLOR)
    ax.grid(color=GRID_COLOR, linestyle='-', linewidth=1)
    ax.set_axisbelow(True)
    ax.xaxis.set_major_locator(MaxNLocator(nbins=X_AXIS_BIN_COUNT, integer=integer_x_axix))


def plot():
    # one row
    # three columns
    # last '1' is the order of this subplot relative to other subplots.
    fig, ax = plt.subplots(ncols=3, nrows=1, figsize=(16, 4), dpi=96, facecolor='white')
    
    # Step 1: Original data
    title = "1. Original input"
    xlabel = "Date"
    ylabel = "Citations"
    generate_plot(ax[0], title, xlabel, ylabel, t1_b_x, t1_b_y, t1_a_x, t1_a_y, True)
    generate_plot(ax[0], title, xlabel, ylabel, t2_b_x, t2_b_y, t2_a_x, t2_a_y, True, "x", "x")

    # Step 2: Normalize citation count for the duration of publication life 
    # (i.e., from the date it was published until today).
    title = "2. Citation count normalized"
    xlabel = "Date"
    ylabel = "Citations per year"
    nt1_b_y = normalize_by_vector_length(t1_b_x, t1_b_y)
    nt1_a_y = normalize_by_vector_length(t1_a_x, t1_a_y)
    nt2_b_y = normalize_by_vector_length(t2_b_x, t2_b_y)
    nt2_a_y = normalize_by_vector_length(t2_a_x, t2_a_y)
    generate_plot(ax[1], title, xlabel, ylabel, t1_b_x, nt1_b_y, t1_a_x, nt1_a_y, False)
    generate_plot(ax[1], title, xlabel, ylabel, t2_b_x, nt2_b_y, t2_a_x, nt2_a_y, False, "x", "x")
    
    # Step 3: Min-max normalize date so all the vectors have common date between -1 and 1.
    title = "Date normalized"
    xlabel = "Min-Max normalized date"
    ylabel = "Citations per year"    
    nt1_b_x = normalize_min_max(t1_b_x, True)
    nt1_a_x = normalize_min_max(t1_a_x, False)
    nt2_b_x = normalize_min_max(t2_b_x, True)
    nt2_a_x = normalize_min_max(t2_a_x, False)
    generate_plot(ax[2], title, xlabel, ylabel, nt1_b_x, nt1_b_y, nt1_a_x, nt1_a_y, False)
    generate_plot(ax[2], title, xlabel, ylabel, nt2_b_x, nt2_b_y, nt2_a_x, nt2_a_y, False, "x", "x")

    plt.show()


if __name__ == "__main__":
    plot()
