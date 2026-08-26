using System.Collections;
using UnityEngine;

public class ForcePLayAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string AnimationName;
    [SerializeField] private bool isLooping = true; // เปิด-ปิด การ Loop ผ่าน Inspector

    private Coroutine loopCoroutine;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        PlayAnimByName(AnimationName);
    }

    public void PlayAnimByName(string stateName)
    {
        if (animator != null && !string.IsNullOrEmpty(stateName))
        {
            if (loopCoroutine != null)
            {
                StopCoroutine(loopCoroutine);
            }

            loopCoroutine = StartCoroutine(LoopAnimationRoutine(stateName));
        }
    }

    private IEnumerator LoopAnimationRoutine(string stateName)
    {
        // วนลูปภายใน Coroutine ตัวเดียว ไม่ต้องเรียกสั่ง StartCoroutine ใหม่ซ้ำๆ
        do
        {
            animator.Play(stateName, 0, 0f);

            // รอ 1 เฟรมเพื่อให้ Animator อัปเดต State ปัจจุบันก่อนอ่านค่าความยาว
            yield return null;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float animLength = stateInfo.length;

            if (animLength <= 0) animLength = 1f;

            // หักลบเวลา 1 เฟรมที่ yield return null ไปแล้ว เพื่อให้ Timing เป๊ะขึ้น
            yield return new WaitForSeconds(animLength - Time.deltaTime);

        } while (isLooping); // ถ้า isLooping เป็น true จะวนกลับไปเริ่มเล่นใหม่ทันที
    }

    private void OnDisable()
    {
        // เคลียร์ Coroutine เมื่อ Object ถูกซ่อนหรือทำลาย ป้องกัน Memory Leak
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
        }
    }
}
