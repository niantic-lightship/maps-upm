// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Platform.Debugging;
using PlatformLog = Niantic.Platform.Debugging.Log;

namespace Niantic.Lightship.Maps.Utilities
{
    /// <summary>
    /// Helper class that simplifies message logging for a specific log channel
    /// using the logging system in the Platform Debugging library.  All messages
    /// logged with an instance of this class will be tagged with the channel name
    /// and given a unique color in the Unity Editor's console, which makes it
    /// easier to find log messages from a particular feature or system.
    /// </summary>
    [PublicAPI]
    public class ChannelLogger
    {
        /// <summary>
        /// The name of the channel associated with events logged from this class
        /// </summary>
        public string ChannelName { get; }

        /// <summary>
        /// The max log level for this channel.  Anything more verbose
        /// than this level will be suppressed.  This can be used to
        /// suppress log messages if they're too verbose or to enable
        /// additional log messages while a system is being debugged.
        /// </summary>
        public LogLevel MaxLogLevel
        {
            get => LogService.TryGetMaxLogLevelForChannel(ChannelName) ?? LogService.MaxLogLevel;
            set => LogService.SetMaxLogLevelForChannel(ChannelName, value);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logChannel">The channel name associated
        /// with log messages from instances of this class</param>
        /// <param name="maxLogLevel">The max log level for this channel</param>
        public ChannelLogger(string logChannel, LogLevel maxLogLevel = LogLevel.Info)
        {
            ChannelName = logChannel;
            MaxLogLevel = maxLogLevel;
        }

        /// <summary>
        /// Log a <see cref="LogLevel.Fatal"/> message to the log channel
        /// </summary>
        public void Fatal(string message)
        {
            PlatformLog.FatalToChannel(ChannelName, message);
        }

        /// <summary>
        /// Log an <see cref="LogLevel.Error"/> message to the log channel
        /// </summary>
        public void Error(string message)
        {
            PlatformLog.ErrorToChannel(ChannelName, message);
        }

        /// <summary>
        /// Log a <see cref="LogLevel.Warning"/> message to the log channel
        /// </summary>
        public void Warning(string message)
        {
            PlatformLog.WarnToChannel(ChannelName, message);
        }

        /// <summary>
        /// Log an <see cref="LogLevel.Info"/> message to the log channel
        /// </summary>
        public void Info(string message)
        {
            PlatformLog.InfoToChannel(ChannelName, message);
        }

        /// <summary>
        /// Log a <see cref="LogLevel.Verbose"/> message to the log channel
        /// </summary>
        public void Verbose(string message)
        {
            PlatformLog.VerboseToChannel(ChannelName, message);
        }

        /// <summary>
        /// Log a <see cref="LogLevel.Trace"/> message to the log channel
        /// </summary>
        public void LogTrace(string message)
        {
            PlatformLog.TraceToChannel(ChannelName, message);
        }

        /// <summary>
        /// Log a message to the log channel
        /// </summary>
        /// <param name="logLevel">The message's severity</param>
        /// <param name="message">The message to log</param>
        public void LogMessage(LogLevel logLevel, string message)
        {
            PlatformLog.LogToChannel(ChannelName, logLevel, message);
        }
    }
}
