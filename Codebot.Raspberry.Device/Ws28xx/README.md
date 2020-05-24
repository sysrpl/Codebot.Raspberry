# NeoPixel Strip Hardware

The Ws28xx project provides access to Ws28xx class of NeoPixel strips including Ws2811, Ws2812, and Ws2811b. 

Use of NeoPixel strips in this project is restricted to SPI communication and pin GPIO 10 of yor Pi. Before using GPIO 10 for SPI communication please refer to [this page](/Help/GPIO.md) to ensure SPI is enabled on your Pi and configured correctly.

## Example

Here is a C# example of how to use the NeoPixelStrip:

````c#
static void TestNeoPixels()
{
    // Control the first 8 NeoPixels on our strip
    using (var pixels = new NeoPixelStrip(8))
    {
        // Set the color of 8 pixels
        pixels[0] = Color.White;
        pixels[1] = Color.Red;
        pixels[2] = Color.Green;
        pixels[3] = Color.Blue;
        pixels[4] = Color.Teal;
        pixels[5] = Color.Orange;
        pixels[6] = Color.Yellow;
        pixels[7] = Color.Purple;
        // Then update the NeoPixels
        pixels.Update();
        // Wait for the user the press enter
        Console.WriteLine("Press enter to quit");
        Console.ReadLine();
        // Turn pixels off
        pixels.Reset();
        // Then update the NeoPixels
        pixels.Update();
    }
}
````

### See also

[Devices and Hardware](/README.md)
