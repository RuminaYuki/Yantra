using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    [Serializable]
    private class SkinMaterialData
    {
        public SkinnedMeshRenderer Renderer;
        public Material[] Materials;
    }

    [Header("Material Settings")]
    [ReadOnly, SerializeField] private Transform _parrentTransform;
    [SerializeField] private List<Material> _characterMaterial;
    [SerializeField] private List<SkinMaterialData> _additionalMaterials = new();

    [Header("Yant Invisible Settings")]
    [SerializeField] private List<Material> _yantInvisbleMaterial;
    [SerializeField] private Color _yantInvisbleColor = new(0.37f, 0.16f, 0.49f, 1f);
    [SerializeField] private ParticleSystem _yantInvisbleSmoke;
    [SerializeField] private bool _yantInvisible;
    [SerializeField] private StatSO _statSOInvis;

    [Header("Heal Settings")]
    [SerializeField] private List<Material> _healMaterial;
    [SerializeField] private float _maxHealRadius = 2.5f;
    [SerializeField] private float _healDuration = 1.5f;
    [SerializeField] private StatSO _statSOHeal;

    [Header("References")]
    [SerializeField] private StatObserver _statObserver;

    [SerializeField]
    private AnimationCurve _healEaseCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private static readonly int HealRadiusID =
        Shader.PropertyToID("_HealRadius");

    private static readonly int HealOpacityID =
        Shader.PropertyToID("_HealOpacity");

    private readonly List<MaterialPropertyBlock> _healPropertyBlockPool = new();

    private Coroutine _healCoroutine;

    #region Unity Lifecycle

    private void OnValidate()
    {
        CacheMaterials();
    }

    private void Awake()
    {
        CacheMaterials();
        EnsureHealPropertyBlockPool();

        SetDefault();
    }

    private void OnEnable()
    {
        _statObserver = GetComponent<StatObserver>();
        if (_statObserver != null) _statObserver.OnStatChanged += HandleStatChanged;
    }

    private void OnDisable()
    {
        _statObserver = GetComponent<StatObserver>();
        if (_statObserver != null) _statObserver.OnStatChanged -= HandleStatChanged;

        if (_healCoroutine != null)
        {
            StopCoroutine(_healCoroutine);
            _healCoroutine = null;
        }

        RemoveHealMaterials();

        SetDefault();
    }

    private void Update()
    {
        

        if (Input.GetKeyDown(KeyCode.I))
        {
            _yantInvisible = !_yantInvisible;
            OnInvisible(_yantInvisible);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            PlayHeal();
        }
    }

    #endregion

    private void HandleStatChanged(StatSO statSO, bool value)
    {
        if (statSO == _statSOInvis) OnInvisible(value);
        if (statSO == _statSOHeal && value == true) PlayHeal();
    }

    #region Yant Invisible

    private void OnInvisible(bool isInvisible)
    {
        _yantInvisible = isInvisible;

        if (isInvisible)
        {
            foreach (Material material in _characterMaterial)
            {
                if (!material)
                {
                    continue;
                }

                material.SetColor(
                    "_BaseColor",
                    _yantInvisbleColor);

                material.SetFloat(
                    "_Opacity",
                    90f);
            }

            foreach (SkinMaterialData skinMaterialData in _additionalMaterials)
            {
                if (!skinMaterialData.Renderer)
                {
                    continue;
                }

                AddMaterials(
                    skinMaterialData.Renderer,
                    _yantInvisbleMaterial);
            }

            if (_yantInvisbleSmoke)
            {
                _yantInvisbleSmoke.Play();
            }
        }
        else
        {
            foreach (Material material in _characterMaterial)
            {
                if (!material)
                {
                    continue;
                }

                material.SetColor(
                    "_BaseColor",
                    Color.white);

                material.SetFloat(
                    "_Opacity",
                    100f);
            }

            foreach (SkinMaterialData skinMaterialData in _additionalMaterials)
            {
                if (!skinMaterialData.Renderer)
                {
                    continue;
                }

                RemoveMaterials(
                    skinMaterialData.Renderer,
                    _yantInvisbleMaterial);
            }

            if (_yantInvisbleSmoke)
            {
                _yantInvisbleSmoke.Stop();
            }
        }
    }

    #endregion

    #region Heal

    public void PlayHeal()
    {
        if (_healCoroutine != null)
        {
            StopCoroutine(_healCoroutine);
        }

        AddHealMaterials();

        ApplyHealOpacity(1f);
        ApplyHealRadius(0f);

        _healCoroutine = StartCoroutine(HealRoutine());
    }

    private IEnumerator HealRoutine()
    {
        float time = 0f;

        while (time < _healDuration)
        {
            time += Time.deltaTime;

            float normalized =
                Mathf.Clamp01(time / _healDuration);

            float eased =
                _healEaseCurve.Evaluate(normalized);

            float currentRadius =
                eased * _maxHealRadius;

            ApplyHealRadius(currentRadius);

            yield return null;
        }

        ApplyHealRadius(_maxHealRadius);

        _healCoroutine = null;
    }

    private void AddHealMaterials()
    {
        foreach (SkinMaterialData skinMaterialData in _additionalMaterials)
        {
            if (!skinMaterialData.Renderer)
            {
                continue;
            }

            AddMaterials(
                skinMaterialData.Renderer,
                _healMaterial);
        }
    }

    private void RemoveHealMaterials()
    {
        foreach (SkinMaterialData skinMaterialData in _additionalMaterials)
        {
            if (!skinMaterialData.Renderer)
            {
                continue;
            }

            RemoveMaterials(
                skinMaterialData.Renderer,
                _healMaterial);
        }
    }

    private void ApplyHealRadius(float radius)
    {
        for (int i = 0; i < _additionalMaterials.Count; i++)
        {
            SkinMaterialData skinMaterialData =
                _additionalMaterials[i];

            if (!skinMaterialData.Renderer)
            {
                continue;
            }

            MaterialPropertyBlock propertyBlock =
                _healPropertyBlockPool[i];

            skinMaterialData.Renderer.GetPropertyBlock(
                propertyBlock);

            propertyBlock.SetFloat(
                HealRadiusID,
                radius);

            skinMaterialData.Renderer.SetPropertyBlock(
                propertyBlock);
        }
    }

    private void ApplyHealOpacity(float opacity)
    {
        for (int i = 0; i < _additionalMaterials.Count; i++)
        {
            SkinMaterialData skinMaterialData =
                _additionalMaterials[i];

            if (!skinMaterialData.Renderer)
            {
                continue;
            }

            MaterialPropertyBlock propertyBlock =
                _healPropertyBlockPool[i];

            skinMaterialData.Renderer.GetPropertyBlock(
                propertyBlock);

            propertyBlock.SetFloat(
                HealOpacityID,
                opacity);

            skinMaterialData.Renderer.SetPropertyBlock(
                propertyBlock);
        }
    }

    #endregion

    private void CacheMaterials()
    {
        if (!_parrentTransform)
        {
            TryGetComponent(out _parrentTransform);
        }

        if (!_parrentTransform)
        {
            return;
        }

        _additionalMaterials.Clear();

        SkinnedMeshRenderer[] skinnedMeshRenderers =
            _parrentTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            if (!skinnedMeshRenderer)
            {
                continue;
            }

            Material[] materials =
                skinnedMeshRenderer.sharedMaterials;

            List<Material> additionalMaterials = new();

            foreach (Material material in materials)
            {
                if (!material)
                {
                    continue;
                }

                if (_characterMaterial.Contains(material))
                {
                    continue;
                }

                if (_healMaterial.Contains(material))
                {
                    continue;
                }

                additionalMaterials.Add(material);
            }

            _additionalMaterials.Add(new SkinMaterialData
            {
                Renderer = skinnedMeshRenderer,
                Materials = additionalMaterials.ToArray()
            });
        }

        EnsureHealPropertyBlockPool();
    }

    private void EnsureHealPropertyBlockPool()
    {
        _healPropertyBlockPool.Clear();

        for (int i = 0; i < _additionalMaterials.Count; i++)
        {
            _healPropertyBlockPool.Add(
                new MaterialPropertyBlock());
        }
    }

    private void SetDefault()
    {
        // Character Default
        foreach (Material material in _characterMaterial)
        {
            if (!material)
            {
                continue;
            }

            material.SetColor(
                "_BaseColor",
                Color.white);

            material.SetFloat(
                "_Opacity",
                100f);
        }

        // Invisible Default
        if (_yantInvisbleSmoke)
        {
            _yantInvisbleSmoke.Stop();
        }

        foreach (SkinMaterialData skinMaterialData in _additionalMaterials)
        {
            if (!skinMaterialData.Renderer)
            {
                continue;
            }

            RemoveMaterials(
                skinMaterialData.Renderer,
                _yantInvisbleMaterial);
        }

        // Heal Default
        RemoveHealMaterials();

        ApplyHealOpacity(0f);
        ApplyHealRadius(0f);
    }

    private void AddMaterials(
        SkinnedMeshRenderer renderer,
        List<Material> materialsToAdd)
    {
        if (!renderer ||
            materialsToAdd == null ||
            materialsToAdd.Count == 0)
        {
            return;
        }

        Material[] currentMaterials =
            renderer.sharedMaterials;

        List<Material> newMaterials =
            new(currentMaterials);

        foreach (Material material in materialsToAdd)
        {
            if (!material)
            {
                continue;
            }

            if (newMaterials.Contains(material))
            {
                continue;
            }

            newMaterials.Add(material);
        }

        renderer.sharedMaterials =
            newMaterials.ToArray();
    }

    private void RemoveMaterials(
        SkinnedMeshRenderer renderer,
        List<Material> materialsToRemove)
    {
        if (!renderer ||
            materialsToRemove == null ||
            materialsToRemove.Count == 0)
        {
            return;
        }

        Material[] currentMaterials =
            renderer.sharedMaterials;

        List<Material> restoredMaterials = new();

        foreach (Material material in currentMaterials)
        {
            if (!material)
            {
                continue;
            }

            if (materialsToRemove.Contains(material))
            {
                continue;
            }

            restoredMaterials.Add(material);
        }

        renderer.sharedMaterials =
            restoredMaterials.ToArray();
    }
}