namespace Astrarium.Types
{
    /// <summary>
    /// Logger interface
    /// </summary>
    public interface ILog
    {
        string Level { get; set; }
        void Debug(string format, params object[] args);
        void Error(string format, params object[] args);
        void Fatal(string format, params object[] args);
        void Info(string format, params object[] args);
        void Trace(string format, params object[] args);
        void Warn(string format, params object[] args);
        void Action(string action, string payload);
    }

    public static class Log
    {
        private static ILog log;

        public static void SetImplementation(ILog implementation)
        {
            log = implementation;
        }

        public static string Level
        {
            get => log?.Level;
            set { if (log != null) log.Level = value; }
        }

        public static void Debug(string format, params object[] args) => log?.Debug(format, args);
        public static void Error(string format, params object[] args) => log?.Error(format, args);
        public static void Fatal(string format, params object[] args) => log?.Fatal(format, args);
        public static void Info(string format, params object[] args) => log?.Info(format, args);
        public static void Trace(string format, params object[] args) => log?.Trace(format, args);
        public static void Warn(string format, params object[] args) => log?.Warn(format, args);
        public static void Action(string action, string payload) => log?.Action(action, payload);
    }
}
