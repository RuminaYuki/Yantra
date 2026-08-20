using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "SetLightEnabledAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Light/Set Light Enabled")]
public class SetLightEnabledActionSO : StateActionSO
{
    [SerializeField] private GameObjectAnchor _lightAnchor;
    [SerializeField] private bool _enabled;
    [SerializeField] private bool _resetOnStateExit = true;

    [Header("Audio (Optional)")]
    [Tooltip("คูปองเสียงที่ต้องการให้เล่นเมื่อ 'เปิด' ไฟ")]
    [SerializeField] private SoundID _turnOnSound;

    [Tooltip("คูปองเสียงที่ต้องการให้เล่นเมื่อ 'ปิด' ไฟ")]
    [SerializeField] private SoundID _turnOffSound;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new SetLightEnabledAction(
            _lightAnchor,
            _enabled,
            _resetOnStateExit,
            _turnOnSound,
            _turnOffSound);
    }
}

public class SetLightEnabledAction : StateAction
{
    private readonly GameObjectAnchor _lightAnchor;
    private readonly bool _enabled;
    private readonly bool _resetOnStateExit;
    private readonly SoundID _turnOnSound;
    private readonly SoundID _turnOffSound;

    private GameObject _owner;
    private Light _targetLight;
    private bool _previousEnabled;
    private bool _isApplied;

    public SetLightEnabledAction(
        GameObjectAnchor lightAnchor,
        bool enabled,
        bool resetOnStateExit,
        SoundID turnOnSound,
        SoundID turnOffSound)
    {
        _lightAnchor = lightAnchor;
        _enabled = enabled;
        _resetOnStateExit = resetOnStateExit;
        _turnOnSound = turnOnSound;
        _turnOffSound = turnOffSound;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _owner = stateMachine.Owner;

        if (_lightAnchor == null)
        {
            Debug.LogError(
                "SetLightEnabledAction has no Light Anchor.",
                _owner);
        }
    }

    public override void OnStateEnter()
    {
        _targetLight = ResolveLight();

        if (_targetLight == null)
        {
            return;
        }

        _previousEnabled = _targetLight.enabled;
        _targetLight.enabled = _enabled;
        _isApplied = true;

        SoundID soundToPlay = _enabled ? _turnOnSound : _turnOffSound;

        if (soundToPlay != null && SoundManager.Instance != null)
        {
            Vector3 soundPos = _targetLight.transform.position;
            SoundManager.Instance.PlaySFX(soundToPlay, soundPos);
        }
    }

    public override void OnUpdate()
    {
    }

    public override void OnStateExit()
    {
        if (_targetLight == null || !_isApplied)
        {
            return;
        }

        if (_resetOnStateExit)
        {
            _targetLight.enabled = _previousEnabled;

            // เพิ่มใหม่: ดักจับตอน State ถูก Reset แล้วเล่นเสียงให้ถูกต้อง
            SoundID soundToPlay = _previousEnabled ? _turnOnSound : _turnOffSound;
            if (soundToPlay != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(soundToPlay, _targetLight.transform.position);
            }
        }

        _targetLight = null;
        _isApplied = false;
    }

    private Light ResolveLight()
    {
        if (_lightAnchor == null || !_lightAnchor.IsSet)
        {
            Debug.LogWarning(
                "SetLightEnabledAction Light Anchor is not set.",
                _owner);
            return null;
        }

        Light targetLight = _lightAnchor.Value.GetComponent<Light>();

        if (targetLight == null)
        {
            Debug.LogWarning(
                $"SetLightEnabledAction could not find a Light on " +
                $"'{_lightAnchor.Value.name}'.",
                _lightAnchor.Value);
        }

        return targetLight;
    }
}