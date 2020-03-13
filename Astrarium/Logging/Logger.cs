using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Diagnostics;
using System.IO;

namespace Astrarium.Logging
{
    /// <summary>
    /// Logger wrapper for log4net.
    /// Incapsulates logging logic dependant from the log4net
    /// </summary>
    public class Logger : TraceListener
    {
        /// <summary>
        /// Path to log file
        /// </summary>
        private readonly string LOG_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium.Algorithms", "Astrarium.log");

        /// <summary>
        /// log4net logger instance 
        /// </summary>
        private readonly ILog logger;

        /// <summary>
        /// Creates new logger
        /// </summary>
        public Logger()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout("[%-5level] [%date] %message%newline");
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

            hierarchy.Root.AddAppender(fileAppender);
            hierarchy.Root.Level = Level.Debug;

            BasicConfigurator.Configure(hierarchy);

            logger = LogManager.GetLogger("Logger");

            Trace.Listeners.Add(this);
        }

        public override void Write(string message)
        {
            WriteToLog(TraceEventType.Verbose, message);
        }

        public override void WriteLine(string message)
        {
            Write(message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            TraceEvent(eventCache, source, eventType, id, string.Empty, new object[0]);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            TraceEvent(eventCache, source, eventType, id, message, new object[0]);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
            {
                WriteToLog(eventType, string.Format(format, args));
            }
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, Object data)
        {
            TraceData(eventCache, source, eventType, id, new object[] { data });
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params Object[] data)
        {
            if (ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
            {
                string message = string.Empty;
                if (data != null)
                {
                    message = string.Join(", ", data);
                }

                WriteToLog(eventType, message);
            }
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            TraceEvent(eventCache, source, TraceEventType.Information, id, $"{message}, relatedActivityId={relatedActivityId}", new object[0]);
        }

        private bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
        {
            return (Filter == null) || Filter.ShouldTrace(cache, source, eventType, id, formatOrMessage, args, data1, data);
        }

        private void WriteToLog(TraceEventType eventType, string message)
        {
            switch (eventType)
            {
                case TraceEventType.Verbose:
                    logger.Debug(message);
                    break;
                case TraceEventType.Information:
                    logger.Info(message);
                    break;
                case TraceEventType.Warning:
                    logger.Warn(message);
                    break;
                case TraceEventType.Error:
                    logger.Error(message);
                    break;
                case TraceEventType.Critical:
                    logger.Fatal(message);
                    break;
                default:
                    break;
            }
        }
    }
}
