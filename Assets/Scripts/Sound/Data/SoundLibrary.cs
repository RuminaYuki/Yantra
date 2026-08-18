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

    [Header("🎯 Identity & Routing")]
    public SoundID id;
    public AudioMixerGroup mixerGroup; // เผื่อไว้แยกช่องเสียง (BGM, SFX)

    [Header("🎵 Audio Clip (โหมดปกติ หรือ สุ่มเสียง)")]
    [Tooltip("เปิดเพื่อใช้ระบบสุ่มเสียงจากหลายๆ ไฟล์ (ถ้าปิด จะใช้แค่ช่อง Clip เดี่ยวๆ)")]
    public bool useRandomClips = false;

    [Tooltip("ใส่ไฟล์เสียงที่นี่ (กรณีปิดการสุ่ม)")]
    public AudioClip clip;

    [Tooltip("ใส่ไฟล์เสียงหลายๆ ไฟล์ที่นี่ (กรณีเปิดการสุ่ม)")]
    public AudioClip[] clips;

    [Header("🎛️ Pitch Settings")]
    [Tooltip("เปิดเพื่อสุ่มเสียงแหลม/ทุ้ม (ทำให้เสียงดูไม่ซ้ำซาก)")]
    public bool useRandomPitch = false;
    [Range(0.1f, 3f)] public float minPitch = 0.9f;
    [Range(0.1f, 3f)] public float maxPitch = 1.1f;

    [Header("Volume & 3D Settings")]
    [Range(0f, 1f)] public float volume = 1f;
    [Tooltip("0 = 2D (เสียงดังเท่ากันหมด), 1 = 3D (ดังตามระยะทาง)")]
    [Range(0f, 1f)] public float spatialBlend = 1f;

    // ==========================================
    // ฟังก์ชันจัดการ Logic ที่ตัวมันเอง (ฉลาดขึ้น)
    // ==========================================

    public AudioClip GetClip()
    {
        // ถ้าไม่เปิดระบบสุ่ม หรือ เปิดไว้แต่ไม่ได้ใส่ไฟล์ใน Array เลย -> ให้ใช้ไฟล์เดี่ยวๆ (แบบเก่า)
        if (!useRandomClips || clips == null || clips.Length == 0)
            return clip;

        // ถ้าเปิดระบบสุ่ม -> สุ่มหยิบมา 1 ไฟล์
        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }

    public float GetPitch()
    {
        // ถ้าเปิดระบบสุ่ม Pitch -> สุ่มค่ามาให้
        if (useRandomPitch)
            return UnityEngine.Random.Range(minPitch, maxPitch);

        // ถ้าไม่เปิด -> คืนค่า 1 (ระดับเสียงปกติเป๊ะๆ)
        return 1f;
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