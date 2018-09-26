using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;

namespace ADK.Tests
{
    public abstract class TestClassBase
    {
        /// <summary>
        /// Reads string lines from embedded resource
        /// </summary>
        /// <param name="resourceName">Name of resource</param>
        /// <param name="encoding">Encoding to decode strings from bytes</param>
        /// <returns>Collection of string lines</returns>
        protected static IEnumerable<string> ReadLinesFromResource(string resourceName, Encoding encoding)
        {
            return ReadLines(() => Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName),encoding).ToList();
        }

        /// <summary>
        /// Reads string lines from a stream
        /// </summary>
        /// <param name="streamProvider">Function provides a stream to read from</param>
        /// <param name="encoding">Encoding to decode strings from bytes</param>
        /// <returns>Collection of string lines</returns>
        private static IEnumerable<string> ReadLines(Func<Stream> streamProvider, Encoding encoding)
        {
            using (var stream = streamProvider())
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
