using Raspberry;

namespace Raspberry.Device
{
    public enum LcdTextDirection
    {
        /// <summary>
        /// Rext flows to the right from the cursor, as if the display is left-justified.
        /// </summary>
        LeftToRight,
        /// <summary>
        /// Text flows to the left from the cursor, as if the display is right-justified
        /// </summary>
        RightToLeft
    }

    /// <summary>
    /// The Lcd class provides an interface to a HD44780 dot-matrix liquid 
    /// crystal display controller
    /// 
    /// See also https://en.wikipedia.org/wiki/Hitachi_HD44780_LCD_controller
    /// </summary>
    [Device("HD44780", "Liquid Crystal Character Display", Category = "Display", Remarks = "Uses any GPIO")]
    public class CharacterLcd : HardwareDevice
    {
        // Commands
        private const byte LCD_CLEARDISPLAY = 0x01;
        private const byte LCD_RETURNHOME = 0x02;
        private const byte LCD_ENTRYMODESET = 0x04;
        private const byte LCD_DISPLAYCONTROL = 0x08;
        private const byte LCD_CURSORSHIFT = 0x10;
        private const byte LCD_FUNCTIONSET = 0x20;
        private const byte LCD_SETCGRAMADDR = 0x40;
        private const byte LCD_SETDDRAMADDR = 0x80;

        // Flags for display entry mode
        private const byte LCD_ENTRYRIGHT = 0x00;
        private const byte LCD_ENTRYLEFT = 0x02;
        private const byte LCD_ENTRYSHIFTINCREMENT = 0x01;
        private const byte LCD_ENTRYSHIFTDECREMENT = 0x00;

        // Flags for display on/off control
        private const byte LCD_DISPLAYON = 0x04;
        private const byte LCD_DISPLAYOFF = 0x00;
        private const byte LCD_CURSORON = 0x02;
        private const byte LCD_CURSOROFF = 0x00;
        private const byte LCD_BLINKON = 0x01;
        private const byte LCD_BLINKOFF = 0x00;

        // Flags for display/cursor shift
        private const byte LCD_DISPLAYMOVE = 0x08;
        private const byte LCD_CURSORMOVE = 0x00;
        private const byte LCD_MOVERIGHT = 0x04;
        private const byte LCD_MOVELEFT = 0x00;

        // Flags for function set
        private const byte LCD_8BITMODE = 0x10;
        private const byte LCD_4BITMODE = 0x00;
        private const byte LCD_2LINE = 0x08;
        private const byte LCD_1LINE = 0x00;
        private const byte LCD_5x10DOTS = 0x04;
        private const byte LCD_5x8DOTS = 0x00;

        private const bool LOW = false;
        private const bool HIGH = true;

        public int Cols { private set; get; }
        public int Rows { private set; get; }

        private GpioPin selectPin;  // Low for commands, high for characters
        private GpioPin enablePin;  // Activated by a high pulse
        private GpioPin[] dataPins;

        private byte functionBits;
        private byte controlBits;
        private byte modeBits;

        private static void Delay(int microseconds)
        {
            Pi.WaitMicroseconds(microseconds);
        }

        private void Init(bool fourBitMode, byte select, byte enable,
            byte d0, byte d1, byte d2, byte d3, byte d4, byte d5, byte d6, byte d7)
        {
            selectPin = Pi.Gpio.Pin(select);
            enablePin = Pi.Gpio.Pin(enable);
            dataPins[0] = Pi.Gpio.Pin(d0);
            dataPins[1] = Pi.Gpio.Pin(d1);
            dataPins[2] = Pi.Gpio.Pin(d2);
            dataPins[3] = Pi.Gpio.Pin(d3);
            dataPins[4] = Pi.Gpio.Pin(d4);
            dataPins[5] = Pi.Gpio.Pin(d5);
            dataPins[6] = Pi.Gpio.Pin(d6);
            dataPins[7] = Pi.Gpio.Pin(d7);
            selectPin.Mode = GpioPinMode.Output;
            enablePin.Mode = GpioPinMode.Output;
            dataPins[0].Mode = GpioPinMode.Output;
            dataPins[1].Mode = GpioPinMode.Output;
            dataPins[2].Mode = GpioPinMode.Output;
            dataPins[3].Mode = GpioPinMode.Output;
            dataPins[4].Mode = GpioPinMode.Output;
            dataPins[5].Mode = GpioPinMode.Output;
            dataPins[6].Mode = GpioPinMode.Output;
            dataPins[7].Mode = GpioPinMode.Output;
            if (fourBitMode)
                functionBits = LCD_4BITMODE;
            else
                functionBits = LCD_8BITMODE;
            Setup(20, 4, false);
        }

