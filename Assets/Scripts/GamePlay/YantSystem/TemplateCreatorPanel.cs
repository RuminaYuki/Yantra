using UnityEngine;

namespace UIEditor
{
    public class TemplateCreatorPanel : MonoBehaviour
    {
        [SerializeField] private YantraShapeMatcher _shapeMatcher;
        [SerializeField] private GameObject _panelRoot;

        private void Awake()
        {
            if (_panelRoot == null) _panelRoot = gameObject;
        }

        private void Update()
        {
            // new: ตรวจสอบแบบแยกส่วนว่า object ถูกทำลาย ปิด GameObject หรือปิด script อยู่หรือไม่
            if (_shapeMatcher == null || !_shapeMatcher.gameObject.activeInHierarchy || !_shapeMatcher.enabled)
            {
                if (_panelRoot != null && _panelRoot.activeSelf)
                {
                    _panelRoot.SetActive(false);
                }
                return;
            }

            /*bool shouldShow = _shapeMatcher.CreateTemplate;

            if (_panelRoot.activeSelf != shouldShow)
            {
                _panelRoot.SetActive(shouldShow);
            }*/
        }
    }
}