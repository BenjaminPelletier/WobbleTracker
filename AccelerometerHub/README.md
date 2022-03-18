# AccelerometerHub

## Purpose

The Arduino sketch in this folder reads four accelerometers and dumps that data in a moderately-efficient format to the USB serial port.

## Hardware

The A0 pins of the accelerometers should be connected to the pins described by `PIN_MPU_A0` in the sketch (which depends on which kind of Arduino hardware is in use, but ESP32 is recommended).  The SDA, SCL, GND, and VCC pins should be connected appropriately to all accelerometers in parallel (be careful to use the correct voltage to power the accelerometers).  See [the thread](https://forum.ih8mud.com/threads/how-can-i-figure-out-why-i-have-death-wobble.1267007/post-14122852) for more information, and/or create an issue in this repository for any additional questions.

The order of the sensors' A0 connections (to correspond with the labels in [Analyzer](../Analyzer)) is expected to be:

1. Passenger knuckle
2. Driver knuckle
3. Passenger axle
4. Driver axle

## Behavior

On startup, the Arduino should print the human-readable status of the accelerometers before starting to continuously dump accelerometer measurements.