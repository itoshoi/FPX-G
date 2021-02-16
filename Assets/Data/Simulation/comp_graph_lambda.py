# -*- coding: utf-8 -*-

from os.path import exists
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import matplotlib
matplotlib.use('Agg')

# apply japanese font
from matplotlib.font_manager import FontProperties
font_path = "/usr/share/fonts/truetype/migmix/migmix-1p-regular.ttf"
font_prop = FontProperties(fname=font_path)
matplotlib.rcParams["font.family"] = font_prop.get_name()

lstyles = [':', '--', '-']
colors = 'rbg'
# colors = 'kkk'


def main():
    fig, axs = plt.subplots(nrows=1, ncols=3, figsize=(16, 4))
    ax_ind = 0

    gmodel = 'BarabasiAlbert'
    n = 200
    for m in [2, 4, 6]:
        path_head = f'{gmodel}_m{m}_n{n}'
        legend = plot(path_head, axs[ax_ind], 100)
        ax_ind += 1

    fig.legend(legend[0], legend[1], loc='upper center',
               ncol=6, fontsize=16, mode='expand', markerscale=1.0)
    plt.subplots_adjust(hspace=0.5, wspace=0.2, top=0.77,
                        bottom=0.15, left=0.05, right=0.98)
    plt.savefig('figs/graph_lambda/barabasi.pdf')

    fig, axs = plt.subplots(nrows=1, ncols=3, figsize=(16, 4))
    ax_ind = 0
    gmodel = 'WattsStrogatz'
    n = 200
    for m in [4, 8, 12]:
        path_head = f'{gmodel}_k{m}_n{n}'
        legend = plot(path_head, axs[ax_ind], 100)
        ax_ind += 1

    fig.legend(legend[0], legend[1], loc='upper center',
               ncol=6, fontsize=16, mode='expand', markerscale=1.0)
    plt.subplots_adjust(hspace=0.5, wspace=0.2, top=0.77,
                        bottom=0.15, left=0.05, right=0.98)
    plt.savefig('figs/graph_lambda/wattsstrogatz.pdf')

    fig, ax = plt.subplots(nrows=1, ncols=1, figsize=(16, 4))
    gmodel = 'Tree'
    path_head = f'{gmodel}'
    legend = plot(path_head, ax, 150)

    fig.legend(legend[0], legend[1], loc='upper center',
               ncol=6, fontsize=16, mode='expand', markerscale=1.0)
    plt.subplots_adjust(hspace=0.5, wspace=0.2, top=0.77,
                        bottom=0.15, left=0.05, right=0.98)
    plt.savefig('figs/graph_lambda/tree.pdf')


def plot(path_head, ax, ylim):
    x = [round(i, 1) for i in np.arange(-3, 2.1, 0.2)]
    x_ticks = [round(i, 1) for i in np.arange(0.1, 2, 0.4)]

    # for dist in range(1, 4):
    for dist in range(2, 4):
        path = f'{path_head}'

        if not exists(path):
            continue

        # print #operations in the baseline
        df = pd.read_csv(f'{path}/lambda_10_0.csv')
        y = df[(dist <= df['Distance']) & (df['Distance'] < dist + 1)]['OperationCount'].mean()
        ax.plot([x[0], x[-1]], [y, y], label=f'距離={dist} (ブラウジング)',
                linestyle=lstyles[dist-1], color='b', linewidth=1.5,
                marker=None)

        # print #operations in the proposed method
        ys = []
        for lam in x:
            if lam == 0:
                lam = 0.0
            lam_str = str(lam).replace('.', '_')
            df = pd.read_csv(f'{path}/lambda_{lam_str}.csv')
            y = df[(dist <= df['Distance']) & (df['Distance'] < dist + 1)]['OperationCount'].mean()
            ys.append(y)
        ax.plot(x, ys, label=f'距離={dist} (FPX-G)',
                linestyle=lstyles[dist-1], linewidth=1.5, color='g',
                marker='', markersize=4, mfc='white')

    path_sp = path_head.split('_')
    if 1 < len(path_sp):
        ax.set_title(path_sp[1][0] + '=' + path_sp[1][1:], fontsize=16)
    ax.tick_params(axis='both', which='major', labelsize=14)
    ax.set_xlabel('$\lambda$', fontsize=20)
    ax.set_ylabel('平均操作回数', fontsize=13)
    ax.set_ylim(0, ylim)
    ax.set_xlim(-3.0, 2.0)
    ax.grid(linestyle=':', color='grey')

    return ax.get_legend_handles_labels()


if __name__ == "__main__":
    main()
