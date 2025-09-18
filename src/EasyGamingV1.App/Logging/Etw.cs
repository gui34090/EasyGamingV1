using System.Diagnostics.Tracing;

namespace EasyGamingV1.Logging;

[EventSource(Name = "EasyGaming.App")]
public sealed class Etw : EventSource
{
    public static readonly Etw Log = new Etw();

    [Event(1, Level = EventLevel.Informational)] public void App_Started() => WriteEvent(1);
    [Event(2, Level = EventLevel.Informational)] public void App_Info(string msg) => WriteEvent(2, msg);
    [Event(3, Level = EventLevel.Error)] public void App_Error(string msg) => WriteEvent(3, msg);
}
