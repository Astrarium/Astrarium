using System;

namespace Astrarium.Types
{
    [Flags]
    public enum ViewFlags
    {
        None                = 0,
        SingleInstance      = 1 << 1,
        TopMost             = 1 << 2,
    }
}
