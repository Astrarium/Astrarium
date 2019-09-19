using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.IO;
using ILogger = Planetarium.Types.ILogger;

namespace Planetarium.Logging
{
    /// <summary>
    /// Logger wrapper for log4net.
    /// Incapsulates logging logic dependant from the log4net
    /// </summary>
    public class Logger : ILogger
    {
        private readonly string LOG_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ADK", "Planetarium.log");

        /// <summary>
        /// log4net logger instance 
        /// </summary>
        private ILog logger;

        /// <summary>
        /// Creates new logger
        /// </summary>
        public Logger()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout("%date [%thread] %-5level - %message%newline");
            patternLayout.ActivateOptions();

            RollingFileAppender fileAppender = new RollingFileAppender()
            {
                AppendToFile = true,
                File = LOG_PATH,
                Layout = patternLayout,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "1GB",
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true
            };

            fileAppender.ActivateOptions();

            ConsoleAppender consoleAppender = new ConsoleAppender()
            {
                Layout = patternLayout
            };
            consoleAppender.ActivateOptions();

            hierarchy.Root.AddAppender(fileAppender);
            hierarchy.Root.AddAppender(consoleAppender);
            hierarchy.Root.Level = Level.Debug;

            BasicConfigurator.Configure(hierarchy);

            logger = LogManager.GetLogger("Logger");
        }

        public void Error(string message)
        {
            logger.Error(message);
        }

        public void Debug(string message)
        {
            logger.Debug(message);
        }

        public void Info(string message)
        {
            logger.Info(message);
        }

        public void Warn(string message)
        {
            logger.Warn(message);
        }
    }
}
