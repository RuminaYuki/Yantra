using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

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
    [SerializeField] private VisualEffect _yantInvisbleSmoke;

    [SerializeField] private bool _yantInvisible;

    private bool _isInvisible = false;

    private void OnValidate()
    {
        CacheMaterials();
    }

    private void Awake()
    {
        CacheMaterials();
        SetDefault();
    }

    private void OnDisable()
    {
        SetDefault();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            _yantInvisible = !_yantInvisible;
            OnInvisible(_yantInvisible);
        }
    }

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

                material.SetColor("_BaseColor", _yantInvisbleColor);
                material.SetFloat("_Opacity", 90f);
            }

            foreach (SkinMaterialData skinMaterialData in _additionalMaterials)
            {
                if (!skinMaterialData.Renderer)
                {
                    continue;
                }

                AddInvisibleMaterials(skinMaterialData.Renderer);
            }

            if (_yantInvisbleSmoke)
            {
                _yantInvisbleSmoke.Play();
            }

#if UNITY_EDITOR
            CacheMaterials();
#endif
        }
        else
        {
            foreach (Material material in _characterMaterial)
            {
                if (!material)
                {
                    continue;
                }

                material.SetColor("_BaseColor", Color.white);
                material.SetFloat("_Opacity", 100f);
            }

            foreach (SkinMaterialData skinMaterialData in _additionalMaterials)
            {
                if (!skinMaterialData.Renderer)
                {
                    continue;
                }

                RemoveInvisibleMaterials(skinMaterialData.Renderer);
            }

            if (_yantInvisbleSmoke)
            {
                _yantInvisbleSmoke.Stop();
            }
        }
    }

    private void AddInvisibleMaterials(SkinnedMeshRenderer renderer)
    {
        if (_yantInvisbleMaterial == null ||
            _yantInvisbleMaterial.Count == 0)
        {
            return;
        }

        Material[] currentMaterials = renderer.sharedMaterials;

        List<Material> newMaterials = new(currentMaterials);

        foreach (Material invisibleMaterial in _yantInvisbleMaterial)
        {
            if (!invisibleMaterial)
            {
                continue;
            }

            if (newMaterials.Contains(invisibleMaterial))
            {
                continue;
            }

            newMaterials.Add(invisibleMaterial);
        }

        renderer.sharedMaterials = newMaterials.ToArray();
    }

    private void RemoveInvisibleMaterials(SkinnedMeshRenderer renderer)
    {
        if (_yantInvisbleMaterial == null ||
            _yantInvisbleMaterial.Count == 0)
        {
            return;
        }

        Material[] currentMaterials = renderer.sharedMaterials;

        List<Material> restoredMaterials = new();

        foreach (Material material in currentMaterials)
        {
            if (!material)
            {
                continue;
            }

            if (_yantInvisbleMaterial.Contains(material))
            {
                continue;
            }

            restoredMaterials.Add(material);
        }

        renderer.sharedMaterials = restoredMaterials.ToArray();
    }

    private void SetDefault()
    {
        if (_yantInvisbleSmoke)
        {
            _yantInvisbleSmoke.Stop();
        }

        foreach (Material material in _characterMaterial)
        {
            if (!material)
            {
                continue;
            }

            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Opacity", 100f);
        }
    }

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

            Material[] materials = skinnedMeshRenderer.sharedMaterials;
            List<Material> additionalMaterials = new();

            foreach (Material material in materials)
            {
                if (material && !_characterMaterial.Contains(material))
                {
                    additionalMaterials.Add(material);
                }
            }

            _additionalMaterials.Add(new SkinMaterialData
            {
                Renderer = skinnedMeshRenderer,
                Materials = additionalMaterials.ToArray()
            });
        }
    }
}