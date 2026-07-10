using Kogetsu.Library.DesignPatternCore;


public record struct PlayerGroundEvent(bool IsGrounded) : IEvent;
