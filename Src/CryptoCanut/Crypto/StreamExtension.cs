using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoCanut.Crypto;

internal static class StreamExtension
{
    extension(Stream stream)
    {
        // see https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/partial-byte-reads-in-streams
        public int ReadFullBuffer(Span<byte> buffer)
        {
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int bytesRead = stream.Read(buffer[totalRead..]);
                if (bytesRead == 0) break;
                totalRead += bytesRead;
            }
            return totalRead;
        }

        public long CopyStream(Stream output, long sizeLimit)
        {
            const int bufferSize = 32768;
            if (sizeLimit <= 0)
                return 0;

            byte[] buffer = new byte[bufferSize];
            int read;
            long totalRead = 0;
            long remaining = sizeLimit;

            do
            {
                int toRead = (buffer.Length < remaining) ? buffer.Length : (int)remaining;
                read = stream.Read(buffer, 0, toRead);
                output.Write(buffer, 0, read);

                remaining -= read;
                totalRead += read;
            }
            while (remaining > 0 && read > 0);

            return totalRead;
        }

    }
}
