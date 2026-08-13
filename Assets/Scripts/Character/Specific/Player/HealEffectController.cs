using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealEffectController : MonoBehaviour
{
    [Header("Target Renderers")]
    [ReadOnly, SerializeField] private Transform _parentTransform;
    [ReadOnly, SerializeField] private List<SkinnedMeshRenderer> _targetRenderers = new();

    [Header("Heal Settings")]
    [SerializeField] private float _maxRadius = 2.5f;
    [SerializeField] private float _duration = 1.5f;

    [SerializeField]
    private AnimationCurve _easeCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private static readonly int HealRadiusID =
        Shader.PropertyToID("_HealRadius");

    private static readonly int HealOpacityID =
        Shader.PropertyToID("_HealOpacity");

    private readonly List<MaterialPropertyBlock> _mpbPool = new();

    private void OnValidate()
    {
        CacheRenderers();
        EnsurePropertyBlockPool();
    }

    private void Awake()
    {
        CacheRenderers();
        EnsurePropertyBlockPool();

        // เริ่มเกมมา Effect มองไม่เห็น
        ApplyOpacityToAll(0f);
        ApplyRadiusToAll(0f);
    }

    private void CacheRenderers()
    {
        if (!_parentTransform)
        {
            TryGetComponent(out _parentTransform);
        }

        if (!_parentTransform)
        {
            return;
        }

        _targetRenderers.Clear();

        SkinnedMeshRenderer[] found =
            _parentTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        foreach (SkinnedMeshRenderer renderer in found)
        {
            if (!renderer)
            {
                continue;
            }

            bool hasHealMaterial = false;

            foreach (Material material in renderer.sharedMaterials)
            {
                if (material && material.HasProperty(HealRadiusID))
                {
                    hasHealMaterial = true;
                    break;
                }
            }

            if (hasHealMaterial)
            {
                _targetRenderers.Add(renderer);
            }
        }
    }

    private void EnsurePropertyBlockPool()
    {
        _mpbPool.Clear();

        for (int i = 0; i < _targetRenderers.Count; i++)
        {
            _mpbPool.Add(new MaterialPropertyBlock());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            PlayHeal();
        }
    }

    public void PlayHeal()
    {
        StopAllCoroutines();

        // กด H ปุ๊บ Effect กลับมาเห็นทันที
        ApplyOpacityToAll(1f);

        StartCoroutine(HealRoutine());
    }

    private IEnumerator HealRoutine()
    {
        float time = 0f;

        while (time < _duration)
        {
            time += Time.deltaTime;

            float normalized =
                Mathf.Clamp01(time / _duration);

            float eased =
                _easeCurve.Evaluate(normalized);

            float currentRadius =
                eased * _maxRadius;

            ApplyRadiusToAll(currentRadius);

            yield return null;
        }

        // ให้ Effect ค้างหลังเล่นจบ
        ApplyRadiusToAll(_maxRadius);
    }

    private void ApplyRadiusToAll(float radius)
    {
        for (int i = 0; i < _targetRenderers.Count; i++)
        {
            SkinnedMeshRenderer renderer = _targetRenderers[i];

            if (!renderer)
            {
                continue;
            }

            MaterialPropertyBlock mpb = _mpbPool[i];

            renderer.GetPropertyBlock(mpb);

            mpb.SetFloat(HealRadiusID, radius);

            renderer.SetPropertyBlock(mpb);
        }
    }

    private void ApplyOpacityToAll(float opacity)
    {
        for (int i = 0; i < _targetRenderers.Count; i++)
        {
            SkinnedMeshRenderer renderer = _targetRenderers[i];

            if (!renderer)
            {
                continue;
            }

            MaterialPropertyBlock mpb = _mpbPool[i];

            renderer.GetPropertyBlock(mpb);

            mpb.SetFloat(HealOpacityID, opacity);

            renderer.SetPropertyBlock(mpb);
        }
    }
}