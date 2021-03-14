namespace Codebot.Raspberry.Device
{
    public static class BipoleMode
    {
        public const int Course = 0;
        public const int FullStep = 0;
        public const int HalfStep = 1;
        public const int QuarterStep = 2;
        public const int EightStep = 3;
        public const int SixteenthStep = 4;
        public const int ThirtySecondStep = 5;
        public const int Fine = 5;
    }

    public class BipoleDriver : IStepperDriver
    {
        const double delay = 0.02d;
        const double enableDelay = 2d;

        readonly double angle;
        int mode;
        GpioPin step;
        GpioPin dir;
        GpioPin enable;
        GpioPin m0;
        GpioPin m1;
        GpioPin m2;

        public BipoleDriver(double stepAngle, int pinStep, int pinDir, 
            int pinEnable, int pinM0, int pinM1, int pinM2)
        {
            angle = stepAngle;
            step = Pi.Gpio.Pin(pinStep, PinKind.Output);
            dir = Pi.Gpio.Pin(pinDir, PinKind.Output);
            enable = Pi.Gpio.Pin(pinEnable, PinKind.Output);
            m0 = Pi.Gpio.Pin(pinM0, PinKind.Output);
            m1 = Pi.Gpio.Pin(pinM1, PinKind.Output);
            m2 = Pi.Gpio.Pin(pinM2, PinKind.Output);
        }

        public void SetDirection(int value)
        {
            dir.Value = value < 0;
            Pi.Wait(delay);
        }

        public void SetEnable(bool value)
        {
            enable.Value = value;
            Pi.Wait(enableDelay);
        }

        public void SetMode(int value)
        {
            switch (value)
            {
                case 1:
                    System.Console.WriteLine("setting mode 1 m0 on");
                    m0.Value = true;
                    m1.Value = false;
                    m2.Value = false;
                    mode = 1;
                    break;
                case 2:
                    m0.Value = false;
                    m1.Value = true;
                    m2.Value = false;
                    mode = 2;
                    break;
                case 3:
                    m0.Value = true;
                    m1.Value = true;
                    m2.Value = false;
                    mode = 3;
                    break;
                case 4:
                    m0.Value = false;
                    m1.Value = false;
                    m2.Value = true;
                    mode = 4;
                    break;
                case 5:
                    m0.Value = true;
                    m1.Value = false;
                    m2.Value = true;
                    mode = 5;
                    break;
                default:
                    m0.Value = false;
                    m1.Value = false;
                    m2.Value = false;
                    mode = 0;
                    break;
            }
            Pi.Wait(delay);
        }

        public void Step()
        {
            step.Value = true;
            Pi.Wait(delay);
            step.Value = false;
        }

        public int GetSPR()
        {
            const double circle = 360d;

            switch (mode)
            {
                case 1:
                    return (int)(circle / angle * 2);
                case 2:
                    return (int)(circle / angle * 4);
                case 3:
                    return (int)(circle / angle * 8);
                case 4:
                    return (int)(circle / angle * 16);
                case 5:
                    return (int)(circle / angle * 32);
                default:
                    return (int)(circle / angle);
            }
        }

        public void Dispose()
        {
            if (step is null)
                return;
            step.Value = false;
            dir.Value = false;
            enable.Value = false;
            m0.Value = false;
            m1.Value = false;
            m2.Value = false;
            Pi.Wait(enableDelay);
            step.Close();
            dir.Close();
            enable.Close();
            m0.Close();
            m1.Close();
            m2.Close();
            step = null;
            dir = null;
            enable = null;
            m0 = null;
            m1 = null;
            m2 = null;
        }
    }
}
