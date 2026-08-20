using Kogetsu.Library.Core;
using Kogetsu.Library.DesignPatternCore;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YantraStatsController : MonoBehaviour
{
    [SerializeField] private FPPMoveController3D _moveController;

    [Header("Yantra Stats")]
    [SerializeField] protected int yantCount;

    // ─── Stats Data ────────────────────────────────────────────────────
    [Header("Stats Data")]
    [Required]
    [SerializeField] protected BaseStatsDataSO StatsData;

    [ReadOnly][SerializeField] protected float CurrentMoveSpeed;
    [ReadOnly][SerializeField] protected float CurrectJumpForce;

    // ─── HP ────────────────────────────────────────────────────────────
    [Header("HP")]
    [ReadOnly][SerializeField] protected float CurrentHp;
    [SerializeField] protected float MinHp;

    // ─── HP UI ─────────────────────────────────────────────────────────

    [Header("Stamina")]
    [ReadOnly][SerializeField] protected float CurrentStamina;
    [SerializeField] protected float MinStamina;

    public bool IsHurt
    {
        get => _isHurt;
        set
        {
            if (_isHurt == value) return; // ป้องกันการทำงานซ้ำซ้อน
            _isHurt = value;
            SpeedManament(); // คำนวณความเร็วใหม่เฉพาะตอนสถานะเปลี่ยน
        }
    }

    public bool IsMoving
    {
        get => _isMoving;
        set
        {
            if (_isMoving == value) return; // ป้องกันการส่ง Event รัวๆ
            _isMoving = value;
            //if (EventBus.Instance) EventBus.Instance.Publish(new PlayerMovingEvent(value));
        }
    }

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (_isRunning == value) return; // ป้องกันการส่ง Event รัวๆ
            _isRunning = value;
            SpeedManament(); // คำนวณความเร็วใหม่เฉพาะตอนสลับวิ่ง/เดิน
            //if (EventBus.Instance) EventBus.Instance.Publish(new PlayerRunningEvent(value));
        }
    }
    public bool IsJumping
    {
        get => _isJumping;
        set
        {
            if (_isJumping == value) return; // ป้องกันการส่ง Event รัวๆ
            _isJumping = value;
            // if (EventBus.Instance) EventBus.Instance.Publish(new PlayerJumpingEvent(value));
        }
    }
    public bool IsGrounded
    {
        get => _isGrounded;
        set
        {
            if (_isGrounded == value) return; // ป้องกันการส่ง Event รัวๆ
            _isGrounded = value;
            // if (EventBus.Instance) EventBus.Instance.Publish(new PlayerGroundEvent(value));
        }
    }

    [ReadOnly]
    [SerializeField] private bool _isHurt;
    [ReadOnly]
    [SerializeField] private bool _isMoving;
    [ReadOnly]
    [SerializeField] private bool _isRunning;
    [ReadOnly]
    [SerializeField] private bool _isJumping;
    [ReadOnly]
    [SerializeField] private bool _isGrounded;

    [SerializeField] private float _staminaRegenRateNormal = 30f;
    [SerializeField] private float _staminaRegenRateBad = 20f;
    [SerializeField] private float _runStaminaCost = 20f;
    [SerializeField] private float _jumpStaminaCost = 25f;
    [SerializeField] private float _regenStaminaDelay = 0.3f;
    [ReadOnly]
    [SerializeField] private float _currentRegenStaminaDelay;

    [Header("Stamina Voice Thresholds")]
    [Tooltip("Stamina เหลือต่ำกว่า % นี้ (0-1) จะเล่นเสียงเหนื่อยปกติ")]
    [SerializeField, Range(0f, 1f)] private float _staminaLowPercent = 0.3f;
    private bool _staminaLowAnnounced;
    private bool _staminaDepletedAnnounced;


    // ─── Properties ────────────────────────────────────────────────────
    public float MaxHp => StatsData != null ? StatsData.BaseHp : 0f;
    public float MaxStamina => StatsData != null ? StatsData.BaseStamina : 0f;

    private void Update()
    {
        // ย้าย SpeedManament() ออกไปแล้ว จะได้ไม่กิน CPU ทุกเฟรม
        StaminaManagement();
    }

    // ─── Stats Accessors ───────────────────────────────────────────────
    public float GetBaseMoveSpeed() => StatsData != null ? StatsData.BaseMoveSpeed : 0f;
    public float GetBaseJumpForce() => StatsData != null ? StatsData.BaseJumpForce : 0f;
    public float GetMoveSpeed() => IsHurt ? CurrentMoveSpeed * 0.8f : CurrentMoveSpeed;
    public float GetJumpForce() => CurrectJumpForce;
    public float GetMoveSpeedPercentage() =>
        GetBaseMoveSpeed() > 0f ? CurrentMoveSpeed / GetBaseMoveSpeed() : 0f;

    // ─── Lifecycle ─────────────────────────────────────────────────────
    protected virtual void Setup()
    {
        CurrentMoveSpeed = GetBaseMoveSpeed();
        CurrectJumpForce = GetBaseJumpForce();

        CurrentHp = MaxHp;
        CurrentStamina = MaxStamina;
    }



    protected virtual void Start() => Setup();

