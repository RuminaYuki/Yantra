using Kogetsu.Library.DesignPatternCore;

public enum KaEventRunName
{
    Event1 = 1,
    Event2 = 2,
    Event3 = 3
}

public record struct KaEventRun(KaEventRunName EventName) : IEvent;
