using System.Security.Cryptography;

namespace Orchid.Core.Services;

public static class BookIdentityService
{
    public static string GenerateId(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        if (!stream.CanSeek) throw new ArgumentException("Stream must support seeking to generate a smart hash.", nameof(stream));

        long originalPosition = stream.Position;
        long fileSize = stream.Length;
        string hash;

        try
        {
            // If < 50KB - hash full
            if (fileSize < 50 * 1024)
            {
                stream.Position = 0;
                using var md5 = MD5.Create();
                var hashBytes = md5.ComputeHash(stream);
                hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
            }
            else
            {
                const int bufferSize = 8192; // 8 KB
                byte[] bufferStart = new byte[bufferSize];
                byte[] bufferEnd = new byte[bufferSize];

                // Read start 
                stream.Position = 0;
                ReadFullBuffer(stream, bufferStart);

                // Read end
                stream.Seek(-bufferSize, SeekOrigin.End);
                ReadFullBuffer(stream, bufferEnd);

                var sizeBytes = BitConverter.GetBytes(fileSize);
                
                byte[] combinedData = new byte[bufferStart.Length + bufferEnd.Length + sizeBytes.Length];
                
                Buffer.BlockCopy(bufferStart, 0, combinedData, 0, bufferStart.Length);
                Buffer.BlockCopy(bufferEnd, 0, combinedData, bufferStart.Length, bufferEnd.Length);
                Buffer.BlockCopy(sizeBytes, 0, combinedData, bufferStart.Length + bufferEnd.Length, sizeBytes.Length);

                using var md5 = MD5.Create();
                var hashBytes = md5.ComputeHash(combinedData);
                hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
            }
        }
        finally
        {
            // Reset position to original
            stream.Position = originalPosition;
        }

        return hash;
    }

    private static void ReadFullBuffer(Stream stream, byte[] buffer)
    {
        int bytesRead;
        int totalBytesRead = 0;
        while (totalBytesRead < buffer.Length && 
               (bytesRead = stream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead)) > 0)
        {
            totalBytesRead += bytesRead;
        }
    }
}
