using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YantraRecognition;

public class YantraGridShapeMatcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DrawOn3DMesh drawOn3DMesh;

    [Header("Templates")]
    [SerializeField] private string fixedTemplateName;
    [SerializeField] private List<ShapeCategory> shapeCategories = new();

    [Header("Settings")]
    [Range(0, 100)]
    [SerializeField] private float minimumSimilarity = 75f;
    public bool DebugMode = false;

    public ShapeMatchResult LastResult { get; private set; }

    private void Awake()
    {
        BuildAllTemplateCache();
    }

    /// <summary>
    /// ���ҧ Grid �ͧ Template �ء�ѹ (��������)
    /// </summary>
    private void BuildAllTemplateCache()
    {
        foreach (ShapeCategory category in shapeCategories)
        {
            if (category == null)
                continue;

            foreach (ShapeTemplate template in category.Templates)
            {
                if (template == null)
                    continue;

                template.BuildCache();
            }
        }
    }

    /// <summary>
    /// ��Ǩ�ͺ�ѹ������ش
    /// </summary>
    public bool AnalyzeDrawing()
    {
        LastResult = null;

        if (drawOn3DMesh == null)
        {
            Debug.LogError("DrawOn3DMesh is missing.");
            return false;
        }

        List<Vector3> points = drawOn3DMesh.GetAllDrawnPointsInWorldSpace();

        if (points == null || points.Count < 2)
        {
            Debug.LogWarning("Not enough drawing points.");
            return false;
        }

        YantraGrid playerGrid =
            YantraRasterizer.Rasterize(
                points,
                drawOn3DMesh.PaperParent);

        LastResult =
            YantraMatcher.Match(
                playerGrid,
                shapeCategories,
                fixedTemplateName);

        if (LastResult == null)
            return false;

        Debug.Log(LastResult);

#if UNITY_EDITOR
        // Log text representation of player grid for debugging
        if (DebugMode)
        {
            try
            {
                string playerGridText = YantraGridDebugger.ToText(playerGrid);
                Debug.Log($"Player Grid:\n{playerGridText}");

                if (LastResult.BestMatchTemplate != null && LastResult.BestMatchTemplate.CachedGrid != null)
                {
                    string templateGridText = YantraGridDebugger.ToText(LastResult.BestMatchTemplate.CachedGrid);
                    Debug.Log($"Matched Template ({LastResult.BestMatchTemplate.TemplateName}):\n{templateGridText}");
                    LogMatchDetails(playerGrid, LastResult.BestMatchTemplate.CachedGrid);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to generate grid text: {ex.Message}");
            }
        }
#endif
        return LastResult.SimilarityPercent >= minimumSimilarity;
    }

    public void ClearLastResult()
    {
        LastResult = null;
    }

    public void ClearDrawing()
    {
        drawOn3DMesh.ClearDrawing();
    }

    public ShapeMatchResult GetLastResult()
    {
        return LastResult;
    }

#if UNITY_EDITOR
    private void LogMatchDetails(YantraGrid player, YantraGrid template)
    {
        int matched = 0;
        int total = 0;
        int tolerance = YantraMatcher.Tolerance;

        int w = Mathf.Min(player.Width, template.Width);
        int h = Mathf.Min(player.Height, template.Height);

        char[,] map = new char[w, h];

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            map[x, y] = '.';

        for (int y = 0; y < template.Height; y++)
        {
            for (int x = 0; x < template.Width; x++)
            {
                if (!template.Get(x, y))
                    continue;

                total++;

                bool has = false;

                for (int oy = -tolerance; oy <= tolerance && !has; oy++)
                for (int ox = -tolerance; ox <= tolerance && !has; ox++)
                {
                    int px = x + ox;
                    int py = y + oy;

                    if (px < 0 || py < 0 || px >= player.Width || py >= player.Height)
                        continue;

                    if (player.Get(px, py))
                        has = true;
                }

                if (has)
                {
                    matched++;
                    if (x < w && y < h) map[x, y] = '#';
                }
                else
                {
                    if (x < w && y < h) map[x, y] = 't';
                }
            }
        }

        // mark player-only pixels
        for (int y = 0; y < player.Height; y++)
        for (int x = 0; x < player.Width; x++)
        {
            if (!player.Get(x, y))
                continue;

            if (x < w && y < h && map[x, y] == '.') map[x, y] = 'p';
        }

        StringBuilder sb = new();
        sb.AppendLine($"Matched {matched}/{total} = {(total == 0 ? 0f : (float)matched / total * 100f):F1}%");
        for (int y = h - 1; y >= 0; y--)
        {
            for (int x = 0; x < w; x++)
            {
                sb.Append(map[x, y]);
            }
            sb.AppendLine();
        }

        Debug.Log($"Match Details:\n{sb}");
    }
#endif
    //API
    public void SetFixedTemplate(string name) => fixedTemplateName = name;
}