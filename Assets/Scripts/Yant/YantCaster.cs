using System.Collections.Generic;
using UnityEngine;
using UIEditor;

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
    [SerializeField] private List<YantPrefabBinding> _bindings = new List<YantPrefabBinding>();

    [Header("Aim")]
    [SerializeField] private float _maxAimDistance = 100f;
    [SerializeField] private LayerMask _aimMask = ~0;

    private void OnValidate()
    {
        if (_matcher == null) _matcher = GetComponentInChildren<YantraShapeMatcher>();
        if (_stats == null) _stats = GetComponentInParent<YantraStatsController>();
        if (_playerRoot == null && _stats != null) _playerRoot = _stats.gameObject;
    }

    public void AnalyzeAndCast()
    {
        TryAnalyzeAndCast();
    }

    public bool TryAnalyzeAndCast()
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

        return SpawnYant(result.MatchedCategoryName);
    }

    private bool SpawnYant(string category)
    {
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

        GameObject yantObj = Instantiate(binding.YantPrefab, spawnPos, spawnRot);
        if (yantObj.TryGetComponent(out IYantEffect effect))
        {
            effect.Initialize(
                _playerRoot != null ? _playerRoot : gameObject,
                _stats,
                GetAimDirection(spawnPos));
        }
        else
        {
            Debug.LogWarning($"<color=#00FFFF>[YantCaster]</color> Prefab '{binding.YantPrefab.name}' has no IYantEffect.");
        }

        Destroy(paper);
        Debug.Log($"<color=#00FFFF>[YantCaster]</color> Cast '{category}' successfully.");
        return true;
    }

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