        protected CharacterLcd()
        {
            dataPins = new GpioPin[8];
        }

        public CharacterLcd(byte select, byte enable,
            byte d0, byte d1, byte d2, byte d3) : this()
        {
            Init(true, select, enable, d0, d1, d2, d3, 0, 0, 0, 0);
        }

        public CharacterLcd(byte select, byte enable,
            byte d0, byte d1, byte d2, byte d3, byte d4, byte d5, byte d6, byte d7) : this()
        {
            Init(false, select, enable, d0, d1, d2, d3, d4, d5, d6, d7);
        }

        public void Setup(int cols, int rows, bool largeFont = false)
        {
            // When the display powers up, it is configured as follows:
            //
            // 1. Display clear
            // 2. Function set: 
            //    DL = 1; 8-bit interface data 
            //    N = 0; 1-line display 
            //    F = 0; 5x8 dot character font 
            // 3. Display on/off control: 
            //    D = 0; Display off 
            //    C = 0; Cursor off 
            //    B = 0; Blinking off 
            // 4. Entry mode set: 
            //    I/D = 1; Increment by 1 
            //    S = 0; No shift 
            //
            // Note, however, that resetting the Pi doesn't reset the LCD, so we
            // can't assume that its in that state when a sketch starts (and the
            // constructor is called).

            // Pull both register slect and enable pins low to start
            selectPin.Write(LOW);
            enablePin.Write(LOW);
            if (cols < 8)
                cols = 8;
            else if (cols > 20)
                cols = 20;
            if (rows < 1)
                rows = 1;
            else if (rows > 4)
                rows = 4;
            Cols = cols;
            Rows = rows;
            if (Rows > 1)
                functionBits |= LCD_2LINE;
            else
                functionBits &= unchecked((byte)~LCD_2LINE);

            byte dotsize = largeFont ? LCD_5x10DOTS : LCD_5x8DOTS;

            // A 10 pixel high font can only be used on a single line display
            if ((dotsize == LCD_5x10DOTS) && (rows == 1))
                functionBits |= LCD_5x10DOTS;
            else
                functionBits &= unchecked((byte)~LCD_5x10DOTS);

            // SEE PAGE 45/46 FOR INITIALIZATION SPECIFICATION!
            // according to datasheet, we need at least 40ms after power rises above 2.7V
            // before sending commands.
            Delay(5000);

            // Put the LCD into 4 bit or 8 bit mode
            if ((functionBits & LCD_8BITMODE) == 0)
            {
                // this is according to the hitachi HD44780 datasheet
                // figure 24, pg 46
                // We start in 8bit mode, try to set 4 bit mode
                Write4bits(0x03);
                Delay(5000); // Wait
                Write4bits(0x03);
                Delay(5000); // Wait 
                Write4bits(0x03);
                Delay(3000); // Wait 
                // Finally, set to 4-bit interface
                Write4bits(0x02);
            }
            else
            {
                // This is according to the hitachi HD44780 datasheet
                // page 45 figure 23
                Command((byte)(LCD_FUNCTIONSET | functionBits));
                Delay(5000);  // Wait 
                Command((byte)(LCD_FUNCTIONSET | functionBits));
                Delay(3000); // Wait 
                Command((byte)(LCD_FUNCTIONSET | functionBits));
            }
            // Set number of lines and font size again
            Command((byte)(LCD_FUNCTIONSET | functionBits));

            Clear();
            Home();

            // Turn the display on with no cursor or blinking default
            controlBits = LCD_DISPLAYON | LCD_CURSOROFF | LCD_BLINKOFF;
            Command((byte)(LCD_DISPLAYCONTROL | controlBits));

            // Set the default text direction 
            modeBits = LCD_ENTRYLEFT | LCD_ENTRYSHIFTDECREMENT;
            Command((byte)(LCD_ENTRYMODESET | modeBits));
        }

        private void PulseEnable()
        {
            enablePin.Write(LOW);
            Delay(20);
            enablePin.Write(HIGH);
            Delay(20);    // Enable pulse must be > 450ns
            enablePin.Write(LOW);
            Delay(100);   // Commands need > 37μs to settle
        }

        private void Write4bits(byte value)
        {
            for (int i = 0; i < 4; i++)
                dataPins[i].Write(((value >> i) & 0x01) == 1);
            PulseEnable();
        }

        private void Write8bits(byte value)
        {
            for (int i = 0; i < 8; i++)
                dataPins[i].Write(((value >> i) & 0x01) == 1);
            PulseEnable();
        }

