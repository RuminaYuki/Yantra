using UnityEngine;

public class SimpleAimIK : MonoBehaviour
{
    [Header("IK Settings")]
    [SerializeField] private Animator _animator;

    [Tooltip("เป้าหมายที่ ตัวและหัว จะหันไปมอง (เอาไว้ไกลๆ หน้ากล้อง Z=15)")]
    [SerializeField] private Transform _lookTarget;

    [Tooltip("เป้าหมายที่ มือขวา จะขยับไปหา (เอาไว้ใกล้ๆ หน้าอก ไม่งั้นแขนยืด)")]
    [SerializeField] private Transform _rightHandTarget;

    public bool IsAiming = false;

    [Range(0, 1)]
    [SerializeField] private float _aimWeight = 0f;
    [SerializeField] private float _transitionSpeed = 10f;

    private void Start()
    {
        if (_animator == null) TryGetComponent(out _animator);
    }

    private void Update()
    {
        float targetWeight = IsAiming ? 1f : 0f;
        _aimWeight = Mathf.Lerp(_aimWeight, targetWeight, Time.deltaTime * _transitionSpeed);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_animator == null) return;

        // --- ส่วนที่ 1: บิดลำตัวและคอ (Look At) ---
        if (_lookTarget != null)
        {
            // น้ำหนัก(Weight, ตัว, หัว, ตา, กันกระดูกหัก)
            _animator.SetLookAtWeight(_aimWeight, 0.3f, 0.8f, 1f, 1f);
            _animator.SetLookAtPosition(_lookTarget.position);
        }

        // --- ส่วนที่ 2: Two-Bone IK (ไหล่ -> ศอก -> มือ) ---
        if (_rightHandTarget != null)
        {
            // เปิดสวิตช์อนุญาตให้โค้ดคุมตำแหน่งมือ
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, _aimWeight);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, _aimWeight);

            // สั่งให้มือขวา ขยับและหมุนไปอยู่ที่ RightHand_Target
            // (Unity จะคำนวณหักข้อศอกและไหล่ให้อัตโนมัติ)
            _animator.SetIKPosition(AvatarIKGoal.RightHand, _rightHandTarget.position);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, _rightHandTarget.rotation);
        }
    }
}