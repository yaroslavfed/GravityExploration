from cmath import pi
from xmlrpc.client import Fault
from mpl_toolkits.mplot3d import axes3d
import matplotlib.pyplot as plt
import numpy as np

fig, ax = plt.subplots(nrows=2, ncols=1, sharex= True, sharey= False)
fig.suptitle('Test')

units = []

with open("output.txt", "r") as f:
    nums = f.readline()[:-1]
    for i in range(int(nums)):
        units.append([int(j) for j in f.readline()[:-2].split(' ')])
    x = f.readline()[:-2].replace(',','.')
    y = f.readline()[:-2].replace(',','.')

x=list(map(float, x.split(' ')))
y=list(map(float, y.split(' ')))

theta = np.linspace(0, 2 * np.pi, 100)
for i in range(int(nums)):
    r = units[i][1]
    x1 = r * np.cos(theta)
    y1 = r * np.sin(theta)
    ax[1].fill_between(x1 + units[i][2], y1 - units[i][0])

#xt1 = np.arange(-5, -4, 0.01)
#xt2 = np.arange(-2, -1, 0.01)
#xt3 = np.arange(-1, 1, 0.01)

#yt1 = -4
#yt2 = -3
#yt3 = xt2
#yt4 = -2
#yt5 = -1

ax[0].plot(x, y)

ax[1] = plt.gca()
#ax[1].axis("equal")

#ax[1].vlines(0, -max(units), 0, color = 'k', alpha=0.3)
ax[1].hlines(0, x[0], x[-1], color = 'k')

#ax[1].fill_between(xt1, yt1, yt2, color="g", alpha=0.3)
#ax[1].fill_between(xt2, yt3, yt4, color="r", alpha=0.3)
#ax[1].fill_between(xt3, yt4, yt5, color="r", alpha=0.3)

plt.show()