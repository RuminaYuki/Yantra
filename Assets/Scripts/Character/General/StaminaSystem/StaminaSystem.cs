using System;
using UnityEngine;

// เพิ่ม , ICutsceneListener ต่อท้าย
public class StaminaSystem : MonoBehaviour, ICutsceneListener
{
    [Header("Stamina Settings")]
    [SerializeField] private float _maxStamina = 100f;
    [SerializeField] private float _recoverThreshold = 30f;

    [Tooltip("เวลาดีเลย์ (วินาที) ที่หลอดจะแดงค้างและไม่ยอมรีเจนตอนพลังหมด")]
    [SerializeField] private float _exhaustedDelay = 1.5f;

    public bool IsPaused { get; set; } = false;

    private bool _isExhausted = false;
    private float _currentStamina;
    private float _exhaustedUnlockTime; // เก็บเวลาในอนาคตที่อนุญาตให้เริ่มรีเจน

    public event Action<float> OnStaminaRatioChanged;
    public event Action OnStaminaExhausted;
    public event Action OnStaminaRecovered; // แจ้งเตือนตอนเด้งถึง 30%

    private void Awake()
    {
        _currentStamina = _maxStamina;
    }

    #region Public API

    public bool HasEnoughStamina(float amount)
    {
        if (_isExhausted) return false;
        return _currentStamina > 0f;
    }

    public bool TryConsumeStamina(float amount)
    {
        if (IsPaused || _isExhausted) return false;

        if (_isExhausted) return false;

        _currentStamina -= amount;

        if (_currentStamina <= 0f)
        {
            _currentStamina = 0f;
            _isExhausted = true;

            //ล็อกเวลาไว้ เช่น ตอนนี้วิที่ 10 + ดีเลย์ 1.5 = จะรีเจนได้ตอนวิที่ 11.5
            _exhaustedUnlockTime = Time.time + _exhaustedDelay;

            OnStaminaExhausted?.Invoke();
        }

        OnStaminaRatioChanged?.Invoke(_currentStamina / _maxStamina);
        return true;
    }

    public void RegenerateStamina(float amount)
    {
        if (IsPaused) return;

        // ถ้าติดหอบ และเวลายังไม่ถึงกำหนด ให้เด้งออกทันที (ไม่ยอมเพิ่มพลัง)
        if (_isExhausted && Time.time < _exhaustedUnlockTime) return;

        if (_currentStamina >= _maxStamina) return;

        _currentStamina += amount;

        // ถ้าพลังเด้งกลับมาถึง 30% ให้ปลดสถานะหอบ
        if (_isExhausted && _currentStamina >= _recoverThreshold)
        {
            _isExhausted = false;
            OnStaminaRecovered?.Invoke(); // ตะโกนบอก UI ให้กระพริบ
        }

        if (_currentStamina > _maxStamina)
        {
            _currentStamina = _maxStamina;
            if (_isExhausted)
            {
                _isExhausted = false;
                OnStaminaRecovered?.Invoke();
            }
        }

        OnStaminaRatioChanged?.Invoke(_currentStamina / _maxStamina);
    }

    // 🌟 ส่วนที่เพิ่มเข้ามาใหม่ เพื่อรับคำสั่งจาก Cutscene
    public void OnCutsceneStateChanged(bool isPlaying)
    {
        IsPaused = isPlaying;
    }

    #endregion
}