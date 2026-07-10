using UnityEngine;

public class ActiveStateSyncer : MonoBehaviour
{
    [Tooltip("Object อ้างอิงที่จะคอยตรวจสอบสถานะ")]
    [SerializeField] private GameObject _referenceObject;

    [Tooltip("Object เป้าหมายที่จะถูกเปิด/ปิดตาม _referenceObject")]
    [SerializeField] private GameObject _targetObject;

    private void Update()
    {
        if (_referenceObject == null || _targetObject == null) return;

        // new: อ่านสถานะจากตัวอ้างอิง และปรับตัวเป้าหมายให้ตรงกัน
        bool isReferenceActive = _referenceObject.activeInHierarchy;

        if (_targetObject.activeSelf != isReferenceActive)
        {
            _targetObject.SetActive(isReferenceActive);
        }
    }
}