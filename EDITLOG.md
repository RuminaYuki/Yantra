# EDITLOG

บันทึกการแก้ไขโปรเจค Yantra (Unity 6.3.15 LTS)

---

## [2026-06-23] แก้ปัญหา External Tools รีเซ็ตและ VS Code Package Error

**ปัญหา:**
- Unity External Tools เปลี่ยน IDE เองทุกครั้งที่ reload
- Package `com.unity.ide.vscode` (v1.2.5) ขึ้น deprecated/error ใน Package Manager
- ต้องการให้ IDE เป็น Visual Studio (สีม่วง) เท่านั้น

**สาเหตุ:**
- `com.unity.ide.vscode` v1.2.5 ถูกติดตั้งเป็น direct dependency ใน manifest.json
- `com.unity.ide.rider` จาก `com.unity.feature.development` แย่ง External Tools จาก Visual Studio

**แก้ไข (`Packages/manifest.json`):**
- ลบ `"com.unity.ide.vscode": "1.2.5"` ออก (deprecated package)
- เพิ่ม `"com.unity.ide.rider": "exclude"` เพื่อป้องกัน Rider แย่ง External Tools

**หลัง Unity reload:**
- ไปตั้งค่า Edit → Preferences → External Tools → เลือก Visual Studio หนึ่งครั้ง
- จากนั้นจะไม่ reset อีก

---

## [2026-06-23] แก้ root cause ของ External Tools รีเซ็ต

**สาเหตุจริง:**
- มี Editor script `Assets/Editor/UseVSCodeAsExternalEditor.cs` ที่ใช้ `[InitializeOnLoad]` + `EditorPrefs.SetString("kScriptsDefaultApp", ...)` force set VS Code ทุก domain reload
- script นี้ชี้ไปที่ `C:\Users\NO\AppData\Local\Programs\Microsoft VS Code\Code.exe`

**แก้ไข:**
- แก้ไฟล์ `Assets/Editor/UseVSCodeAsExternalEditor.cs` ให้ชี้ไปที่ Visual Studio (`devenv.exe`) แทน
- เพิ่ม guard `File.Exists()` ก่อน set เพื่อความปลอดภัย

---

## [2026-06-23] Ghost Audio System + State Machine Refactor

**ไฟล์ที่แก้ไข:**
- `Assets/Scripts/Ghost/GhostAudioManager.cs`
- `Assets/Scripts/Ghost/GhostAnimationController.cs`
- `Assets/Scripts/Tani/Sate/TaniStateMachine.cs`
- `Assets/Scripts/Tani/Sate/Search.cs`

**การเปลี่ยนแปลง GhostAudioManager:**
- เพิ่ม `GhostAudioPlayMode` enum (Loop / OneShot)
- เพิ่ม `GhostAudioEntry` class รวม playMode + clips list แต่ละ state
- Loop: สุ่ม clip แล้ว set ลง `_voiceSource.clip` — ถ้า clip เดิมยัง playing ข้ามไป
- OneShot: `PlayOneShot` ปล่อยทิ้ง ไม่หยุดตอนเปลี่ยน state
- `StopLoopAudio()`: หยุดเฉพาะ loop channel เมื่อเปลี่ยน state
- เพิ่ม footstep channel (`_footstepSource`, `_walkFootstepClip`, `_runFootstepClip`) รอรับเสียงในอนาคต — null = ข้าม
- `OnValidate()` auto-assign `_voiceSource` จาก child

**การเปลี่ยนแปลง TaniStateMachine:**
- เพิ่ม `OnValidate()` auto-assign agent, animator, _animationController, _audioManager
- `ChangeState()` เรียก `_audioManager?.StopLoopAudio()` ก่อน Exit เพื่อหยุด loop เสมอ

**การเปลี่ยนแปลง Search state:**
- `Enter()` เรียก `PlaySearchAnimation()` ทันที (animation + audio) แม้จะยังเดินไปจุดสุดท้าย
- `StartScanning()` ลบ `PlaySearchAnimation()` ออก (play ไปแล้วตอน Enter)

---

## [2026-06-23] Attack State Deal Damage

**ไฟล์ที่แก้ไข:**
- `Assets/Scripts/Tani/Sate/Attack.cs`
- `Assets/Scripts/Tani/Sate/TaniStateMachine.cs`

**การเปลี่ยนแปลง:**
- `TaniStateMachine`: เพิ่ม `[SerializeField] private float attackDamage = 25f;` ใน Attack header
- `TaniStateMachine`: cache `_playerStats` (YantraStatsController) ตอน `TryFindPlayer()`
- `TaniStateMachine`: เพิ่ม `DealDamageToPlayer()` — เรียก `_playerStats?.TakeDamage(attackDamage)`
- `Attack`: เพิ่ม `hasDealtDamage` flag — reset ทุก `Enter()`, deal damage ครั้งเดียวต่อ swing เมื่อ `IsPlayerInAttackRange()` เป็น true ระหว่าง attack window

