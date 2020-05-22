# Raspberry Pi Device Hardware Interfaces 

This document outlines some of the steps that may be necessary to configure your Raspberry Pi to work with some hardware.

First, from [this github issue](https://github.com/raspberrypi/linux/issues/1983) we make PWM hardware accessible without root privileges:

Add the following to ``/etc/udev/rules.d/99-com.rules``:

```console
SUBSYSTEM=="pwm*", PROGRAM="/bin/sh -c '\
        chown -R root:gpio /sys/class/pwm && chmod -R 770 /sys/class/pwm;\
        chown -R root:gpio /sys/devices/platform/soc/*.pwm/pwm/pwmchip* && chmod -R 770 /sys/devices/platform/soc/*.pwm/pwm/pwmchip*\
'"
```
## Editing Boot Configuration

We turn on certain pin protocols by editing ``/boot/config.txt``:

```console
# turn on pwm one channel GPIO18
dtoverlay=pwm
```
# turn on spi and configure it it to work with the correct frequencies
dtparam=spi=on
core_freq=250
core_freq_min=250
```

Other hints available and a DMA neopixel libary [are located here](https://github.com/jgarff/rpi_ws281x).

``sudo reboot``
