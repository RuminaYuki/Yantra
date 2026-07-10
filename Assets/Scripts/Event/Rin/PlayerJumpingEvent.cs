using Kogetsu.Library.DesignPatternCore;


public record struct PlayerJumpingEvent(bool IsJumping) : IEvent;
