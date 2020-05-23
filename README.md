# Raspberry Pi Dotnet Core Projects and Tools

This repository contains projects and tools for working with Raspberry Pi GPIO pins and a collection of hardware peripherals.

If you are just getting started with your Raspberry Pi and this repository please [read these topics](/Help/README.md). The contain information which might be vital accessing your Pi and using it with the hardware interfaces in this repository.

## Projects Within This Repository

This repository contains projects which can be used for the following:

### [Raspberry](/Raspberry)

The Raspberry project provides access to your Pi and its hardware. It provides abstractions to all the various Pi pins and their I/O interfaces GPIO, PWM, SPI, and I2C.

### [Raspberry.Board](/Raspberry.Board)

The Raspberry.Board project is based off the [Microsoft IoT](https://github.com/dotnet/iot) dotnet core project. It has been modified to work with the Raspberry Pi and assumes your Pi is running a Linux operating system. You should not need to use any of the items in this project as they are all abstracted by the [Raspberry](/Raspberry) project.

### [Raspberry.Device](/Raspberry.Device)

The Raspberry.Device folder contains a growing list of projects that abstracts devices which can be connected to your Raspberry Pi. Some of the devices include hardware like LCD character displays, NeoPixel strips, and temperature and humidity sensors