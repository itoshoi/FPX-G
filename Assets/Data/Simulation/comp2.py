# -*- coding: utf-8 -*-

from os.path import exists
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import matplotlib
matplotlib.use('Agg')


lstyles = [':', '--', '-']

colors_2d = 'b'
colors_3d = 'g'
# colors = 'kkk'
# colors = 'brg'


def main():
    fig, axs = plt.subplots(nrows=1, ncols=5, figsize=(16, 4))
    ax_ind = 0

    gmodel = 'BarabasiAlbert'
    n = 200
    for m in [2, 3, 4, 5, 6]:
        path_head = f'{gmodel}_m{m}_n{n}'
        legend = plot(path_head, axs[ax_ind], 500)
        ax_ind += 1

    fig.legend(legend[0], legend[1], loc='upper center',
               ncol=6, fontsize=16, mode='expand', markerscale=1.0)
    plt.subplots_adjust(hspace=0.5, wspace=0.2, top=0.77,
                        bottom=0.1, left=0.05, right=0.98)
    plt.savefig('figs/barabasi.pdf')

    fig, axs = plt.subplots(nrows=1, ncols=5, figsize=(16, 4))
    ax_ind = 0
    gmodel = 'WattsStrogatz'
    n = 200
    for m in [4, 6, 8, 10, 12]:
        path_head = f'{gmodel}_k{m}_n{n}'
        legend = plot(path_head, axs[ax_ind], 500)
        ax_ind += 1

    fig.legend(legend[0], legend[1], loc='upper center',
               ncol=6, fontsize=16, mode='expand', markerscale=1.0)
    plt.subplots_adjust(hspace=0.5, wspace=0.2, top=0.77,
                        bottom=0.1, left=0.05, right=0.98)
    plt.savefig('figs/wattsstrogatz.pdf')

    fig, ax = plt.subplots(nrows=1, ncols=1, figsize=(16, 4))
    gmodel = 'Tree'
    path_head = f'{gmodel}'
    legend = plot(path_head, ax, 500)

    fig.legend(legend[0], legend[1], loc='upper center',
               ncol=6, fontsize=16, mode='expand', markerscale=1.0)
    plt.subplots_adjust(hspace=0.5, wspace=0.2, top=0.77,
                        bottom=0.1, left=0.05, right=0.98)
    plt.savefig('figs/tree.pdf')


def plot(path_head, ax, ylim):
    x = [round(i, 1) for i in np.arange(-3, 2, 0.4)]
    x_ticks = [round(i, 1) for i in np.arange(0.1, 2, 0.4)]

    # for dist in range(1, 4):
    for dist in range(0, 1):
        path = f'{path_head}'

        if not exists(path):
            continue

        # print #operations in the baseline
        df = pd.read_csv(f'{path}/lambda_10_0.csv')
        y = df[(dist <= df['Distance']) & (df['Distance'] < dist + 1)]['OperationCount'].mean()
        ax.plot([x[0], x[-1]], [y, y], label=f'Dist={dist} (2D)',
                linestyle=lstyles[dist-1], color=colors_2d[dist-1], linewidth=1.5,
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
        ax.plot(x, ys, label=f'Dist={dist} (3D)',
                linestyle=lstyles[dist-1], linewidth=1.5, color=colors_3d[dist-1],
                marker='d', markersize=4, mfc='white')

    ax.set_title(path_head[:-5], fontsize=16)
    ax.tick_params(axis='both', which='major', labelsize=14)
    ax.set_ylim(0, ylim)
    ax.set_xlim(-3.0, 2.0)
    ax.grid(linestyle=':', color='grey')

    return ax.get_legend_handles_labels()


if __name__ == "__main__":
    main()
