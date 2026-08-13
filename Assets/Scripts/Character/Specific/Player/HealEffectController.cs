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
    [SerializeField] private float maxRadius = 2.5f;
    [SerializeField] private float duration = 1.5f;
    [SerializeField]
    private AnimationCurve easeCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private static readonly int HealRadiusID = Shader.PropertyToID("_HealRadius");
    private readonly List<MaterialPropertyBlock> _mpbPool = new();

    private void OnValidate()
    {
        CacheRenderers();
    }

    private void Awake()
    {
        CacheRenderers();
        EnsurePropertyBlockPool();
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

            // เอาเฉพาะตัวที่มี material ที่ใช้ _HealRadius จริงๆ (กันไปเซ็ตค่าใส่ renderer ที่ไม่เกี่ยว)
            bool hasHealMaterial = false;
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat && mat.HasProperty(HealRadiusID))
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
        StartCoroutine(HealRoutine());
    }

    private IEnumerator HealRoutine()
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);
            float eased = easeCurve.Evaluate(normalized);
                float currentRadius = eased * maxRadius;

            ApplyRadiusToAll(currentRadius);

            yield return null;
        }

        yield return new WaitForSeconds(0.3f);
        ApplyRadiusToAll(0f);
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
}