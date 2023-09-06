using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Niantic.Platform.Debugging.Unity
{
    public class UnityLogStream : ILogStream
    {
        private readonly Settings _settings;
        private readonly int _startTime;

#if UNITY_EDITOR
        private readonly bool _isProSkin = EditorGUIUtility.isProSkin;
#endif

        public UnityLogStream(Settings settings = null)
        {
            _settings = settings ?? new Settings();
            _startTime = Environment.TickCount;
        }

        public void LogMessage(string channel, LogLevel logLevel, string message)
        {
            Assert.That(channel != null);

            var fullMessage = CreateFullMessage(channel, logLevel, message);

            switch (logLevel)
            {
                case LogLevel.Verbose:
                case LogLevel.Info:
                case LogLevel.Trace:
                    Debug.Log(fullMessage);
                    break;
                case LogLevel.Error:
                    Debug.LogError(fullMessage);
                    break;
                case LogLevel.Fatal:
                    Debug.LogError(fullMessage);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(fullMessage);
                    break;
                default:
                    // Print the message anyway instead of throwing a new exception
                    // Better for logging code to not cause further breakage in these cases
                    Debug.LogErrorFormat(
                        "Invalid log level '{0}' provided to UnityLogStream! Message: {1}",
                        logLevel,
                        fullMessage
                    );
                    break;
            }
        }

        private int GetElapsedMilliseconds()
        {
            return Environment.TickCount - _startTime;
        }

        private string GetLogLevelStr(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Warning:
                    return "W";

                case LogLevel.Fatal:
                    return "F";

                case LogLevel.Error:
                    return "E";

                case LogLevel.Info:
                    return "I";

                case LogLevel.Verbose:
                    return "V";

                case LogLevel.Trace:
                    return "T";

                default:
                    return "?";
            }
        }

        private string CreateFullMessage(string channel, LogLevel logLevel, string message)
        {
            if (channel == LogService.DefaultChannel)
            {
                channel = "Platform";
            }

#if UNITY_EDITOR
            if (_settings.UseColorsInEditor)
            {
                if (logLevel == LogLevel.Trace)
                {
                    return string.Format(
                        "[{0}] <b><color={1}>[{2}]</color></b> <color={3}>{4}</color>",
                        GetLogLevelStr(logLevel),
                        GetChannelColor(channel),
                        channel,
                        TraceColor,
                        message
                    );
                }

                return string.Format(
                    "[{0}] <b><color={1}>[{2}]</color></b> {3}",
                    GetLogLevelStr(logLevel),
                    GetChannelColor(channel),
                    channel,
                    message
                );
            }
#endif

            // Don't bother printing the time when inside unity editor
            // because the unity console already provides this
            if (!Application.isEditor && _settings.IncludeTime)
            {
                return string.Format(
                    "[{0}] [T{1:000000}] [{2}] {3}",
                    GetLogLevelStr(logLevel),
                    GetElapsedMilliseconds(),
                    channel,
                    message
                );
            }

            return string.Format("[{0}] [{1}] {2}", GetLogLevelStr(logLevel), channel, message);
        }

        public class Settings
        {
            public Settings(bool includeTime = false, bool useColorsInEditor = true)
            {
                IncludeTime = includeTime;
                UseColorsInEditor = useColorsInEditor;
            }

            public bool IncludeTime { get; }
            public bool UseColorsInEditor { get; }
        }

#if UNITY_EDITOR
        private const string TraceColor = "#666666";

        // Choose a consistent random color for the given channel name
        private string GetChannelColor(string channel)
        {
            if (_isProSkin)
            {
                return LightReadableColors[
                    Math.Abs(channel.GetHashCode()) % LightReadableColors.Length
                ];
            }

            return DarkReadableColors[Math.Abs(channel.GetHashCode()) % DarkReadableColors.Length];
        }
#endif

#if UNITY_EDITOR
        // Taken from here: https://www.rapidtables.com/web/color/RGB_Color.html
        private static readonly string[] DarkReadableColors =
        {
            "#4682B4", // steel blue
            "#6A5ACD", // slate blue
            "#800000", // maroon
            "#8B0000", // dark red
            "#A52A2A", // brown
            "#B22222", // firebrick
            "#556B2F", // dark olive green
            "#006400", // dark green
            "#008000", // green
            "#228B22", // forest green
            "#2E8B57", // sea green
            "#2F4F4F", // dark slate gray
            "#008080", // teal
            "#008B8B", // dark cyan
            "#191970", // midnight blue
            "#000080", // navy
            "#00008B", // dark blue
            "#0000CD", // medium blue
            "#0000FF", // blue
            "#8A2BE2", // blue violet
            "#4B0082", // indigo
            "#483D8B", // dark slate blue
            "#8B008B", // dark magenta
            "#800080", // purple
            "#8B4513", // saddle brown
            "#A0522D", // sienna
            "#000000" // black
        };

        private static readonly string[] LightReadableColors =
        {
            "#FF0000", // red
            "#D2691E", // chocolate
            "#FF6347", // tomato
            "#FF7F50", // coral
            "#F08080", // light coral
            "#E9967A", // dark salmon
            "#FA8072", // salmon
            "#FFA07A", // light salmon
            "#FF4500", // orange red
            "#FF8C00", // dark orange
            "#FFA500", // orange
            "#FFD700", // gold
            "#DAA520", // golden rod
            "#EEE8AA", // pale golden rod
            "#BDB76B", // dark khaki
            "#F0E68C", // khaki
            "#FFFF00", // yellow
            "#9ACD32", // yellow green
            "#7CFC00", // lawn green
            "#7FFF00", // chart reuse
            "#ADFF2F", // green yellow
            "#00FF00", // lime
            "#32CD32", // lime green
            "#90EE90", // light green
            "#98FB98", // pale green
            "#00FA9A", // medium spring green
            "#00FF7F", // spring green
            "#66CDAA", // medium aqua marine
            "#3CB371", // medium sea green
            "#20B2AA", // light sea green
            "#00FFFF", // aqua
            "#00FFFF", // cyan
            "#E0FFFF", // light cyan
            "#00CED1", // dark turquoise
            "#40E0D0", // turquoise
            "#48D1CC", // medium turquoise
            "#AFEEEE", // pale turquoise
            "#7FFFD4", // aqua marine
            "#B0E0E6", // powder blue
            "#5F9EA0", // cadet blue
            "#6495ED", // corn flower blue
            "#00BFFF", // deep sky blue
            "#1E90FF", // dodger blue
            "#ADD8E6", // light blue
            "#87CEEB", // sky blue
            "#87CEFA", // light sky blue
            "#4169E1", // royal blue
            "#7B68EE", // medium slate blue
            "#9370DB", // medium purple
            "#9400D3", // dark violet
            "#9932CC", // dark orchid
            "#BA55D3", // medium orchid
            "#D8BFD8", // thistle
            "#DDA0DD", // plum
            "#EE82EE", // violet
            "#FF00FF", // magenta / fuchsia
            "#DA70D6", // orchid
            "#C71585", // medium violet red
            "#DB7093", // pale violet red
            "#FF1493", // deep pink
            "#FF69B4", // hot pink
            "#FFB6C1", // light pink
            "#FFC0CB", // pink
            "#FAEBD7", // antique white
            "#F5F5DC", // beige
            "#FFE4C4", // bisque
            "#FFEBCD", // blanched almond
            "#F5DEB3", // wheat
            "#FFF8DC", // corn silk
            "#FFFACD", // lemon chiffon
            "#FAFAD2", // light golden rod yellow
            "#FFFFE0", // light yellow
            "#F4A460", // sandy brown
            "#DEB887", // burly wood
            "#D2B48C", // tan
            "#FFE4B5", // moccasin
            "#FFDEAD", // navajo white
            "#FFDAB9", // peach puff
            "#FFE4E1", // misty rose
            "#FFF0F5", // lavender blush
            "#FAF0E6", // linen
            "#FDF5E6", // old lace
            "#FFEFD5", // papaya whip
            "#FFF5EE", // sea shell
            "#F5FFFA", // mint cream
            "#E6E6FA", // lavender
            "#FFFAF0", // floral white
            "#F0F8FF", // alice blue
            "#F8F8FF", // ghost white
            "#F0FFF0", // honeydew
            "#FFFFF0", // ivory
            "#F0FFFF", // azure
            "#FFFAFA", // snow
            "#C0C0C0", // silver
            "#D3D3D3", // light gray / light grey
            "#DCDCDC", // gainsboro
            "#F5F5F5", // white smoke
            "#FFFFFF", // white
            "#CD5C5C", // indian red
            "#CD853F", // peru
            "#BC8F8F", // rosy brown
            "#8FBC8F" // dark sea green
        };
#endif
    }
}
