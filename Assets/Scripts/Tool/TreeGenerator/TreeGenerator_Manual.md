# Procedural Tree Generator — คู่มือการใช้งาน

**เวอร์ชัน:** 1.0 (2026-07-16)
**สภาพแวดล้อม:** Unity 6000.3+ / HDRP 17.x
**ตำแหน่งโค้ด:** `Assets/Scripts/Tool/TreeGenerator/` (namespace `TreeTool`)

---

## สารบัญ

1. [ภาพรวม](#1-ภาพรวม)
2. [เริ่มต้นใช้งาน](#2-เริ่มต้นใช้งาน)
3. [ระบบ Seed (การสุ่ม)](#3-ระบบ-seed-การสุ่ม)
4. [โหมด Geometry: Procedural กับ Prefabs](#4-โหมด-geometry-procedural-กับ-prefabs)
5. [พารามิเตอร์ทั้งหมด](#5-พารามิเตอร์ทั้งหมด)
6. [ระบบ LOD](#6-ระบบ-lod)
7. [Material](#7-material)
8. [ข้อมูลลมสำหรับ Wind Shader (อนาคต)](#8-ข้อมูลลมสำหรับ-wind-shader-อนาคต)
9. [การ Export เป็น Mesh Asset / ทำ Prefab](#9-การ-export-เป็น-mesh-asset--ทำ-prefab)
10. [ข้อจำกัดและ Tips](#10-ข้อจำกัดและ-tips)

---

## 1. ภาพรวม

Procedural Tree Generator เป็นเครื่องมือสร้างต้นไม้แบบ procedural ภายใน Unity
แนวคิดเดียวกับ Tree Creator ของ Unity แต่ออกแบบมาสำหรับ **HDRP** และเพิ่มความสามารถ:

- ปรับค่าใน Inspector แล้ว **โมเดลอัพเดททันทีใน Edit Mode** (ลาก slider ดูแบบ real-time)
- ค่าเกือบทั้งหมดเป็น **ช่วงสุ่ม (min–max range)** ลากปรับได้
- ระบบ **seed แยก 3 ตัว** — สุ่มทรงต้น / กิ่ง / ใบ แยกกันได้อิสระ
- สร้าง **LOD หลายระดับ + LODGroup อัตโนมัติ** พร้อม cross-fade
- รองรับ 2 โหมด: สร้าง geometry เองทั้งหมด หรือใช้ **prefab (FBX) ที่ปั้นมาเอง** สำหรับลำต้น กิ่ง ใบ (ใส่ได้หลาย variant แล้วสุ่มเลือก)
- Bake **ข้อมูลลมลง vertex color** เตรียมไว้สำหรับ wind shader ในอนาคต

องค์ประกอบหลัก:

| ไฟล์ | หน้าที่ |
|---|---|
| `ProceduralTree.cs` | Component หลัก วางบน GameObject |
| `ProceduralTreeSettings.cs` | พารามิเตอร์ทั้งหมด |
| `TreeSkeleton.cs` | สุ่มโครงต้นไม้ (เส้น spline ของลำต้น/กิ่ง + ตำแหน่งใบ) |
| `TreeMeshBuilder.cs` | แปลงโครงเป็น mesh ต่อ LOD |
| `Editor/ProceduralTreeEditor.cs` | Inspector, ปุ่มสุ่ม seed, stats, export |
| `Editor/MinMaxRangeDrawer.cs` | slider แบบ min–max |

---

## 2. เริ่มต้นใช้งาน

1. คลิกขวาใน **Hierarchy → 3D Object → Procedural Tree (Tool)**
   (หรือสร้าง GameObject เปล่าแล้ว Add Component → `Tools/Procedural Tree`)
2. ต้นไม้จะถูก generate ทันที เป็นลูก `LOD0`, `LOD1`, `LOD2` + `LODGroup` บนตัวแม่
3. เลือกตัวแม่ (ที่มี component `ProceduralTree`) แล้วปรับค่าใน Inspector — โมเดลจะอัพเดททันทีทุกครั้งที่ค่าเปลี่ยน
4. ใส่ Material จริงในช่อง **Bark Material** / **Leaf Material** (ก่อนใส่จะเป็นสี placeholder น้ำตาล/เขียว)
5. พอใจแล้วกด **Export Meshes To Assets** ถ้าต้องการทำเป็น prefab (ดูข้อ 9)

> ปุ่มใน Inspector: **Rebuild Now** = บังคับ generate ใหม่, **Export Meshes To Assets** = freeze mesh เป็นไฟล์ asset

ด้านล่าง Inspector มีกล่อง **stats** แสดงจำนวนกิ่ง และ vertex / triangle / ใบ ของแต่ละ LOD

---

## 3. ระบบ Seed (การสุ่ม)

ทุกการสุ่มในระบบเป็น **deterministic** — settings เดิม + seed เดิม ได้ต้นเดิมเป๊ะ 100%

| Seed | ควบคุม | ปุ่ม |
|---|---|---|
| `Seed` | ทรงลำต้นและทุกอย่างที่ต่อยอดจากมัน (seed หลักของทั้งต้น) | **New Tree** — สุ่มต้นใหม่ทั้งต้น |
| `Branch Seed` | เฉพาะกิ่ง — สลับตำแหน่ง/มุม/ความยาวกิ่งใหม่ **โดยลำต้นไม่เปลี่ยน** | **Shuffle** |
| `Leaf Seed` | เฉพาะใบ — สลับตำแหน่ง/ขนาด/การหมุนของใบ **โดยกิ่งไม่เปลี่ยน** | **Shuffle** |

การใช้งานจริง: กด New Tree จนได้โครงที่ชอบ → Shuffle กิ่งจนพุ่มสวย → Shuffle ใบเก็บรายละเอียด

ค่าที่เป็นช่วง (min–max slider) ทั้งหมดคือ "ขอบเขตการสุ่ม" เช่น Height 4.5–6 หมายถึงแต่ละต้น (ตาม seed) จะสูงค่าใดค่าหนึ่งในช่วงนี้

---

## 4. โหมด Geometry: Procedural กับ Prefabs

Section **Geometry** เลือก source แยกได้ 3 ส่วน: Trunk / Branch / Leaf (ผสมกันได้)

### โหมด Procedural (ค่าเริ่มต้น)
สร้าง geometry ใน Unity ทั้งหมด — ลำต้น/กิ่งเป็นท่อตามเส้น spline, ใบเป็น card (Quad / Cross / TripleCross) ใช้ texture + alpha clip ทำรูปใบ

### โหมด Prefabs (ใช้ FBX ที่ปั้นเอง)
ใส่ prefab ได้**หลายอันต่อช่อง** ระบบจะสุ่มเลือก variant ให้:

- ลำต้น: สุ่ม 1 อันต่อต้น
- กิ่ง: สุ่มต่อกิ่ง (แต่ละกิ่งใช้ variant ต่างกันได้)
- ใบ: สุ่มต่อใบ

การสุ่ม variant ผูกกับ seed เดิม (Branch Seed / Leaf Seed สลับ variant ได้ด้วย)

**กติกาการปั้น FBX:**

| ส่วน | แกนการปั้น | Pivot | หมายเหตุ |
|---|---|---|---|
| ลำต้น / กิ่ง | ยืดตามแกน **+Y** | ที่โคน | ควรมี segment ตามแนวยาวพอสมควร เพื่อให้ดัดโค้งสวย |
| ใบ / ช่อใบ | ยื่นออกตามแกน **+Z** (+Y = ด้านหน้าใบ) | ที่จุดเสียบกิ่ง | ปั้นขนาดราว 1 unit แล้วให้ค่า Leaf Size เป็นตัวสเกล |

พฤติกรรมสำคัญของโหมด Prefabs:

- Mesh ลำต้น/กิ่งจะถูก **ดัดโค้งตามเส้น spline** ที่สุ่มไว้ และสเกลหน้าตัดให้ตรงกับรัศมีกิ่งจุดนั้น ๆ (taper / root flare มีผลทับรูปที่ปั้น) — FBX ก้อนเดียวได้ต้นไม่ซ้ำกันทุกต้น
- Mesh ทั้งหมดถูก bake รวมเป็น mesh เดียวต่อ LOD (ไม่ใช่ instantiate prefab) เพื่อประหยัด draw call
- จึง**ใช้ Material ของ tree ไม่ใช่ของ prefab** — เอา material เปลือกไม้/ใบของ FBX มาใส่ช่อง Bark/Leaf Material แทน

---

## 5. พารามิเตอร์ทั้งหมด

### Trunk (ลำต้น)

| ค่า | ความหมาย |
|---|---|
| Height | ความสูง (เมตร) — ช่วงสุ่ม |
| Radius | รัศมีโคนต้น (เมตร) — ช่วงสุ่ม |
| Taper | ความเรียวปลาย (0 = ทรงกระบอก, 1 = แหลม) |
| Root Flare / Root Flare Height | โคนบานพิเศษ + ระยะที่บานขึ้นไป |
| Segments | จำนวนท่อนตามความสูง (มาก = โค้งเนียน) |
| Crookedness | ความคดเคี้ยว |
| Lean | องศาเอียงทั้งต้น — ช่วงสุ่ม |

### Branch Levels (ชั้นกิ่ง — เป็น list เพิ่ม/ลดชั้นได้)

ชั้นที่ 0 งอกจากลำต้น, ชั้นที่ 1 งอกจากชั้นที่ 0, ต่อไปเรื่อย ๆ (default: Main Branches → Twigs)

| ค่า | ความหมาย |
|---|---|
| Enabled | เปิด/ปิดชั้นนี้ |
| Count | จำนวนกิ่งต่อกิ่งแม่ — ช่วงสุ่ม |
| Spawn Range | ตำแหน่งบนกิ่งแม่ที่งอกได้ (0 = โคน, 1 = ปลาย) |
| Angle | มุมกางออกจากกิ่งแม่ (องศา) — ช่วงสุ่ม |
| Azimuth Randomness | ความสุ่มของการหมุนรอบกิ่งแม่ (0 = เรียงเกลียว golden angle เป๊ะ) |
| Length Ratio | ความยาวเทียบกิ่งแม่ — ช่วงสุ่ม |
| Length Falloff | กิ่งที่งอกใกล้ปลายแม่จะสั้นลงเท่านี้ |
| Radius Ratio | ความหนาเทียบกิ่งแม่ ณ จุดงอก |
| Taper | ความเรียวปลายกิ่ง |
| Gravity | + = ห้อยลง, − = เชิดขึ้นหาแสง |
| Crookedness | ความคดของกิ่ง |
| Segments | จำนวนท่อนต่อกิ่ง |

### Leaves (ใบ)

| ค่า | ความหมาย |
|---|---|
| Enabled | เปิด/ปิดใบ |
| Shape | Quad / Cross / TripleCross (ใช้เมื่อ source เป็น Procedural) |
| Count Per Branch | จำนวนใบต่อกิ่ง — ช่วงสุ่ม |
| Size | ขนาดใบ (เมตร) — ช่วงสุ่ม (สเกล prefab ด้วยถ้าใช้โหมด Prefabs) |
| Spawn Range | ตำแหน่งบนกิ่งที่ใบเกิดได้ |
| Orientation Randomness | 0 = ใบเรียงตามกิ่ง, 1 = หมุนมั่วเต็มที่ |
| Surface Offset | ดันใบออกจากผิวกิ่ง (เมตร) |
| Min Branch Level | ใบเกิดบนกิ่งชั้นนี้ขึ้นไป (ลำต้น = 0) |

### Mesh

| ค่า | ความหมาย |
|---|---|
| Radial Segments | จำนวนเหลี่ยมรอบลำต้น |
| Radial Decay Per Level | กิ่งชั้นลึกลดเหลี่ยมลงเท่านี้ต่อชั้น |
| Bark UV Tiling | ความถี่ UV แนวตั้งของเปลือกต่อเมตร |
| Generate Tangents | เปิดไว้ถ้าใช้ normal map |

---

## 6. ระบบ LOD

Section **Lods** สร้าง mesh แยกต่อระดับ + ตั้ง LODGroup ให้อัตโนมัติ

| ค่า | ความหมาย |
|---|---|
| Generate LOD Group | ปิด = สร้าง LOD0 เต็มรายละเอียดอันเดียว |
| Cross Fade | เฟดนุ่ม ๆ ตอนสลับ LOD |
| Compensate Leaf Size | LOD ไกล ๆ ใบเหลือน้อย → ขยายใบที่เหลือชดเชยให้พุ่มไม่โหรงเหรง |
| Levels (ต่อระดับ) | ดูตารางล่าง |

ต่อ LOD level:

| ค่า | ความหมาย |
|---|---|
| Screen Height | LOD นี้แสดงจนกว่าต้นไม้จะเล็กกว่าสัดส่วนหน้าจอนี้ — **ค่าของระดับสุดท้ายคือจุด cull ทั้งต้น** |
| Radial Resolution | ตัวคูณจำนวนเหลี่ยม (ลด poly ระยะไกล) |
| Max Branch Level | ตัด geometry กิ่งชั้นลึกกว่านี้ทิ้ง (ใบยังอยู่ เพื่อรักษา silhouette พุ่ม) |
| Leaf Density | สัดส่วนใบที่เหลือ (การสุ่มตัดใบ deterministic — rebuild แล้วใบชุดเดิมหาย/อยู่เหมือนเดิม) |

ค่า default: 3 ระดับ (0.45 / 0.18 / 0.05)

---

## 7. Material

- **Bark Material** — เปลือกไม้ HDRP/Lit ปกติ (มี UV + tangent รองรับ normal map)
- **Leaf Material** — ควรเป็น HDRP/Lit ที่เปิด **Double-Sided** + **Alpha Clipping** ใส่ texture ใบไม้
- ปล่อยว่าง = ใช้ placeholder อัตโนมัติ (น้ำตาล/เขียว) — ใช้ชั่วคราวเท่านั้น อย่าลืมใส่ของจริง
- Mesh มี 2 submesh เสมอ: submesh 0 = เปลือก, submesh 1 = ใบ

---

## 8. ข้อมูลลมสำหรับ Wind Shader (อนาคต)

เมื่อเปิด **Wind → Bake Wind Data** (เปิดเป็นค่าเริ่มต้น) ทุก vertex จะมีน้ำหนักลมอยู่ใน **vertex color**:

| Channel | ข้อมูล | ใช้ทำ |
|---|---|---|
| **R** | Main bend: 0 ที่โคน → 1 ที่ยอด (โค้งตาม Bend Exponent) | โยกทั้งต้นตามลม |
| **G** | Branch sway: 0 ที่โคนกิ่ง → 1 ที่ปลายกิ่ง (ลำต้น = 0) | กิ่งแกว่ง |
| **B** | Leaf flutter mask: ใบ = 1, เปลือก = 0 | ใบสั่นระริก |
| **A** | Random phase ต่อกิ่ง/ต่อใบ (0–1) | ให้แต่ละกิ่ง/ใบไม่แกว่งพร้อมกัน |

แนวทางตอนทำ shader (Shader Graph บน HDRP): อ่าน Vertex Color แล้ว offset ตำแหน่ง vertex สามชั้น
`ลมหลัก × R` + `แกว่งกิ่ง × G × sin(time + A × 2π)` + `สั่นใบ × B × noise ความถี่สูง`
ค่า **Bend Exponent** ปรับได้ว่าให้ครึ่งบนของต้นโยกมากแค่ไหนเทียบครึ่งล่าง

---

## 9. การ Export เป็น Mesh Asset / ทำ Prefab

Mesh ที่ generate ปกติจะ**ไม่ถูก save ลง scene** (ไฟล์ scene ไม่บวม) และ regenerate เองตอนโหลด scene
ถ้าต้องการต้นไม้ "นิ่ง" สำหรับทำ prefab / ส่งต่อ:

1. ตั้งชื่อ GameObject ให้สื่อ (ชื่อนี้ใช้ตั้งชื่อไฟล์)
2. กด **Export Meshes To Assets**
3. Mesh ทุก LOD จะถูกบันทึกไปที่ `Assets/GeneratedTrees/<ชื่อต้น>/` และ MeshFilter จะชี้ไปที่ asset ทันที
4. ลาก GameObject ลง Project เป็น prefab ได้เลย

> หลัง export ถ้าแก้ค่าใด ๆ อีก ระบบจะ generate mesh ชั่วคราวมาแทน (ไฟล์ asset เดิมยังอยู่บนดิสก์) — แก้เสร็จก็ export ทับใหม่

---

## 10. ข้อจำกัดและ Tips

**ข้อจำกัด**

- โหมด Prefabs: จำนวน vertex ของ mesh ตายตัวตามที่ปั้น — `Radial Resolution` ต่อ LOD ไม่มีผล (แต่ Max Branch Level กับ Leaf Density ยังช่วยลด poly ระยะไกลได้)
- ยังไม่มี billboard LOD ระดับสุดท้าย (ใช้การ cull แทน)
- ยังไม่สร้าง collider ให้ (แนะนำใส่ Capsule Collider ที่ลำต้นเองตามขนาดจริง)
- Prefab ที่ใส่ต้องมี MeshFilter (mesh ธรรมดา ไม่รองรับ SkinnedMeshRenderer)

**Tips**

- ต้นไกล ๆ จำนวนมาก: ลด `Count` ของ Twigs แล้วเพิ่ม `Count Per Branch` ของใบแทน — ได้พุ่มแน่นใน poly ที่ถูกกว่า
- อยากได้ป่าหลากหลาย: ใช้ settings เดียวกันแล้วเปลี่ยนแค่ `Seed` ต่อต้น
- ต้นสน/ต้นทรงเฉพาะ: เพิ่ม Branch Level ชั้นเดียว count เยอะ ๆ, Angle แคบ, Gravity บวก
- ถ้า scene มีต้นไม้เยอะแล้วเปิด Unity ช้าลง: export mesh เป็น asset จะไม่ต้อง regenerate ตอนโหลด

---

*สร้างโดย Tree Generator Tool — Yantra Project*
