import pandas as pd
import matplotlib.pyplot as plt
from statistics import mean
import numpy as np
import sys
import os

dirName = sys.argv[1]

x = []
y = []

for i in np.arange(0.1, 2, 0.1):
    i = round(i, 1)
    if i == 0:
        continue
    elif i > 0:
        data = pd.read_csv(dirName +
                           '/lambda_'+str(i)[0]+'_'+str(i)[2]+'.csv', encoding='UTF8')
    else:
        data = pd.read_csv(dirName +
                           '/lambda_-'+str(i)[1]+'_'+str(i)[3]+'.csv', encoding='UTF8')

    x.append(i)
    y.append(mean(data['OperationCount']))
    print(data)

if os.path.exists(dirName + '/lambda_10_0.csv'):
    data = pd.read_csv(dirName + '/lambda_10_0.csv', encoding='UTF8')

# x.append(i)
# y.append(mean(data['OperationCount']))
print("始点ノードに戻るWikiモデルの平均操作数 " + str(mean(data['OperationCount'])))

fig = plt.figure()
plt.plot(x, y)
fig.savefig(dirName + "/graph.png")
plt.show()
