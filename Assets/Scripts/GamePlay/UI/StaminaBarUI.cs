using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StaminaSystem _staminaSystem;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private List<Image> _imageList = new();

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        _canvasGroup.alpha = 0f; // เริ่มต้นซ่อน UI

        _staminaSystem.OnStaminaRatioChanged += UpdateStaminaBar;
        _staminaSystem.OnStaminaExhausted += ShowExhaustedState;
        _staminaSystem.OnStaminaRecovered += ShowRecoveredState;
    }

    private void UpdateStaminaBar(float ratio)
    {
        foreach (var image in _imageList)
        {
            image.fillAmount = ratio;
        }

        switch(ratio)
        {
            //Change color to green when stamina is above 30%
            case >= 0.3f:
                ShowRecoveredState();
                break;
            //Change color to red when stamina is below 30%
            case <= 0.3f:
                ShowExhaustedState();
                break;
        }
        switch (ratio)
        {
            //Hide UI when stamina is full
            case >= 1f:
                FadeTo(0);
                break;
            //Show UI when stamina is below 100%
            case < 1f:
                FadeTo(1);
                break;
        }
    }

    private void ShowExhaustedState()
    {
        FadeTo(1); // แสดง UI
        foreach (var image in _imageList)
        {
            image.color = Color.red; // เปลี่ยนสีเป็นแดง
        }
    }

    private void ShowRecoveredState()
    {
        FadeTo(1); // แสดง UI
        foreach (var image in _imageList)
        {
            image.color = Color.green; // เปลี่ยนสีเป็นเขียว
        }
    }

    private void FadeTo(float targetAlpha)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCanvas(targetAlpha));
    }

    private IEnumerator FadeCanvas(float targetAlpha)
    {
        float startAlpha = _canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / fadeDuration;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        _canvasGroup.alpha = targetAlpha;
        fadeCoroutine = null;
    }
}
