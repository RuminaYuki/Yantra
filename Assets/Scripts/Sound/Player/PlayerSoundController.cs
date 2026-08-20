using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    public enum FootstepMode
    {
        DistanceBased,
        AnimationEvent
    }

    [System.Serializable]
    public struct SurfaceSound
    {
        [Tooltip("Surface Tag name (e.g. Wood, Grass, Dirt)")]
        public string surfaceTag;

        public SoundID crouchSound;
        public SoundID walkSound;
        public SoundID runSound;
    }

    [System.Serializable]
    public struct TerrainSound
    {
        [Tooltip("ตั้งชื่อกลุ่มให้ดูง่ายๆ เช่น Grass, Dirt, Rock")]
        public string groupName;

        [Tooltip("ใส่ Terrain Layer ได้หลายอันเลย (ยัดหญ้าทุกแบบเข้ามาในช่องนี้ได้เลย)")]
        public TerrainLayer[] terrainLayers;

        public SoundID crouchSound;
        public SoundID walkSound;
        public SoundID runSound;
    }

    [Header("Surface Sounds Settings (For 3D Models)")]
    [SerializeField] private SurfaceSound[] surfaceSounds;

    [Header("Terrain Sounds Settings (For Unity Terrain)")]
    [SerializeField] private TerrainSound[] terrainSounds;

    [Header("Default Sounds (Fallback)")]
    [SerializeField] private SoundID defaultCrouchID;
    [SerializeField] private SoundID defaultWalkID;
    [SerializeField] private SoundID defaultRunID;

    [Header("Item Sounds")]
    [SerializeField] private SoundID flashlightToggleID;
    [SerializeField] private SoundID openNotebookID;

    [Header("Footstep Mode")]
    [SerializeField] private FootstepMode mode = FootstepMode.DistanceBased;

    [Header("Distance Mode Settings (Meters)")]
    [SerializeField] private float crouchStrideLength = 1.0f;
    [SerializeField] private float walkStrideLength = 1.6f;
    [SerializeField] private float runStrideLength = 2.4f;

    [Header("Movement Speed Thresholds")]
    [SerializeField] private float minMoveSpeed = 0.15f;
    [SerializeField] private float walkSpeedThreshold = 1.5f;
    [SerializeField] private float runSpeedThreshold = 3.5f;

    [Header("Ground Detection & Cooldown")]
    [SerializeField] private float minStepInterval = 0.15f;
    [SerializeField] private bool requireGrounded = true;
    [SerializeField] private LayerMask groundLayer = ~0;

    [Header("Debug")]
    [SerializeField] private bool logFootsteps = false;

    private CharacterController charController;
    private float lastStepTime = -999f;
    private Vector3 lastPosition;
    private float measuredSpeed;
    private float distanceAccumulated;
    private Vector3 currentMoveDir = Vector3.forward;

    public float CurrentSpeed => measuredSpeed;
    public bool IsRunning => measuredSpeed >= runSpeedThreshold;
    public bool IsCrouching => measuredSpeed >= minMoveSpeed && measuredSpeed < walkSpeedThreshold;

    private void Awake()
    {
        charController = GetComponent<CharacterController>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;

        float distanceThisFrame = delta.magnitude;

        if (Time.deltaTime > 0f)
            measuredSpeed = distanceThisFrame / Time.deltaTime;

        Vector3 flatDelta = new Vector3(delta.x, 0f, delta.z);
        if (flatDelta.magnitude > 0.001f)
        {
            currentMoveDir = flatDelta.normalized;
        }

        if (mode == FootstepMode.DistanceBased)
            UpdateDistanceFootstep(distanceThisFrame);
    }

    private void UpdateDistanceFootstep(float distanceThisFrame)
    {
        if (measuredSpeed < minMoveSpeed || !IsGrounded())
        {
            distanceAccumulated = Mathf.Min(distanceAccumulated, walkStrideLength * 0.8f);
            return;
        }

        distanceAccumulated += distanceThisFrame;

        float stride = walkStrideLength;
        if (IsRunning) stride = runStrideLength;
        else if (IsCrouching) stride = crouchStrideLength;

        if (distanceAccumulated >= stride)
        {
            distanceAccumulated -= stride;
            TriggerFootstep();
        }
    }

    private bool IsGrounded()
    {
        if (!requireGrounded) return true;
        if (charController != null && charController.isGrounded) return true;

        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 1.2f, groundLayer);
    }

    private void TriggerFootstep()
    {
        if (Time.time - lastStepTime < minStepInterval) return;
        lastStepTime = Time.time;

        SoundID crouchToPlay = defaultCrouchID;
        SoundID walkToPlay = defaultWalkID;
        SoundID runToPlay = defaultRunID;
        string hitTag = "Untagged";

        Vector3 rayOrigin = transform.position + (Vector3.up * 0.5f) + (currentMoveDir * 0.3f);

        // จุดกำเนิดเสียง (เริ่มต้นที่ตำแหน่งตัวละคร)
        Vector3 soundSpawnPos = transform.position;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 1.5f, groundLayer))
        {
            // อัปเดตพิกัดเสียงให้ไปเกิดที่จุดกระทบพื้นแบบ 3D
            soundSpawnPos = hit.point;

            Terrain hitTerrain = hit.collider.GetComponent<Terrain>();

            if (hitTerrain != null)
            {
                TerrainLayer dominantLayer = GetDominantTerrainLayer(hit.point, hitTerrain);

                if (dominantLayer != null)
                {
                    hitTag = dominantLayer.name;
                    bool foundMatch = false;

                    foreach (var tSound in terrainSounds)
                    {
                        if (tSound.terrainLayers == null) continue;

                        // วนลูปเช็คว่าเลเยอร์ที่เหยียบอยู่ ตรงกับอันไหนในตะกร้าบ้าง
                        foreach (var layer in tSound.terrainLayers)
                        {
                            if (layer == dominantLayer)
                            {
                                crouchToPlay = tSound.crouchSound;
                                walkToPlay = tSound.walkSound;
                                runToPlay = tSound.runSound;
                                foundMatch = true;
                                break;
                            }
                        }
                        if (foundMatch) break;
                    }
                }
            }
            else
            {
                hitTag = hit.collider.tag;

                foreach (var surface in surfaceSounds)
                {
                    if (surface.surfaceTag == hitTag)
                    {
                        crouchToPlay = surface.crouchSound;
                        walkToPlay = surface.walkSound;
                        runToPlay = surface.runSound;
                        break;
                    }
                }
            }
        }

        SoundID idToPlay = walkToPlay;
        string moveState = "Walk";

        if (IsRunning)
        {
            idToPlay = runToPlay != null ? runToPlay : walkToPlay;
            moveState = "Run";
        }
        else if (IsCrouching)
        {
            idToPlay = crouchToPlay != null ? crouchToPlay : walkToPlay;
            moveState = "Crouch";
        }

        if (idToPlay == null) return;

        if (logFootsteps)
            Debug.Log($"Footstep: [{moveState}] on surface [{hitTag}], speed = {measuredSpeed:F2}");

        SoundManager.Instance.PlaySFX(idToPlay, soundSpawnPos);
    }

    private TerrainLayer GetDominantTerrainLayer(Vector3 worldPos, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        float mapX = ((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth;
        float mapZ = ((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight;

        int x = Mathf.FloorToInt(mapX);
        int z = Mathf.FloorToInt(mapZ);

        if (x < 0 || z < 0 || x >= terrainData.alphamapWidth || z >= terrainData.alphamapHeight)
            return null;

        float[,,] aMap = terrainData.GetAlphamaps(x, z, 1, 1);

        float maxMix = 0f;
        int maxIndex = 0;

        for (int n = 0; n < aMap.GetUpperBound(2) + 1; n++)
        {
            if (aMap[0, 0, n] > maxMix)
            {
                maxIndex = n;
                maxMix = aMap[0, 0, n];
            }
        }

        if (terrainData.terrainLayers != null && maxIndex < terrainData.terrainLayers.Length)
        {
            return terrainData.terrainLayers[maxIndex];
        }

        return null;
    }

    public void PlaySneakSound()
    {
        if (mode != FootstepMode.AnimationEvent) return;
        if (measuredSpeed < (minMoveSpeed * 1.5f)) return;
        if (!IsGrounded()) return;

        TriggerFootstep();
    }

    public void PlayFootstepSound()
    {
        if (mode != FootstepMode.AnimationEvent) return;
        if (measuredSpeed < (minMoveSpeed * 1.5f)) return;
        if (!IsGrounded()) return;

        TriggerFootstep();
    }

    public void PlayFlashlightSound()
    {
        if (flashlightToggleID != null)
            SoundManager.Instance.PlaySFX(flashlightToggleID, transform.position);
    }

    public void PlayNotebookSound()
    {
        if (openNotebookID != null)
            SoundManager.Instance.PlaySFX(openNotebookID, transform.position);
    }
}