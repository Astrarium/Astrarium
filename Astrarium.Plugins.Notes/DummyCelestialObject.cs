using Astrarium.Types;

namespace Astrarium.Plugins.Notes
{
    /// <summary>
    /// Used for unknown objects found in notes
    /// </summary>
    internal class DummyCelestialObject : CelestialObject
    {
        public DummyCelestialObject(string type, string name) 
        { 
            typeHolder = type;
            nameHolder = name;
        }

        private string typeHolder;
        private string nameHolder;

        public override string[] Names => new string[] { nameHolder };
        public override string[] DisplaySettingNames => new string[0];
        public override string Type => typeHolder;
        public override string CommonName => nameHolder;
    }
}
