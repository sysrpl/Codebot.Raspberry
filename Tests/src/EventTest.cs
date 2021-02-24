using System;
using System.Threading.Tasks;
using Codebot.Raspberry;

namespace Tests
{
    public static class EventTest
    {
        const int leftPin = 17;
        const int rightPin = 27;
        const int buttonPin = 22;

        static void TestRotaryEncoder()
        {
            Console.WriteLine("Testing event driven rotary encoder");
            var position = 1;
            var left = Pi.Gpio.Pin(leftPin, PinKind.InputPullUp);
            var right = Pi.Gpio.Pin(rightPin, PinKind.InputPullUp);
            var button = Pi.Gpio.Pin(buttonPin, PinKind.InputPullUp);
            // Connect rotate and click events
            left.OnFallingEdge += Rotate;
            button.OnRisingEdge += Click;
            // Run until the rotary encoder button is clicked
            var running = true;
            while (running)
                Pi.Wait(10);
            // Disconnect rotate and click events
            left.OnFallingEdge -= Rotate;
            button.OnRisingEdge -= Click;

            // Called on the falling edge of the left pin
            void Rotate(object sender, PinEventHandlerArgs args)
            {
                if (args.Bounced)
                    return;
                if (left.Value == right.Value)
                {
                    Console.WriteLine("counterclockwise rotate");
                    position--;
                }
                else
                {
                    Console.WriteLine("clockwise rotate");
                    position++;
                }
                Console.WriteLine("button rotate {0}", position);
            }

            // Called on the rising edge of the button pin
            void Click(object sender, PinEventHandlerArgs args)
            {
                if (args.Bounced)
                    return;
                Console.WriteLine("click");
                running = false;
            }
        }

        static void TestTimeoutWait()
        {
            Console.WriteLine("Testing wait with a timeout of 10 seconds");
            var button = Pi.Gpio.Pin(buttonPin, PinKind.InputPullUp);
            // Wait 10 seconds for the button pin rising edge 
            if (button.WaitForEdge(PinEdge.Rising, 10000))
                Console.WriteLine("Rising edge found");
            else
                Console.WriteLine("Rising edge timed out after 10 seconds");
        }

        static void TestTimeoutWaitAsync()
        {
            Console.WriteLine("Testing async wait with a timeout of 10 seconds");
            var button = Pi.Gpio.Pin(buttonPin, PinKind.InputPullUp);
            // Request a 10 second task to wait for the button pin rising edge
            var task = button.WaitForEdgeAsync(PinEdge.Rising, 10000);
            // Idly wait for the task to complete
            while (!task.IsCompleted)
                Pi.Wait(10);
            // Check if the task detected a rising edge before timeout was reached
            if (task.Result)
                Console.WriteLine("Rising edge found");
            else
                Console.WriteLine("Rising edge timed out after 10 seconds");
        }

        static void ClickNext()
        {
            Console.WriteLine("Click the button to continue");
            // Wait for a button click to continue
            Pi.Gpio.Pin(buttonPin, PinKind.InputPullUp).WaitForEdge(PinEdge.Rising);
        }

        public static void Run()
        {
            // Test the functions of a rotary encoder
            TestRotaryEncoder();
            ClickNext();
            // Test for a button click using a wait with a timeout 
            TestTimeoutWait();
            ClickNext();
            // Test for a button click using an asynchronous wait with a timeout 
            TestTimeoutWaitAsync();
        }
    }
}
