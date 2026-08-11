using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class SubtitleUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI _subtitleText;

    [Header("Fade Settings")]
    [SerializeField] private float _fadeDuration = 0.25f;

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        SubtitleSystem.OnSubtitleToggle += HandleSubtitleToggle;
        SubtitleSystem.OnSubtitleUpdate += HandleSubtitleUpdate;
    }

    private void OnDisable()
    {
        SubtitleSystem.OnSubtitleToggle -= HandleSubtitleToggle;
        SubtitleSystem.OnSubtitleUpdate -= HandleSubtitleUpdate;
    }

    private void HandleSubtitleToggle(bool isVisible)
    {
        if (!isVisible)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeOutAndClearRoutine());
        }
    }

    private void HandleSubtitleUpdate(string newText)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(CrossfadeTextRoutine(newText));
    }

    private IEnumerator CrossfadeTextRoutine(string newText)
    {
        // 1. ถ้ามีข้อความเก่าค้างอยู่ ให้เฟดออกอย่างนุ่มนวล
        while (_canvasGroup.alpha > 0f)
        {
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, 0f, Time.deltaTime / _fadeDuration);
            yield return null;
        }

        // 🌟 จุดแก้บั๊ก: ล็อคให้มืดสนิท 100% ก่อนเปลี่ยนข้อความ ป้องกันการกระตุกฟึบ!
        _canvasGroup.alpha = 0f;

        if (_subtitleText != null)
        {
            _subtitleText.text = newText;
        }

        // 2. ค่อยๆ เฟดสว่างข้อความใหม่เข้ามาบนจอ
        while (_canvasGroup.alpha < 1f)
        {
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, 1f, Time.deltaTime / _fadeDuration);
            yield return null;
        }

        // ล็อคให้สว่างเต็มที่ ป้องกันบั๊กจุดทศนิยม
        _canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutAndClearRoutine()
    {
        while (_canvasGroup.alpha > 0f)
        {
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, 0f, Time.deltaTime / _fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        if (_subtitleText != null)
        {
            _subtitleText.text = "";
        }
    }
}