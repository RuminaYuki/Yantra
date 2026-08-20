using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

[Serializable]
public class SoundData
{
#if UNITY_EDITOR
    [SerializeField, HideInInspector]
    private string idName;

    public void UpdateName()
    {
        idName = id != null ? id.name : "None";
    }
#endif

    [Header("Identity & Routing")]
    public SoundID id;
    public AudioMixerGroup mixerGroup;

    [Header("Audio Clip")]
    [Tooltip("เปิดเพื่อใช้ระบบสุ่มเสียงจากหลายๆ ไฟล์ (ถ้าปิด จะใช้แค่ช่อง Clip เดี่ยวๆ)")]
    public bool useRandomClips = false;

    [Header("Random Chance")]
    [Tooltip("โอกาสที่จะหยิบเสียงจากใน List มาเล่น (100 = สุ่มจาก List เสมอ, 20 = มีโอกาสแค่ 20%)")]
    [Range(0f, 100f)] public float randomChance = 100f;

    [Tooltip("ใส่ไฟล์เสียงที่นี่ (กรณีปิดการสุ่ม)")]
    public AudioClip clip;

    [Tooltip("ใส่ไฟล์เสียงหลายๆ ไฟล์ที่นี่ (กรณีเปิดการสุ่ม)")]
    public AudioClip[] clips;

    [Header("Pitch Settings")]
    [Tooltip("เปิดเพื่อสุ่มเสียงแหลม/ทุ้ม (ทำให้เสียงดูไม่ซ้ำซาก)")]
    public bool useRandomPitch = false;
    [Range(0.1f, 3f)] public float minPitch = 0.9f;
    [Range(0.1f, 3f)] public float maxPitch = 1.1f;

    [Header("Volume & 3D Settings")]
    [Tooltip("เปิดเพื่อสุ่มความดังแต่ละก้าว (น้ำหนักเท้าหนัก/เบา)")]
    public bool useRandomVolume = false;
    [Range(0f, 1f)] public float minVolume = 0.85f;
    [Range(0f, 1f)] public float maxVolume = 1.0f;

    [Range(0f, 1f)] public float volume = 1f;
    [Tooltip("0 = 2D (เสียงดังเท่ากันหมด), 1 = 3D (ดังตามระยะทาง)")]
    [Range(0f, 1f)] public float spatialBlend = 1f;

    // ==========================================
    // Core Logic
    // ==========================================

    public AudioClip GetClip()
    {
        if (!useRandomClips || clips == null || clips.Length == 0)
            return clip;

        if (UnityEngine.Random.Range(0f, 100f) <= randomChance)
        {
            return clips[UnityEngine.Random.Range(0, clips.Length)];
        }

        return clip;
    }

    public float GetPitch()
    {
        if (useRandomPitch)
            return UnityEngine.Random.Range(minPitch, maxPitch);

        return 1f;
    }

    public float GetVolume()
    {
        if (useRandomVolume)
            return UnityEngine.Random.Range(minVolume, maxVolume);

        return volume;
    }
}

[CreateAssetMenu(menuName = "Sound/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    public List<SoundData> sounds;

#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (var s in sounds)
        {
            if (s != null)
                s.UpdateName();
        }
    }
#endif
}