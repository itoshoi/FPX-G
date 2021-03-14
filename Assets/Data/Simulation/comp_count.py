# -*- coding: utf-8 -*-

from os.path import exists
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import matplotlib
import collections
matplotlib.use('Agg')

# apply japanese font
from matplotlib.font_manager import FontProperties
font_path = "/usr/share/fonts/truetype/migmix/migmix-1p-regular.ttf"
font_prop = FontProperties(fname=font_path)
matplotlib.rcParams["font.family"] = font_prop.get_name()

lstyles = [':', '--', '-']
colors = 'rgb'


def main():
    plt.style.use('dark_background')
    fig, axs = plt.subplots(nrows=1, ncols=1, figsize=(8, 4))
    ax_ind = 0

    gmodel = 'BarabasiAlbert'
    print(gmodel)
    n = 200
    for m in [3]:
        path_head = f'{gmodel}_m{m}_n{n}'
        # legend = plot(path_head, axs[ax_ind], 50)
        legend = plot(path_head, axs, 0.04)
        ax_ind += 1

    # fig.legend(legend[0], legend[1], loc='upper center',
    #            ncol=6, fontsize=16, mode='expand', markerscale=1.0)
    plt.subplots_adjust(hspace=0.5, wspace=0.2, top=0.9,
                        bottom=0.15, left=0.1, right=0.9)
    plt.savefig('figs/count-op/barabasi.png')

    fig, axs = plt.subplots(nrows=1, ncols=1, figsize=(8, 4))
    ax_ind = 0
    gmodel = 'WattsStrogatz'
    print(gmodel)
    n = 200
    for m in [6]:
        path_head = f'{gmodel}_k{m}_n{n}'
        # legend = plot(path_head, axs[ax_ind], 50)
        legend = plot(path_head, axs, 0.04)
        ax_ind += 1

    # fig.legend(legend[0], legend[1], loc='upper center',
    #            ncol=6, fontsize=16, mode='expand', markerscale=1.0)
    plt.subplots_adjust(hspace=0.5, wspace=0.2, top=0.9,
                        bottom=0.15, left=0.1, right=0.9)
    plt.savefig('figs/count-op/wattsstrogatz.png')

    fig, ax = plt.subplots(nrows=1, ncols=1, figsize=(8, 4))
    gmodel = 'Tree'
    print(gmodel)
    path_head = f'{gmodel}'
    legend = plot(path_head, ax, 0.04)

    # fig.legend(legend[0], legend[1], loc='upper center',
    #            ncol=6, fontsize=16, mode='expand', markerscale=1.0)
    plt.subplots_adjust(hspace=0.5, wspace=0.2, top=0.9,
                        bottom=0.15, left=0.1, right=0.9)
    plt.savefig('figs/count-op/tree.png')


def plot(path_head, ax, ylim):
    # x = [round(i, 1) for i in np.arange(-3, 2, 0.4)]
    # x = np.arange(0, 1, 300)
    x_ticks = [round(i, 1) for i in np.arange(0.1, 2, 0.4)]

    # for dist in range(1, 4):
    for dist in range(1, 2):
        path = f'{path_head}'

        if not exists(path):
            continue

        # print #operations in the baseline
        df = pd.read_csv(f'{path}/lambda_10_0.csv')
        # y = df[(dist <= df['Distance']) & (df['Distance'] < dist + 1)]['OperationCount'].mean()
        ops = df[(1 <= df['Distance']) & (df['Distance'] <= 4)]['OperationCount']
        # ops = df['OperationCount']
        cs = collections.Counter(ops)
        cs = sorted(cs.items(), key=lambda x: x[0])
        y = [c[1] / len(ops) for c in cs]
        x = [c[0] for c in cs]
        # ax.plot(x, y, label='ブラウジング',
        #         linestyle='-', color='b', linewidth=2.5,
        #         marker=None)
        ax.hist(x, label='ブラウジング',
                color=[0, 0.7, 1], bins=20, density=True, alpha=0.5)
        print('node count : ' + str(len(ops)))
        print('browsing mean : ' + str(ops.mean()))
        print('browsing median : ' + str(ops.median()))
        print('browsing std : ' + str(ops.std()))

        df = pd.read_csv(f'{path}/lambda_0_4.csv')
        # y = df[(dist <= df['Distance']) & (df['Distance'] < dist + 1)]['OperationCount'].mean()
        ops = df[(1 <= df['Distance']) & (df['Distance'] <= 4)]['OperationCount']
        # ops = df['OperationCount']
        cs = collections.Counter(ops)
        cs = sorted(cs.items(), key=lambda x: x[0])
        y = [c[1] / len(ops) for c in cs]
        x = [c[0] for c in cs]
        # ax.plot(x, y, label='FPX-G',
        #         linestyle='-', color='g', linewidth=2.5,
        #         marker=None)
        ax.hist(ops, label='FPX-G',
                color='y', bins=20, density=True, alpha=0.5)
        print('node count : ' + str(len(ops)))
        print('fpx-g mean : ' + str(ops.mean()))
        print('fpx-g median : ' + str(ops.median()))
        print('fpx-g std : ' + str(ops.std()))

        # ys = []
        # for lam in x:
        #     if lam == 0:
        #         lam = 0.0
        #     lam_str = str(lam).replace('.', '_')
        #     df = pd.read_csv(f'{path}/lambda_{lam_str}.csv')
        #     y = df[(dist <= df['Distance']) & (df['Distance'] < dist + 1)]['OperationCount'].mean()
        #     ys.append(y)
        # ax.plot(x, ys, label=f'Dist={dist} (3D)',
        #         linestyle=lstyles[dist-1], linewidth=1.5, color=colors[dist-1],
        #         marker='d', markersize=4, mfc='white')

    # ax.set_title(path_head[:-5], fontsize=16)
    ax.tick_params(axis='both', which='major', labelsize=14)
    ax.set_xlabel('操作回数', fontsize=15)
    ax.set_ylim(0, ylim)
    ax.set_xlim(0, 420)
    ax.grid(linestyle=':', color='grey')
    ax.legend(fontsize=15, markerscale=1.0)

    return ax.get_legend_handles_labels()


if __name__ == "__main__":
    main()
