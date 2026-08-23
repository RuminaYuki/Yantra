using UnityEngine;

public class GhostSoundController : MonoBehaviour
{
    // ==========================================
    // Voice Profiles
    // ==========================================
    [System.Serializable]
    public struct VoiceProfile
    {
        public string profileName;
        public float dropWeight;
        public SoundID idleMoan;
        public SoundID attackScream;
        public SoundID death;
    }

    [Header("Voice Gacha Settings")]
    [SerializeField] private VoiceProfile[] voiceProfiles;
    private VoiceProfile currentVoice;

    // ==========================================
    // Auto-Moan Settings
    // ==========================================
    [Header("Auto-Moan Settings")]
    [SerializeField] private bool useAutoMoan = true;

    [Tooltip("โอกาสที่จะส่งเสียงร้องในแต่ละรอบ (0-100) แนะนำ 30-50 สำหรับมอนสเตอร์ที่อยู่เป็นฝูง")]
    [Range(0f, 100f)]
    [SerializeField] private float moanChance = 40f;

    [SerializeField] private float minMoanDelay = 8f;
    [SerializeField] private float maxMoanDelay = 15f;

    private float nextMoanAllowedTime = 0f;
    private bool isDead = false;

    [Header("Action Sounds")]
    [SerializeField] private SoundID[] actionSounds;

    [Header("Footstep Settings & Cooldown")]
    [SerializeField] private float minStepInterval = 0.2f;
    private float lastStepTime = -999f;

    [Header("Footstep Culling & Budget")]
    [Tooltip("ไกลเกินระยะนี้ (เมตร) จะไม่คำนวณเสียงเท้าเลย ประหยัดทั้งแรงและหู")]
    [SerializeField] private float maxFootstepHearDistance = 14f;

    [Tooltip("ปิดถ้าอยากให้ผีตัวนี้ส่งเสียงเท้าได้เสมอ เช่น บอสตัวสำคัญ")]
    [SerializeField] private bool useSharedStepBudget = true;

    [Header("Auto-Detect Surface")]
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private float raycastDistance = 1.5f;

    [SerializeField] private PlayerSoundController.SurfaceSound[] surfaceSounds;
    [SerializeField] private PlayerSoundController.TerrainSound[] terrainSounds;
    [SerializeField] private SoundID defaultFootstep;

    private void OnEnable()
    {
        isDead = false;
        RollVoiceGacha();
        ResetMoanTimer(0f);
    }

    private void Update()
    {
        if (!useAutoMoan || isDead) return;

        if (Time.time >= nextMoanAllowedTime)
        {
            // สุ่มว่ารอบนี้จะร้องหรือไม่
            if (Random.Range(0f, 100f) <= moanChance)
            {
                PlayIdleMoan();
            }
            else
            {
                // ถ้ารอบนี้เงียบ ก็ให้รีเซ็ตเวลาไปรอรอบถัดไปเลย
                ResetMoanTimer(0f);
            }
        }
    }

    private void ResetMoanTimer(float clipDuration)
    {
        float delay = clipDuration + Random.Range(minMoanDelay, maxMoanDelay);
        nextMoanAllowedTime = Time.time + delay;
    }

    private void RollVoiceGacha()
    {
        if (voiceProfiles == null || voiceProfiles.Length == 0) return;

        float totalWeight = 0f;
        foreach (var profile in voiceProfiles)
        {
            totalWeight += profile.dropWeight;
        }

        float randomVal = Random.Range(0f, totalWeight);
        float currentSum = 0f;

        foreach (var profile in voiceProfiles)
        {
            currentSum += profile.dropWeight;
            if (randomVal <= currentSum)
            {
                currentVoice = profile;
                break;
            }
        }
    }

    // ==========================================
    // Vocals
    // ==========================================
    public void PlayIdleMoan()
    {
        if (Time.time < nextMoanAllowedTime) return;
        float duration = PlaySoundAttached(currentVoice.idleMoan);
        ResetMoanTimer(duration);
    }

    public void PlayAttackScream()
    {
        float duration = PlaySoundAttached(currentVoice.attackScream);
        ResetMoanTimer(duration);
    }

    public void PlayDeathSound()
    {
        PlaySoundAttached(currentVoice.death);
        isDead = true;
    }

    // ==========================================
    // Actions
    // ==========================================
    public void PlayActionSound(int index)
    {
        if (actionSounds != null && index >= 0 && index < actionSounds.Length)
        {
            PlaySoundAttached(actionSounds[index]);
        }
    }

    // ==========================================
    // Footsteps
    // ==========================================
    public void PlayFootstep()
    {
        if (Time.time - lastStepTime < minStepInterval) return;

        // [ADD] ด่าน 1: ไกลเกินได้ยิน ตัดทิ้งก่อนเลย
        // ต้องเช็คก่อน Raycast เพราะของเดิมยิงเรดาร์ + อ่าน Terrain alphamap ทุกก้าว
        // แม้ผีตัวนั้นจะอยู่ไกล 50 เมตรจนไม่มีทางได้ยิน
        float maxSqr = maxFootstepHearDistance * maxFootstepHearDistance;
        if (AudioListenerCache.SqrDistanceToListener(transform.position) > maxSqr) return;

        // [ADD] ด่าน 2: ขอโควต้าจากงบกลางที่ผีทุกตัวแชร์กัน
        // ผีมาเยอะ = เสียงเท้าซ้อนกันจนรก อันนี้จำกัดไว้ไม่ให้เกินที่หูรับไหว
        if (useSharedStepBudget && !CreatureFootstepBudget.TryConsume()) return;

        lastStepTime = Time.time;

        SoundID footstepToPlay = defaultFootstep;
        Vector3 rayOrigin = transform.position + (Vector3.up * 0.5f);
        Vector3 soundSpawnPos = transform.position;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
        {
            soundSpawnPos = hit.point;
            Terrain hitTerrain = hit.collider.GetComponent<Terrain>();

            if (hitTerrain != null)
            {
                TerrainLayer dominantLayer = GetDominantTerrainLayer(hit.point, hitTerrain);
                if (dominantLayer != null)
                {
                    bool foundMatch = false;
                    foreach (var tSound in terrainSounds)
                    {
                        if (tSound.terrainLayers == null) continue;
                        foreach (var layer in tSound.terrainLayers)
                        {
                            if (layer == dominantLayer)
                            {
                                footstepToPlay = tSound.walkSound;
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
                string hitTag = hit.collider.tag;
                foreach (var surface in surfaceSounds)
                {
                    if (surface.surfaceTag == hitTag)
                    {
                        footstepToPlay = surface.walkSound;
                        break;
                    }
                }
            }
        }

        PlaySound(footstepToPlay, soundSpawnPos);
    }

    // ==========================================
    // Core Logic
    // ==========================================
    private float PlaySound(SoundID id, Vector3 position)
    {
        if (id != null && SoundManager.Instance != null)
        {
            return SoundManager.Instance.PlaySFX(id, position);
        }
        return 0f;
    }

    private float PlaySoundAttached(SoundID id)
    {
        if (id != null && SoundManager.Instance != null)
        {
            return SoundManager.Instance.PlaySFXAttached(id, transform);
        }
        return 0f;
    }

    private TerrainLayer GetDominantTerrainLayer(Vector3 worldPos, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;
        float mapX = ((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth;
        float mapZ = ((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight;
        int x = Mathf.FloorToInt(mapX);
        int z = Mathf.FloorToInt(mapZ);

        if (x < 0 || z < 0 || x >= terrainData.alphamapWidth || z >= terrainData.alphamapHeight) return null;

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
}