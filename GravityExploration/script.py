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

ax[0].plot(x, y)

ax[1] = plt.gca()
ax[1].hlines(0, x[0], x[-1], color = 'k')

plt.show()