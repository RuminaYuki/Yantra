using Kogetsu.Library.DesignPatternCore;


public record struct PlayerRunningEvent(bool IsRunning) : IEvent;
