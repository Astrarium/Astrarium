using Astrarium.Types;
using NLog;

namespace Astrarium.Plugins.Logger
{
    public class LoggerPlugin : AbstractPlugin
    {
        public LoggerPlugin()
        {
            Log.SetImplementation((DefaultLogger)LogManager.GetLogger("", typeof(DefaultLogger)));
        }

        public override void Initialize()
        {
            Log.Info($"Starting Astrarium...");
        }
    }
}
