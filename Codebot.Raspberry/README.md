# Codebot.Raspberry

The Codebot.Raspberry assembly is your main interface to working with your Raspberry Pi. It provides access to your Pi and its hardware using abstractions to all the various hardware on your Pi and its I/O interfaces GPIO, PWM, SPI, and I2C.

## Serial Port

The Raspberry Pi includes a dedicated pair of UART pins for serial communication. These pins are located at GPIO pins 14 and 15 for transmitting (TX) and receiving (RX) serial data. Inside this assembly you'll find a SerialPort class providing you access to UART serial communication using these pins.

Before using this class and the UART pins you must enable serial communication on your Pi in one of two ways. You can use the ``raspi-config`` tool to disable the login shell through the serial pins, and enabled the serial hardware port. You can also edit your ``/boot/config.txt`` file to manually enabled serial hardware ports.

<details>
  <summary>Manually enabling serial port hardware</summary>

There are two UARTs available on the Raspberry Pi - PL011 and mini UART. The PL011 is a capable, broadly 16550-compatible UART, while the mini UART has a reduced feature set. Only one of these two UARTs is available to the user at anytime.

To force the PL011 UART as your primary edit ``/boot/config.txt``  and add the following lines.

````terminal
enable_uart=1
dtoverlay=pi3-miniuart-bt
````

After saving and rebooting the device file ``/dev/serial0`` will be linked to ``/dev/ttyAMA0`` and be available on pins 14 and 15. Enabling ``pi3-miniuart-bt`` will have the side effect of cause your Pi's Bluetooth module to use the mini UART, a less capable device.

You may also want to check ``/boot/cmdline.txt`` and remove any references to serial port consoles or the serial port login shell.

</details>

### See also

[Projects and Tools](/README.md)
{"mode":"full","isActive":false}
