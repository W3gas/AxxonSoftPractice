using NLog;

namespace AxxonSoft_Prac
{
    /// <summary>
    /// Centralized logger for the entire application.
    /// Uses NLog to write logs to a file.
    /// </summary>
    public static class Logger
    {
        private static readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Info(string message)
        {
            _logger.Info(message);
        }

        public static void Warn(string message)
        {
            _logger.Warn(message);
        }

        public static void Error(string message, System.Exception? exception = null)
        {
            _logger.Error(exception, message);
        }

        public static void Debug(string message)
        {
            _logger.Debug(message);
        }
    }
}