# Analyzer

## Purpose

This folder contains a short Python script to visualize and (sometimes) automatically characterize the wobbles recorded by [DataCollector](../DataCollector).

## Usage

Install the Python packages in [requirements.txt](requirements.txt) then run [`analyze.py`](analyze.py).  This script will look for data collection artifacts in the [data folder](../data) and generalize visualizations (in the `visualizations` subfolder of the data folder) for any data collection artifact not yet visualized.  The script attempts to isolate the wobble waveform and characterize it (frequency, initial amplitude, decay rate), but the success and accuracy of this detection and characterization is hit-and-miss.

IntelliJ's PyCharm IDE is highly recommended if you do not have much experience with Python.