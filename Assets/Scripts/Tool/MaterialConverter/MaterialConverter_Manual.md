# Material Converter — คู่มือการใช้งาน

**เวอร์ชัน:** 1.0 (2026-07-17)
**สภาพแวดล้อม:** Unity 6000.3+ / HDRP 17.x (ฝั่ง URP ต้องติดตั้งแพ็กเกจ Universal RP เพิ่มถ้าจะสร้าง material ฝั่ง URP)
**ตำแหน่งโค้ด:** `Assets/Scripts/Tool/MaterialConverter/` (Editor tool ทั้งหมด, namespace `MaterialConverterTool`)

---

## สารบัญ

1. [ภาพรวม](#1-ภาพรวม)
2. [ความต่างของ Texture ระหว่าง URP กับ HDRP](#2-ความต่างของ-texture-ระหว่าง-urp-กับ-hdrp)
3. [วิธีเรียกใช้](#3-วิธีเรียกใช้)
4. [ระบบจับกลุ่มอัจฉริยะจากชื่อไฟล์](#4-ระบบจับกลุ่มอัจฉริยะจากชื่อไฟล์)
5. [เมนูทั้ง 8 คำสั่ง](#5-เมนูทั้ง-8-คำสั่ง)
6. [ตารางชื่อ map ที่ระบบรู้จัก](#6-ตารางชื่อ-map-ที่ระบบรู้จัก)
7. [ไฟล์ที่ถูกสร้างและกติกาการตั้งชื่อ](#7-ไฟล์ที่ถูกสร้างและกติกาการตั้งชื่อ)
8. [ข้อจำกัดและหมายเหตุ](#8-ข้อจำกัดและหมายเหตุ)

---

## 1. ภาพรวม

Material Converter เป็นเครื่องมือฝั่ง Editor สำหรับ **รวม / แยก / แปลง texture map (PNG และ EXR)
และ material ระหว่างรูปแบบของ URP กับ HDRP** ใช้งานจากการคลิกขวาใน Project window

ปัญหาที่เครื่องมือนี้แก้: asset ที่ซื้อมาหรือ export จากโปรแกรมปั้น (Substance, Blender ฯลฯ)
มักมาเป็น map แยกไฟล์แบบ URP/มาตรฐานทั่วไป (Metallic, Roughness, AO แยกกัน)
แต่ **HDRP ใช้ "Mask Map" ที่รวม 4 ข้อมูลไว้ในภาพเดียว** — การรวมมือใน Photoshop ทีละไฟล์เสียเวลามาก
เครื่องมือนี้ทำให้อัตโนมัติทั้งชุด รวมถึงแปลงย้อนกลับ และสร้าง material ให้เสร็จในคลิกเดียว

---

## 2. ความต่างของ Texture ระหว่าง URP กับ HDRP

| ข้อมูล | URP (Lit) | HDRP (Lit) |
|---|---|---|
| สีพื้น | Base Map | Base Color Map |
| Metallic | **Metallic Map — channel R** | **Mask Map — channel R** |
| Smoothness | **Metallic Map — channel A** | **Mask Map — channel A** |
| Ambient Occlusion | Occlusion Map (ไฟล์แยก, อ่าน channel G) | **Mask Map — channel G** |
| Detail Mask | ไฟล์แยก | **Mask Map — channel B** |
| Normal | Bump Map | Normal Map |
| Height | Parallax Map | Height Map |
| Emission | Emission Map | Emissive Color Map |

สรุป: หัวใจของการแปลงคือ **Mask Map ของ HDRP = Metallic(R) + AO(G) + Detail(B) + Smoothness(A)**
ส่วน URP แยกเป็น MetallicSmoothness (R + A) กับ Occlusion อีกไฟล์

> ถ้าไฟล์ต้นทางเป็น **Roughness** (มาตรฐาน PBR ภายนอก) ระบบจะกลับค่าให้เป็น Smoothness อัตโนมัติ (smoothness = 1 − roughness)

---

## 3. วิธีเรียกใช้

1. ใน **Project window** เลือก texture หรือ material **กี่ไฟล์ก็ได้** (ลากคลุม / Ctrl+click ข้ามโฟลเดอร์ได้)
2. **คลิกขวา → Material Converter →** เลือกหมวดและคำสั่ง
3. รอ progress bar เสร็จ จะมี dialog สรุปว่า ตรวจพบกี่ object, สร้าง/อัพเดทไฟล์อะไรบ้าง, มีคำเตือนอะไร (รายละเอียดเต็มดูใน Console)

เมนูจะกดได้เฉพาะเมื่อ selection ตรงประเภท (คำสั่ง texture ต้องเลือก texture, คำสั่ง material ต้องเลือก material)

---

## 4. ระบบจับกลุ่มอัจฉริยะจากชื่อไฟล์

เลือก texture หลายไฟล์พร้อมกันได้เลย ระบบจะ **แยกชื่อ object ออกจากชื่อ map** แล้วจับกลุ่มให้เอง:

```
เลือก 12 ไฟล์:
  Rock_BaseColor.png   Rock_Normal.png   Rock_Roughness.png   Rock_AO.png
  Crate_BaseColor.png  Crate_Normal.png  Crate_Metallic.png   Crate_Roughness.png
  Barrel_Albedo.png    Barrel_Normal.png Barrel_Metallic.exr  Barrel_AO.png

ระบบตรวจพบ 3 object:  Rock, Crate, Barrel
→ สร้าง Mask Map 3 ไฟล์ / Material 3 ชิ้น ชื่อ Rock.mat, Crate.mat, Barrel.mat
```

กติกา:

- ส่วนท้ายชื่อไฟล์ = ชนิด map (ดูตารางข้อ 6), ส่วนหน้า = ชื่อ object → **material ที่สร้างใช้ชื่อ object**
- token ขยะท้ายชื่อถูกข้ามให้อัตโนมัติ: `2K, 4K, 1024, 2048, OGL, DX, sRGB, Linear` ฯลฯ
  (เช่น `Rock_Roughness_4K.png` → object "Rock")
- ตัวคั่นที่รองรับ: `_` `-` `.` เว้นวรรค / ไม่สนตัวพิมพ์เล็ก-ใหญ่
- ไฟล์ที่เดาชนิดไม่ได้ จะถือเป็น Base Color พร้อมแจ้งเตือน
- texture ที่ **ไม่ได้เปิด Read/Write หรือถูก compress ก็ใช้ได้** — ระบบสลับ import settings ชั่วคราวแล้วคืนค่าเดิมให้

---

## 5. เมนูทั้ง 8 คำสั่ง

### หมวด Create Texture (รวม map แยก → map แพ็ค)

| # | คำสั่ง | ต้องเลือก | ผลลัพธ์ |
|---|---|---|---|
| 1 | **Create New Texture On HDRP Format** | map แยกของแต่ละ obj (Metallic / Roughness หรือ Smoothness / AO — มีเท่าไหนใช้เท่านั้น) | `<obj>_MaskMap.png` (R=Metallic, G=AO, B=1, A=Smoothness) |
| 2 | **Create New Texture On URP Format** | map แยกเช่นเดียวกัน | `<obj>_MetallicSmoothness.png` (R=Metallic, A=Smoothness) |

ค่า default เมื่อไม่มี map: Metallic = 0, AO = 1 (ขาว), Smoothness = 0.5

### หมวด Convert Texture (แปลง map แพ็คข้ามไปป์ไลน์)

| # | คำสั่ง | ต้องเลือก | ผลลัพธ์ |
|---|---|---|---|
| 3 | **Convert Texture HDRP To URP Format** | Mask Map ของ HDRP | `<obj>_MetallicSmoothness.png` + `<obj>_Occlusion.png` |
| 4 | **Convert Texture URP To HDRP Format** | MetallicSmoothness ของ URP (+ AO ถ้ามี เลือกมาด้วยจะถูกใส่ channel G ให้) | `<obj>_MaskMap.png` |

> ฉลาดเพิ่ม: ถ้าเลือกมา 1 ไฟล์แล้วชื่อไม่บอกชนิด ระบบจะถือว่าไฟล์นั้นคือ map แพ็คที่ต้องการแปลง

### หมวด Convert Material (แปลง material ที่มีอยู่)

| # | คำสั่ง | ต้องเลือก | ผลลัพธ์ |
|---|---|---|---|
| 5 | **Convert Material HDRP To URP Format** | material shader HDRP/Lit | `<ชื่อเดิม>_URP.mat` (URP/Lit) + แตก Mask Map เป็น MetallicSmoothness/Occlusion ให้อัตโนมัติ |
| 6 | **Convert Material URP To HDRP Format** | material shader URP/Lit | `<ชื่อเดิม>_HDRP.mat` (HDRP/Lit) + รวม Mask Map จาก MetallicGloss/Occlusion ให้อัตโนมัติ |

สิ่งที่ถูกย้ายให้: Base Map/Color, Normal (+scale), Metallic/Smoothness (ทั้งแบบ map และ slider), AO, Height, Emission (+สี), Tiling/Offset, GPU Instancing — **material ต้นฉบับไม่ถูกแตะ** (สร้างไฟล์ใหม่ข้าง ๆ)

### หมวด Create Material (จาก texture → material จบในคลิกเดียว)

| # | คำสั่ง | ต้องเลือก | ผลลัพธ์ |
|---|---|---|---|
| 7 | **Create And Convert URP Texture To HDRP Material** | texture map แบบ URP/แยกไฟล์ ของกี่ obj ก็ได้ | Mask Map + **`<obj>.mat` (HDRP/Lit)** ครบทุก obj |
| 8 | **Create And Convert HDRP Texture To URP Material** | texture แบบ HDRP (มี Mask Map) ของกี่ obj ก็ได้ | MetallicSmoothness + Occlusion + **`<obj>.mat` (URP/Lit)** ครบทุก obj |

Material จะถูกใส่ map ให้ครบทุกช่องที่มีข้อมูล: Base Color, Normal (ตั้ง import type เป็น Normal Map ให้ด้วย), Mask/MetallicSmoothness, AO, Height, Emission พร้อมเปิด keyword ที่ต้องใช้

---

## 6. ตารางชื่อ map ที่ระบบรู้จัก

| ชนิด map | คำที่รู้จัก (ท้ายชื่อไฟล์, ไม่สนตัวพิมพ์) |
|---|---|
| Base Color | basecolor, base_color, albedo, diffuse, diff, basemap, color, col, alb, d |
| Normal | normal, normal_map, normalgl, normaldx, nrm, nor, norm, bump, n |
| Metallic | metallic, metalness, metal, mtl, met, m |
| Roughness | roughness, rough, rgh, r |
| Smoothness | smoothness, smooth, gloss, glossiness |
| MetallicSmoothness (URP แพ็ค) | metallicsmoothness, metallic_smoothness, metallicgloss, ms |
| Mask Map (HDRP แพ็ค) | maskmap, mask_map, mask |
| Occlusion | ao, occlusion, ambientocclusion, ambient_occlusion, occ |
| Height | height, heightmap, displacement, disp, parallax, h |
| Emission | emission, emissive, emit, glow, e |

---

## 7. ไฟล์ที่ถูกสร้างและกติกาการตั้งชื่อ

- ไฟล์ใหม่ถูกวางไว้ **โฟลเดอร์เดียวกับ texture ต้นทาง** (material conversion วางข้างไฟล์ต้นทาง)
- texture ที่สร้าง: `<obj>_MaskMap.png`, `<obj>_MetallicSmoothness.png`, `<obj>_Occlusion.png`
  — ตั้งค่า **sRGB off (linear)** ให้อัตโนมัติ, ขนาดภาพ = ขนาดใหญ่สุดของ map ต้นทาง (map เล็กถูกขยายแบบ bilinear)
- material ที่สร้างจาก texture: `<obj>.mat` / จากการแปลง material: `<ชื่อเดิม>_URP.mat`, `<ชื่อเดิม>_HDRP.mat`
- **รันซ้ำ = อัพเดทของเดิม** (ไม่สร้างไฟล์ซ้ำเพิ่มเรื่อย ๆ) — texture ถูกเขียนทับ, material ถูกอัพเดทค่า

---

## 8. ข้อจำกัดและหมายเหตุ

- **โปรเจกต์นี้เป็น HDRP** — คำสั่งที่สร้าง material ฝั่ง URP (ข้อ 5, 8) ต้องติดตั้งแพ็กเกจ
  `com.unity.render-pipelines.universal` ก่อน ไม่งั้นระบบจะแจ้งเตือนและไม่ทำงาน (คำสั่ง texture ใช้ได้หมดไม่ต้องมี URP)
- รองรับ workflow **Metallic** เท่านั้น (Specular workflow ยังไม่รองรับ)
- คุณสมบัติ surface ขั้นสูงของ material (Transparent, Alpha Clipping, Detail Map, Coat ฯลฯ) ไม่ถูกย้ายอัตโนมัติ — ตั้งเองหลังแปลง
- map ที่แพ็คออกมาเป็น **PNG 8-bit** (Metallic/AO/Smoothness ไม่ต้องการ bit depth สูง) — EXR ต้นทางอ่านได้ปกติ
  ส่วน Height/Emission ที่เป็น EXR จะถูก assign เข้าตรง ๆ ไม่ผ่านการแปลง จึงไม่เสียข้อมูล HDR
- Emission จะถูกตั้งสีเป็นขาวเมื่อมี emission map — ปรับ intensity เองตามต้องการ
- การเขียนไฟล์ texture ไม่สามารถ Undo ได้ (ไฟล์ .png ถูกสร้าง/เขียนทับจริง) — แต่ต้นฉบับไม่ถูกแก้ไขเสมอ

---

*Material Converter Tool — Yantra Project*
