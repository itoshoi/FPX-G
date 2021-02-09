import pandas as pd
import matplotlib.pyplot as plt
from statistics import mean
import numpy as np

x = []
y = []

for i in np.arange(0.1, 2, 0.1):
    data = pd.read_csv('lambda_'+str(i)[0]+'_'+str(i)[2]+'.csv', encoding='UTF8')
    x.append(i)
    y.append(mean(data['OperationCount']))
    print(data['Distance'])

data = pd.read_csv('lambda_10.csv', encoding='UTF8')
x.append(i)
y.append(mean(data['OperationCount']))


plt.plot(x, y)
plt.show()
