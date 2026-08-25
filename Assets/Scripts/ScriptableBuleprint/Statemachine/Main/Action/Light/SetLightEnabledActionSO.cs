using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

public enum LightTargetMode
{
    LightComponent,
    GameObject
}

[CreateAssetMenu(
    fileName = "SetLightEnabledAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Light/Set Light Enabled")]
public class SetLightEnabledActionSO : StateActionSO
{
    [SerializeField] private GameObjectAnchor _lightAnchor;

    [Tooltip("LightComponent = เปิด/ปิด Light component (พฤติกรรมเดิม)\n" +
        "GameObject = เปิด/ปิด GameObject ทั้งตัว (ใช้กับไฟฉายที่เป็นโมเดล 3D)")]
    [SerializeField] private LightTargetMode _targetMode = LightTargetMode.LightComponent;
    [SerializeField] private bool _enabled;
    [SerializeField] private bool _resetOnStateExit = true;

    [Tooltip("เล่นเสียงตอนออกจาก State ด้วยหรือไม่\n" +
        "ปิดได้ถ้า State นั้นมีเสียงอื่นดังอยู่แล้ว เช่นจบพร้อมคัตซีน")]
    [SerializeField] private bool _playSoundOnExit = true;

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
            _turnOffSound,
            _targetMode,
            _playSoundOnExit);
    }
}

public class SetLightEnabledAction : StateAction
{
    private readonly GameObjectAnchor _lightAnchor;
    private readonly bool _enabled;
    private readonly bool _resetOnStateExit;
    private readonly SoundID _turnOnSound;
    private readonly SoundID _turnOffSound;
    private readonly LightTargetMode _targetMode;
    private readonly bool _playSoundOnExit;

    private GameObject _owner;
    private Light _targetLight;
    private GameObject _targetObject;
    private bool _previousEnabled;
    private bool _isApplied;

    public SetLightEnabledAction(
        GameObjectAnchor lightAnchor,
        bool enabled,
        bool resetOnStateExit,
        SoundID turnOnSound,
        SoundID turnOffSound,
        LightTargetMode targetMode,
        bool playSoundOnExit)
    {
        _lightAnchor = lightAnchor;
        _enabled = enabled;
        _resetOnStateExit = resetOnStateExit;
        _turnOnSound = turnOnSound;
        _turnOffSound = turnOffSound;
        _targetMode = targetMode;
        _playSoundOnExit = playSoundOnExit;
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
        if (_targetMode == LightTargetMode.GameObject)
        {
            _targetObject = ResolveGameObject();
            if (_targetObject == null) return;

            _previousEnabled = _targetObject.activeSelf;

            // จำตำแหน่งไว้ก่อนปิด เพราะพอ SetActive(false) แล้วยังอ่านได้ก็จริง
            // แต่เก็บไว้ก่อนชัดเจนกว่า และกันเคสที่ object ถูกย้ายระหว่างนั้น
            Vector3 objectPos = _targetObject.transform.position;

            _targetObject.SetActive(_enabled);
            _isApplied = true;

            PlayStateSound(_enabled, objectPos);
            return;
        }

        _targetLight = ResolveLight();

        if (_targetLight == null)
        {
            return;
        }

        _previousEnabled = _targetLight.enabled;
        _targetLight.enabled = _enabled;
        _isApplied = true;

        PlayStateSound(_enabled, _targetLight.transform.position);
    }

    public override void OnUpdate()
    {
    }

    public override void OnStateExit()
    {
        if (!_isApplied)
        {
            return;
        }

        if (_targetMode == LightTargetMode.GameObject)
        {
            if (_targetObject != null && _resetOnStateExit)
            {
                Vector3 objectPos = _targetObject.transform.position;
                _targetObject.SetActive(_previousEnabled);

                if (_playSoundOnExit)
                    PlayStateSound(_previousEnabled, objectPos);
            }

            _targetObject = null;
            _isApplied = false;
            return;
        }

        if (_targetLight == null)
        {
            _isApplied = false;
            return;
        }

        if (_resetOnStateExit)
        {
            _targetLight.enabled = _previousEnabled;

            // ดักจับตอน State ถูก Reset แล้วเล่นเสียงให้ถูกต้อง
            if (_playSoundOnExit)
                PlayStateSound(_previousEnabled, _targetLight.transform.position);
        }

        _targetLight = null;
        _isApplied = false;
    }

    // รวมตรรกะเล่นเสียงไว้ที่เดียว จะได้ไม่เขียนซ้ำ 4 รอบ
    private void PlayStateSound(bool isOn, Vector3 position)
    {
        SoundID soundToPlay = isOn ? _turnOnSound : _turnOffSound;

        if (soundToPlay == null || SoundManager.Instance == null) return;

        SoundManager.Instance.PlaySFX(soundToPlay, position);
    }

    private GameObject ResolveGameObject()
    {
        if (_lightAnchor == null || !_lightAnchor.IsSet)
        {
            Debug.LogWarning(
                "SetLightEnabledAction Light Anchor is not set.",
                _owner);
            return null;
        }

        return _lightAnchor.Value;
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

        // ใช้ GetComponentInChildren เพราะโมเดลไฟฉาย 3D จะเป็นตัวแม่
        // ส่วน Light component เป็นลูกอยู่ข้างใน — GetComponent เฉยๆ จะหาไม่เจอ
        // (หาบนตัวเองก่อนเสมอ ของเดิมที่ Light อยู่บนตัวแม่จึงยังใช้ได้ปกติ)
        Light targetLight = _lightAnchor.Value.GetComponentInChildren<Light>(true);

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