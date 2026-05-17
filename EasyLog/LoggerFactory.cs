namespace EasyLog
{
    public static class LoggerFactory
    {
        public static ILogger Resolve(string format, string logDirectory)
        {
            return format.ToLower() switch
            {
                "json" => new JsonLogger(logDirectory),
                "xml" => new XmlLogger(logDirectory),
                _ => throw new NotSupportedException($"Log format not supported: {format}")
            };
        }
        public static ILogger ResolveComposite(
            string format,
            string logDirectory,
            string serverUrl,
            Func<LogStorageMode> getMode)
        {
            ILogger local = Resolve(format, logDirectory);
            ILogger central = new ApiLogger(serverUrl);
            return new CompositeLogger(local, central, getMode);
        }
    }
}