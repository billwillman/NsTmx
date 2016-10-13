using System.IO;
using System.IO.Compression;
using zlib;

namespace TmxCSharp.Loader
{
    internal static class Decompression
    {
        private static void CopyStream(Stream input, Stream output)
        {
            const int bufferSize = 4096;

            // NOTE: cannot use this due to ZLib
            // bug with output stream always reporting false for CanWrite
            //input.CopyTo(output, bufferSize);

            byte[] buffer = new byte[bufferSize];
            
            int count;

            while ((count = input.Read(buffer, 0, buffer.Length)) != 0)
            {
                output.Write(buffer, 0, count);
            }
            
            output.Flush();
        }

        public static byte[] Decompress(string compression, byte[] compressedData)
        {
            byte[] decompressedData;

            switch (compression)
            {
                case "zlib":
                    DecompressZLibData(compressedData, out decompressedData);
                    break;
                case "gzip":
                    DecompressGZipData(compressedData, out decompressedData);
                    break;
                default:
                    if (string.IsNullOrEmpty(compression))
                    {
                        decompressedData = compressedData;
                    }
                    else
                    {
                        throw new System.Exception("Unsupported compression (expected zlib or gzip)");
                    }
                    break;
            }

            return decompressedData;
        }

        private static void DecompressZLibData(byte[] inData, out byte[] outData)
        {
            // used to work.
            using (MemoryStream outMemoryStream = new MemoryStream(1000000))
            {
                using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
                {
                    using (Stream inMemoryStream = new MemoryStream(inData))
                    {
                        CopyStream(inMemoryStream, outZStream);
                        outZStream.finish();
                        outData = outMemoryStream.ToArray();
                    }
                }
            }
        }

        private static void DecompressGZipData(byte[] inData, out byte[] outData)
        {
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                using (GZipStream outGStream = new GZipStream(inMemoryStream, CompressionMode.Decompress))
                {
                    using (MemoryStream outMemoryStream = new MemoryStream())
                    {
                        CopyStream(outGStream, outMemoryStream);
                        outData = outMemoryStream.ToArray();
                    }
                }
            }
        }
    }
}