---

## [2026-06-23] Yant Effect System (พลัก / ฮีล / หายตัว)

**ความสัมพันธ์กับระบบเดิม:**
- วาดบนกระดาษ → `DrawOn3DMesh` เก็บเส้น → `YantraShapeMatcher.AnalyzeDrawing()` → `LastResult` (CategoryName + SimilarityPercent)
- ระบบใหม่นี้ต่อ "ปลายทาง" หลัง match เสร็จ ไม่แตะอกอริทึม matcher (เป็น $1-recognizer + golden-section rotation search ทำงานดีอยู่แล้ว)

**ไฟล์ใหม่ (`Assets/Scripts/Yant/Effects/`):**
- `YantCastContext.cs` — บริบทการร่าย (caster, stats, aim origin/direction, category, %)
- `YantEffectSO.cs` — ScriptableObject ฐานของ effect ทุกชนิด มี `Cast(context)`
- `HealYantSO.cs` — ยนต์ฮีล: `Heal(_healAmount)` ให้ผู้ร่าย
- `VanishYantSO.cs` — ยนต์หายตัว: เปลี่ยน tag ผู้ร่ายเป็น Untagged (ถาวร/ชั่วคราวคืน tag เอง)
- `YantVanish.cs` — helper MonoBehaviour เก็บ tag เดิม + coroutine คืน tag
- `PushYantSO.cs` — ยนต์พลัก: ปา `YantProjectile` ตั้ง speed/มุมเริ่มต้น/gravity/lifetime + พารามิเตอร์ดัน
- `YantProjectile.cs` — ลูกยนต์ที่ปา ชนแล้วติด `YantGradualPush` ให้เป้า
- `YantGradualPush.cs` — ดันวัตถุทีละนิดต่อเฟรม (ForceMode.Acceleration) ตาม lifeTime แล้วลบตัวเอง
- `YantCaster.cs` — ตัวกลาง: `AnalyzeAndCast()` เรียก matcher → เช็ค `_minSimilarityPercent` → map หมวด→effect (`YantBinding`) → ร่าย พร้อมเล็งด้วย raycast จากกล้อง FPP

**วิธีต่อใน Unity (ต้องตั้งใน Inspector):**
1. ใส่ `YantCaster` บน player, assign `_matcher`, `_stats`, `_aimCamera`, `_muzzle`, `_playerRoot`
2. สร้าง effect asset: Create → Yantra/Effects → Heal/Vanish/Push Yant ตั้งค่าตามต้องการ
3. PushYant ต้องมี prefab ลูกยนต์ (Rigidbody + Collider + `YantProjectile`)
4. ใน `YantCaster._bindings` ผูกชื่อหมวด (ตรงกับ `ShapeCategory.CategoryName` เช่น PushYant/HealYant/InviYant) → effect asset
5. เรียก `AnalyzeAndCast()` ตอนวาดเสร็จ (ผูกกับปุ่มยืนยัน หรือเรียกหลังกด confirm)

---

## [2026-06-24] ซ่อม Scene พังจาก Merge + หา Root Cause

**Repo:** Plastic Cloud `Yanta/Yanta@cloud` (branch `/main/Maya`)

**ซ่อม Villge.unity (Broken PPtr):**
- PrefabInstance `873697060` (instance ตอไม้ Birch_stump) หายตอน merge เหลือ orphan stripped Transform `353985542`
- ลบ orphan Transform block + addedObject reference ออก, backup ไว้ `Villge.unity.bak_*`
- สแกนยืนยัน m_PrefabInstance reference 308 ตัว resolve ครบ 380 def

**Root cause — ทำไม merge พัง + ช้า:**
1. ช้า: Assets 8.6GB มี terrain/asset binary หนักจริง (`TerrainDemoScene_HDRP` 3.8GB — **village ใช้ terrain layer/material จากในนี้ผ่าน GUID ห้ามลบ**)
2. พัง: ไม่มี `lock.conf` → หลายคนแก้ `.unity` พร้อมกัน → text-merge → orphan fileID; terrain เป็น binary merge ไม่ได้

**แก้ไข (วิธีหลัก = file locking):**
- สร้าง `lock.conf` ที่ root (lock `.unity`/`.prefab`/`.asset`/`.controller`/`.anim`/`.mat`/`.terrainlayer`/`.lighting`)
- ⚠️ ต้องเปิด lock rules ใน Plastic Cloud dashboard (Repository → Locking) ด้วยถึงมีผลกับทุกคน
- EditorSettings: serialization = ForceText (2) ถูกแล้ว, vector inline ถูกแล้ว
- **ไม่ลบ demo terrain** (ยืนยันแล้วว่า village ใช้ asset จากในนั้นผ่าน GUID)
