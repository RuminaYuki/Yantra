public interface IAttackStrategy
{
    bool TryAttack(
        PairedAnimationManager manager,
        PairedAnimationActor attacker,
        PairedAnimationActor victim);
}