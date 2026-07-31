
[System.Serializable]
public class ShapeMatchResult
{
    public string MatchedCategoryName;
    public ShapeTemplate BestMatchTemplate;
    public float SimilarityPercent;

    public override string ToString() =>
        $"Best Match: {MatchedCategoryName} | Similarity: {SimilarityPercent:F1}%";
}