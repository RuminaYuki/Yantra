using UnityEngine;
using System.Collections.Generic;

public class LevelAudioManager : MonoBehaviour
{
    public static LevelAudioManager Instance { get; private set; } // ให้สคริปต์ประตูเรียกใช้ได้

    [Header("เพลงประกอบฉาก (BGM)")]
    [SerializeField] private SoundID sceneBGM;

    [Header("เสียงบรรยากาศภายนอก (Outside Ambient)")]
    [SerializeField] private SoundID[] outsideAmbientSounds;

    // จดจำว่าลำโพงตัวไหนกำลังเล่นเสียงข้างนอกอยู่
    private List<SFXPlayer> activeOutsideAmbients = new List<SFXPlayer>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (SoundManager.Instance == null) return;

        if (sceneBGM != null)
            SoundManager.Instance.PlayBGM(sceneBGM);

        if (outsideAmbientSounds != null)
        {
            foreach (var ambient in outsideAmbientSounds)
            {
                if (ambient != null)
                {
                    // เรียกใช้งานผ่าน SoundManager 
                    SFXPlayer p = SoundManager.Instance.PlayLoopSFXForever(ambient, transform.position);
                    if (p != null) activeOutsideAmbients.Add(p);
                }
            }
        }
    }

    /// <summary>ฟังก์ชันสั่งเสียงข้างนอกให้ดังหรือแว่วๆ (รับค่าเป๊ะๆ จากกล่อง Trigger)</summary>
    public void MuffleOutsideAmbients(float targetMultiplier, float fadeTime)
    {
        foreach (var p in activeOutsideAmbients)
        {
            if (p != null && p.IsPlaying)
            {
                p.FadeToVolumeMultiplier(targetMultiplier, fadeTime);
            }
        }
    }
}