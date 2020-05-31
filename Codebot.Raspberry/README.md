# Codebot.Raspberry

The Codebot.Raspberry assembly is your main interface to working with your Raspberry Pi. It provides access to your Pi and its hardware using abstractions to all the various hardware on your Pi and its I/O interfaces GPIO, PWM, SPI, and I2C.

## Serial Port

The Raspberry Pi includes a dedicated pair of UART pins for serial communication. These pins are located at GPIO pins 14 and 15 for transmitting (TX) and receiving (RX) serial data. Inside this assembly you'll find a SerialPort class providing you access to UART serial communication with these pins.

Before using this class and the UART pins you must enable serial communication on your Pi in one of two ways. You can use the ``raspi-config`` tool to disable the login shell through the serial pins, and enabled serial hardware port. You can also edit your ``/boot/config.txt`` file to manually enabled serial hardware ports.

<details>
  <summary>Manually enabling serial port hardware</summary>

There are two types of UART available on the Raspberry Pi - PL011 and mini UART. The PL011 is a capable, broadly 16550-compatible UART, while the mini UART has a reduced feature set. You can enabled only one of these UARTs at a any one time. 

 To enabled the PL011 UART edit ``/boot/config.txt``  and add the following line at the end.

````terminal
dtoverlay=pi3-miniuart-bt
````

After saving and rebooting the device file ``/dev/serial0`` will be linked to ``/dev/ttyAMA0`` which is the PL011 serial port. Enabling ``pi3-miniuart-bt`` will have the side effect of turning off your Pi's Bluetooth module. This is because the Bluetooth module conflicts with the PL011 hardware and its driver.

If you want to enable both Bluetooth and UART serial communication, you may instead enable the mini UART driver. The do this comment out ``dtoverlay=pi3-miniuart-bt`` in your ``/boot/config.txt`` and add these lines in its place.

````terminal
enable_uart=1
core_freq=250
core_freq_min=250
````

After saving and rebooting the device file ``/dev/serial0`` will be linked to ``/dev/ttyS0`` which is the mini UART port.

</details>

### See also

[Projects and Tools](/README.md)