        private void WriteByte(byte value, bool characters)
        {
            selectPin.Write(characters);
            if ((functionBits & LCD_8BITMODE) > 0)
                Write8bits(value);
            else
            {
                Write4bits((byte)(value >> 4));
                Write4bits(value);
            }
        }

        private void Command(byte value)
        {
            WriteByte(value, false);
        }

        #region Methods
        /// <summary>
        /// Clear display and reset cursor position 
        /// </summary>
        public void Clear()
        {
            Command(LCD_CLEARDISPLAY);
            Delay(3000); // This command takes a long time!
        }

        /// <summary>
        /// Set cursor position to zero
        /// </summary>
        public void Home()
        {
            Command(LCD_RETURNHOME);
            Delay(3000); // This command takes a long time!
        }

        /// <summary>
        /// Move the cursor to a column and row
        /// </summary>
        public void MoveCursor(int col, int row)
        {
            if (col < 0)
                col = 0;
            else if (col >= Cols)
                col = Cols - 1;
            if (row < 0)
                row = 0;
            else if (row >= Rows)
                row = Rows - 1;
            int addr;
            switch (row)
            {
                case 0:
                    addr = 0;
                    break;
                case 1:
                    addr = 0x40;
                    break;
                case 2:
                    addr = Cols;
                    break;
                case 3:
                    addr = Cols + 0x40;
                    break;
                default:
                    return;
            }
            Command((byte)(LCD_SETDDRAMADDR | (col + addr)));
        }

        /// <summary>
        /// Scroll the display and cursor left one character
        /// </summary>
        public void ScrollLeft()
        {
            Command(LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVELEFT);
        }

        /// <summary>
        /// Scroll the display and cursor right one character
        /// </summary>
        public void ScrollRight()
        {
            Command(LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVERIGHT);
        }

        /// <summary>
        /// Write some text to the lcd
        /// </summary>
        public void Write(string s)
        {
            if (string.IsNullOrEmpty(s))
                return;
            var buffer = System.Text.Encoding.ASCII.GetBytes(s);
            foreach (var b in buffer)
                WriteByte(b, true);
        }
        #endregion

        #region Properties
        /// <summary>
        /// When AutoScroll is true all the text is moved one space to the left 
        /// each time a letter is added
        /// </summary>
        public bool AutoScroll
        {
            get => (modeBits & LCD_ENTRYSHIFTINCREMENT) == LCD_ENTRYSHIFTINCREMENT;
            set
            {
                if (value)
                    modeBits |= LCD_ENTRYSHIFTINCREMENT;
                else
                    modeBits &= unchecked((byte)~LCD_ENTRYSHIFTINCREMENT);
                Command((byte)(LCD_ENTRYMODESET | modeBits));
            }
        }

        /// <summary>
        /// When Blinking is true the cursor will blink at regular periodic intervals 
        /// </summary>
        public bool Blinking
        {
            get => (controlBits & LCD_BLINKON) == LCD_BLINKON;
            set
            {
                if (value)
                    controlBits |= LCD_BLINKON;
                else
                    controlBits &= unchecked((byte)~LCD_BLINKON);
                Command((byte)(LCD_DISPLAYCONTROL | controlBits));
            }
        }

        /// <summary>
        /// When Cursor is true the display will show the the cursor
        /// </summary>
        public bool Cursor
        {
            get => (controlBits & LCD_CURSORON) == LCD_CURSORON;
            set
            {
                if (value)
                    controlBits |= LCD_CURSORON;
                else
                    controlBits &= unchecked((byte)~LCD_CURSORON);
                Command((byte)(LCD_DISPLAYCONTROL | controlBits));
            }
        }

        /// <summary>
        /// When Display is false the text and cursor are hidden from view
        /// </summary>
        public bool Display
        {
            get => (controlBits & LCD_DISPLAYON) == LCD_DISPLAYON;
            set
            {
                if (value)
                    controlBits |= LCD_DISPLAYON;
                else
                    controlBits &= unchecked((byte)~LCD_DISPLAYON);
                Command((byte)(LCD_DISPLAYCONTROL | controlBits));
            }
        }

        /// <summary>
        /// Gets or sets the text direction
        /// </summary>
        public LcdTextDirection TextDirection
        {
            get => (modeBits & LCD_ENTRYLEFT) == LCD_ENTRYLEFT ?
                LcdTextDirection.LeftToRight : LcdTextDirection.RightToLeft;
            set
            {
                if (value == LcdTextDirection.LeftToRight)
                    modeBits |= LCD_ENTRYLEFT;
                else
                    modeBits &= unchecked((byte)~LCD_ENTRYLEFT);
                Command((byte)(LCD_ENTRYMODESET | modeBits));

            }
        }
        #endregion
    }
}
