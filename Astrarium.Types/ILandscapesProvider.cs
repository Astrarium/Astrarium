using System.Collections.Generic;

namespace Astrarium.Types
{
    /// <summary>
    /// Provides access for available landscapes
    /// </summary>
    public interface ILandscapesProvider
    {
        IEnumerable<string> GetAvailableLandscapes();
    }

    /// <summary>
    /// Stub when no actual landscapes provider is registered
    /// </summary>
    public class LandscapesProviderStub : ILandscapesProvider
    {
        public IEnumerable<string> GetAvailableLandscapes()
        {
            return new string[0];
        }
    }
}
