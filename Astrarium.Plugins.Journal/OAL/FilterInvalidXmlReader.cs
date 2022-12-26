using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.OAL
{
    class FilterInvalidXmlReader : System.IO.TextReader
    {
        private System.IO.StreamReader _streamReader;

        public System.IO.Stream BaseStream => _streamReader.BaseStream;

        public FilterInvalidXmlReader(System.IO.Stream stream) => _streamReader = new System.IO.StreamReader(stream);

        public override void Close() => _streamReader.Close();

        protected override void Dispose(bool disposing) => _streamReader.Dispose();

        public override int Peek()
        {
            var peek = _streamReader.Peek();

            while (IsInvalid(peek, true))
            {
                _streamReader.Read();

                peek = _streamReader.Peek();
            }

            return peek;
        }

        public override int Read()
        {
            var read = _streamReader.Read();

            while (IsInvalid(read, true))
            {
                read = _streamReader.Read();
            }

            return read;
        }


        public static bool IsInvalid(int c, bool invalidateCompatibilityCharacters)
        {
            if (c == -1)
            {
                return false;
            }

            if (invalidateCompatibilityCharacters && ((c >= 0x7F && c <= 0x84) || (c >= 0x86 && c <= 0x9F) || (c >= 0xFDD0 && c <= 0xFDEF)))
            {
                return true;
            }

            if (c == 0x9 || c == 0xA || c == 0xD || (c >= 0x20 && c <= 0xD7FF) || (c >= 0xE000 && c <= 0xFFFD))
            {
                return false;
            }

            return true;
        }
    }
}
