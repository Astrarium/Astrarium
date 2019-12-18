using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Diagnostics;
using System.IO;

namespace Planetarium.Logging
{
    /// <summary>
    /// Logger wrapper for log4net.
    /// Incapsulates logging logic dependant from the log4net
    /// </summary>
    public class Logger : TraceListener
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

            hierarchy.Root.AddAppender(fileAppender);
            hierarchy.Root.Level = Level.Debug;

            BasicConfigurator.Configure(hierarchy);

            logger = LogManager.GetLogger("Logger");

            Trace.Listeners.Add(this);
        }

        public static Level Convert(TraceEventType eventType)
        {
            switch (eventType)
            {
                case TraceEventType.Verbose:
                    return Level.Debug;
                case TraceEventType.Information:
                    return Level.Info;
                case TraceEventType.Warning:
                    return Level.Warn;
                case TraceEventType.Error:
                    return Level.Error;
                case TraceEventType.Critical:
                    return Level.Fatal;
                default:
                    throw new ArgumentException(string.Format("LogLevel does not support value {0}.", eventType), "logLevel");
            }
        }

        public override void Write(string message)
        {
            logger.Debug(message);
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

        public override void TraceEvent(
       TraceEventCache eventCache,
       string source,
       TraceEventType eventType, int id,
       string format,
       params Object[] args)
        {
            if (ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
            {
                var level = Convert(eventType);
                logger.Logger.Log(this.GetType(), level, string.Format(format, args), null);
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
                var level = Convert(eventType);

                logger.Logger.Log(this.GetType(), level, message, null);
            }
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            TraceEvent(eventCache, source, TraceEventType.Information, id,
                message + ", relatedActivityId=" + relatedActivityId, new object[0]);
        }

        private bool ShouldTrace(
       TraceEventCache cache,
       string source,
       TraceEventType eventType,
       int id,
       string formatOrMessage,
       object[] args,
       object data1,
       object[] data)
        {
            return ((Filter == null) || Filter.ShouldTrace(cache, source, eventType, id, formatOrMessage, args, data1, data));
        }
    }
}
