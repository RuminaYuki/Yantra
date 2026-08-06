public interface IPairedAttackStrategy
{
    bool TryAttack(
        PairedAnimationManager manager,
        PairedAnimationActor attacker,
        PairedAnimationActor victim);
}