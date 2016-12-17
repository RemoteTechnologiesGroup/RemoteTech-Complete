using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace RemoteTech.Common
{
    /// <summary>
    ///     Logging levels to log messages for debugging. The higher the level, the less is logged.
    ///     For example, setting the level to <see cref="Error" /> means that only <see cref="Error" /> and
    ///     <see cref="Critical" /> message are logged.
    ///     See also <seealso cref="Logging.CurrentLogLevel" /> to set the appropriate level of the logger.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>Default level: in this state nothing is logged. Can be used to disable the logger.</summary>
        NotSet,

        /// <summary>Debug level: useful when debugging only.</summary>
        Debug,

        /// <summary>Information level: can be used to give some feedback.</summary>
        Info,

        /// <summary>Warning level: something went probably wrong but it's not an error per se.</summary>
        Warning,

        /// <summary>Error level: An error occurred but it can be dealt with.</summary>
        Error,

        /// <summary>
        ///     Critical level: useful only when something went really wrong and can't be fixed (probably leading to a crash
        ///     state).
        /// </summary>
        Critical
    }

    /// <summary>
    ///     Main logging class.
    /// </summary>
    public static class Logging
    {
        /// <summary>
        ///     Log journal used to keep a record of the logged entries with their level. Only active if
        ///     <see cref="LogJournalActive" /> is true.
        /// </summary>
        /// <remarks>Always active when this assembly is compiled in DEBUG mode.</remarks>
        public static readonly Dictionary<LogLevel, List<string>> LogJournal = new Dictionary<LogLevel, List<string>>();

        /// <summary>
        ///     Static constructor.
        /// </summary>
        static Logging()
        {
            // initialize LogJournal
            foreach (LogLevel lvl in Enum.GetValues(typeof(LogLevel)))
                LogJournal.Add(lvl, new List<string>());

#if DEBUG
            LogJournalActive = true;
            LogCallerInfo = true;
#endif
        }

        /// <summary>
        ///     Activate or deactivate the <see cref="LogJournal" />.
        /// </summary>
        /// <remarks>
        ///     Depends on <see cref="GameSettings.VERBOSE_DEBUG_LOG" />. Automatically set to true if the assembly if
        ///     compiled in DEBUG version.
        /// </remarks>
        public static bool LogJournalActive { get; set; } = GameSettings.VERBOSE_DEBUG_LOG;

        /// <summary>
        ///     If true, the name class and namespace of the function calling the logging function is also logged.
        /// </summary>
        /// <remarks>Automatically set to true if the assembly is compiled in DEBUG version.</remarks>
        public static bool LogCallerInfo { get; set; } = false;

        /// <summary>
        ///     Set or Get the current minimum log level. Any log level function below this level will *not* be logged. Default to
        ///     <see cref="LogLevel.Debug" />, see also <seealso cref="LogLevel" />.
        /// </summary>
        public static LogLevel CurrentLogLevel { get; set; } = LogLevel.Debug;

        /// <summary>
        ///     Log a string (and optional parameters) to the KSP Log.
        /// </summary>
        /// <param name="askedLevel">
        ///     The logging level that was asked for. See <see cref="LogLevel" /> and
        ///     <seealso cref="CurrentLogLevel" />.
        /// </param>
        /// <param name="message">A composite format string.</param>
        /// <param name="ex">An exception to log, if any.</param>
        /// <param name="params">Format arguments.</param>
        /// <remarks>see the MSDN documentation on Composite Formatting.</remarks>
        private static void Log(LogLevel askedLevel, string message, Exception ex, params object[] @params)
        {
            // nothing is logged if the level is not set.
            if (CurrentLogLevel == LogLevel.NotSet)
                return;

            // the level asked for must be equal or greater to the current log level.
            if (askedLevel < CurrentLogLevel)
                return;

            LogDelegate logMethod;

            switch (askedLevel)
            {
                case LogLevel.NotSet:
                    return;

                case LogLevel.Debug:
                    logMethod = UnityEngine.Debug.Log;
                    break;

                case LogLevel.Info:
                    goto case LogLevel.Debug;

                case LogLevel.Warning:
                    logMethod = UnityEngine.Debug.LogWarning;
                    break;

                case LogLevel.Error:
                    logMethod = UnityEngine.Debug.LogError;
                    break;

                case LogLevel.Critical:
                    goto case LogLevel.Error;

                default:
                    throw new ArgumentException($"Unknown log level: {askedLevel}");
            }

            // get caller (note: 0 is GetCallingAssembly, 1 is this function, 2 is one of the log functions [e.g. Logging.Debug()] and 3 is the caller of the log function)
            var pluginName = GetCallingAssembly(3);
            var logHeader = $"[{pluginName}] [{askedLevel}]";

            // format message body
            var logBody = (@params != null) && (@params.Length > 0) ? string.Format(message, @params) : message;

            // log caller information (function name, class and namespace) if flag is active
            if (LogCallerInfo)
            {
                var st = new StackTrace();
                var method = st.GetFrame(2).GetMethod();
                logBody += $" [caller: '{method}' in '{method.DeclaringType}']";
            }

            // do logging with Unity
            var logString = $"{logHeader}: {logBody}";
            logMethod(logString);

            // add to journal if journal is active.
            if (LogJournalActive)
            {
                var timedlogString = $"[{TimeString()}] {logString}";
                LogJournal[askedLevel].Add(timedlogString);
            }

            // use unity logging for exception
            if (ex != null)
                UnityEngine.Debug.LogException(ex);
        }


        /*
         * convenience functions
         */

        /// <summary>
        ///     Log a debug message to the Unity Console and KSP log file.
        /// </summary>
        /// <param name="message">A composite format string.</param>
        /// <param name="params">Format arguments.</param>
        /// <remarks>see the MSDN documentation on Composite Formatting.</remarks>
        public static void Debug(string message, params object[] @params)
        {
            Log(LogLevel.Debug, message, null, @params);
        }

        /// <summary>
        ///     Log an informative message to the Unity Console and KSP log file.
        /// </summary>
        /// <param name="message">A composite format string.</param>
        /// <param name="params">Format arguments.</param>
        /// <remarks>see the MSDN documentation on Composite Formatting.</remarks>
        public static void Info(string message, params object[] @params)
        {
            Log(LogLevel.Info, message, null, @params);
        }

        /// <summary>
        ///     Log a warning message to the Unity Console and KSP log file.
        /// </summary>
        /// <param name="message">A composite format string.</param>
        /// <param name="params">Format arguments.</param>
        /// <remarks>see the MSDN documentation on Composite Formatting.</remarks>
        public static void Warning(string message, params object[] @params)
        {
            Log(LogLevel.Warning, message, null, @params);
        }

        /// <summary>
        ///     Log an error message to the Unity Console and KSP log file.
        /// </summary>
        /// <param name="message">A composite format string.</param>
        /// <param name="params">Format arguments.</param>
        /// <remarks>see the MSDN documentation on Composite Formatting.</remarks>
        public static void Error(string message, params object[] @params)
        {
            Log(LogLevel.Error, message, null, @params);
        }

        /// <summary>
        ///     Log an error message and an exception to the Unity Console and KSP log file.
        ///     <para>Useful when an exception has been caught and must be logged.</para>
        /// </summary>
        /// <param name="message">A composite format string.</param>
        /// <param name="ex">An exception.</param>
        /// <param name="params">Format arguments.</param>
        /// <remarks>see the MSDN documentation on Composite Formatting.</remarks>
        public static void Error(string message, Exception ex, params object[] @params)
        {
            Log(LogLevel.Error, message, ex, @params);
        }

        /// <summary>
        ///     Log a critical error message to the Unity Console and KSP log file.
        /// </summary>
        /// <param name="message">A composite format string.</param>
        /// <param name="params">Format arguments.</param>
        /// <remarks>see the MSDN documentation on Composite Formatting.</remarks>
        public static void Critical(string message, params object[] @params)
        {
            Log(LogLevel.Critical, message, null, @params);
        }

        /// <summary>
        ///     Log a critical error message and an exception to the Unity Console and KSP log file.
        ///     <para>Useful when an exception has been caught and must be logged.</para>
        /// </summary>
        /// <param name="message">A composite format string.</param>
        /// <param name="ex">An exception.</param>
        /// <param name="params">Format arguments.</param>
        /// <remarks>see the MSDN documentation on Composite Formatting.</remarks>
        public static void Critical(string message, Exception ex, params object[] @params)
        {
            Log(LogLevel.Critical, message, ex, @params);
        }

        /// <summary>
        ///     Get the calling assembly of the function at <paramref name="index" /> in the function stack frame.
        /// </summary>
        /// <param name="index">The index of the function (in the stack frame) from which the assembly should be retrieved.</param>
        /// <returns>The qualified name of the assembly if successful, an empty string otherwise.</returns>
        /// <remarks>This function is a index 0, the caller at index 1, etc.</remarks>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCallingAssembly(int index)
        {
            var st = new StackTrace();
            var method = st.GetFrame(index).GetMethod();
            var declaringType = method.DeclaringType;
            return declaringType?.Module.Name.Split('.')[0] ?? string.Empty;
        }

        /// <summary>
        ///     Convenience function: Log a debug message telling a function is entered.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public static void EnterFunction()
        {
            var methodName = GetMethodNameInStackFrame(2);
            Debug($"{methodName}: Entering");
        }

        /// <summary>
        ///     Convenience function: Log a debug message telling the function is exited.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public static void LeaveFunction()
        {
            var methodName = GetMethodNameInStackFrame(2);
            Debug($"{methodName}: Leaving");
        }

        /// <summary>
        ///     Get the calling method name at <paramref name="index" /> in the stack frame.
        /// </summary>
        /// <param name="index">Function location on stack frame.</param>
        /// <returns>The method name at <paramref name="index" /> in the stack frame.</returns>
        /// <remarks>This function is a index 0, the caller at index 1, etc.</remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetMethodNameInStackFrame(int index)
        {
            var st = new StackTrace();
            var sf = st.GetFrame(index);
            return sf.GetMethod().Name;
        }

        /// <summary>
        ///     Get the whole stack trace leading to a method.
        /// </summary>
        /// <returns>The stack trace string.</returns>
        public static string StackTraceToString()
        {
            var sb = new StringBuilder(0x1000);
            var frames = new StackTrace().GetFrames();
            if (frames == null)
                return string.Empty;

            // start at 1 to ignore current method
            for (var i = 1; i < frames.Length; i++)
            {
                var currFrame = frames[i];
                var method = currFrame.GetMethod();
                sb.AppendLine($"{method.ReflectedType?.Name ?? string.Empty}:{method.Name}");
            }
            return sb.ToString();
        }

        internal delegate void LogDelegate(string message);

        private static string TimeString(string format = null)
        {
            if (string.IsNullOrEmpty(format))
                format = "HH:mm:ss.fff";
            return DateTime.Now.ToString(format);
        }
    }
}