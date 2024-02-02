using UnityEngine;


namespace ZoboUI.Core.Utils
{

    public interface ICustomLogger
    {
        /// <summary>
        /// The prefix to be added to the message. If the prefix is empty, the message is returned as is.
        /// </summary>
        string Prefix { get; set; }

        /// <summary>
        /// The log level of the logger. Messages with a log level lower than the logger's log level will not be logged.
        /// </summary>
        LogLevel LogLevel { get; set; }

        /// <summary>
        /// Formats the message to be logged to the console. If the prefix is empty, the message is returned as is.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string FormatMessage(string message);

        /// <summary>
        /// Logs the message to the console.
        /// </summary>
        /// <param name="message"></param>
        void Log(string message);

        /// <summary>
        /// Logs a progress related message to the console.
        /// </summary>
        /// <param name="message"></param>
        void LogProgress(string message);

        /// <summary>
        /// Logs the message to the console as a warning.
        /// </summary>
        /// <param name="message"></param>
        void LogWarning(string message);

        /// <summary>
        /// Logs the message to the console as an error.
        /// </summary>
        /// <param name="message"></param>
        void LogError(string message);
    }

    public enum LogLevel
    {
        Info = 1,
        Progress = 2,
        Warning = 3,
        Error = 4,

        None = 5

    }



    /// <summary>
    /// A simple logger that can be used to log messages to the console.
    /// </summary>
    public class CustomLogger : ICustomLogger
    {

        public string Prefix { get; set; }

        public LogLevel LogLevel { get; set; }

        public CustomLogger(string prefix = "", LogLevel logLevel = LogLevel.Info)
        {
            Prefix = prefix;
            LogLevel = logLevel;

        }

        /// <summary>
        /// Formats the message to be logged to the console. If the prefix is empty, the message is returned as is.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string FormatMessage(string message)
        {
            if (string.IsNullOrEmpty(this.Prefix))
                return message;

            return $"{this.Prefix} : {message}";
        }

        public void Log(string message)
        {
            if (this.LogLevel <= LogLevel.Info)
                Debug.Log(FormatMessage(message));
        }

        public void LogProgress(string message)
        {
            if (this.LogLevel <= LogLevel.Progress)
                Debug.Log(FormatMessage(message));
        }

        public void LogWarning(string message)
        {
            if (this.LogLevel <= LogLevel.Warning)
                Debug.LogWarning(FormatMessage(message));
        }

        public void LogError(string message)
        {
            if (this.LogLevel <= LogLevel.Error)
                Debug.LogError(FormatMessage(message));
        }

    }
}