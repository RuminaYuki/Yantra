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
    [SerializeField] private YantraShapeMatcher _matcher;
    [SerializeField] private YantraStatsController _stats;
    [SerializeField] private Transform _aimCamera;
    [SerializeField] private GameObject _playerRoot;
    [SerializeField] private GameObject _yantPaper;

    [Header("Matching")]
    [SerializeField, Range(0f, 100f)] private float _minSimilarityPercent = 50f;
    [SerializeField] private List<YantPrefabBinding> _bindings = new();

    [Header("Aim")]
    [SerializeField] private float _maxAimDistance = 100f;
    [SerializeField] private LayerMask _aimMask = ~0;

    public GameObject _lastSpawnedYant;


    private void OnValidate()
    {
        if (_matcher == null) _matcher = GetComponentInChildren<YantraShapeMatcher>();
        if (_stats == null) _stats = GetComponentInParent<YantraStatsController>();
        if (_playerRoot == null && _stats != null) _playerRoot = _stats.gameObject;
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
        Vector3 spawnPos = paper.transform.position;
        Quaternion spawnRot = paper.transform.rotation;
        GameObject yantObj = null;

        yantObj = Instantiate(binding.YantPrefab, spawnPos, spawnRot);

        if (yantObj.TryGetComponent<YantEffectController>(out YantEffectController yantEffectController))
        {
            yantEffectController.SetDefaultValue(
                _playerRoot != null ? _playerRoot : gameObject,
                _stats,
                GetAimDirection(yantObj.transform.position));
            //Debug.Log(_playerRoot + " " + _stats + " " + GetAimDirection(yantObj.transform.position));
        }
        else
        {
            Debug.LogWarning("ไม่เจอ YantEffectController ใน yant ที่ Instantiate");
            Destroy(yantObj);
            return false;
        }

        _lastSpawnedYant = yantObj;

        // Clear the drawing on the paper after casting
        DrawOn3DMesh drawOn3DMesh = paper.GetComponent<DrawOn3DMesh>();
        if (drawOn3DMesh != null)
        {
            drawOn3DMesh.ClearDrawing();
        }

        Debug.Log($"<color=#00FFFF>[YantCaster]</color> Cast '{category}' successfully.");
        //EventBus.Instance.Publish(new YantCastEvent(result, yantObj));
        return true;
    }
    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Cast Yant on Input
    //เปลี่ยนไปสั่งใน YantEffectController
    public void tryCastYant(float holdTime)
    {
        CastYant(holdTime);
    }

    private bool CastYant(float holdTime)
    {
        GameObject yant = _lastSpawnedYant;
        if (yant == null)
        {
            Debug.LogWarning("<color=#00FFFF>[YantCaster]</color> No yant to cast.");
            return false;
        }
        if (yant.TryGetComponent(out YantEffectController effect))
        {
            effect.TryInitialize(holdTime);
            return true;
        }
        else
        {
            Debug.LogWarning($"<color=#00FFFF>[YantCaster]</color> Prefab '{yant.name}' has no IYantEffect.");
            return false;
        }
    }
    #endregion

    private Vector3 GetAimDirection(Vector3 fromPosition)
    {
        if (_aimCamera == null) return transform.forward;

        Vector3 targetPoint = Physics.Raycast(
            _aimCamera.position,
            _aimCamera.forward,
            out RaycastHit hit,
            _maxAimDistance,
            _aimMask,
            QueryTriggerInteraction.Ignore)
            ? hit.point
            : _aimCamera.position + _aimCamera.forward * _maxAimDistance;

        Vector3 dir = (targetPoint - fromPosition).normalized;
        return dir.sqrMagnitude > 1e-4f ? dir : _aimCamera.forward;
    }
}
