#ifndef TREETOOL_TREE_WIND_INCLUDED
#define TREETOOL_TREE_WIND_INCLUDED

// Driven entirely by TreeWindZoneDriver.cs from a real Unity WindZone component -
// direction/strength/turbulence/time AND Main/Branch/Leaf Amplitude/Speed are all
// plain global shader properties (Shader.SetGlobalFloat/Vector), not material
// properties. Every tree material in the scene reads the exact same values, so
// bark and leaves (even across different materials built from this same graph)
// always sway in the same direction and tempo with no per-material setup and no
// duplicate knobs to keep in sync.
float3 _TreeWindDirection;
float  _TreeWindStrength;
float  _TreeWindTurbulence;
float  _TreeWindTime;

float _TreeMainAmplitude;
float _TreeBranchAmplitude;
float _TreeLeafAmplitude;
float _TreeMainSpeed;
float _TreeBranchSpeed;
float _TreeLeafSpeed;

float TreeWindHash(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

// Custom Function Node target (Type: File, Name: TreeWindDisplacement).
// Input order matters - Custom Function nodes match by position, not name, so
// the node's Inputs list must read PositionWS, VertexColor, UV top to bottom.
// PositionWS  - world-space vertex position (stable input for the turbulence hash)
// VertexColor - baked wind weights from ProceduralTreeSettings:
//                 R = main bend x that part's Wind Response
//                 G = local branch sway x that branch level's Wind Response
//                 B = leaf flutter x Leaf Wind Flutter Response
//                 A = random phase (0-1) - small per-part timing stagger only,
//                     never changes which direction anything sways
// UV          - vertex UV0. Leaf cards are authored with the stem/pivot at UV
//               (0,0) and the tip near UV (1,1) (see TreeMeshBuilder.BuildLeaf) -
//               used so the leaf's own flutter fades to zero at the stem and
//               grows toward the tip, like a candle flame: the base stays
//               pinned to the branch, only the tip swings. Has no effect on
//               bark (VertexColor.b is 0 there, which already zeroes the leaf
//               flutter term regardless of UV).
// PositionOffsetWS - add this to the world-space vertex position
void TreeWindDisplacement_float(float3 PositionWS, float4 VertexColor, float2 UV,
    out float3 PositionOffsetWS)
{
    float strength = _TreeWindStrength;
    float phase = VertexColor.a;
    float3 dir = _TreeWindDirection;
    float3 crossDir = normalize(cross(dir, float3(0, 1, 0)) + 1e-5);

    // main bend: the whole part leans downwind together, with a slow gust
    // breathing on top. No per-part phase here on purpose - a real gust pushes
    // a whole tree-sized object at roughly the same time, so every branch and
    // leaf leans the same way in lockstep for this component.
    float mainLean = 0.6 + 0.4 * sin(_TreeWindTime * _TreeMainSpeed * 0.3);
    float3 mainOffset = dir * VertexColor.r * _TreeMainAmplitude * strength * mainLean;

    // local branch sway: whips back and forth around the main lean. Only a
    // small phase stagger between branches - enough to avoid a robotic
    // identical wobble, nowhere near enough to point different directions.
    float branchWave = sin(_TreeWindTime * _TreeBranchSpeed + phase * 1.5);
    float branchWaveCross = sin(_TreeWindTime * _TreeBranchSpeed * 1.3 + phase * 2.0);
    float3 branchOffset = (dir * branchWave + crossDir * branchWaveCross * 0.3)
                         * VertexColor.g * _TreeBranchAmplitude * strength;

    // leaf flutter: the leaf's stem always moves exactly with its branch
    // (mainOffset + branchOffset above use the same weights the branch itself
    // gets, so the stem vertex never drifts away from the branch surface).
    // This extra term is what actually flutters the blade, scaled to zero at
    // the stem (UV near the origin) and full strength at the tip.
    // dir is the DOMINANT axis here (same as the trunk/branch terms above) so
    // every leaf visibly agrees on one wind direction; crossDir only adds a
    // smaller twist so leaves don't all move as one perfectly rigid unit.
    float stemWeight = saturate(length(UV));
    float leafWaveA = sin(_TreeWindTime * _TreeLeafSpeed + phase * 3.0);
    float leafWaveB = sin(_TreeWindTime * _TreeLeafSpeed * 1.7 + phase * 4.0);
    float3 leafOffset = (dir * (0.5 + 0.5 * leafWaveA) + crossDir * leafWaveB * 0.35)
                       * VertexColor.b * _TreeLeafAmplitude * stemWeight
                       * (0.35 + 0.65 * saturate(strength));

    // turbulence: a smooth per-PART wave. The old version hashed the world
    // position PER VERTEX (white noise), which pushed neighboring vertices of
    // the same trunk ring in uncorrelated directions - the bark surface itself
    // got torn/stretched. phase is constant across a whole branch/leaf, so
    // deriving the jitter only from phase makes the entire part move together
    // and the surface stays rigid.
    //
    // Gated separately for bark (R/G, no stem concept - always eligible) and
    // leaf (B, but only away from the stem via stemWeight) - without the
    // stemWeight gate here, a leaf's stem vertex still had B > 0 (its own
    // flutter response) even though R/G are 0, so it picked up this term and
    // drifted off the branch despite leafOffset above already correctly
    // pinning it to zero. This was the actual cause of leaves "flying off"
    // their attach point.
    float turbFreq = 0.7 + TreeWindHash(float2(phase, 0.37));
    float turbWave = sin(_TreeWindTime * _TreeBranchSpeed * turbFreq * 2.3 + phase * 6.283);
    float turbWeight = saturate(VertexColor.r + VertexColor.g) + VertexColor.b * stemWeight;
    float3 turbulenceOffset = dir * turbWave * _TreeWindTurbulence * _TreeMainAmplitude * 0.5
                             * saturate(turbWeight);

    // --- bend, don't translate: the anti-stretch fix ---
    // A raw sideways offset shears the mesh: the top of a trunk/branch
    // translates while its base stays put, so the tube visibly elongates like
    // taffy once the wind is strong. The first version of this fix preserved
    // each vertex's distance from the WHOLE TREE's single base point - that
    // stopped the stretching, but it also meant every branch bent around the
    // *trunk's* base instead of its own attachment point, warping the mesh
    // right where branches join the trunk (worst right at the very base).
    //
    // This version pivots each vertex around its own small LOCAL virtual arm
    // instead of any shared point in the mesh, so it can't warp anything
    // relative to anything else - the arm's length scales with how flexible
    // this vertex is meant to be (from the baked wind weights: near 0 for
    // rigid wood, larger for floppy tips/leaves), so stiff parts barely bend
    // and springy parts arc noticeably, exactly like a real branch.
    float3 windOffset = mainOffset + branchOffset + leafOffset + turbulenceOffset;
    float flex = saturate(turbWeight); // same stem-aware weight as the turbulence gate above
    float armLength = lerp(0.05, 0.5, flex);
    float3 restArm = float3(0, -armLength, 0);
    float3 bentArm = restArm + windOffset;
    float3 bentArmNormalized = bentArm * (armLength / max(length(bentArm), 1e-5));

    PositionOffsetWS = bentArmNormalized - restArm;
}

#endif
