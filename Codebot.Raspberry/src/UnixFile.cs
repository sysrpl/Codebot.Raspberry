using System;

namespace Codebot.Raspberry
{
    public enum UnixSeek
    {
        // Seek from the beginning of the stream
        Begining = 0,
        // Seek from the current position of the stream
        Current = 1,
        // Seek backwards from the end of the stream
        End = 2
    }

    /// <summary>
    /// Unix file provide a writable access to file.
    /// </summary>
    public class UnixFile : IDisposable
    {
        bool disposed;
        readonly IntPtr stream;

        /// <summary>
        /// Create or open a file given a filename.
        /// </summary>
        public UnixFile(string fileName)
        {
            disposed = false;
            stream = Libc.fopen(fileName, "w");
        }

        /// <summary>
        /// Write contexts to a file at the current position.
        /// </summary>
        public void Write(string text)
        {
            Libc.fprintf(stream, text);
            Libc.fflush(stream);
        }

        /// <summary>
        /// Reset the file to have zero length.
        /// </summary>
        public void Reset()
        {
            Libc.ftruncate(Libc.fileno(stream), 0);
            Libc.fflush(stream);
        }

        /// <summary>
        /// Seek to a point in the file
        /// </summary>
        public void Seek(long position, UnixSeek whence = 0)
        {
            Libc.fseek(stream, position, (int)whence);
        }

        /// <summary>
        /// Releases all resource used by this object.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            Libc.fclose(stream);
        }
    }
}
