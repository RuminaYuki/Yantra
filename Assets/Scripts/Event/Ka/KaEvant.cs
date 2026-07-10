using UnityEngine;
using Kogetsu.Library.DesignPatternCore;

public class KaEvant : MonoBehaviour
{
    [Header("Ka Ghosts")]
    [SerializeField] private KaStateMachine[] kaGhosts;

    [Header("Event Speed Multipliers")]
    [SerializeField] private float event1SpeedMultiplier = 1.2f;
    [SerializeField] private float event2SpeedMultiplier = 1.5f;
    [SerializeField] private float event3SpeedMultiplier = 1f;

    private bool isSubscribed;

    private void Awake()
    {
        FindKaGhostsIfNeeded();
    }

    private void Start()
    {
        SubscribeIfPossible();
    }

    private void OnEnable()
    {
        SubscribeIfPossible();
    }

    private void OnDisable()
    {
        if (!isSubscribed || !EventBus.Instance)
        {
            return;
        }

        EventBus.Instance.Unsubscribe<KaEventRun>(OnKaEventRun);
        isSubscribed = false;
    }

    private void SubscribeIfPossible()
    {
        if (isSubscribed)
        {
            return;
        }

        if (!EventBus.Instance)
        {
            return;
        }

        EventBus.Instance.Subscribe<KaEventRun>(OnKaEventRun);
        isSubscribed = true;
    }

    private void OnKaEventRun(KaEventRun kaEventRun)
    {
        switch (kaEventRun.EventName)
        {
            case KaEventRunName.Event1:
                ApplyKaEvent(1, event1SpeedMultiplier);
                break;
            case KaEventRunName.Event2:
                ApplyKaEvent(2, event2SpeedMultiplier);
                break;
            case KaEventRunName.Event3:
                ApplyKaEvent(3, event3SpeedMultiplier);
                break;
            default:
                Debug.LogWarning($"{nameof(KaEvant)} on {name} received unknown Ka event name {kaEventRun.EventName}.");
                break;
        }
    }

    private void ApplyKaEvent(int activeGhostCount, float speedMultiplier)
    {
        if (kaGhosts == null || kaGhosts.Length == 0)
        {
            FindKaGhostsIfNeeded();
        }

        if (kaGhosts == null || kaGhosts.Length == 0)
        {
            Debug.LogWarning($"{nameof(KaEvant)} on {name} needs Ka Ghosts assigned.");
            return;
        }

        int clampedActiveCount = Mathf.Clamp(activeGhostCount, 0, kaGhosts.Length);
        for (int i = 0; i < kaGhosts.Length; i++)
        {
            KaStateMachine stateMachine = kaGhosts[i];
            if (stateMachine == null)
            {
                Debug.LogWarning($"{nameof(KaEvant)} on {name} has an empty Ka ghost slot at index {i}.");
                continue;
            }

            GameObject kaGhost = stateMachine.gameObject;
            if (kaGhost == null)
            {
                Debug.LogWarning($"{nameof(KaEvant)} on {name} has an empty Ka ghost slot at index {i}.");
                continue;
            }

            bool shouldBeActive = i < clampedActiveCount;
            kaGhost.SetActive(shouldBeActive);
            stateMachine.SetSpeedMultiplier(speedMultiplier);
        }

        Debug.Log($"Applied {nameof(KaEventRun)}: active ghosts={clampedActiveCount}, speed multiplier={speedMultiplier}.");
    }

    private void FindKaGhostsIfNeeded()
    {
        if (kaGhosts != null && kaGhosts.Length > 0)
        {
            return;
        }

        kaGhosts = FindObjectsByType<KaStateMachine>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        System.Array.Sort(kaGhosts, (left, right) => string.CompareOrdinal(left.name, right.name));
    }
}