#if UNITY_EDITOR
    /// <summary>
    /// Called by Unity when any serialized field changes in the Inspector (edit + play mode).
    /// In play mode: clamps current values so they stay within updated min/max bounds.
    /// </summary>
    protected virtual void OnValidate()
    {
        if (!Application.isPlaying) return;
        CurrentHp = Mathf.Clamp(CurrentHp, MinHp, MaxHp);
        CurrentStamina = Mathf.Clamp(CurrentStamina, MinStamina, MaxStamina);
    }
#endif

    protected void OnEnable()
    {
        // if (!EventBus.Instance) return;
        // EventBus.Instance.Subscribe<DealDamageEvent>(OnTakeDamage);
    }

    protected void OnDisable()
    {
        // if (!EventBus.Instance) return;
        // EventBus.Instance.Unsubscribe<DealDamageEvent>(OnTakeDamage);
    }

    public void TakeDamage(float damage)
    {
        CurrentHp = Mathf.Clamp(CurrentHp - damage, MinHp, MaxHp);

        // if (EventBus.Instance)
            // EventBus.Instance.Publish(new PlayerTakeDamageEvent(damage, CurrentHp));
    }

    public void Heal(float healAmount)
    {
        CurrentHp = Mathf.Clamp(CurrentHp + healAmount, MinHp, MaxHp);
    }

    public void SetCurrentHp(float hp)
    {
        CurrentHp = Mathf.Clamp(hp, MinHp, MaxHp);
    }

    public void ResetHp()
    {
        CurrentHp = MaxHp;
    }

    public float GetCurrentHp() => CurrentHp;
    public float GetMaxHp() => MaxHp;
    public float GetMinHp() => MinHp;

    public void Run(bool isRunning)
    {
        IsRunning = isRunning;
    }

    public void SpeedManament()
    {
        CurrentMoveSpeed = IsHurt ? GetBaseMoveSpeed() * 0.8f : GetBaseMoveSpeed();

        if (IsHurt) return;

        CurrentMoveSpeed = IsRunning ? GetBaseMoveSpeed() * 1.5f : GetBaseMoveSpeed();
    }

    // ─── Stamina ───────────────────────────────────────────────────────
    public void UseStamina(float amount)
    {
        CurrentStamina = Mathf.Clamp(CurrentStamina - amount, MinStamina, MaxStamina);
        _currentRegenStaminaDelay = _regenStaminaDelay;
    }

    public void RestoreStamina(float amount)
    {
        CurrentStamina = Mathf.Clamp(CurrentStamina + amount, MinStamina, MaxStamina);
    }

    public void ResetStamina()
    {
        CurrentStamina = MaxStamina;
    }

    public float GetCurrentStamina() => CurrentStamina;
    public float GetMaxStamina() => MaxStamina;
    public float GetRunStaminaCost() => _runStaminaCost;
    public float GetJumpStaminaCost() => _jumpStaminaCost;

    // ─── EventBus Handler ──────────────────────────────────────────────
    protected void OnTakeDamage(DealDamageEvent data)
    {
        TakeDamage(data.Damage);
        // if (EventBus.Instance) EventBus.Instance.Publish(new TakeDamageEvent(data.Damage, CurrentHp));
    }

    public int GetYantCount()
    {
        return yantCount;
    }

    public void SetYantCount(int count)
    {
        yantCount = Mathf.Max(0, count);
    }

    public void ApplySavedValues(float hp, float stamina)
    {
        CurrentHp = Mathf.Clamp(hp, MinHp, MaxHp);
        CurrentStamina = Mathf.Clamp(stamina, MinStamina, MaxStamina);
    }

    public void AddYantCount(int amount)
    {
        yantCount += amount;
    }


    private void StaminaManagement()
    {
        float staminaPercent = MaxStamina > 0f ? CurrentStamina / MaxStamina : 1f;

        if (CurrentStamina <= 0f)
        {
            IsHurt = true;

            if (!_staminaDepletedAnnounced)
            {
                _staminaDepletedAnnounced = true;
                _staminaLowAnnounced = true; // depleted ครอบ low ด้วย
                PlayerVoice.Publish(PlayerVoice.StaminaDepleted);
            }
        }
        else if (!_staminaLowAnnounced && staminaPercent <= _staminaLowPercent)
        {
            _staminaLowAnnounced = true;
            PlayerVoice.Publish(PlayerVoice.StaminaLow);
        }

        if (CurrentStamina >= MaxStamina)
        {
            IsHurt = false;
            _staminaDepletedAnnounced = false;
            _staminaLowAnnounced = false; // re-arm ทั้งคู่เมื่อ stamina เต็ม
        }

        if (IsHurt)
        {
            RestoreStamina(_staminaRegenRateBad * Time.deltaTime);
        }

        else if (IsRunning)
        {
            UseStamina(_runStaminaCost * Time.deltaTime);
        }
        else
        {
            _currentRegenStaminaDelay -= Time.deltaTime;
            if (_currentRegenStaminaDelay <= 0f)
            {
                _currentRegenStaminaDelay = 0f;
                RestoreStamina(_staminaRegenRateNormal * Time.deltaTime);
            }

        }
    }
}