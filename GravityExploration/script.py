import numpy as np
from mpl_toolkits.mplot3d import Axes3D
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

fig = plt.figure(figsize=(6, 9.5))

# plot "Underground"
ax1 = fig.add_subplot(212, projection='3d')

xgrid, ygrid = np.meshgrid(xf, yf)
zgrid = xgrid * xgrid * 0
ax1.plot_surface(xgrid, ygrid, zgrid, alpha=.3)

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
ax2 = fig.add_subplot(211, projection='3d')
ax2.set_xlabel('X')
ax2.set_ylabel('Y')
ax2.set_zlabel('Z')

xgrid, ygrid = np.meshgrid(xf, yf)

zgrid = np.array(res_arr)

ax2.plot_surface(xgrid, ygrid, zgrid, cmap='jet')

ax1.axis("equal")

plt.title('Graph ' + number)
plt.subplots_adjust(wspace=0, hspace=0)
plt.show()