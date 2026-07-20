using UnityEngine;

namespace TreeTool
{
    /// <summary>
    /// Bridges Unity's real <see cref="WindZone"/> component to the global shader
    /// properties <c>TreeWind.hlsl</c> reads. Drop one of these anywhere in the
    /// scene (or let a ProceduralTree add one automatically) and every tree
    /// material using the TreeWind function reacts to the same WindZone artists
    /// already use for particles / cloth - no per-material wind setup needed.
    ///
    /// Unity's built-in SpeedTree wind pipeline bakes a proprietary per-vertex
    /// format that only the closed-source SpeedTree shaders decode, so a custom
    /// mesh can't plug into it directly. This driver reads the same WindZone
    /// component instead (direction, main strength, turbulence, pulse) and
    /// re-exposes it as plain global shader properties that our own vertex
    /// color weights (see ProceduralTreeSettings wind response fields) combine
    /// with in TreeWind.hlsl - same scene-authoring workflow, our own shader math.
    ///
    /// Main/Branch/Leaf Amplitude and Speed live here too (not as per-material
    /// Shader Graph properties) so there is exactly one place to tune them -
    /// every material built from the tree wind shader graph reads the same
    /// values, so bark and leaves (even across separate material instances)
    /// always sway in the same direction and tempo with no risk of one
    /// material's knobs drifting out of sync with another's.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("Tools/Tree Wind Zone Driver")]
    public class TreeWindZoneDriver : MonoBehaviour
    {
        static readonly int DirectionId = Shader.PropertyToID("_TreeWindDirection");
        static readonly int StrengthId = Shader.PropertyToID("_TreeWindStrength");
        static readonly int TurbulenceId = Shader.PropertyToID("_TreeWindTurbulence");
        static readonly int TimeId = Shader.PropertyToID("_TreeWindTime");

        static readonly int MainAmplitudeId = Shader.PropertyToID("_TreeMainAmplitude");
        static readonly int BranchAmplitudeId = Shader.PropertyToID("_TreeBranchAmplitude");
        static readonly int LeafAmplitudeId = Shader.PropertyToID("_TreeLeafAmplitude");
        static readonly int MainSpeedId = Shader.PropertyToID("_TreeMainSpeed");
        static readonly int BranchSpeedId = Shader.PropertyToID("_TreeBranchSpeed");
        static readonly int LeafSpeedId = Shader.PropertyToID("_TreeLeafSpeed");

        [Tooltip("Optional - pin a specific WindZone. Leave empty to auto-use the first " +
                 "enabled WindZone found in the scene each frame.")]
        public WindZone windZone;

        [Range(0f, 3f)]
        [Tooltip("Overall multiplier on top of the WindZone's own strength, in case the " +
                 "WindZone is shared with other systems (particles, cloth) tuned differently.")]
        public float globalMultiplier = 1f;

        [Header("Response (applies to every tree in the scene)")]
        [FineRange(0f, 1f)]
        [Tooltip("Meters of whole-part lean/gust movement. Lower = calmer.")]
        public float mainAmplitude = 0.15f;

        [FineRange(0f, 1f)]
        [Tooltip("Meters of extra local branch whip on top of the main lean. Lower = calmer.")]
        public float branchAmplitude = 0.08f;

        [FineRange(0f, 1f)]
        [Tooltip("Meters of leaf-tip flutter (the leaf's own stem never moves by this amount - " +
                 "only the tip does, see TreeWind.hlsl). Lower = calmer.")]
        public float leafAmplitude = 0.05f;

        [FineRange(0.05f, 3f)]
        [Tooltip("How fast the whole tree leans/gusts. Lower = slower.")]
        public float mainSpeed = 0.2f;

        [FineRange(0.1f, 5f)]
        [Tooltip("How fast branches whip locally. Lower = slower.")]
        public float branchSpeed = 0.15f;

        [FineRange(0.1f, 10f)]
        [Tooltip("How fast leaf tips flutter. Lower = slower.")]
        public float leafSpeed = 0.4f;

        float _clockStart;

        void OnEnable() => _clockStart = Now();

        void Update() => Apply();

        void Apply()
        {
            WindZone zone = windZone != null ? windZone : FindActiveWindZone();
            float time = Now() - _clockStart;
            Shader.SetGlobalFloat(TimeId, time);

            Shader.SetGlobalFloat(MainAmplitudeId, mainAmplitude);
            Shader.SetGlobalFloat(BranchAmplitudeId, branchAmplitude);
            Shader.SetGlobalFloat(LeafAmplitudeId, leafAmplitude);
            Shader.SetGlobalFloat(MainSpeedId, mainSpeed);
            Shader.SetGlobalFloat(BranchSpeedId, branchSpeed);
            Shader.SetGlobalFloat(LeafSpeedId, leafSpeed);

            if (zone == null)
            {
                Shader.SetGlobalVector(DirectionId, Vector3.right);
                Shader.SetGlobalFloat(StrengthId, 0f);
                Shader.SetGlobalFloat(TurbulenceId, 0f);
                return;
            }

            // matches Unity's own WindZone pulse shaping: a slow sine breathes the
            // base strength up and down between gusts
            float pulse = 1f + Mathf.Sin(time * zone.windPulseFrequency) * zone.windPulseMagnitude;
            float strength = zone.windMain * pulse * globalMultiplier;

            Vector3 direction = zone.mode == WindZoneMode.Directional
                ? zone.transform.forward
                : (transform.position - zone.transform.position).normalized;

            Shader.SetGlobalVector(DirectionId, direction);
            Shader.SetGlobalFloat(StrengthId, strength);
            Shader.SetGlobalFloat(TurbulenceId, zone.windTurbulence);
        }

        static WindZone FindActiveWindZone()
        {
#if UNITY_2023_1_OR_NEWER
            var zones = FindObjectsByType<WindZone>(FindObjectsSortMode.None);
#else
    var zones = FindObjectsOfType<WindZone>();
#endif

            foreach (WindZone z in zones)
            {
                if (z.gameObject.activeInHierarchy)
                    return z;
            }

            return null;
        }

        float Now()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return (float)UnityEditor.EditorApplication.timeSinceStartup;
#endif
            return Time.time;
        }
    }
}
