using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    internal static class StreamExtensions
    {
        /// <summary>
        /// Converts the content of the stream to a base64 string.
        /// </summary>
        /// <param name="stream">The stream to convert.</param>
        /// <returns>The base64 string representation of the stream content.</returns>
        public static string ToBase64String(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }
}
