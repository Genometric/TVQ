import pandas as pd
from matplotlib import pyplot as plt
df = pd.read_csv('C:/Users/Vahid/Desktop/iris.csv', sep="\t")
pd.plotting.parallel_coordinates(
        df, 'Category',
        color=('#556270', '#4ECDC4', '#C7F464'))
plt.show()
