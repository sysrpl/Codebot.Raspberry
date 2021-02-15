# Raspberry Pi Device Hardware 

When first using your Raspberry Pi with external hardware it is helpful to configure your Pi to allow normal users access to hardware. This document contains some steps that may be necessary to either allow To normal users or certain hardware to work with your Pi.

<details>
  <summary>Allow normal users access to PWM hardware</summary>
  
Some [users have discussed](https://github.com/raspberrypi/linux/issues/1983) the problem of PWM hardware requiring root access. To fix this problem add the following to /etc/udev/rules.d/99-com.rules:

```console
SUBSYSTEM=="pwm*", PROGRAM="/bin/sh -c '\
        chown -R root:gpio /sys/class/pwm && chmod -R 770 /sys/class/pwm;\
        chown -R root:gpio /sys/devices/platform/soc/*.pwm/pwm/pwmchip* && chmod -R 770 /sys/devices/platform/soc/*.pwm/pwm/pwmchip*\
'"
```

</details>
<details>
  <summary>Editing Boot Configuration</summary>
  
In order for some devices and protocols to operate your will need to edit your /boot/config.txt to include these items:

```console
# turn on pwm one channel GPIO18
dtoverlay=pwm
# turn on spi and configure it
dtparam=spi=on
core_freq=250
core_freq_min=250
```

If you need a larger SPI size buffer, you can add this (or your preferred buffer size) to your /boot/cmdline.txt:


```console
spidev.bufsiz=65536
```

After rebooting you can check the SPI buffer size with: 

```console
cat /sys/module/spidev/parameters/bufsiz
```
</details>

## Other Information

Other possible tips about hardware configurations are [are located here](https://github.com/jgarff/rpi_ws281x).

When any of the above changes are you will need to reboot your Pi for them to take effect.

### See also

[Table of Contents](README.md)
