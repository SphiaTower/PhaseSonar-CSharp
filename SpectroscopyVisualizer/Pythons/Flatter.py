import threading

import numpy as np
import matplotlib.pyplot as plt


def binary_search(sorted_array, key) -> int:
    lo = 0
    hi = len(sorted_array) - 1
    while lo < hi:
        mid = int((lo + hi) / 2)
        array_mid = sorted_array[mid]
        if key > array_mid:
            lo = mid + 1
        elif key < array_mid:
            hi = mid - 1
        else:
            return mid
    return lo


def savitzky_golay(y, window_size, order, deriv=0, rate=1):
    r"""Smooth (and optionally differentiate) data with a Savitzky-Golay filter.
    The Savitzky-Golay filter removes high frequency noise from data.
    It has the advantage of preserving the original shape and
    features of the signal better than other types of filtering
    approaches, such as moving averages techniques.
    Parameters
    ----------
    y : array_like, shape (N,)
        the values of the time history of the signal.
    window_size : int
        the length of the window. Must be an odd integer number.
    order : int
        the order of the polynomial used in the filtering.
        Must be less then `window_size` - 1.
    deriv: int
        the order of the derivative to compute (default = 0 means only smoothing)
    Returns
    -------
    ys : ndarray, shape (N)
        the smoothed signal (or it's n-th derivative).
    Notes
    -----
    The Savitzky-Golay is a type of low-pass filter, particularly
    suited for smoothing noisy data. The main idea behind this
    approach is to make for each point a least-square fit with a
    polynomial of high order over a odd-sized window centered at
    the point.
    Examples
    --------
    t = np.linspace(-4, 4, 500)
    y = np.exp( -t**2 ) + np.random.normal(0, 0.05, t.shape)
    ysg = savitzky_golay(y, window_size=31, order=4)
    import matplotlib.pyplot as plt
    plt.plot(t, y, label='Noisy signal')
    plt.plot(t, np.exp(-t**2), 'k', lw=1.5, label='Original signal')
    plt.plot(t, ysg, 'r', label='Filtered signal')
    plt.legend()
    plt.show()
    References
    ----------
    .. [1] A. Savitzky, M. J. E. Golay, Smoothing and Differentiation of
       Data by Simplified Least Squares Procedures. Analytical
       Chemistry, 1964, 36 (8), pp 1627-1639.
    .. [2] Numerical Recipes 3rd Edition: The Art of Scientific Computing
       W.H. Press, S.A. Teukolsky, W.T. Vetterling, B.P. Flannery
       Cambridge University Press ISBN-13: 9780521880688
    """
    from math import factorial

    try:
        window_size = np.abs(np.int(window_size))
        order = np.abs(np.int(order))
    except ValueError:
        raise ValueError("window_size and order have to be of type int")
    if window_size % 2 != 1 or window_size < 1:
        raise TypeError("window_size size must be a positive odd number")
    if window_size < order + 2:
        raise TypeError("window_size is too small for the polynomials order")
    order_range = range(order + 1)
    half_window = (window_size - 1) // 2
    # precompute coefficients
    b = np.mat([[k ** i for i in order_range] for k in range(-half_window, half_window + 1)])
    m = np.linalg.pinv(b).A[deriv] * rate ** deriv * factorial(deriv)
    # pad the signal at the extremes with
    # values taken from the signal itself
    firstvals = y[0] - np.abs(y[1:half_window + 1][::-1] - y[0])
    lastvals = y[-1] + np.abs(y[-half_window - 1:-1][::-1] - y[-1])
    y = np.concatenate((firstvals, y, lastvals))
    return np.convolve(m[::-1], y, mode='valid')


def strip_margin(string: str):
    margin = ' "\'\n\t'
    return string.lstrip(margin).rstrip(margin)


def read_all(data_path: str):
    with open(data_path) as fp:
        return np.array(list(map(float, fp)))


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


class IntervalMarker:
    def __init__(self, axis, data, fig):
        self.click_indices = []
        self.fig = fig
        self.axis = axis
        self.data = data
        self.intervals = []

    def on_event(self, event):
        index = binary_search(self.axis, event.xdata)
        self.click_indices.append(index)
        plt.plot([event.xdata, event.xdata], [event.ydata * 0.8, event.ydata * 1.2], 'r')
        self.fig.canvas.draw()
        if len(self.click_indices) == 2:
            left, right = sorted(self.click_indices)[:2]
            # smoothed = savitzky_golay(self.data[left:right], int((right - left) / 4) * 2 + 1, 1)
            p = np.polyfit(self.axis[left:right], self.data[left:right], 2)
            smoothed = np.polyval(p, self.axis[left:right])
            plt.plot(self.axis[left:right], smoothed, 'y')
            self.intervals.append((left, right, smoothed))
            self.click_indices.clear()
            self.fig.canvas.draw()


