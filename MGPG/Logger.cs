using System;
using System.IO;
using System.Xml;

namespace MGPG
{
    public class Logger
    {
        /// <summary>
        /// Set this flag to log errors instead of throwing a <see cref="GeneratorException"/>.
        /// </summary>
        public bool SupressErrors { get; set; }

        /// <summary>
        /// Log message of lower priority than this level will not get logged. If set to <see cref="MGPG.LogLevel.None"/> logging is disabled.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// The output stream to write log messages to. Defaults to <see cref="System.Console.Out"/>.
        /// </summary>
        public TextWriter LogWriter { get; set; }

        public Logger()
        {
            SupressErrors = false;
            LogLevel = LogLevel.Info;
            LogWriter = Console.Out;
        }
        
        public void Log(LogLevel level, string file, IXmlLineInfo xli, string message)
        {
            Log(level, file, xli.LineNumber, xli.LinePosition, message);
        }

        public void Log(LogLevel level, string fileName, int line, int column, string message)
        {
            if (level >= LogLevel.Error && !SupressErrors)
                throw new GeneratorException(message, fileName, line, column);
            Log(level, $"{fileName} -> Ln {line}, Col {column}: {message}");
        }

        public void Log(LogLevel level, string msg)
        {
            if (level >= LogLevel.Error && !SupressErrors)
                throw new GeneratorException(msg);
            if (level >= LogLevel)
                LogWriter.WriteLine($"[{level}] {msg}");
        }
    }
}