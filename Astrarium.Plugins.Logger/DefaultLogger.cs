using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog;
using System;
using System.Linq;
using System.IO;
using Astrarium.Types;

namespace Astrarium.Plugins.Logger
{
    /// <summary>
    /// Logger wrapper for log4net.
    /// Incapsulates logging logic dependant from the log4net
    /// </summary>
    public partial class DefaultLogger : NLog.Logger, ILog
    {
        /// <summary>
        /// Path to log file
        /// </summary>
        private readonly string LOG_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "Astrarium.log");

        public DefaultLogger()
        {
            var config = new LoggingConfiguration();
            var target = new FileTarget
            {
                FileName = LOG_PATH,
                KeepFileOpen = true,
                Layout = Layout.FromString("[${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff}] - [${level}] ${logger}: ${message}")
            };

            var logLevel
#if DEBUG
            = LogLevel.Debug;
#else
            = LogLevel.Info;
#endif
            var rule = new LoggingRule("*", logLevel, target);
            config.LoggingRules.Add(rule);
            config.AddTarget("File", target);
            LogManager.Configuration = config;
        }

        public string Level
        {
            get => LogManager.Configuration.LoggingRules[0].Levels.Min().Name;
            set
            {
                LogManager.Configuration.LoggingRules[0].SetLoggingLevels(LogLevel.FromString(value), LogLevel.Fatal);
                LogManager.ReconfigExistingLoggers();
            }
        }
    }
}
