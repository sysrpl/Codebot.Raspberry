using System;
using System.Runtime.InteropServices;
#pragma warning disable IDE1006 // Naming Styles

namespace Codebot.Raspberry
{
    public static class Libc
    {
        const string libc = "libc.so.6";

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fopen", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr fopen(string filename, string mode);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fclose", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int fclose(IntPtr file);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fprintf", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int fprintf(IntPtr file, string format);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fflush", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int fflush(IntPtr file);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ftruncate", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int ftruncate(IntPtr file, ulong length);

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fseek", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int fseek(IntPtr file, long offset, int whence);

        public const int SEEK_SET = 0;
        public const int SEEK_CUR = 1;
        public const int SEEK_END = 2;

        [DllImport(libc, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fileno", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr fileno(IntPtr file);
    }
}
