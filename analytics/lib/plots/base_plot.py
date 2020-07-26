"""

"""

import numpy as np
import os
import sys
import pandas as pd
import matplotlib.pyplot as plt
import scipy.cluster.hierarchy as shc
from sklearn.cluster import AgglomerativeClustering
import seaborn as sns
import sklearn
from matplotlib.lines import Line2D


class BasePlot(object):
    """description of class"""

    def __init__(self):
        pass

    def set_plot_style(self, nrows, ncols, width=None, height=None, wspace=0.15, hspace=0.35, dpi=300, sharex=False, width_ratios=[2, 1]):
        sns.set()
        sns.set_context("paper")
        sns.set_style("darkgrid")

        if not height:
            height = 3 * nrows

        if not width:
            width = 4 * ncols

        fig, axes = plt.subplots(nrows=nrows, ncols=ncols, figsize=(height, width), dpi=dpi, sharex=sharex, gridspec_kw={'width_ratios':width_ratios})
        plt.subplots_adjust(wspace=wspace, hspace=hspace)
        return fig, axes
