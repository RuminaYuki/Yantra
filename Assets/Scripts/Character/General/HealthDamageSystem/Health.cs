using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable,IHeal
{
    [SerializeField] protected float maxHealth = 10f;
    public float MaxHealth => maxHealth;
    
    [field: SerializeField]
    public float CurrentHP { get; protected set; }
    public bool IsDead => CurrentHP <= 0;

    public event Action OnDead;
    public event Action<float> OnHealthChanged;
    public event Action Onhit;

    protected void DeadEvent() => OnDead?.Invoke();
    protected void HealthChangedEvent() => OnHealthChanged?.Invoke(CurrentHP);

    protected virtual void Awake()
    {
        CurrentHP = maxHealth;
    }

    private void Update()
    {
        // Debugging purpose only, remove this in production
        if (Input.GetKeyDown(KeyCode.J))
        {
            TakeDamage(1f);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Kill();
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (IsDead)
            {
                CurrentHP = maxHealth; // Revive with full health
            }
            Heal(1f); // Heal by 1
        }
    }

    public virtual void Heal(float amount)
    {
        if (IsDead || IgnoreDamage) return;

        CurrentHP += amount;
        if (CurrentHP > maxHealth)
            CurrentHP = maxHealth;
        HealthChangedEvent();
        Debug.Log($"{gameObject.name} <color=#32CD32> healed {amount}.</color> Current HP: {CurrentHP}/{maxHealth}");
    }

    public virtual void TakeDamage(float damage)
    {
        if (IsDead || IgnoreDamage) return;

        CurrentHP -= damage;
        HealthChangedEvent();
        Onhit?.Invoke();

        Debug.Log($"{gameObject.name} took {damage} damage. Current HP: {CurrentHP}/{maxHealth}");
        if (CurrentHP <= 0)
        {
            Debug.Log($"{gameObject.name} is dead.");
            CurrentHP = 0;
            DeadEvent();
        }
    }

    public void Kill()
    {
        if (IsDead) return;

        CurrentHP = 0;
        HealthChangedEvent();
        DeadEvent();
    }

    public bool IgnoreDamage { get; private set; } = false;
    public void EnableIgnoreDamage(bool enable)
    {
        IgnoreDamage = enable;
    }
    
    public void SetCurrentHealth(float amount)
    {
        if(amount <= 0) return;
        CurrentHP = amount;
    }

    public void RestoreFullHealth()
    {
        CurrentHP = maxHealth;
        HealthChangedEvent();
    }
}
