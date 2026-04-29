namespace EasySave.Model.Logger
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
    }
}