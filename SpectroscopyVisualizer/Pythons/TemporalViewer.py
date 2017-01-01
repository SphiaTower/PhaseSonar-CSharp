import matplotlib.pyplot as plt
import numpy as np
import os


def read_all(data_path: str) -> np.ndarray:
    with open(data_path) as fp:
        return np.array(list(map(float, fp)))

if __name__ == '__main__':
    print('plotting...')
    data= read_all(r"C:\SpectroscopyVisualizer\temporal\temporal.txt")
    plt.plot(data)
    indices = read_all(r"C:\SpectroscopyVisualizer\temporal\crests.txt")
    for index in indices:
        plt.plot(index, data[index],'ro')
    plt.show()
    quit()