using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Astrarium
{
    public static class CursorsHelper
    {
        [DllImport("user32.dll")]
        static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern Int32 SystemParametersInfo(UInt32 uiAction, UInt32 uiParam, String pvParam, UInt32 fWinIni);

        [DllImport("user32.dll")]
        static extern IntPtr CopyIcon(IntPtr pcur);

        [DllImport("user32.dll")]
        static extern IntPtr CreateIconFromResource(IntPtr pbIconBits, int dwResSize, bool fIcon, int dwVer);

        /// <summary>
        /// Standard arrow and small hourglass
        /// </summary>
        const uint OCR_APPSTARTING = 32650;

        /// <summary>
        /// Standard arrow
        /// </summary>
        const uint OCR_NORMAL = 32512;

        /// <summary>
        /// Crosshair
        /// </summary>
        const uint OCR_CROSS = 32515;

        /// <summary>
        /// Windows 2000/XP: Hand
        /// </summary>
        const uint OCR_HAND = 32649;

        /// <summary>
        /// Arrow and question mark
        /// </summary>
        const uint OCR_HELP = 32651;

        /// <summary>
        /// I-beam
        /// </summary>
        const uint OCR_IBEAM = 32513;

        /// <summary>
        /// Slashed circle
        /// </summary>
        const uint OCR_NO = 32648;

        /// <summary>
        /// Four-pointed arrow pointing north, south, east, and west
        /// </summary>
        const uint OCR_SIZEALL = 32646;

        /// <summary>
        /// Double-pointed arrow pointing northeast and southwest
        /// </summary>
        const uint OCR_SIZENESW = 32643;

        /// <summary>
        /// Double-pointed arrow pointing north and south
        /// </summary>
        const uint OCR_SIZENS = 32645;

        /// <summary>
        /// Double-pointed arrow pointing northwest and southeast
        /// </summary>
        const uint OCR_SIZENWSE = 32642;

        /// <summary>
        /// Double-pointed arrow pointing west and east
        /// </summary>
        const uint OCR_SIZEWE = 32644;

        /// <summary>
        /// Vertical arrow
        /// </summary>
        const uint OCR_UP = 32516;

        /// <summary>
        /// Hourglass
        /// </summary>
        const uint OCR_WAIT = 32514;

        static Dictionary<uint, string> CursorsFiles = new Dictionary<uint, string>()
        {
            [OCR_NORMAL] = "Normal.cur",
            [OCR_SIZENS] = "SizeNS.cur",
            [OCR_SIZEWE] = "SizeWE.cur",
            [OCR_SIZENESW] = "SizeNESW.cur",
            [OCR_SIZENWSE] = "SizeNWSE.cur",
            [OCR_IBEAM] = "IBeam.cur",
            [OCR_CROSS] = "Cross.cur",
            [OCR_HAND] = "Hand.cur",
            [OCR_NO] = "No.cur"
        };

        static Dictionary<uint, IntPtr> CursorsHandles = new Dictionary<uint, IntPtr>();

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        struct IconHeader
        {
            [FieldOffset(0)]
            public short reserved;

            [FieldOffset(2)]
            public short type;

            [FieldOffset(4)]
            public short count;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        struct IconInfo
        {
            [FieldOffset(0)]
            public byte width;

            [FieldOffset(1)]
            public byte height;

            [FieldOffset(2)]
            public byte colors;

            [FieldOffset(3)]
            public byte reserved;

            [FieldOffset(4)]
            public short planes;

            [FieldOffset(6)]
            public short bpp;

            [FieldOffset(4)]
            public short hotspot_x;

            [FieldOffset(6)]
            public short hotspot_y;

            [FieldOffset(8)]
            public int size;

            [FieldOffset(12)]
            public int offset;
        }

        public static void SetSystemCursors()
        {
            SystemParametersInfo(0x0057, 0, null, 0);
        }

        public static void SetCustomCursors()
        {
            foreach (var kv in CursorsFiles)
            {
                uint cursorType = kv.Key;
                string cursorName = kv.Value;
                var ptr = CursorsHandles[cursorType];
                SetSystemCursor(CopyIcon(ptr), cursorType);
            }
        }

        static IntPtr LoadEmbeddedCursor(byte[] cursorResource, int imageIndex = 0)
        {
            var resourceHandle = GCHandle.Alloc(cursorResource, GCHandleType.Pinned);

            var header = (IconHeader)Marshal.PtrToStructure(resourceHandle.AddrOfPinnedObject(), typeof(IconHeader));

            if (imageIndex >= header.count)
                throw new ArgumentOutOfRangeException(nameof(imageIndex));

            var iconInfoPtr = resourceHandle.AddrOfPinnedObject() + Marshal.SizeOf(typeof(IconHeader)) + imageIndex * Marshal.SizeOf(typeof(IconInfo));
            var info = (IconInfo)Marshal.PtrToStructure(iconInfoPtr, typeof(IconInfo));

            var iconImage = Marshal.AllocHGlobal(info.size + 4);
            Marshal.WriteInt16(iconImage + 0, info.hotspot_x);
            Marshal.WriteInt16(iconImage + 2, info.hotspot_y);
            Marshal.Copy(cursorResource, info.offset, iconImage + 4, info.size);

            return CreateIconFromResource(iconImage, info.size + 4, false, 0x30000);
        }

        static byte[] ExtractResource(string filename)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream resFilestream = a.GetManifestResourceStream(filename))
            {
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        static CursorsHelper()
        {
            foreach (var kv in CursorsFiles)
            {
                uint cursorType = kv.Key;
                string cursorName = kv.Value;
                CursorsHandles[cursorType] = LoadEmbeddedCursor(ExtractResource($"Astrarium.Cursors.{cursorName}"));
            }
        }
    }
}
