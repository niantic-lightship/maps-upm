using System.Linq;
using UnityEngine;

namespace Niantic.Platform.Debugging.Unity
{
    public class UnityLogForwarder : MonoBehaviour
    {
        public void Awake()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        public void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        public static void OnLogMessageReceived(string message, string stackTrace, LogType type)
        {
            Assert.That(
                !LogService.Streams.OfType<UnityLogStream>().Any(),
                "Cannot use both UnityLogForwarder and UnityLogStream classes at the same time!"
            );

            switch (type)
            {
                case LogType.Log:
                    Log.Info(message);
                    break;
                case LogType.Warning:
                    Log.Warn(message);
                    break;
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    Log.Error(string.Format("{0}\nStack Trace:\n{1}", message, stackTrace));
                    break;
                default:
                    Assert.Fail("Unexpected log type '{0}'", type);
                    break;
            }
        }
    }
}
