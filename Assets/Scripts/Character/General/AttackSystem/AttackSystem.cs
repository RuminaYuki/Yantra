using UnityEngine;

[RequireComponent(typeof(PairedAnimationActor))]
public class AttackSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PairedAnimationActor _victim;

    private PairedAnimationManager _manager;
    private PairedAnimationActor _attacker;

    [Header("Attack Prefabs")]
    [SerializeField] private GameObject[] _attackPrefabs;
    [SerializeField] private GameObject _defaultAttackPrefab;

    private GameObject _currentPrefab;
    private IAttackStrategy _currentAttack;

    private void Awake()
    {
        _attacker = GetComponent<PairedAnimationActor>();

        GameObject gameManager =
            GameObject.FindGameObjectWithTag("GameManager");

        if (gameManager != null)
            _manager = gameManager.GetComponent<PairedAnimationManager>();

        if (_manager == null)
            Debug.LogError(
                "AttackSystem cannot find PairedAnimationManager on GameManager.",
                this);

        if (_defaultAttackPrefab != null)
            SetAttack(_defaultAttackPrefab);
    }

    public bool SetAttack(GameObject attackPrefab)
    {
        if (attackPrefab == null)
            return false;

        if (!ContainsPrefab(attackPrefab))
        {
            Debug.LogWarning(
                $"{attackPrefab.name} is not registered.",
                this);
            return false;
        }

        if (_currentPrefab == attackPrefab &&
            _currentAttack != null)
        {
            return true;
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

        _currentPrefab = attackPrefab;
        _currentAttack = strategy;
        return true;
    }

    public bool TryAttack()
    {
        if (_currentAttack == null)
        {
            Debug.LogWarning("Current attack is missing.", this);
            return false;
        }

        if (_manager == null || _attacker == null || _victim == null)
        {
            Debug.LogWarning( "AttackSystem references are missing.", this);
            return false;
        }

        return _currentAttack.TryAttack(
            _manager,
            _attacker,
            _victim);
    }

    public void SetVictim(PairedAnimationActor victim)
    {
        _victim = victim;
    }

    private bool ContainsPrefab(GameObject attackPrefab)
    {
        if (_attackPrefabs == null)
            return false;

        foreach (GameObject prefab in _attackPrefabs)
        {
            if (prefab == attackPrefab)
                return true;
        }

        return false;
    }

    private IAttackStrategy FindStrategy(GameObject attackObject)
    {
        MonoBehaviour[] components =
            attackObject.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour component in components)
        {
            if (component is IAttackStrategy strategy)
                return strategy;
        }

        return null;
    }
}
