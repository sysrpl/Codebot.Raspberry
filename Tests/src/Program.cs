using System;

namespace Tests
{

    class MainClass
    { 
        // GPIO access requires these packages: gpiod libgpiod-dev libgpiod-doc
        public static void Main(string[] args)
        {
            PulseTest.Run();
        }
    }
}
