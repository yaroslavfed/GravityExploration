from mpl_toolkits.mplot3d import axes3d
import matplotlib.pyplot as plt
import numpy as np

fig, ax = plt.subplots(nrows=2, ncols=1, sharex= True)
fig.suptitle('Plots Stacked Vertically')

x = np.linspace(-6, 6)
y = pow(x, -2)

xt1 = np.arange(-5, -4, 0.01)
xt2 = np.arange(-2, -1, 0.01)
xt3 = np.arange(-1, 1, 0.01)

yt1 = -4
yt2 = -3
yt3 = xt2
yt4 = -2
yt5 = -1

ax[0].plot(x, y)

ax[1].vlines(0, -5, 0, color = 'k')
ax[1].hlines(0, -6, 6, color = 'k')

ax[1].fill_between(xt1, yt1, yt2, color="g", alpha=0.3)
ax[1].fill_between(xt2, yt3, yt4, color="r", alpha=0.3)
ax[1].fill_between(xt3, yt4, yt5, color="r", alpha=0.3)

plt.show()