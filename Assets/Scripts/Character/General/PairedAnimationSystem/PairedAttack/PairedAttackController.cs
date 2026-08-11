using UnityEngine;

[RequireComponent(typeof(PairedAnimationActor))]
public class PairedAttackController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PairedAnimationActor _victim;

    private PairedAnimationManager _manager;
    private PairedAnimationActor _attacker;

    private float _currentDamage;
    private bool _hasDealtDamage;


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
        if(_victim == null)
        {
            Debug.LogWarning("Victim not assign");
            _victim = GameObject.FindGameObjectWithTag("Player").GetComponent<PairedAnimationActor>();
        }
    }

    public bool TryAttack(GameObject attackPrefab, float damage)
    {
        if (attackPrefab == null)
        {
            Debug.LogWarning(
                "Attack Prefab is missing.",
                this);
            return false;
        }

        IPairedAttackStrategy strategy =
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

        _currentDamage = damage;
        _hasDealtDamage = false;

        return strategy.TryAttack(
            _manager,
            _attacker,
            _victim);
    }

    public void SetVictim(PairedAnimationActor victim)
    {
        _victim = victim;
    }

    private IPairedAttackStrategy FindStrategy(
        GameObject attackPrefab)
    {
        MonoBehaviour[] components =
            attackPrefab.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour component in components)
        {
            if (component is IPairedAttackStrategy strategy)
                return strategy;
        }

        return null;
    }

    public void ApplyPairedDamage()
{
    if (_victim == null || _hasDealtDamage)
        return;

    IDamageable damageable = _victim.GetComponentInParent<IDamageable>();

    if (damageable == null)
        return;

    damageable.TakeDamage(_currentDamage);
    _hasDealtDamage = true;
}
}