def invoke_event(listeners, arg):
    for listener in listeners:
        listener.on_event(arg)


import scipy.interpolate as ip


def combine_intervals(array, a, b):
    return np.append(array[a[0]:a[1]], array[b[0]:b[1]])


import sys


def main():
    if len(sys.argv) > 1:
        address = sys.argv[1]
    else:
        address = strip_margin(input('plz input the address of folder/file:\n\t'))
    data = read_all(address)
    wavelength_address = address.replace('.txt', '[WavelengthAxis].txt')
    try:
        axis = read_all(wavelength_address)
    except Exception:
        print('plz generate wavelength axis 1st')
        return
    if axis[0] > axis[1]:
        axis = axis[::-1]
        data = data[::-1]
    start = binary_search(axis, 1520)
    end = binary_search(axis, 1565)

    print("Click the MIDDLE button to set the start or stop index for divisions,\n"
          "Click the RIGHT button to finish dividing and start fitting,\n"
          "CLOSE the window to finish fitting,\n"
          "Click the RIGHT button again to restart dividing.")
    fig = plt.figure()
    plt.title("Click the MIDDLE button to set the start or stop index for divisions,\n"
              "Click the RIGHT button to finish dividing and start fitting.")
    plt.xlabel("CLOSE the window to finish fitting,\n"
               "Click the RIGHT button again to restart dividing.")
    dispatcher = CanvasEventDispatcher(fig.canvas)
    axis = axis[start:end]
    data = data[start:end]
    marker = IntervalMarker(axis, data, fig)
    dispatcher.middle_click_listeners.append(marker)

    class DipFiller:
        def __init__(self):
            self._odd = False
            self.result = False

        def on_event(self, event):
            self._odd = not self._odd
            if not self._odd:
                marker.intervals.clear()
                plt.clf()
                plt.plot(axis, data)
                fig.canvas.draw()
                return
            intervals = marker.intervals
            if len(intervals) <= 1:
                return
            for i in range(len(intervals) - 1):
                current = intervals[i]
                next = intervals[i + 1]
                func = ip.interp1d(combine_intervals(axis, current, next), np.append(current[2], next[2]))
                # func = ip.interp1d(combine_intervals(axis, current, next), combine_intervals(data, current, next))
                dip_axis = axis[current[1]:next[0]]
                dip_interp = list(map(func, dip_axis))
                intervals.append((current[1], next[0], dip_interp))
                plt.plot(dip_axis, dip_interp, 'y')
            fig.canvas.draw()

            intervals.sort(key=lambda v: v[0])
            self.new_axis = np.array([])
            self.new_data = np.array([])
            for iv in intervals:
                iv_ = axis[iv[0]:iv[1]]
                self.new_axis = np.append(self.new_axis, iv_)
                l = iv[2]
                self.new_data = np.append(self.new_data, l)

            self.result = True

    filler = DipFiller()
    dispatcher.right_click_listeners.append(filler)

    plt.plot(axis, data)
    plt.show()
    if filler.result:
        plt.figure()
        plt.subplot(121)
        plt.plot(axis, data)
        plt.plot(filler.new_axis, filler.new_data)
        plt.subplot(122)
        l = divide(axis, data, filler.new_axis, filler.new_data)
        import os
        filename = os.path.basename(address).split('.')[0]
        directory = os.path.dirname(address)
        save(directory, filename + '[Flat]', filler.new_axis, l)
        plt.plot(filler.new_axis, l)
        plt.show()


def save(directory, short_name, axis_array, data_array):
    class WriteCacheTask(threading.Thread):
        def run(self):
            with open(directory + '\\' + short_name + '.txt', 'w') as fp:
                for tp in zip(axis_array, data_array):
                    fp.write(str(tp[0]) + '\t' + str(tp[1]) + '\n')

    task = WriteCacheTask()
    task.start()
    task.join()


def divide(axis1, data1, axis2, data2):
    """
    :param axis2: axis2 is shorter
    """
    start = False
    i2 = 0
    i1 = 0
    result = []
    for a in axis1:
        if i2 >= len(axis2):
            return result
        if a == axis2[0]:
            start = True
        if start:
            d1 = data1[i1]
            d2 = data2[i2]
            result.append(d1 / d2)
            i2 += 1
        i1 += 1
    return result


if __name__ == '__main__':
    main()
