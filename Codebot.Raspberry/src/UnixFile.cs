using System;
using System.IO;
using static Codebot.Raspberry.Libc;

/* The following modes can be used with UnixFile

   Mode   Description
   ------ ---------------------------------------------------------   
    r      read (file must exist). starts at beginning
    w      write (deletes context or creates file if it doesn't exist). starts at beginning
    a      write appending (creates file if it doesn't exist). starts at end
    r+     read write (file must exist). starts at beginning
    w+     read write (deletes context or creates file if it doesn't exist). starts at beginning
    a+     read write appending (creates file if it doesn't exist). starts at beginning  */

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
    /// A file using purely Unix functions.
    /// </summary>
    public class UnixFile : IDisposable
    {
        IntPtr file;

        void CheckDisposed()
        {
            if (file == IntPtr.Zero)
                throw new ObjectDisposedException(GetType().ToString());
        }

        /// <summary>
        /// Create or open a file given a filename and optional mode.
        /// </summary>
        public UnixFile(string fileName, string mode = "a+")
        {
            FileName = fileName;
            file = fopen(fileName, mode);
            if (file == IntPtr.Zero)
                throw new IOException($"Could not open {fileName}.");
        }

        /// <summary>
        /// Reopen the current file using a different mode.
        /// </summary>
        public void Reopen(string mode)
        {
            Dispose();
            file = fopen(FileName, mode);
            if (file == IntPtr.Zero)
                throw new IOException($"Could not open {FileName}.");
        }

        /// <summary>
        /// Open a different file.
        /// </summary>
        public void Change(string fileName, string mode = "a+")
        {
            Dispose();
            FileName = fileName;
            file = fopen(fileName, mode);
            if (file == IntPtr.Zero)
                throw new IOException($"Could not open {fileName}.");
        }

        /// <summary>
        /// EOF is true if the position indicator is and the end of the file.
        /// </summary>
        public bool EOF
        {
            get
            {
                return Position == Length;
            }
        }

        /// <summary>
        /// The first error number encountered if any are set.
        /// </summary>
        public int Error
        {
            get
            {
                CheckDisposed();
                return ferror(file);
            }
        }

        /// <summary>
        /// The name of the file associated with the unix file object.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Get the length of the file in bytes.
        /// </summary>
        public long Length
        {
            get
            {
                CheckDisposed();
                var p = ftell(file);
                fseek(file, 0, (int)UnixSeek.End);
                var length = ftell(file);
                fseek(file, p, (int)UnixSeek.Begining);
                return length;
            }
        }

        /// <summary>
        /// Gets or sets the position indicator associated with the unix file.
        /// </summary>
        /// <value>The position.</value>
        public long Position
        {
            get
            {
                CheckDisposed();
                return ftell(file);
            }
            set
            {
                Seek(value);
            }
        }

        /// <summary>
        /// Clears any errors associated with the file.
        /// </summary>
        public void ClearError()
        {
            CheckDisposed();
            clearerr(file);
        }

        /// <summary>
        /// Read from a file into an array of bytes.
        /// </summary>
        public int Read(byte[] buffer)
        {
            CheckDisposed();
            return (int)fread(buffer, (IntPtr)1, (IntPtr)buffer.Length, file);
        }

        /// <summary>
        /// Read from a file length bytes and convert it to a string.
        /// </summary>
        public string Read(int length)
        {
            var buffer = new byte[length];
            var count = Read(buffer);
            return System.Text.Encoding.UTF8.GetString(buffer, 0, count);
        }

        /// <summary>
        /// Write an array of bytes to the file.
        /// </summary>
        public int Write(byte[] buffer)
        {
            CheckDisposed();
            var written = (int)fwrite(buffer, (IntPtr)1, (IntPtr)buffer.Length, file);
            fflush(file);
            return written;
        }

        /// <summary>
        /// Write a string to the file.
        /// </summary>
        public int Write(string text)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(text);
            return Write(buffer);
        }

        /// <summary>
        /// Reset the file to have a length of zero.
        /// </summary>
        public void Reset()
        {
            CheckDisposed();
            ftruncate(fileno(file), 0);
            fflush(file);
        }

        /// <summary>
        /// Seek to a point in the file.
        /// </summary>
        public void Seek(long position, UnixSeek whence = UnixSeek.Begining)
        {
            CheckDisposed();
            fseek(file, position, (int)whence);
        }

        /// <summary>
        /// Truncate the file at its current position.
        /// </summary>
        public void Truncate()
        {
            CheckDisposed();
            ftruncate(fileno(file), ftell(file));
            fflush(file);
        }

        /// <summary>
        /// Dispose releases all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            if (file == IntPtr.Zero)
                return;
            fclose(file);
            file = IntPtr.Zero;
        }
    }
}
