import glob
import math
import os
import warnings

from bokeh.colors import RGB, HSL
from bokeh.models import LinearAxis, Range1d
import bokeh.plotting
import matplotlib.pyplot as plt
import numpy as np

import analysis


warnings.filterwarnings("ignore", category=DeprecationWarning)

MPH_PER_KNOT = 1.15078
DEG_PER_SECOND_PER_ADC_COUNT = 2000.0 / 32767
G_PER_ADC_COUNT = 16.0 / 32767
SENSOR_CHANNELS = {
  0: 'Time',
  1: 'P knuckle ax', 2: 'P knuckle ay', 3: 'P knuckle az', 4: 'P knuckle rx', 5: 'P knuckle ry', 6: 'P knuckle rz',
  7: 'D knuckle ax', 8: 'D knuckle ay', 9: 'D knuckle az', 10: 'D knuckle rx', 11: 'D knuckle ry', 12: 'D knuckle rz',
  13: 'P axle ax', 14: 'P axle ay', 15: 'P axle az', 16: 'P axle rx', 17: 'P axle ry', 18: 'P axle rz',
  19: 'D axle ax', 20: 'D axle ay', 21: 'D axle az', 22: 'D axle rx', 23: 'D axle ry', 24: 'D axle rz',
}
GPS_CHANNELS = {0: 'Time', 1: 'Latitude', 2: 'Longitude', 3: 'Speed'}
WOBBLE_CHANNEL = 'D knuckle rz'
BUMP_CHANNELS = ['D knuckle az', 'D axle az', 'P axle az', 'P knuckle az']


