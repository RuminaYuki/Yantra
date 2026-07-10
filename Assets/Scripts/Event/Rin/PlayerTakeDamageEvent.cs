using Kogetsu.Library.DesignPatternCore;

public record struct PlayerTakeDamageEvent(float Damage, float CurrentHp) : IEvent;
