using System;
using System.Runtime.InteropServices;
#pragma warning disable IDE1006 // Naming Styles

namespace Codebot.Raspberry
{
    public static class Libc
    {
        const string libc = "libc.so.6";

        public const int O_RDWR = 0x002;
        public const int O_NOCTTY = 0x100;
        public const uint TCFLSH = 0x540b;
        public const int TCIFLUSH = 0;
        public const int TCIOFLUSH = 2;
        public const int TCOFLUSH = 1;
        public const int TCSANOW = 0;

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fopen64")]
        public static extern IntPtr fopen(string filename, string mode);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fclose")]
        public static extern int fclose(IntPtr file);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fread")]
        public static extern IntPtr fread(byte[] data, IntPtr size, IntPtr nmemb, IntPtr file);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fwrite")]
        public static extern IntPtr fwrite(byte[] data, IntPtr size, IntPtr number, IntPtr file);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fputs")]
        public static extern int fputs(string str, IntPtr file);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fgets")]
        public static extern IntPtr fgets(string str, int num, IntPtr file);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fprintf")]
        public static extern int fprintf(IntPtr file, string format);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fflush")]
        public static extern int fflush(IntPtr file);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ftruncate64")]
        public static extern int ftruncate(int fd, long length);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fseeko64")]
        public static extern int fseek(IntPtr file, long offset, int whence);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ftello64")]
        public static extern long ftell(IntPtr file);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ferror")]
        public static extern int ferror(IntPtr file);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "feof")]
        public static extern int feof(IntPtr file);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fileno")]
        public static extern int fileno(IntPtr file);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "clearerr")]
        public static extern void clearerr(IntPtr file);

        [StructLayout(LayoutKind.Explicit)]
        public struct TermiosStruct
        {
            [FieldOffset(0)]
            public uint c_iflag;

            [FieldOffset(4)]
            public uint c_oflag;

            [FieldOffset(8)]
            public uint c_cflag;

            [FieldOffset(12)]
            public uint c_lflag;

            [FieldOffset(16)]
            public byte c_line;

            [FieldOffset(17)]
            public byte c_cc0;

            [FieldOffset(18)]
            public byte c_cc1;

            [FieldOffset(19)]
            public byte c_cc2;

            [FieldOffset(20)]
            public byte c_cc3;

            [FieldOffset(21)]
            public byte c_cc4;

            [FieldOffset(22)]
            public byte c_cc5;

            [FieldOffset(23)]
            public byte c_cc6;

            [FieldOffset(24)]
            public byte c_cc7;

            [FieldOffset(25)]
            public byte c_cc8;

            [FieldOffset(26)]
            public byte c_cc9;

            [FieldOffset(27)]
            public byte c_cc10;

            [FieldOffset(28)]
            public byte c_cc11;

            [FieldOffset(29)]
            public byte c_cc12;

            [FieldOffset(30)]
            public byte c_cc13;

            [FieldOffset(31)]
            public byte c_cc14;

            [FieldOffset(32)]
            public byte c_cc15;

            [FieldOffset(33)]
            public byte c_cc16;

            [FieldOffset(34)]
            public byte c_cc17;

            [FieldOffset(35)]
            public byte c_cc18;

            [FieldOffset(36)]
            public byte c_cc19;

            [FieldOffset(37)]
            public byte c_cc20;

            [FieldOffset(38)]
            public byte c_cc21;

            [FieldOffset(39)]
            public byte c_cc22;

            [FieldOffset(40)]
            public byte c_cc23;

            [FieldOffset(41)]
            public byte c_cc24;

            [FieldOffset(42)]
            public byte c_cc25;

            [FieldOffset(43)]
            public byte c_cc26;

            [FieldOffset(44)]
            public byte c_cc27;

            [FieldOffset(45)]
            public byte c_cc28;

            [FieldOffset(46)]
            public byte c_cc29;

            [FieldOffset(47)]
            public byte c_cc30;

            [FieldOffset(48)]
            public byte c_cc31;

            [FieldOffset(49)]
            public byte c_cc32;

            [FieldOffset(50)]
            public byte c_cc33;

            [FieldOffset(51)]
            public byte c_cc34;

            [FieldOffset(52)]
            public uint c_ispeed;

            [FieldOffset(56)]
            public uint c_ospeed;
        }

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "open64")]
        public static extern int open(string path, int flags);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "close")]
        static public extern int close(int fd);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "write")]
        public static extern int write(int fd, byte[] buffer, int numBytes);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "read")]
        public static extern int read(int fd, byte[] buffer, int numBytes);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ioctl")]
        public static extern int ioctl(int fd, uint request, int value);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tcgetattr")] public static extern int tcgetattr(int fd, ref TermiosStruct term);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tcsetattr")]
        public static extern int tcsetattr(int fd, int actions, ref TermiosStruct term);

        public const int CLOCK_REALTIME = 0;
        public const int CLOCK_MONOTONIC = 1;
        public const int CLOCK_PROCESS_CPUTIME_ID = 2;
        public const int CLOCK_THREAD_CPUTIME_ID = 3;
        public const int CLOCK_MONOTONIC_RAW = 4;

        public struct timespec
        {
            public IntPtr tv_sec;
            public IntPtr tv_nsec;

            public static timespec FromMilliseconds(double milliseconds)
            {
                if (milliseconds < 0)
                    return new timespec();
                uint s = (uint)(milliseconds / 1000);
                long n = (long)(milliseconds - s) * 1_000_000_000;
                return new timespec()
                {
                    tv_sec = (IntPtr)s,
                    tv_nsec = (IntPtr)n
                };
            }

            public double ToMilliseconds()
            {
                return (long)tv_sec * 1000d + (long)tv_nsec / 1_000_000d;
            }
        }

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosleep")]
        public static extern int nanosleep(ref timespec request, IntPtr nullptr);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "clock_gettime")]
        public static extern int clock_gettime(int clockid, out timespec tp);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "clock_getres")]
        public static extern int clock_getres(int clockid, out timespec res);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "clock_nanosleep")]
        public static extern int clock_nanosleep(int clockid, int flags, ref timespec request, IntPtr nullptr);
    }
}