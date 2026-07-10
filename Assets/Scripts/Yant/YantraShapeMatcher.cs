using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIEditor
{
    /// <summary>
    /// สคริปต์หลักสำหรับการเทียบรูปทรงโดยใช้ Job System
    /// </summary>
    public class YantraShapeMatcher : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private DrawOn3DMesh _drawOn3DMesh;
        [SerializeField] private Transform _paperTransform;

        [Header("Categories")]
        [SerializeField] private List<ShapeCategory> _shapeCategories = new List<ShapeCategory>();

        [Header("Matching Settings")]
        [Tooltip("จำนวนจุดที่จะใช้คำนวณ")]
        [SerializeField] private int _globalResampleCount = 1000;

        [Tooltip("Phase 2 scoring: ยิ่งมาก = เข้มงวดขึ้น\n" +
                 "ค่าต่ำ (5-10) = ให้คะแนนง่าย / ค่าสูง (50+) = เข้มงวด")]
        [SerializeField] private float _scoreSharpness = 8f;

        [Tooltip("Phase 1 search range (องศา) — ใช้ค้นหา template ที่ใกล้เคียงที่สุด")]
        [SerializeField] private float _rotationSearchRangeDeg = 45f;
        [SerializeField] private float _rotationSearchThresholdDeg = 2f;

        public bool CreateTemplate;
        public ShapeMatchResult LastResult { get; private set; }

        private NativeArray<float2> _nativeAllTemplates;
        private int _totalTemplateCount;

        private void Start()
        {
            InitializeNativeData();
        }

        private void OnDestroy()
        {
            if (_nativeAllTemplates.IsCreated)
            {
                _nativeAllTemplates.Dispose();
            }
        }

        private void InitializeNativeData()
        {
            List<Vector2> allPointsList = new List<Vector2>();
            _totalTemplateCount = 0;

            foreach (var category in _shapeCategories)
            {
                foreach (var template in category.Templates)
                {
                    if (template != null && template.ReferencePoints != null && template.ReferencePoints.Count == _globalResampleCount)
                    {
                        allPointsList.AddRange(template.ReferencePoints);
                        _totalTemplateCount++;
                    }
                }
            }

            if (_totalTemplateCount == 0) return;

            _nativeAllTemplates = new NativeArray<float2>(allPointsList.Count, Allocator.Persistent);
            for (int i = 0; i < allPointsList.Count; i++)
            {
                _nativeAllTemplates[i] = new float2(allPointsList[i].x, allPointsList[i].y);
            }
        }

        public void AnalyzeDrawing()
        {
            if (_drawOn3DMesh == null || !_drawOn3DMesh.gameObject.activeInHierarchy || !_drawOn3DMesh.enabled) return;

            List<Vector3> worldPoints = _drawOn3DMesh.GetAllDrawnPointsInWorldSpace();

            if (worldPoints == null || worldPoints.Count < 5 || _totalTemplateCount == 0)
            {
                Debug.LogWarning("<color=#00FFFF>[YantraShapeMatcher]</color> <color=#FFA500>[Check Failed]</color> วาดสั้นเกินไป หรือไม่มีข้อมูลแม่แบบในระบบ!");
                return;
            }

            List<Vector2> drawnPoints2D = ProjectToLocalPlane(worldPoints, _paperTransform);
            List<Vector2> drawnCanonical = ToCanonicalForm(drawnPoints2D, _globalResampleCount);

            NativeArray<float2> nativeDrawnPoints = new NativeArray<float2>(_globalResampleCount, Allocator.TempJob);
            for (int i = 0; i < _globalResampleCount; i++)
            {
                nativeDrawnPoints[i] = new float2(drawnCanonical[i].x, drawnCanonical[i].y);
            }

            NativeArray<float> nativeResults = new NativeArray<float>(_totalTemplateCount, Allocator.TempJob);

            // Phase 1: Job เปรียบเทียบกับทุก template แบบ parallel → ได้ raw distance
            ShapeMatchJob matchJob = new ShapeMatchJob
            {
                DrawnPoints = nativeDrawnPoints,
                AllTemplatePoints = _nativeAllTemplates,
                SampleCount = _globalResampleCount,
                SearchRange = _rotationSearchRangeDeg * Mathf.Deg2Rad,
                SearchThreshold = _rotationSearchThresholdDeg * Mathf.Deg2Rad,
                Results = nativeResults
            };

            JobHandle jobHandle = matchJob.Schedule(_totalTemplateCount, 8);
            jobHandle.Complete();

            // Phase 1 result: หา template ที่ raw distance น้อยที่สุด (= เหมือนมากที่สุด)
            float bestRawDist = float.MaxValue;
            int bestIndex = -1;

            for (int i = 0; i < _totalTemplateCount; i++)
            {
                if (nativeResults[i] < bestRawDist)
                {
                    bestRawDist = nativeResults[i];
                    bestIndex = i;
                }
            }

            nativeDrawnPoints.Dispose();
            nativeResults.Dispose();

            if (bestIndex < 0) return;

            // Phase 2: เทียบ 1-ต่อ-1 กับ template ที่ดีที่สุดเท่านั้น
            // ใช้ full 360° grid search เพื่อหา rotation ที่ดีที่สุด แม่นกว่า GSS
            float finalDist = ComputePreciseDistance(drawnCanonical, bestIndex);

            // Exponential scoring: exp(-k * dist²) * 100
            // k=55 → dist≈0 = 100%, dist≈0.03 = 95%, dist≈0.1 = 58%, dist≈0.3 = 0.1%
            float finalScore = Mathf.Clamp(
                Mathf.Exp(-_scoreSharpness * finalDist * finalDist) * 100f,
                0f, 100f);

            AssignBestResult(bestIndex, finalScore);

            if (LastResult != null)
            {
                Debug.Log($"<color=#00FFFF>[YantraShapeMatcher]</color> <color=#00FF00>[Check Result]</color> " +
                          $"Phase1 เทียบ <color=#FFFF00>{_totalTemplateCount}</color> แม่แบบ → " +
                          $"Phase2 dist=<color=#FFFF00>{finalDist:F4}</color> " +
                          $"| Best: <color=#FFFF00>{LastResult.MatchedCategoryName}</color> " +
                          $"| Score: <color=#FFFF00>{LastResult.SimilarityPercent:F2}%</color>");
            }
        }

        private void AssignBestResult(int bestIndex, float bestScorePercent)
        {
            int currentIndex = 0;
            foreach (var category in _shapeCategories)
            {
                foreach (var template in category.Templates)
                {
                    if (template != null && template.ReferencePoints != null && template.ReferencePoints.Count == _globalResampleCount)
                    {
                        if (currentIndex == bestIndex)
                        {
                            LastResult = new ShapeMatchResult
                            {
                                MatchedCategoryName = category.CategoryName,
                                BestMatchTemplate = template,
                                // new: รับค่า % มาจาก Job ตรงๆ ได้เลย ไม่ต้องคำนวณซ้ำ
                                SimilarityPercent = bestScorePercent
                            };
                            return;
                        }
                        currentIndex++;
                    }
                }
            }
        }

        public void SaveToNextAvailableSlot(int categoryIndex)
        {
            if (_shapeCategories == null || categoryIndex < 0 || categoryIndex >= _shapeCategories.Count)
            {
                Debug.LogError($"<color=#00FFFF>[YantraShapeMatcher]</color> <color=#FF0000>[Save Error]</color> ไม่พบ Category Index: {categoryIndex}");
                return;
            }

            List<Vector2> canonicalPoints = GetCurrentDrawingCanonicalPoints();

            if (canonicalPoints == null || canonicalPoints.Count == 0)
            {
                Debug.LogWarning("<color=#00FFFF>[YantraShapeMatcher]</color> <color=#FFA500>[Save Failed]</color> ไม่พบเส้นที่วาด หรือวาดสั้นเกินไป!");
                return;
            }

            ShapeCategory targetCategory = _shapeCategories[categoryIndex];

            foreach (var template in targetCategory.Templates)
            {
                if (template != null && (template.ReferencePoints == null || template.ReferencePoints.Count == 0))
                {
#if UNITY_EDITOR
                    template.RecordPoints(canonicalPoints);

                    if (_nativeAllTemplates.IsCreated)
                    {
                        _nativeAllTemplates.Dispose();
                    }
                    InitializeNativeData();

                    Debug.Log($"<color=#00FFFF>[YantraShapeMatcher]</color> <color=#00FF00>[Save Success]</color> บันทึกแม่แบบลงในหมวด <color=#FFFF00>{targetCategory.CategoryName}</color> สำเร็จ! (อัปเดต <color=#FFFF00>{template.name}</color> ด้วยขนาด {canonicalPoints.Count} จุด)");
#endif
                    return;
                }
            }

            Debug.LogError($"<color=#00FFFF>[YantraShapeMatcher]</color> <color=#FF0000>[Save Error]</color> สล็อตในหมวด <color=#FFFF00>{targetCategory.CategoryName}</color> เต็มแล้ว กรุณาเพิ่มสล็อตใหม่!");
        }

        public List<Vector2> GetCurrentDrawingCanonicalPoints()
        {
            if (_drawOn3DMesh == null || !_drawOn3DMesh.gameObject.activeInHierarchy || !_drawOn3DMesh.enabled) return null;

            List<Vector3> worldPoints = _drawOn3DMesh.GetAllDrawnPointsInWorldSpace();
            if (worldPoints == null || worldPoints.Count < 5)
            {
                return null;
            }

            List<Vector2> points2D = ProjectToLocalPlane(worldPoints, _paperTransform);
            return ToCanonicalForm(points2D, _globalResampleCount);
        }

        /// <summary>
        /// Phase 2: Grid search 360° เพื่อหา minimum distance กับ template เดียว
        /// แม่นกว่า GSS เพราะค้นหาครบทุกมุม ไม่ติด local minimum
        /// </summary>
        private float ComputePreciseDistance(List<Vector2> drawn, int templateIndex)
        {
            int startIndex = templateIndex * _globalResampleCount;
            float minDist = float.MaxValue;

            const int steps = 360; // 1° per step
            for (int s = 0; s < steps; s++)
            {
                float angle = (2f * Mathf.PI * s) / steps;
                float dist = DistanceAtAngleManaged(drawn, startIndex, angle);
                if (dist < minDist) minDist = dist;
            }

            return minDist;
        }

        private float DistanceAtAngleManaged(List<Vector2> drawn, int templateStart, float angle)
        {
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            float sum = 0f;

            for (int i = 0; i < _globalResampleCount; i++)
            {
                Vector2 p = drawn[i];
                float rx = p.x * cos - p.y * sin;
                float ry = p.x * sin + p.y * cos;

                Unity.Mathematics.float2 refPt = _nativeAllTemplates[templateStart + i];
                float dx = rx - refPt.x;
                float dy = ry - refPt.y;
                sum += Mathf.Sqrt(dx * dx + dy * dy);
            }

            return sum / _globalResampleCount;
        }

        private List<Vector2> ToCanonicalForm(List<Vector2> rawPoints, int n)
        {
            List<Vector2> points = Resample(rawPoints, n);
            points = RotateToZero(points);
            points = ScaleToSquare(points);
            points = TranslateToOrigin(points);
            return points;
        }

        private List<Vector2> ProjectToLocalPlane(List<Vector3> worldPoints, Transform plane)
        {
            var result = new List<Vector2>(worldPoints.Count);
            foreach (var wp in worldPoints)
            {
                Vector3 local = plane.InverseTransformPoint(wp);
                result.Add(new Vector2(local.x, local.y));
            }
            return result;
        }

        private List<Vector2> Resample(List<Vector2> points, int n)
        {
            if (points.Count == 0) return new List<Vector2>();
            if (points.Count == 1) return new List<Vector2>(new[] { points[0] });

            var src = new List<Vector2>(points);

            float totalLength = 0f;
            for (int i = 1; i < src.Count; i++)
                totalLength += Vector2.Distance(src[i - 1], src[i]);

            if (totalLength < 1e-6f)
            {
                var flat = new List<Vector2>(n);
                for (int i = 0; i < n; i++) flat.Add(src[0]);
                return flat;
            }

            float interval = totalLength / (n - 1);
            var resampled = new List<Vector2> { src[0] };
            float accumulated = 0f;
            int pi = 1;

            while (resampled.Count < n && pi < src.Count)
            {
                float segLen = Vector2.Distance(src[pi - 1], src[pi]);
                float remaining = interval - accumulated;

                if (segLen >= remaining)
                {
                    float t = segLen > 1e-6f ? remaining / segLen : 0f;
                    Vector2 newPoint = Vector2.Lerp(src[pi - 1], src[pi], t);
                    resampled.Add(newPoint);
                    src.Insert(pi, newPoint);
                    accumulated = 0f;
                }
                else
                {
                    accumulated += segLen;
                    pi++;
                }
            }

            while (resampled.Count < n)
                resampled.Add(src[src.Count - 1]);

            return resampled;
        }

        private List<Vector2> RotateToZero(List<Vector2> points)
        {
            Vector2 centroid = Centroid(points);
            float angle = Mathf.Atan2(points[0].y - centroid.y, points[0].x - centroid.x);
            return RotateBy(points, -angle);
        }

        private List<Vector2> RotateBy(List<Vector2> points, float angleRad)
        {
            Vector2 centroid = Centroid(points);
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);

            var result = new List<Vector2>(points.Count);
            foreach (var p in points)
            {
                float dx = p.x - centroid.x;
                float dy = p.y - centroid.y;
                result.Add(new Vector2(
                    dx * cos - dy * sin + centroid.x,
                    dx * sin + dy * cos + centroid.y
                ));
            }
            return result;
        }

        private List<Vector2> ScaleToSquare(List<Vector2> points)
        {
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var p in points)
            {
                if (p.x < minX) minX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.x > maxX) maxX = p.x;
                if (p.y > maxY) maxY = p.y;
            }

            float width = maxX - minX;
            float height = maxY - minY;

            if (width < 1e-6f) width = 1e-6f;
            if (height < 1e-6f) height = 1e-6f;

            var result = new List<Vector2>(points.Count);
            foreach (var p in points)
                result.Add(new Vector2(p.x * (1f / width), p.y * (1f / height)));

            return result;
        }

        private List<Vector2> TranslateToOrigin(List<Vector2> points)
        {
            Vector2 centroid = Centroid(points);
            var result = new List<Vector2>(points.Count);
            foreach (var p in points)
                result.Add(p - centroid);
            return result;
        }

        private Vector2 Centroid(List<Vector2> points)
        {
            float sx = 0f, sy = 0f;
            foreach (var p in points) { sx += p.x; sy += p.y; }
            return new Vector2(sx / points.Count, sy / points.Count);
        }
    }
}