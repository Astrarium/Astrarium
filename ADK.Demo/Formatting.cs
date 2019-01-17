namespace ADK.Demo
{
    public interface IEphemFormatter
    {
        string Format(object value);
    }

    public static class Formatters
    {
        private class SimpleFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                return value?.ToString();
            }
        }

        public static readonly IEphemFormatter RA = new SimpleFormatter();
        public static readonly IEphemFormatter Dec = new SimpleFormatter();
    }
}