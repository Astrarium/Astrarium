using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.IO;
using System.Linq;

namespace Astrarium
{
    /// <summary>
    /// Logger wrapper for log4net.
    /// Incapsulates logging logic dependant from the log4net
    /// </summary>
    public class DefaultLogger : Logger, Types.ILog
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

            var rule = new LoggingRule("*", LogLevel.Info, target);
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
