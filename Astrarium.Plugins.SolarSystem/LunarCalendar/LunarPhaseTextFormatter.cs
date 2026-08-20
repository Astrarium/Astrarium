using Astrarium.Algorithms;
using Astrarium.Types;

namespace Astrarium.Plugins.SolarSystem
{
    public class LunarPhaseTextFormatter : IEphemFormatter
    {
        public string Format(object value)
        {
            var phase = value as MoonPhase?;
            if (phase == null)
                return null;
            else
                return Text.Get($"Moon.ShortPhases.{phase}");
        }
    }
}
