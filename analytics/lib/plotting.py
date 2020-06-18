"""
TODO: Add doc string.
"""

import matplotlib.pyplot as plt
import numpy as np
import os
import sys

STEP_SIZE = 10
BACKGROND_COLOR = (0.9, 0.9, 0.9)
GRID_COLOR = (0.95, 0.95, 0.95)

def read_input(filename):
    before_x = []
    before_y = []
    after_x = []
    after_y = []
    
    with open(filename) as f:
        lines = [line.rstrip() for line in f]
        for line in lines:
            columns = line.split("\t")
            x = columns[2]
            y = columns[3]
            if float(columns[2]) < 0:
                before_x.append(float(x))
                before_y.append(float(y))
            else:
                after_x.append(float(x))
                after_y.append(float(y))
    return before_x, before_y, after_x, after_y


def plot(root, filename):
    before_x, before_y, after_x, after_y = read_input(os.path.join(root, filename))
    fig, ax = plt.subplots()
    ax.scatter(before_x, before_y, c="red", alpha=0.5, marker = "o", label="Before adding to repository")
    ax.scatter(after_x, after_y, c="green", alpha=0.5, marker = "o", label="After adding to repository")

    # start, end = ax.get_ylim()
    # ax.plot([0, 0], [0, end], c="blue", alpha=0.5)
    
    # start, end = ax.get_xlim()
    # stepsize = (end-start)/10.0
    start = -1
    end = 1.01
    stepsize = 0.4
    ax.xaxis.set_ticks(np.arange(start, end, stepsize))
    ax.set_xlabel("Date offset from adding to repository")

    # start, end = ax.get_ylim()
    # stepsize = end / 10.0
    end = 1.01
    stepsize = 0.2
    ax.yaxis.set_ticks(np.arange(0, end, stepsize))
    ax.set_ylabel("Citations")

    ax.set_facecolor(BACKGROND_COLOR)
    ax.legend(loc="upper left")
    ax.grid(color=GRID_COLOR, linestyle='-', linewidth=1)
    ax.set_axisbelow(True)

    filename_without_extension = os.path.splitext(filename)[0]
    plt.title(filename_without_extension)

    # plt.show()

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
            
