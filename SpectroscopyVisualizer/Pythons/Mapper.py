import threading

import matplotlib.pyplot as plt
import numpy as np
import os


def strip_margin(string: str):
    margin = ' "\'\n'
    return string.lstrip(margin).rstrip(margin)


def read_all(data_path: str):
    with open(data_path) as fp:
        return np.array(list(map(float, fp)))


def find_min_index(data, start, end):
    minimum = data[start]
    index = 0
    i = 0
    for d in data[start:end]:
        if d < minimum:
            minimum = d
            index = i
        i += 1
    return index + start


class CanvasEventDispatcher:
    def __init__(self, canvas):
        self.canvas = canvas
        self.left_click_listeners = []
        self.right_click_listeners = []
        self.middle_click_listeners = []
        canvas.mpl_connect('button_press_event', self.on_press)

    def on_press(self, event):
        if event.button == 1:
            invoke_event(self.left_click_listeners, event)
        elif event.button == 3:
            invoke_event(self.right_click_listeners, event)
        elif event.button == 2:
            invoke_event(self.middle_click_listeners, event)


def invoke_event(listeners, arg):
    for listener in listeners:
        listener.on_event(arg)


def infinite_input(prompt, target_type):
    while True:
        try:
            return target_type(input(prompt))
        except Exception:
            print('invalid input')


C = 3e8


class EventHandler:
    def __init__(self, axis, data, fig):
        self.delta_f = axis[1] - axis[0]
        self.start_f = axis[0]
        self.click_indices = []
        self.fig = fig
        self.axis = axis
        self.data = data
        self.peak_freqs = []
        self.all_peaks_found_listeners = []
        self.result = False

    def on_event(self, event):
        index = round((event.xdata - self.start_f) / self.delta_f)
        self.click_indices.append(index)
        plt.plot([event.xdata, event.xdata], [event.ydata * 0.8, event.ydata * 1.2], 'r')
        self.fig.canvas.draw()
        if len(self.click_indices) == 2:
            left, right = sorted(self.click_indices)[:2]
            min_index = find_min_index(self.data, left, right)
            self.click_indices.clear()
            plt.plot(self.axis[min_index], self.data[min_index], 'ro')
            self.fig.canvas.draw()
            self.peak_freqs.append(self.axis[min_index])
            if len(self.peak_freqs) == 2:
                plt.close(self.fig)
                self.result = True
            elif len(self.peak_freqs) == 1:
                print('plz find 1 more peak')


def convert(axis, data):
    print('plz identify 2 peaks')
    fig = plt.figure()
    canvas = fig.canvas
    dispatcher = CanvasEventDispatcher(canvas)
    handler = EventHandler(axis, data, fig)
    dispatcher.middle_click_listeners.append(handler)
    while not handler.result:
        plt.plot(axis, data)
        plt.show()
    return match_wavelength(handler.peak_freqs)


def build_axis(data, sample_rate):
    axis = np.linspace(-sample_rate / 2, sample_rate / 2, len(data) * 2)[len(data):]
    return axis


def match_wavelength(peak_freqs):
    wl1 = infinite_input('plz input the corresponding wavelength of the 1st peak/nm\n', float) / 1e9
    wl0 = infinite_input('plz input the corresponding wavelength of the 2nd peak/nm\n', float) / 1e9
    rf0 = peak_freqs[0]
    rf1 = peak_freqs[1]
    times = (C / wl1 - C / wl0) / (rf1 - rf0)
    ceo = C / wl0 - times * rf0
    return times, ceo


import sys


def main():
    if len(sys.argv) > 1:
        address = sys.argv[1]
    else:
        address = strip_margin(input('plz input the address of folder/file:\n\t'))
    data = read_all(address)
    axis = build_axis(data, 100e6)
    times, ceo = convert(axis, data)
    print("rf coefficients: ", times)
    print("ceo :", ceo)
    wavelength_axis = list(map(lambda f: C / (f * times + ceo) * 1e9, axis))
    plt.figure('rf')
    plt.plot(axis, data)
    plt.figure('wl')
    plt.plot(wavelength_axis, data)
    plt.xlim(1500, 1560)
    filename = os.path.basename(address).split('.')[0]
    directory = os.path.dirname(address)
    save(directory, filename + '[WavelengthAxis]', wavelength_axis)
    print()
    print("wavelength axis saved as \n\t" + directory + '\\' + filename + '[WavelengthAxis].txt')
    plt.show()


def save(directory, short_name, data_array):
    class WriteCacheTask(threading.Thread):
        def run(self):
            with open(directory + '\\' + short_name + '.txt', 'w') as fp:
                for line in data_array:
                    fp.write(str(line) + '\n')

    task = WriteCacheTask()
    task.start()
    task.join()


if __name__ == '__main__':
    main()
