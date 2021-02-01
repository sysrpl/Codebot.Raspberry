using System;
using System.Linq;
using System.Collections.Generic;

namespace Codebot.Raspberry.Device
{
    public sealed class ConsoleLcd
    {
        private readonly CharacterLcd lcd;
        private readonly List<string> lines = new List<string>();

        public ConsoleLcd(CharacterLcd lcd)
        {
            this.lcd = lcd;
            this.lcd.Display = false;
            this.lcd.Clear();
            this.lcd.Blinking = false;
            this.lcd.Cursor = false;
            this.lcd.AutoScroll = false;
            this.lcd.TextDirection = LcdTextDirection.LeftToRight;
            this.lcd.Display = true;
            lines.Add(string.Empty);
            update = 0;
            row = 0;
        }

        private int update;
        private int row;

        public int RowCount { get => lines.Count; }
        public int Row { get => row; set => ScrollTo(value); }

        public void ScrollTo(int row)
        {
            if (row < 0)
                row = 0;
            if (row >= lines.Count)
                row = lines.Count - 1;
            if (row == this.row)
                return;
            this.row = row;
            Refresh();
        }

        private void Refresh()
        {
            lcd.Display = false;
            int r = row;
            if (lines[r].Length == 0)
                r--;
            string s;
            for (var i = 0; i < lcd.Rows; i++)
            {
                if (r < 0)
                    s = string.Empty;
                else
                    s = lines[r];
                lcd.MoveCursor(0, lcd.Rows - i - 1);
                lcd.Write(s.PadRight(lcd.Cols));
                r--;
            }
            lcd.Display = true;
        }

        public void BeginUpdate()
        {
            update++;
        }

        public void EndUpdate()
        {
            update--;
            if (update < 1)
            {
                update = 0;
                row = lines.Count - 1;
                Refresh();
            }
        }

        public void Clear()
        {
            BeginUpdate();
            lines.Clear();
            lines.Add(string.Empty);
            EndUpdate();
        }

        public void NewLine()
        {
            BeginUpdate();
            lines.Add(string.Empty);
            row++;
            EndUpdate();
        }

        private void WriteOne(string s)
        {
            var a = lines.Last() + s;
            var b = string.Empty;
            while (a.Length > 0)
            {
                if (a.Length > lcd.Cols)
                    b = a.Substring(lcd.Cols);
                else
                    b = string.Empty;
                a = a.Substring(0, Math.Min(a.Length, lcd.Cols));
                a = lines[lines.Count - 1] + a;
                lines[lines.Count - 1] = a.Trim();
                lines.Add(string.Empty);
                a = b;
            }
            if (lines.Last().Length == 0)
                lines.RemoveAt(lines.Count - 1);
        }

        public void Write(string s)
        {
            BeginUpdate();
            string[] items = s.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );
            int i = 0;
            foreach (var item in items)
            {
                if ((i > 0) || (item.Length == 0))
                    lines.Add(string.Empty);
                WriteOne(item);
                i++;
            }
            update--;
            EndUpdate();
        }

        public void WriteLine(string s)
        {
            BeginUpdate();
            Write(s);
            NewLine();
            EndUpdate();
        }
    }
}
