using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
// เพิ่ม , ICutsceneListener ต่อท้าย
public class StaminaUI : MonoBehaviour, ICutsceneListener
{
    [Header("References")]
    [SerializeField] private StaminaSystem _staminaSystem;
    [SerializeField] private RectTransform _fillBarRect;
    [SerializeField] private Image _fillBarImage;

    [Header("Fade Settings")]
    [SerializeField] private float _fadeSpeed = 5f;

    [Header("Color & Effect Settings")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _warningColor = new Color(1f, 0.5f, 0f); // ส้ม
    [SerializeField] private Color _exhaustedColor = Color.red; // แดง
    [SerializeField] private Color _recoveredFlashColor = Color.green;

    [Tooltip("ถ้าน้อยกว่าค่านี้จะเป็นสีส้ม (ปรับให้เตือนไวขึ้นเป็น 40%)")]
    [SerializeField] private float _warningThreshold = 0.40f;

    [Tooltip("ถ้าน้อยกว่าค่านี้จะเป็นสีแดงค้าง (ปรับเป็น 25%)")]
    [SerializeField] private float _criticalThreshold = 0.25f;

    [Tooltip("ถ้าน้อยกว่าค่านี้จะกระพริบเตือนก่อนหมด (ปรับเป็น 15% จะได้มองทัน)")]
    [SerializeField] private float _blinkThreshold = 0.15f;

    [Tooltip("ความเร็วกระพริบเตือนตอน 15% (ไฟไซเรน)")]
    [SerializeField] private float _warningBlinkSpeed = 10f;

    [Tooltip("ความเร็วกระพริบตอนหลอดแดงหมดเกลี้ยง (ยิ่งน้อยยิ่งช้า)")]
    [SerializeField] private float _exhaustedBlinkSpeed = 3f;

    [Tooltip("ความเร็วกระพริบตอนเด้งถึง 30%")]
    [SerializeField] private float _recoveryBlinkSpeed = 8f;

    private CanvasGroup _canvasGroup;
    private float _targetAlpha = 0f;
    private float _currentRatio = 1f;

    private bool _isExhaustedState = false;
    private bool _isRecoveryBlinking = false;
    private float _recoveryBlinkTimer = 0f;

    private bool _isCutsceneActive = false;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        if (_staminaSystem != null)
        {
            _staminaSystem.OnStaminaRatioChanged += UpdateStaminaUI;
            _staminaSystem.OnStaminaExhausted += HandleStaminaExhausted;
            _staminaSystem.OnStaminaRecovered += HandleStaminaRecovered;
        }
    }

    private void OnDisable()
    {
        if (_staminaSystem != null)
        {
            _staminaSystem.OnStaminaRatioChanged -= UpdateStaminaUI;
            _staminaSystem.OnStaminaExhausted -= HandleStaminaExhausted;
            _staminaSystem.OnStaminaRecovered -= HandleStaminaRecovered;
        }
    }

    private void HandleStaminaExhausted()
    {
        _isExhaustedState = true;
    }

    private void HandleStaminaRecovered()
    {
        _isExhaustedState = false;
        _isRecoveryBlinking = true;
        _recoveryBlinkTimer = 0.8f;
    }

    // 🌟 เปลี่ยนชื่อฟังก์ชันตรงนี้เพื่อรับคำสั่งจาก Cutscene
    public void OnCutsceneStateChanged(bool isPlaying)
    {
        _isCutsceneActive = isPlaying;

        // ถ้าเป็นคัทซีน บังคับเฟดหาย (_targetAlpha = 0)
        // ถ้าออกคัทซีน ให้เช็คว่าพลังเต็มไหม ถ้าไม่เต็มก็โชว์กลับมา (_targetAlpha = 1)
        _targetAlpha = isPlaying ? 0f : (_currentRatio >= 0.99f ? 0f : 1f);
    }

    private void UpdateStaminaUI(float ratio)
    {
        _currentRatio = ratio;
        _fillBarRect.localScale = new Vector3(ratio, 1f, 1f);

        if (!_isCutsceneActive)
        {
            _targetAlpha = (ratio >= 0.99f) ? 0f : 1f;
        }

        if (!_isExhaustedState && !_isRecoveryBlinking && _fillBarImage != null)
        {
            // ปล่อยให้ Update จัดการกระพริบถ้าต่ำกว่า 15%
            if (ratio <= _blinkThreshold && ratio > 0f)
            {
                return;
            }

            if (ratio <= _criticalThreshold) _fillBarImage.color = _exhaustedColor;
            else if (ratio <= _warningThreshold) _fillBarImage.color = _warningColor;
            else _fillBarImage.color = _normalColor;
        }
    }

    private void Update()
    {
        if (!Mathf.Approximately(_canvasGroup.alpha, _targetAlpha))
        {
            float speed = _fadeSpeed * 0.4f;
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, _targetAlpha, Time.deltaTime * speed);
        }

        if (_fillBarImage != null)
        {
            // 1. ตอนหลอดหมดเกลี้ยง (กระพริบแดงเข้ม-อ่อน ช้าๆ)
            if (_isExhaustedState)
            {
                float blinkAlpha = Mathf.PingPong(Time.time * _exhaustedBlinkSpeed, 0.6f) + 0.4f;
                Color blinkColor = _exhaustedColor;
                blinkColor.a = blinkAlpha;
                _fillBarImage.color = blinkColor;
            }
            // 2. ตอนเด้งแตะ 30% (แฟลชเขียว-ขาว)
            else if (_isRecoveryBlinking)
            {
                _recoveryBlinkTimer -= Time.deltaTime;
                float blinkLerp = Mathf.PingPong(Time.time * _recoveryBlinkSpeed, 1f);
                _fillBarImage.color = Color.Lerp(_normalColor, _recoveredFlashColor, blinkLerp);

                if (_recoveryBlinkTimer <= 0f)
                {
                    _isRecoveryBlinking = false;
                    UpdateStaminaUI(_currentRatio);
                }
            }
            // 3. กระพริบเตือนตอน 15% สุดท้าย (แฟลช แดง-ขาว รัวๆ เหมือนไซเรน!)
            else if (_currentRatio <= _blinkThreshold && _currentRatio > 0f)
            {
                // ใช้ Color.Lerp เพื่อสลับสีระหว่าง แดง กับ ขาว จะทำให้มองเห็นชัดเจนมากในฉากที่มืด
                float blinkLerp = Mathf.PingPong(Time.time * _warningBlinkSpeed, 1f);
                _fillBarImage.color = Color.Lerp(_exhaustedColor, Color.white, blinkLerp);
            }
        }
    }
}