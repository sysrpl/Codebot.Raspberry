# Raspberry Pi Dotnet Core Projects and Tools

This repository contains projects and tools for working with Raspberry Pi GPIO pins and a collection of hardware peripherals.

If you are just getting started with your Raspberry Pi and this repository please [read these topics](/Help/README.md). The contain information which might be vital accessing your Pi and using it with the hardware interfaces in this repository.

## Projects Within This Repository

This repository contains several projects which are summarized as follows:

### [Codebot.Raspberry](/Codebot.Raspberry/README.md)

The Codebot.Raspberry project provides programmatic access to your Raspberry Pi and its hardware. It contains abstractions to all the various Pi pins and their I/O interfaces GPIO, PWM, SPI, and I2C.

### [Codebot.Raspberry.Board](/Codebot.Raspberry.Board/README.md)

The Codebot.Raspberry.Board project is based off the [Microsoft IoT](https://github.com/dotnet/iot) dotnet core project. It has been modified to work with the Raspberry Pi and assumes your Pi is running a Linux operating system. You should not need to use any of the items in this project as they are all abstracted by the [Codebot.Raspberry](/Codebot.Raspberry/README.md) project.

### [Codebot.Raspberry.Device](/Codebot.Raspberry.Device/README.md)

The Codebot.Raspberry.Device folder contains multiple projects which abstract hardware devices capable of being be connected to your Raspberry Pi. Some of the hardware devices include an liquid crystal character display, NeoPixel light emitting diode strips, and temperature and humidity monitoring sensors.

## Related Repositories

### [Codebot.Web](https://github.com/sysrpl/Codebot.Web)

A sister repository to this is Codebot.Web. The Codebot.Web repository can be summarized as a simplified ASP.NET core library purpose designed to making websites and service as simple to create and manage as possible.

The Codebot.Web repository pairs well the this one when used to create web interfaces and actions to control or format information read from hardware devices connected to your Pi as user friendly web pages accessible to anyone through their web browser.