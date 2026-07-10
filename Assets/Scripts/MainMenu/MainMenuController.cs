using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Villge";
    [SerializeField] private float inputDelay = 0.25f;
    [SerializeField] private float fadeDuration = 1.25f;
    [SerializeField] private Texture2D backgroundTexture;
    [SerializeField] private RawImage backgroundImage;
    [SerializeField] private Text promptLabel;
    [SerializeField] private Image fadeImage;
    [SerializeField] private MainManu inputGate;

    private float _readyTime;
    private bool _isLoading;

    private void Awake()
    {
        BindSceneReferences();
        ApplyMenuValues();
    }

    private void OnEnable()
    {
        _readyTime = Time.unscaledTime + inputDelay;
        _isLoading = false;

        if (inputGate == null)
        {
            inputGate = FindFirstObjectByType<MainManu>();
        }

        if (inputGate != null)
        {
            inputGate.AnyKeyPressed += HandleAnyKeyPressed;
        }
    }

    private void OnDisable()
    {
        if (inputGate != null)
        {
            inputGate.AnyKeyPressed -= HandleAnyKeyPressed;
        }
    }

    private void Update()
    {
        if (_isLoading || Time.unscaledTime < _readyTime)
        {
            return;
        }

        if (inputGate == null && (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            LoadTargetScene();
        }
    }

    private void HandleAnyKeyPressed(string inputName)
    {
        LoadTargetScene();
    }

    public void LoadTargetScene()
    {
        if (_isLoading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning($"{nameof(MainMenuController)} has no target scene.");
            return;
        }

        _isLoading = true;
        StartCoroutine(FadeAndLoadScene());
    }

    private IEnumerator FadeAndLoadScene()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = fadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeDuration);
            SetFadeAlpha(alpha);
            yield return null;
        }

        SetFadeAlpha(1f);
        SceneManager.LoadScene(targetSceneName);
    }

    private void BindSceneReferences()
    {
        if (backgroundImage == null)
        {
            var backgroundObject = GameObject.Find("Background");
            if (backgroundObject != null)
            {
                backgroundImage = backgroundObject.GetComponent<RawImage>();
            }
        }

        if (promptLabel == null)
        {
            var promptObject = GameObject.Find("Press Any Key Text");
            if (promptObject != null)
            {
                promptLabel = promptObject.GetComponent<Text>();
            }
        }

        if (fadeImage == null)
        {
            var fadeObject = GameObject.Find("Fade Overlay");
            if (fadeObject != null)
            {
                fadeImage = fadeObject.GetComponent<Image>();
            }
        }
    }

    private void ApplyMenuValues()
    {
        if (backgroundImage != null)
        {
            backgroundImage.texture = backgroundTexture;
        }

        if (promptLabel != null)
        {
            promptLabel.enabled = true;
            promptLabel.canvasRenderer.SetAlpha(1f);
        }

        SetFadeAlpha(0f);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null)
        {
            return;
        }

        fadeImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
    }
}
