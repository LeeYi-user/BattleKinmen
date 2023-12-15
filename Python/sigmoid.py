import numpy as np
import math
import matplotlib.pyplot as plt

slope = 1
timeLimit = 300
enemyLimit = 100

x = np.arange(0, timeLimit / min(1, slope), 0.1)
y = []

for t in x:
    y_1 = enemyLimit / (1 + math.exp(-slope / (timeLimit / 10) * (t - (timeLimit / 2))))
    y.append(y_1)

plt.plot(x, y, label="Sigmoid")
plt.xlabel("Time Limit")
plt.ylabel("Enemy Limit")
plt.ylim(-1, enemyLimit + 1)
plt.legend()
plt.show()