def analyze(logname: str) -> bokeh.plotting.Figure:
  sensors_raw = np.loadtxt(open('../data/{}_sensors.csv'.format(logname), 'rb'), delimiter=',', skiprows=1)
  gps_raw = np.loadtxt(open('../data/{}_gps.csv'.format(logname), 'rb'), delimiter=',', skiprows=1)

  # Label sensor data channels
  sensors = {v: sensors_raw[:,k] for k, v in SENSOR_CHANNELS.items()}

  # Convert gyro data into degrees per second and accelerometer data into Gs
  for i, channel in SENSOR_CHANNELS.items():
    if i == 0:
      continue  # Skip Time channel
    if (i - 1) % 6 < 3:
      # Accelerometer channel
      sensors[channel] *= G_PER_ADC_COUNT
    else:
      # Gyro channel
      sensors[channel] *= DEG_PER_SECOND_PER_ADC_COUNT

  # Label GPS data channels
  gps = {v: gps_raw[:, k] for k, v in GPS_CHANNELS.items()}

  # Put gps speed in MPH
  gps['Speed'] *= MPH_PER_KNOT

  # Reduce timestamp discretization in sensor data
  sensors['Time'] = analysis.smooth_time(sensors['Time'])

  # Find first bump
  bumps = [analysis.find_bump(sensors['Time'], sensors[channel])
           for channel in BUMP_CHANNELS]
  t_bump = np.median(np.fromiter((t for t in bumps if t is not None), float))

  # Find second bump
  MIN_SECOND_BUMP_DELAY = 0.2  # seconds
  mask = sensors['Time'] >= t_bump + MIN_SECOND_BUMP_DELAY
  bumps2 = [analysis.find_bump(sensors['Time'][mask], sensors[channel][mask])
            for channel in BUMP_CHANNELS]
  t_bump2 = np.median(np.fromiter((t for t in bumps2 if t is not None), float))
  if t_bump2 > t_bump + 0.5:
    print('  Warning: bump interval {}s'.format(t_bump2 - t_bump))

  # Crop data to wobble period
  t_stop = t_bump + 4
  t_start = t_bump - 1
  sensor_mask = np.logical_and(t_start <= sensors['Time'], sensors['Time'] <= t_stop)
  for k in SENSOR_CHANNELS.values():
    sensors[k] = sensors[k][sensor_mask]
  gps_mask = np.logical_and(t_start <= gps['Time'], gps['Time'] <= t_stop)
  for k in GPS_CHANNELS.values():
    gps[k] = gps[k][gps_mask]

  # Find resonant frequency
  f_resonant = analysis.find_resonant_frequency(
    sensors[WOBBLE_CHANNEL], (sensors['Time'][-1:] - sensors['Time'][0]) / sensors['Time'].size)

  # Summarize information about wobble
  sensors_mask = sensors['Time'] > t_bump2 + (t_bump2 - t_bump) / 2
  t_wobble = sensors['Time'][sensors_mask]
  wobble_speed = np.interp(t_wobble, gps['Time'], gps['Speed'])
  wobble_summary = analysis.summarize_wobble(
    t_wobble, wobble_speed, sensors[WOBBLE_CHANNEL][sensors_mask])

  # Compute total spectral power as a function of time
  #sensors_pwr = np.copy(sensors_raw)
  #for c in range(1, sensors_raw.shape[1]):
  #  sensors_pwr[:,c] = compute_power_series(sensors_raw[:, c], 61, 1 / 120)

  p = bokeh.plotting.figure(
    title='Sensor data {}; {:.2f} Hz resonance: {}, measured at {:.1f}'.format(
      logname, f_resonant, str(wobble_summary), np.interp(t_bump2, gps['Time'], gps['Speed'])),
    x_axis_label='Time (seconds)',
    y_axis_label='Acceleration (G\'s)',
    width=1400,
    height=700,
    y_range=(-10, 10))

  # Plot GPS speed on new axis
  p.extra_y_ranges = {'speed': Range1d(start=00, end=50), 'gyro': Range1d(start=-250, end=250)}
  p.add_layout(LinearAxis(y_range_name='speed', axis_label='Speed (mph)'), 'right')
  feature = p.line(
    gps['Time'] - t_bump, gps['Speed'], legend_label='Speed', line_width=1, line_color='black',
    y_range_name='speed')

  # Add gyro axis
  p.add_layout(LinearAxis(y_range_name='gyro', axis_label='Rotation (deg/s)'), 'right')

  # Plot modeled gyro output from wobbling sensor
  modeled_wobble = analysis.model_wobble(t_wobble, wobble_speed, wobble_summary)
  feature = p.line(t_wobble - t_bump, modeled_wobble, legend_label='Modeled wobble', line_width=1,
                   line_color=bokeh.colors.RGB(0, 0, 0, 0.3), y_range_name='gyro')

  for i, (key, v) in enumerate(sensors.items()):
    if key == 'Time':
      continue
    s = (i - 1) % 6
    h = 360 * s / 6
    l = 0.25 + 0.5 * math.floor((i - 1) / 6) / 3
    color = RGB.from_hsl(HSL(h, 1, l))
    if s >= 3:
      feature = p.line(sensors['Time'] - t_bump, v,
                       legend_label=key, line_width=1, line_color=color, y_range_name='gyro')
    else:
      feature = p.line(sensors['Time'] - t_bump, v, legend_label=key, line_width=1, line_color=color)
    if key not in {'P knuckle rz', 'P knuckle az'}:
      feature.visible = False

  p.legend.location = "top_left"
  p.legend.click_policy = "hide"

  return p


if __name__ == '__main__':
  viz_path = '../data/visualizations'
  os.makedirs(viz_path, exist_ok=True)
  sensor_files = glob.glob('../data/*_sensors.csv')
  for sensor_file in sensor_files:
    logname = os.path.split(sensor_file)[-1][0:-len('_sensors.csv')]
    html_file = os.path.join(viz_path, logname + '.html')
    if not os.path.exists(html_file):
      print('Processing {}...'.format(logname))
      try:
        p = analyze(logname)
        bokeh.plotting.output_file(html_file, title=logname, mode='inline')
        bokeh.plotting.save(p)
      except ValueError as e:
        print('  ValueError: {}'.format(e))
      # except IndexError as e:
      #   print('  IndexError: {}'.format(e))
      except StopIteration as e:
        print('  StopIteration: {}'.format(e))
    else:
      print('Skipping {} (already rendered)'.format(logname))
