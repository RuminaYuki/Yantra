using UnityEngine;

[RequireComponent(typeof(PairedAnimationActor))]
public class AttackSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PairedAnimationActor _victim;

    private PairedAnimationManager _manager;
    private PairedAnimationActor _attacker;

    private void Awake()
    {
        _attacker = GetComponent<PairedAnimationActor>();

        GameObject gameManager =
            GameObject.FindGameObjectWithTag("GameManager");

        if (gameManager != null)
        {
            _manager =
                gameManager.GetComponent<PairedAnimationManager>();
        }

        if (_manager == null)
        {
            Debug.LogError(
                "AttackSystem cannot find PairedAnimationManager.",
                this);
        }
    }

    public bool TryAttack(GameObject attackPrefab)
    {
        if (attackPrefab == null)
        {
            Debug.LogWarning(
                "Attack Prefab is missing.",
                this);
            return false;
        }

        IAttackStrategy strategy =
            FindStrategy(attackPrefab);

        if (strategy == null)
        {
            Debug.LogError(
                $"{attackPrefab.name} does not contain IAttackStrategy.",
                this);
            return false;
        }

        if (_manager == null ||
            _attacker == null ||
            _victim == null)
        {
            Debug.LogWarning(
                "AttackSystem references are missing.",
                this);
            return false;
        }

        return strategy.TryAttack(
            _manager,
            _attacker,
            _victim);
    }

    public void SetVictim(PairedAnimationActor victim)
    {
        _victim = victim;
    }

    private IAttackStrategy FindStrategy(
        GameObject attackPrefab)
    {
        MonoBehaviour[] components =
            attackPrefab.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour component in components)
        {
            if (component is IAttackStrategy strategy)
                return strategy;
        }

        return null;
    }
}