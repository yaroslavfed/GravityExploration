from matplotlib.ticker import LinearLocator
import numpy as np
from mpl_toolkits.mplot3d import Axes3D
from matplotlib.widgets import Button
from mpl_toolkits.mplot3d.art3d import Poly3DCollection, Line3DCollection
import matplotlib.pyplot as plt

units = []
res_arr = []

with open("output.txt", "r") as f:
    number = f.readline()[:-1]

with open("output" + number + ".txt", "r") as f:
    nums = f.readline()[:-1]
    for i in range(int(nums)):
        units.append([float(j) for j in f.readline()[:-2].replace(',','.').split(' ')])
    xf = f.readline()[:-2].replace(',','.')
    yf = f.readline()[:-2].replace(',','.')
    ynums = f.readline()[:-1]
    for i in range(int(ynums)):
        res_arr.append([float(j) for j in f.readline()[:-2].replace(',','.').split(' ')])
xf=list(map(float, xf.split(' ')))
yf=list(map(float, yf.split(' ')))

fig = plt.figure(figsize=(12, 8))
fig.suptitle('Graph ' + number)
fig.subplots_adjust(top=0.9, bottom=0.05)

# plot "Underground"
ax1 = fig.add_subplot(234, projection='3d')

xgrid, ygrid = np.meshgrid(xf, yf)
zgrid = xgrid * xgrid * 0
ax1.plot_wireframe(xgrid, ygrid, zgrid, alpha=.8, color='k')

for i in range(int(nums)):
    points = np.array([[units[i][0], units[i][2], units[i][4]],
                  [units[i][1], units[i][2], units[i][4]],
                  [units[i][1], units[i][3], units[i][4]],
                  [units[i][0], units[i][3], units[i][4]],
                  [units[i][0], units[i][2], units[i][5]],
                  [units[i][1], units[i][2], units[i][5]],
                  [units[i][1], units[i][3], units[i][5]],
                  [units[i][0], units[i][3], units[i][5]]])
    
    Z = np.zeros((8,3))
    for i in range(8):
        Z[i,:] = np.dot(points[i,:], 1)

    # plot vertices
    #ax.axis('equal')
    ax1.scatter(Z[:, 0], Z[:, 1], Z[:, 2])

    # list of sides' polygons of figure
    verts = [[Z[0],Z[1],Z[2],Z[3]],
    [Z[4],Z[5],Z[6],Z[7]], 
    [Z[0],Z[1],Z[5],Z[4]], 
    [Z[2],Z[3],Z[7],Z[6]], 
    [Z[1],Z[2],Z[6],Z[5]],
    [Z[4],Z[7],Z[3],Z[0]]]

    # plot sides
    ax1.add_collection3d(Poly3DCollection(verts, linewidths=.2, edgecolors='b', alpha=.2))

ax1.set_xlabel('X')
ax1.set_ylabel('Y')
ax1.set_zlabel('Z')

# plot "Sensor readings"
ax2 = fig.add_subplot(231, projection='3d')

ax2.set_xlabel('X')
ax2.set_ylabel('Y')

xgrid, ygrid = np.meshgrid(xf, yf)
zgrid = np.array(res_arr)
surf = ax2.plot_surface(xgrid, ygrid, zgrid, cmap = 'gnuplot2')
ax2.zaxis.set_major_locator(LinearLocator(5))
ax2.set_aspect('equalxy')

# ax1.axis("equal")
ax1.set_aspect('equal')

y_index = 0
ax3 = fig.add_subplot(233)
fig.colorbar(surf,  ax=ax3)

l, = ax3.plot(xf, zgrid[y_index], lw=2)
ax3.set_title(ygrid[y_index][0])

class Index:
    ind = 0

    def next(self, event):
        self.ind += 1
        i = self.ind % len(zgrid)
        ydata = zgrid[i]
        l.set_ydata(ydata)
        ax3.set_title(ygrid[i][0])
        plt.draw()

    def prev(self, event):
        self.ind -= 1
        i = self.ind % len(zgrid)
        ydata = zgrid[i]
        l.set_ydata(ydata)
        ax3.set_title(ygrid[i][0])
        plt.draw()

callback = Index()
axprev = fig.add_axes([0.7, 0.05, 0.1, 0.075])
axnext = fig.add_axes([0.81, 0.05, 0.1, 0.075])
bnext = Button(axnext, 'Next')
bnext.on_clicked(callback.next)
bprev = Button(axprev, 'Previous')
bprev.on_clicked(callback.prev)

arr = np.array(zgrid)
maxarr = []
minarr = []

for i in arr:
    maxarr.append(max(i))
    minarr.append(min(i))
max_number = max(maxarr)
min_number = min(minarr)

ax3.set_ylim(top=max_number)  # adjust the top leaving bottom unchanged
ax3.set_ylim(bottom=min_number)  # adjust the bottom leaving top unchanged

plt.subplots_adjust(wspace=0, hspace=0)
plt.show()