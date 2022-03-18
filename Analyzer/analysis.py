from dataclasses import dataclass
import math

import matplotlib.pyplot as plt
import numpy as np
from numpy.fft import fft
import scipy.optimize
import scipy.stats


def smooth_time(t: np.array) -> np.array:
  t_unique = np.unique(t)
  indices = np.arange(t.shape[0], like=t_unique)
  t_indices = np.zeros(shape=(t_unique.size, 2))
  for i in range(t_unique.size):
    t_indices[i, 0] = indices[t == t_unique[i]].mean()
    t_indices[i, 1] = t_unique[i]
  regression = scipy.stats.linregress(t_indices[:, 0], t_indices[:, 1])
  t = indices * regression.slope + regression.intercept
  return t


def find_bump(t, v):
    v_delta = v - v.mean()
    v_delta[v_delta > 0] = 0
    v_delta = -v_delta
    v_max = v_delta.max()
    if v_max == 0:
        return None
    v_3sig = v_delta.mean() + 5 * v_delta.std()
    v_thresh = min((v_3sig + v_max) / 2, v_max)
    return t[v_delta >= v_thresh][0]


def compute_power(x: np.array, dt: float=1):
  N = x.shape[0]  # Define the total number of data points
  T = N * dt  # Define the total duration of the data

  xf = fft(x - x.mean())  # Compute Fourier transform of x
  Sxx = 2 * dt ** 2 / T * (xf * xf.conj())  # Compute spectrum
  Sxx = Sxx[:int(len(x) / 2)]  # Ignore negative frequencies

  return Sxx.real.sum()


def compute_power_spectrum(x: np.array, dt: float):
  N = x.shape[0]  # Define the total number of data points
  T = N * dt  # Define the total duration of the data

  xf = fft(x - x.mean())  # Compute Fourier transform of x
  Sxx = 2 * dt ** 2 / T * (xf * xf.conj())  # Compute spectrum
  Sxx = Sxx[:int(len(x) / 2)]  # Ignore negative frequencies

  df = 1 / T  # Determine frequency resolution
  fNQ = 1 / dt / 2  # Determine Nyquist frequency
  faxis = np.arange(0, fNQ, df)  # Construct frequency axis

  #pl.plot(faxis, Sxx.real)  # Plot spectrum vs frequency
  #pl.show()

  return faxis, Sxx.real


def find_resonant_frequency(x: np.array, dt: float):
  f, p = compute_power_spectrum(x, dt)
  fmax0 = p.argmax()
  y = p[fmax0-5:fmax0+6]
  x = f[fmax0-5:fmax0+6]
  def perr(k, mu, sigma):
    yhat = k * np.exp(-np.power(x - mu, 2) / (2 * sigma))
    return np.power(yhat - y, 2).sum()
  result = scipy.optimize.minimize(lambda v: perr(v[0], v[1], v[2]), np.array((p[fmax0], f[fmax0], f[fmax0] - f[fmax0-1])), method='Nelder-Mead')
  return result.x[1]


def compute_power_series(x: np.array, window: int=61, dt: float=1):
  p = np.zeros(x.shape)
  w = int((window - 1) / 2)
  for i in range(w, x.size - w):
    pi = compute_power(x[i-w:i+w+1], dt)
    p[i] = pi
  return p


@dataclass
class WobbleSummary:
  offset: float
  phase: float
  frequency: float

  initial_amplitude: float
  decay_constant: float

  def __str__(self):
    return '{:.2f} Hz, {:.1f} initially decaying {:.2f}/s'.format(
      self.frequency, self.initial_amplitude, self.decay_constant)


def model_wobble(t, speed, wobble: WobbleSummary):
  v_modeled = np.zeros_like(t)
  theta = wobble.phase
  for i in range(np.size(t)):
    amplitude = wobble.initial_amplitude * math.exp(-wobble.decay_constant * (t[i] - t[0]))
    v_modeled[i] = wobble.offset + amplitude * math.sin(theta)
    frequency = wobble.frequency
    theta += 2 * math.pi * (t[i + 1] - t[i]) * frequency if i < np.size(t) - 1 else 0
  return v_modeled


def summarize_wobble(t, speed, v):
  def make_summary(p):
    return WobbleSummary(offset=p[0], phase=p[1], frequency=p[2],
                         initial_amplitude=p[3], decay_constant=p[4])

  def model_error(wobble):
    v_modeled = model_wobble(t, speed, wobble)
    e = math.sqrt(np.mean(np.power(v_modeled - v, 2)))
    # plt.plot(t, v_modeled)
    # plt.plot(t, v)
    # plt.title('{}'.format(e))
    # plt.show()
    return e

  FREQUENCY_GUESS = 6  # Hz
  first_cycle = v[t < t[0] + 1 / FREQUENCY_GUESS]
  initial_amplitude_guess = (max(first_cycle) - min(first_cycle)) / 2
  result = scipy.optimize.minimize(lambda p: model_error(make_summary(p)),
                                   np.array((v.mean(), math.pi / 4, FREQUENCY_GUESS, initial_amplitude_guess, 0.1)),
                                   method='Nelder-Mead')
  for i in range(9):
    result = scipy.optimize.minimize(lambda p: model_error(make_summary(p)),
                                     result.x,
                                     method='Nelder-Mead')
  return make_summary(result.x)
