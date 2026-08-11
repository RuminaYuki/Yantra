using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using System;
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
    [SerializeField] private Material _yantInvisbleMaterial;
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
        Debug.Log($"Yant Invisible is {isInvisible}");

        if (isInvisible)
        {
            foreach (var material in _characterMaterial)
            {
                if (!material) continue;

                material.SetColor("_BaseColor", _yantInvisbleColor);
                Debug.Log($"Set BaseColor to {_yantInvisbleColor.GetHashCode()}");
                
                material.SetFloat("_Opacity", 90f);
                Debug.Log($"Set _Opacity to 90%");
            }

            foreach (SkinMaterialData skinMaterialData in _additionalMaterials)
            {
                if (!skinMaterialData.Renderer) continue;

                Material[] currentMaterials =
                    skinMaterialData.Renderer.sharedMaterials;

                Material[] newMaterials =
                    new Material[currentMaterials.Length + 1];

                Array.Copy(
                    currentMaterials,
                    newMaterials,
                    currentMaterials.Length
                );

                newMaterials[^1] = _yantInvisbleMaterial;

                skinMaterialData.Renderer.sharedMaterials = newMaterials;

                Debug.Log($"Add {_yantInvisbleMaterial.name} to {skinMaterialData.Renderer.name}");
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
            foreach (var material in _characterMaterial)
            {
                if (!material) continue;


                material.SetColor("_BaseColor", Color.white);
                Debug.Log($"Set BaseColor to white");

                material.SetFloat("_Opacity", 100f);
                Debug.Log($"Set _Opacity to 100%");
            }

            foreach (SkinMaterialData skinMaterialData in _additionalMaterials)
            {
                if (!skinMaterialData.Renderer) continue;

                Material[] currentMaterials =
                    skinMaterialData.Renderer.sharedMaterials;

                List<Material> restoredMaterials = new();

                foreach (Material material in currentMaterials)
                {
                    if (material != _yantInvisbleMaterial)
                    {
                        restoredMaterials.Add(material);
                    }
                }

                skinMaterialData.Renderer.sharedMaterials =
                    restoredMaterials.ToArray();

                Debug.Log($"Restore Materials to {skinMaterialData.Renderer.name}");
            }

            if (_yantInvisbleSmoke)
            {
                _yantInvisbleSmoke.Stop();
            }
        }
    }

    private void SetDefault()
    {
        if (_yantInvisbleSmoke)
        {
            _yantInvisbleSmoke.Stop();
        }

        foreach (var material in _characterMaterial)
        {
            if (!material) continue;

            material.SetColor("_BaseColor", Color.white);
            Debug.Log($"Set BaseColor to white");

            material.SetFloat("_Opacity", 100f);
            Debug.Log($"Set _Opacity to 100%");
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
            if (!skinnedMeshRenderer) continue;

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