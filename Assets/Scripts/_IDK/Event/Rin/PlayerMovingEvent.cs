using Kogetsu.Library.DesignPatternCore;


public record struct PlayerMovingEvent(bool IsMoving) : IEvent;
