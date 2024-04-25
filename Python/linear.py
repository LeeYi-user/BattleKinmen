import numpy as np
import matplotlib.pyplot as plt

waveLimit = 10

x = np.arange(1, waveLimit + 1)
y = []

for t in x:
    y_1 = t * 10
    y.append(y_1)

plt.plot(x, y, label="Linear")
plt.xlabel("Wave")
plt.xticks(x)
plt.ylabel("Enemy")
plt.yticks(y)
plt.legend()
plt.show()
