using System.Collections.Generic;
using UnityEngine;
using UIEditor;
using Kogetsu.Library.DesignPatternCore;

[System.Serializable]
public class YantPrefabBinding
{
    [Tooltip("Must match ShapeCategory.CategoryName in YantraShapeMatcher.")]
    public string CategoryName;

    [Tooltip("Yantra prefab with an IYantEffect MonoBehaviour on its root.")]
    public GameObject YantPrefab;
}

public class YantCaster : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private YantraGridShapeMatcher _matcher;
    [SerializeField] private GameObject _playerRoot;
    [SerializeField] private Transform _yantSpawnPoint;
    [SerializeField] private GameObject _yantPaper;
    [SerializeField] private DrawOn3DMesh _drawOn3DMesh;
    [SerializeField] private Inventory _inventory;

    [Header("Yant Caster Setting")]
    [SerializeField] private bool _useConsumableForCasting;
    [SerializeField] private ItemData _itemConsumable;

    [Header("Matching")]
    [SerializeField, Range(0f, 100f)] private float _minSimilarityPercent = 50f;
    [SerializeField] private List<YantPrefabBinding> _bindings = new List<YantPrefabBinding>();

    public GameObject _lastSpawnedYant;

    private void OnEnable()
    {
        if (_inventory != null && _useConsumableForCasting)
        {
            int _yantAmount = _inventory.GetItemCount(_itemConsumable);
            _drawOn3DMesh.SetCanDraw(_yantAmount > 0);
        }
        else if (!_useConsumableForCasting)
        {
            _drawOn3DMesh.SetCanDraw(true);
        }
    }

    private void OnValidate()
    {
        if (_matcher == null) _matcher = GetComponentInChildren<YantraGridShapeMatcher>();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Analyze Input
    public void Analyze()
    {
        TryAnalyze();
    }

    private bool TryAnalyze()
    {
        if (_matcher == null)
        {
            Debug.LogWarning("<color=#00FFFF>[YantCaster]</color> Missing matcher.");
            return false;
        }

        _matcher.AnalyzeDrawing();

        ShapeMatchResult result = _matcher.LastResult;
        if (result == null) return false;

        if (result.SimilarityPercent < _minSimilarityPercent)
        {
            Debug.Log($"<color=#00FFFF>[YantCaster]</color> Match {result.SimilarityPercent:F1}% is lower than {_minSimilarityPercent}%.");
            return false;
        }

        bool FinalResult = SpawnYant(result);
        if (FinalResult)
        {
            _matcher.ClearLastResult();
        }

        _inventory.TryRemoveItem(_itemConsumable, 1);

        return FinalResult;
    }

    private bool SpawnYant(ShapeMatchResult result)
    {
        // Find the corresponding prefab for the matched category
        string category = result.MatchedCategoryName;
        YantPrefabBinding binding = null;
        foreach (YantPrefabBinding b in _bindings)
        {
            if (b != null && b.YantPrefab != null && b.CategoryName == category)
            {
                binding = b;
                break;
            }
        }

        if (binding == null)
        {
            Debug.LogWarning($"<color=#00FFFF>[YantCaster]</color> No prefab bound for category '{category}'.");
            return false;
        }

        GameObject paper = _yantPaper != null ? _yantPaper : (_matcher != null ? _matcher.gameObject : gameObject);
        Vector3 spawnPos = _yantSpawnPoint.transform.position;
        Quaternion spawnRot = _yantSpawnPoint.transform.rotation;
        GameObject yantObj = null;

        yantObj = Instantiate(binding.YantPrefab, spawnPos, spawnRot, _yantSpawnPoint);

        if (yantObj.TryGetComponent<YantEffectController>(out YantEffectController yantEffectController))
        {
            yantEffectController.SetDefaultValue(_playerRoot != null ? _playerRoot : gameObject);
        }
        else
        {
            Destroy(yantObj);
            Debug.LogWarning("����� YantEffectController � yant ��� Instantiate");
            return false;
        }

        _lastSpawnedYant = yantObj;

        // Clear the drawing on the paper after casting
        if (_drawOn3DMesh != null)
        {
            _drawOn3DMesh.ClearDrawing();
        }

        Debug.Log($"<color=#00FFFF>[YantCaster]</color> Cast '{category}' successfully.");
        //EventBus.Instance.Publish(new YantCastEvent(result, yantObj));
        return true;
    }
    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Cast Yant on Input
    //����¹����� YantEffectController
    public void tryCastYant(float holdTime, bool holdLMB = false)
    {
        CastYant(holdTime, holdLMB);
    }

    private bool CastYant(float holdTime, bool holdLMB = false)
    {
        GameObject yant = _lastSpawnedYant;
        if (yant == null)
        {
            Debug.LogWarning("<color=#00FFFF>[YantCaster]</color> No yant to cast.");
            return false;
        }
        if (yant.TryGetComponent(out YantEffectController effect))
        {
            effect.TryInitialize(holdTime, holdLMB);
            return true;
        }
        else
        {
            Debug.LogWarning($"<color=#00FFFF>[YantCaster]</color> Prefab '{yant.name}' has no IYantEffect.");
            return false;
        }
    }
    #endregion
}
