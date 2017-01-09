import matplotlib.pyplot as plt
import numpy as np
import os
import sys


def read_all(data_path: str) -> np.ndarray:
    with open(data_path) as fp:
        return np.array(list(map(float, fp)))

if __name__ == '__main__':
    if len(sys.argv) > 1:
        address = sys.argv[1]
    else:
        address = strip_margin(input('plz input the address of folder/file:\n\t'))
    print('plotting...')
    data= read_all(address+r"temporal\temporal.txt")
    plt.plot(data)
    indices = read_all(address+r"temporal\crests.txt")
    for index in indices:
        plt.plot(index, data[index],'ro')
    plt.show()
    quit()