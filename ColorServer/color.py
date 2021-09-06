#!/usr/bin/python3

import time
import board
import sys
import RPi.GPIO as GPIO
import adafruit_tcs34725

led = 23
GPIO.setmode(GPIO.BCM)
GPIO.setup(led, GPIO.OUT)
GPIO.output(led, 1)

# Create sensor object, communicating over the board's default I2C bus
i2c = board.I2C()  # uses board.SCL and board.SDA
sensor = adafruit_tcs34725.TCS34725(i2c)

# Change sensor integration time to values between 2.4 and 614.4 milliseconds
if len(sys.argv) > 1:
	sensor.integration_time = float(sys.argv[1])
	
# Change sensor gain to 1, 4, 16, or 60
if len(sys.argv) > 2:
	sensor.gain = float(sys.argv[2])

def getColor(r, g, b):
    text = '██████████'
    color = "\033[38;2;{};{};{}m{} \033[38;2;255;255;255m".format(r, g, b, text)
    return color 

# data from the sensor in a 4-tuple of red, green, blue, clear light component values
r, g, b, a = sensor.color_raw
print("[r g b]", r / a, g / a, b / a)
GPIO.output(led, 0)
# r = int(r / a * 255.0)
# g = int(g / a * 255.0)
# b = int(b / a * 255.0)
# s = getColor(r, g, b)
# print(s)
