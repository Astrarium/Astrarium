using System;

namespace Astrarium.Types
{
    public interface ITextureManager
    {
        Action FallbackAction { get; set; }
        int GetTexture(string path, string fallbackPath = null, bool permanent = false, Action action = null, bool alphaChannel = false);
        void Cleanup();
    }
}
