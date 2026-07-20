# Procedural Tree Generator — คู่มือการใช้งาน

**เวอร์ชัน:** 1.3.5 (2026-07-20)
**สภาพแวดล้อม:** Unity 6000.3+ / HDRP 17.x
**ตำแหน่งโค้ด:** `Assets/Scripts/Tool/TreeGenerator/` (namespace `TreeTool`)

> **แก้ไข v1.3.5:** ชื่อ Reference ในตารางข้อ 8 ปรับให้ตรงกับที่ใช้จริงใน
> `Tree-HDRP-Lit-PBR.shadergraph` (ไม่ใช่ชื่อที่เสนอไว้ตอนแรกใน v1.3.2-1.3.4) —
> `_AmbientOcclusionMap` (แทน `_OcclusionMap`), `_SmoothnessMinScale`/`_SmoothnessMaxScale`
> (แทน `_SmoothnessRemapMin/Max`), `_AmbientOcclusionMinScale`/`_AmbientOcclusionMaxScale`
> (แทน `_AORemapMin/Max`), และ Keyword Reference `MAPFORMAT` (ไม่มี underscore นำหน้า แทน
> `_MapFormat`) — อัพเดท `Editor/TreeWindShaderGUI.cs` ให้ค้นหาด้วยชื่อจริงเหล่านี้แล้ว
> (แก้ script ให้ตรงกับกราฟที่มีอยู่ เพราะกราฟ wiring ถูกต้องอยู่แล้ว ไม่ต้องแก้กราฟใหม่)

---

## สารบัญ

1. [ภาพรวม](#1-ภาพรวม)
2. [เริ่มต้นใช้งาน](#2-เริ่มต้นใช้งาน)
3. [ระบบ Seed (การสุ่ม)](#3-ระบบ-seed-การสุ่ม)
4. [โหมด Geometry: Procedural กับ Prefabs](#4-โหมด-geometry-procedural-กับ-prefabs)
5. [พารามิเตอร์ทั้งหมด](#5-พารามิเตอร์ทั้งหมด)
6. [ระบบ LOD](#6-ระบบ-lod)
7. [Material](#7-material)
8. [ระบบลม: ใช้งานได้จริงกับ WindZone ของ Unity](#8-ระบบลม-ใช้งานได้จริงกับ-windzone-ของ-unity)
9. [การ Export เป็น Mesh Asset / ทำ Prefab](#9-การ-export-เป็น-mesh-asset--ทำ-prefab)
10. [ข้อจำกัดและ Tips](#10-ข้อจำกัดและ-tips)

---

## 1. ภาพรวม

Procedural Tree Generator เป็นเครื่องมือสร้างต้นไม้แบบ procedural ภายใน Unity
แนวคิดเดียวกับ Tree Creator ของ Unity แต่ออกแบบมาสำหรับ **HDRP** และเพิ่มความสามารถ:

- ปรับค่าใน Inspector แล้ว **โมเดลอัพเดททันทีใน Edit Mode** (ลาก slider ดูแบบ real-time —
  ระหว่างลากระบบ rebuild เฉพาะ LOD0 แบบเบา ๆ ให้ลื่น แล้วค่อย rebuild ครบทุก LOD อัตโนมัติเมื่อปล่อยมือ)
- ค่าเกือบทั้งหมดเป็น **ช่วงสุ่ม (min–max range)** ลากปรับได้
- ระบบ **seed แยก 3 ตัว** — สุ่มทรงต้น / กิ่ง / ใบ แยกกันได้อิสระ
- สร้าง **LOD หลายระดับ + LODGroup อัตโนมัติ** พร้อม cross-fade
- รองรับ 2 โหมด: สร้าง geometry เองทั้งหมด หรือใช้ **prefab (FBX) ที่ปั้นมาเอง** สำหรับลำต้น กิ่ง ใบ (ใส่ได้หลาย variant แล้วสุ่มเลือก)
- **ระบบลมใช้งานได้จริง** ขับเคลื่อนจาก **WindZone ตัวจริงของ Unity** — ปรับได้ทีละส่วนว่าลำต้น/กิ่งแต่ละชั้น/ใบ จะฟริ้วกับลมมากแค่ไหน

องค์ประกอบหลัก:

| ไฟล์ | หน้าที่ |
|---|---|
| `ProceduralTree.cs` | Component หลัก วางบน GameObject |
| `ProceduralTreeSettings.cs` | พารามิเตอร์ทั้งหมด |
| `TreeSkeleton.cs` | สุ่มโครงต้นไม้ (เส้น spline ของลำต้น/กิ่ง + ตำแหน่งใบ) |
| `TreeMeshBuilder.cs` | แปลงโครงเป็น mesh ต่อ LOD (bake ข้อมูลลมลง vertex color ด้วย) |
| `TreeWindZoneDriver.cs` | อ่าน WindZone จริงในฉาก ส่งเป็น global shader property |
| `TreeWind.hlsl` | ฟังก์ชันคำนวณการขยับ vertex ตามลม ใช้กับ Shader Graph (Custom Function Node) |
| `Editor/ProceduralTreeEditor.cs` | Inspector, ปุ่มสุ่ม seed, stats, export, ปุ่มเพิ่ม Wind Zone |
| `Editor/TreeWindShaderGUI.cs` | Material Inspector ของ wind shader — dropdown สลับ HDRP/URP texture format |
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

Inspector จะ**ซ่อน field ที่ไม่เกี่ยวกับโหมดที่เลือกให้อัตโนมัติ** (v1.1):
โหมด Procedural ซ่อนช่องใส่ prefab / โหมด Prefabs ซ่อน field ฝั่ง procedural
(เช่น Leaf Shape, Radial Segments, Bark UV Tiling)

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
| Radial Segments | จำนวนเหลี่ยมรอบลำต้น — **เฉพาะลำต้น** (สูงสุด 64) แยกจากกิ่งแต่ละชั้น |
| Crookedness | ความคดเคี้ยว |
| Lean | องศาเอียงทั้งต้น — ช่วงสุ่ม |
| Wind Response | ลำต้นจะฟริ้วตามลมแค่ไหน (0 = แข็งนิ่ง, 1 = ปกติ) — default ต่ำ (0.25) เพราะลำต้นควรนิ่งกว่ากิ่ง |

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
| Radius Ratio | ความหนาเทียบกิ่งแม่ ณ จุดงอก (เกิน 1 = หนากว่าแม่ได้) |
| Thickness Scale | **ตัวคูณความหนาเฉพาะชั้นนี้** — ปรับชั้นเดียวไม่กระทบชั้นบน แต่ส่งต่อให้ชั้นลูกตามลำดับชั้นโดยธรรมชาติ (ลูกวัด Radius Ratio จากความหนาจริงของชั้นนี้) |
| Radial Segments | จำนวนเหลี่ยมของกิ่ง**ชั้นนี้** (แยกอิสระจากลำต้นและชั้นอื่น) |
| Joint Smoothing | สัดส่วนช่วงต้นกิ่งที่**โค้งออกจากทิศกิ่งแม่แบบสมูท** แทนที่จะหักมุมทันที — ช่วยซ่อนรอยต่อ |
| Joint Flare | ความหนาพิเศษที่โคนกิ่ง ค่อย ๆ จางไปตามช่วงรอยต่อ — กลืนโคนกิ่งเข้ากับกิ่งแม่ |
| Taper | ความเรียวปลายกิ่ง |
| Gravity | + = ห้อยลง, − = เชิดขึ้นหาแสง |
| Crookedness | ความคดของกิ่ง |
| Segments | จำนวนท่อนต่อกิ่ง |
| Wind Response | กิ่งชั้นนี้ฟริ้วตามลมแค่ไหน (0 = แข็ง, 1 = ปกติ, สูงกว่า = พลิ้วมาก) — default ชั้นกิ่งใหญ่ 1.0, ชั้นกิ่งฝอย 1.8 (ยิ่งเล็กยิ่งพลิ้ว) |

### Leaves (ใบ)

| ค่า | ความหมาย |
|---|---|
| Enabled | เปิด/ปิดใบ |
| Shape | Quad / Cross / TripleCross (แสดงเฉพาะโหมด Procedural) |
| Count Per Branch | จำนวนใบต่อกิ่ง — ช่วงสุ่ม (สูงสุด 300) |
| Size | ขนาดใบ (เมตร) — ช่วงสุ่ม สูงสุด 10 ม. (สเกล prefab ด้วยถ้าใช้โหมด Prefabs) |
| Spawn Range | ตำแหน่งบนกิ่งที่ใบเกิดได้ |
| Orientation Randomness | 0 = ใบเรียงตามกิ่ง, 1 = หมุนมั่วเต็มที่ |
| Surface Offset | ระยะห่างใบจากผิวกิ่ง (เมตร) — **ช่วงสุ่ม ปรับได้ −2 ถึง +5** (ติดลบ = จมเข้ากิ่ง) |
| Rotation Offset | หมุนใบเพิ่มทุกใบ (องศา XYZ) ก่อนสุ่ม — จุดหมุนคือโคนใบที่ปักกับกิ่ง |
| Wind Flutter Response | ใบสั่น/บิดเองในลมแค่ไหน (แยกจากการแกว่งของกิ่งที่ใบเกาะอยู่) — default 1.5 |
| Min Branch Level | ใบเกิดบนกิ่งชั้นนี้ขึ้นไป (ลำต้น = 0) |

> **ทิศทางใบ (v1.1):** จุดปัก (pivot) ของ card คือ **มุมล่างซ้ายของ texture (UV 0,0)** = โคน/ก้านใบ
> ปักติดกับกิ่งเสมอ และ card ถูกหมุนให้ **แนวทแยงชี้ออกจากกิ่ง** — ดังนั้นให้วาด texture ใบแบบ
> โคนอยู่มุมล่างซ้าย ปลายใบอยู่มุมบนขวา จะได้ทิศถูกต้องอัตโนมัติ

### Mesh

| ค่า | ความหมาย |
|---|---|
| Bark UV Tiling | ความถี่ UV แนวตั้งของเปลือกต่อเมตร |
| Generate Tangents | เปิดไว้ถ้าใช้ normal map |

> **v1.2:** ค่าเหลี่ยม (Radial Segments) ย้ายไปอยู่กับ**แต่ละส่วน** — ลำต้นอยู่ใน Trunk,
> กิ่งแต่ละชั้นอยู่ใน Branch Level นั้น ๆ ปรับแยกอิสระได้เลย (ใบไม่มีเหลี่ยม เป็น card/prefab)

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

> **Sync สองทาง (v1.1):** ถ้าไปลากปรับระยะบน **LODGroup component โดยตรง** ค่าจะถูกดึงกลับเข้า
> settings ของ tool ให้อัตโนมัติ — rebuild ครั้งถัดไปจะไม่เขียนทับค่าที่ปรับไว้

---

## 7. Material

- **Bark Material** — เปลือกไม้ HDRP/Lit ปกติ (มี UV + tangent รองรับ normal map)
- **Leaf Material** — ควรเป็น HDRP/Lit ที่เปิด **Double-Sided** + **Alpha Clipping** ใส่ texture ใบไม้
- ปล่อยว่าง = ใช้ placeholder อัตโนมัติ (น้ำตาล/เขียว) — ใช้ชั่วคราวเท่านั้น อย่าลืมใส่ของจริง
- Mesh มี 2 submesh เสมอ: submesh 0 = เปลือก, submesh 1 = ใบ

---

## 8. ระบบลม: ใช้งานได้จริงกับ WindZone ของ Unity

ต้นไม้จาก tool นี้ **ใช้งานลมได้จริงแล้ว** ขับเคลื่อนจาก **component `WindZone` ตัวจริงของ Unity**
(อันเดียวกับที่ลาก Direction / Wind Main / Turbulence / Pulse ในฉาก) — ไม่ต้องตั้งค่าลมแยกทีละ material

> **หมายเหตุความถูกต้อง:** ระบบลมภายในของ Unity ที่ใช้กับ SpeedTree เป็น pipeline ปิด
> (proprietary) ที่ bake ข้อมูล vertex เฉพาะรูปแบบของตัวเอง mesh ที่ tool นี้สร้างเองจึงต่อเข้า
> pipeline นั้นตรง ๆ ไม่ได้ — สิ่งที่ทำได้คือ**อ่านค่าจริงจาก component `WindZone`** (ทิศทาง,
> ความแรง, turbulence, pulse) แล้วคำนวณการขยับ vertex ด้วยสูตรของเราเอง ผลคือ artist ยังคง
> ใช้ workflow เดิม (ลาก WindZone ลงฉาก ปรับค่าที่นั่นที่เดียว) ต้นไม้ทุกต้นในฉากขยับตาม

### วิธีเปิดใช้งาน (3 ขั้นตอน)

1. ในฉากต้องมี **`Tree Wind Zone Driver`** (Add Component → `Tools/Tree Wind Zone Driver`) —
   หรือกดปุ่ม **"Add Wind Zone To Scene"** ที่ขึ้นเตือนใต้ section Wind ใน `ProceduralTree`
   inspector เมื่อยังไม่มี (จะสร้าง `WindZone` + `Tree Wind Zone Driver` ให้พร้อมกันในคลิกเดียว)
2. ปรับค่าลมที่ **component `WindZone`** ตามปกติ (Mode, Wind Main, Turbulence, Pulse Magnitude/Frequency)
   — เหมือนตั้งค่าลมให้ต้นไม้ปกติของ Unity ทุกประการ
3. ทำ material เปลือก/ใบด้วย **HDRP Shader Graph** ที่มี node **Custom Function** ชี้ไปที่ไฟล์
   `TreeWind.hlsl` ฟังก์ชัน `TreeWindDisplacement_float` แล้วบวกผลลัพธ์เข้ากับตำแหน่ง vertex
   (ดูรายละเอียด node ด้านล่าง) — ใส่ material นี้ในช่อง Bark/Leaf Material ของต้นไม้

Component `Tree Wind Zone Driver` จะหา `WindZone` ที่ enable อยู่ตัวแรกในฉากให้อัตโนมัติ
(หรือ assign เจาะจงเองในช่อง `Wind Zone` ก็ได้) แล้วส่งค่าออกเป็น **global shader property**
ทุกเฟรม จึง**ไม่ต้อง assign อะไรเพิ่มที่ material แต่ละอัน**

### ข้อมูลที่ bake ไว้ใน vertex color (ทำอัตโนมัติ ไม่ต้องทำเอง)

เมื่อเปิด **Wind → Bake Wind Data** (default เปิด) ทุก vertex จะมีน้ำหนักลมอยู่ใน vertex color
โดยคูณด้วยค่า **Wind Response ของส่วนนั้น ๆ** ที่ตั้งไว้ตอนสร้างต้นไม้ (ข้อ 5) เรียบร้อยแล้ว:

| Channel | ข้อมูล | มาจาก |
|---|---|---|
| **R** | Main bend: โค้งตามความสูง (Bend Exponent) × Wind Response ของส่วนนั้น | Trunk/Branch Level `Wind Response` |
| **G** | Local sway: 0 ที่โคนกิ่ง → 1 ที่ปลายกิ่ง × Wind Response ของกิ่งนั้น (ลำต้น = 0, ใบรับค่าจากกิ่งที่เกาะ) | Branch Level `Wind Response` |
| **B** | Leaf flutter: ความสั่น/บิดของใบเอง (เปลือก = 0) | Leaves `Wind Flutter Response` |
| **A** | Random phase ต่อกิ่ง/ต่อใบ (0–1) | สุ่มอัตโนมัติ ไม่ต้องตั้ง |

**นี่คือจุดที่ตอบคำถาม "set แต่ละส่วนว่าจะฟริ้วแค่ไหน"** — ปรับ `Wind Response` ที่ Trunk /
ที่แต่ละ Branch Level / `Wind Flutter Response` ที่ Leaves ตอนสร้างต้นไม้ได้เลย ไม่ต้องไปนั่งแก้ shader
เช่น ลำต้นแทบไม่ขยับ (0.25) → กิ่งใหญ่ขยับปานกลาง (1.0) → กิ่งฝอยขยับเยอะ (1.8) → ใบสั่นเร็วสุด (1.5)

### Custom Function node (Shader Graph) — ต่อครั้งเดียวจบ

`TreeWind.hlsl` เขียนสูตรลมทั้งหมดไว้ในฟังก์ชันเดียวแล้ว (main bend + local sway + leaf flutter +
turbulence) ไม่ต้องต่อ node เองทีละเส้น:

1. ใน HDRP Lit Shader Graph เพิ่ม node **Custom Function**, ตั้ง Type = **File**, ชี้ไปที่ `TreeWind.hlsl`,
   ช่อง **Name** ใส่ `TreeWindDisplacement`
   > **สำคัญ:** ห้ามใส่ `_float` ต่อท้ายใน Name field — Shader Graph เติม `_float`/`_half`
   > ให้เองอัตโนมัติตอน generate โค้ดตาม precision ของกราฟ ถ้าใส่ `_float` เองด้วยจะกลายเป็น
   > เรียกหาฟังก์ชันชื่อ `TreeWindDisplacement_float_float` ซึ่งไม่มีจริง แล้วขึ้น error
   > `undeclared identifier` สีแดงตรงจุดต่อ Add
2. ตั้ง Input ของ node ตามลำดับนี้ (ชื่อ/ชนิดต้องตรง):
   `PositionWS (Vector3)`, `VertexColor (Vector4)`, `MainAmplitude (Float)`, `BranchAmplitude (Float)`,
   `LeafAmplitude (Float)`, `MainSpeed (Float)`, `BranchSpeed (Float)`, `LeafSpeed (Float)`
   และ Output หนึ่งช่อง: `PositionOffsetWS (Vector3)`
3. เพิ่ม node **Position** ตั้ง Space = **World** → ต่อเข้า `PositionWS`, และ **Vertex Color** → ต่อเข้า `VertexColor`
4. Amplitude/Speed ให้ **Expose เป็น material property** แล้วตั้งค่าเริ่มต้นแนะนำ:
   MainAmplitude ≈ 0.15–0.3, BranchAmplitude ≈ 0.1–0.2, LeafAmplitude ≈ 0.05–0.1,
   MainSpeed ≈ 0.6, BranchSpeed ≈ 1.8, LeafSpeed ≈ 5 (ปรับตามสเกลต้นไม้ได้)
5. **แปลงเฉพาะ offset กลับเป็น Object Space** (อย่าแปลงตำแหน่งรวมทั้งก้อน เพราะเสี่ยงพลาดง่ายกว่าและ
   แม่นยำน้อยกว่า): เพิ่ม node **Transform** ตั้ง Type = **Direction**, From = **World**, To = **Object**
   แล้วต่อ `PositionOffsetWS` เข้า node นี้
6. เพิ่ม node **Position** อีกตัว ตั้ง Space = **Object** แล้วใช้ **Add**: `A` = Position (Object) ตัวนี้,
   `B` = output จาก Transform ข้อ 5 (**ห้ามต่อ Vertex Color เข้า Add โดยตรง** — เป็นข้อผิดพลาดที่พบบ่อย
   เพราะ node หน้าตาคล้าย Position จนสับสน แต่ Vertex Color คือน้ำหนักลม ไม่ใช่ตำแหน่ง)
7. ต่อ `Add.Out(3)` เข้า **Vertex Position** ใน Master Stack

ทำ shader graph นี้ 2 อัน (bark, leaf — ของใบเปิด Double-Sided + Alpha Clipping) แล้วใส่ในช่อง
Bark Material / Leaf Material ตามปกติ ต้นไม้ทุกต้นที่ใช้ material นี้จะพลิ้วตาม WindZone ทันที

### Fragment stage: PBR เต็มรูปแบบ + สลับ Texture Format ได้ (HDRP / URP)

ขั้นตอนข้างบนต่อแค่ **Vertex stage (ลม)** เท่านั้น — Shader Graph เปล่า ๆ ไม่มีช่อง Base Color /
Normal Map / Mask Map ให้เหมือน HDRP/Lit อัตโนมัติ ต้องต่อ node ฝั่ง **Fragment** เพิ่มเอง (คนละ
stage กับ Vertex ที่ทำไปแล้ว ไม่กระทบกัน)

Material ยังคง**เป็น HDRP เต็มรูปแบบ** (render ด้วย HDRP Lit target) — แค่ **เลือกได้ว่า texture
ที่ป้อนเข้ามาเป็น texture ที่แพ็คแบบ HDRP (Mask Map) หรือแบบ URP (Metallic + Occlusion แยกไฟล์)**
ผ่าน dropdown บน Inspector ของ material เอง เลือกโหมดไหน อีกโหมดจะ**ซ่อนช่องใส่และไม่ถูกนำไปคำนวณเลย**
(ตัด branch ที่ไม่ได้ใช้ออกตอน compile shader ด้วย Shader Feature keyword ไม่ใช่แค่ซ่อน UI เฉย ๆ)

**1. เพิ่ม Property ใน Blackboard** (คลิก `+` มุมบนซ้าย) — ตั้งชื่อ **Reference ให้ตรงตามตารางนี้เป๊ะ**
(ตัวพิมพ์เล็ก/ใหญ่มีผล) เพราะ script `TreeWindShaderGUI.cs` จะค้นหาด้วยชื่อนี้:

| Reference | Type | ใช้ทำ |
|---|---|---|
| `_BaseColor` | Color | สีปรับโทนคูณกับ Base Map |
| `_BaseMap` | Texture2D | สี/ลายเปลือกไม้หรือใบ |
| `_NormalMap` | Texture2D | normal map — ตั้ง Mode = **Bump** ในกล่อง property (ไม่ใช่ Default) |
| `_NormalScale` | Float | ความแรง normal map (default 1) |
| `_Tiling` | Vector2 | default (1, 1) |
| `_MaskMap` | Texture2D | **HDRP format**: Metallic(R) / AO(G) / Detail(B) / Smoothness(A) |
| `_MetallicGlossMap` | Texture2D | **URP format**: Metallic(R) / Smoothness(A) |
| `_AmbientOcclusionMap` | Texture2D | **URP format**: AO (อ่านช่อง G ตาม convention ของ URP/Standard) |
| `_MetallicScale` | Float | คูณกับค่า Metallic ที่อ่านได้ — ปรับความเป็นโลหะขึ้น/ลงโดยไม่ต้องแก้ texture |
| `_SmoothnessMinScale` / `_SmoothnessMaxScale` | Float ทั้งคู่ | remap ค่า Smoothness ที่อ่านได้จาก [0,1] เดิม ไปเป็น [Min,Max] — ชื่อ/พฤติกรรมตรงกับ **Smoothness Remapping** ของ HDRP Material จริง |
| `_AmbientOcclusionMinScale` / `_AmbientOcclusionMaxScale` | Float ทั้งคู่ | remap ค่า AO เหมือนกัน — ตรงกับ **Ambient Occlusion Remapping** ของ HDRP Material จริง |

**2. เพิ่ม Keyword แบบ Enum ใน Blackboard** (คลิก `+` → Keyword → Enum):

- ตั้งชื่อ (Name) = `Map Format`, **Reference** = `MAPFORMAT` (พิมพ์ตรง ๆ ห้ามให้ Shader Graph auto-gen)
- Entries: เพิ่ม 2 อัน ชื่อ `HDRP` (index 0) แล้ว `URP` (index 1) — **เรียงลำดับนี้เท่านั้น** ต้องตรงกับ script
- Definition = **Shader Feature** (ไม่ใช่ Global/Multi Compile — ทำให้ Unity คอมไพล์แยก variant ต่อ
  material จริง ๆ ไม่ใช่แค่ if ตอน runtime)

**3. ต่อ node ฝั่ง Fragment:**

```
UV(0) ──► Tiling And Offset (Tile = _Tiling) ──► ป้อนเข้า Sample Texture 2D ทุกตัวด้านล่าง

Sample Texture 2D (_BaseMap)                     → RGB × _BaseColor.rgb → Base Color
                                                  → A                    → Alpha

Sample Texture 2D (_NormalMap, Type = Normal)    → Normal Strength(_NormalScale) → Normal (Tangent Space)

Sample Texture 2D (_MaskMap)          .R ─┐
Sample Texture 2D (_MetallicGlossMap) .R ─┴─► Keyword node #1 (ลาก MAPFORMAT จาก Blackboard มาวาง)
    ช่อง "HDRP" ← MaskMap.R   ช่อง "URP" ← MetallicGlossMap.R   → Out × _MetallicScale ─► Metallic

Sample Texture 2D (_MaskMap)          .G ─┐
Sample Texture 2D (_AmbientOcclusionMap)     .G ─┴─► Keyword node #2 (ลาก MAPFORMAT มาอีกตัว)
    ช่อง "HDRP" ← MaskMap.G   ช่อง "URP" ← OcclusionMap.G
    → Out → Lerp(A=_AmbientOcclusionMinScale, B=_AmbientOcclusionMaxScale, T=Out) ─► Ambient Occlusion

Sample Texture 2D (_MaskMap)          .A ─┐
Sample Texture 2D (_MetallicGlossMap) .A ─┴─► Keyword node #3 (ลาก MAPFORMAT มาอีกตัว)
    ช่อง "HDRP" ← MaskMap.A   ช่อง "URP" ← MetallicGlossMap.A
    → Out → Lerp(A=_SmoothnessMinScale, B=_SmoothnessMaxScale, T=Out) ─► Smoothness
```

> **เพิ่มปรับค่าแต่ละ Map ได้แบบ HDRP Material จริง:** `_MetallicScale` คูณตรง ๆ กับ Metallic,
> ส่วน Smoothness/AO ใช้ **Lerp node** (T = ค่าที่อ่านจาก map, A = ...RemapMin, B = ...RemapMax) —
> นี่คือกลไกเดียวกับที่ Inspector ของ HDRP Material เรียกว่า "Smoothness Remapping" /
> "Ambient Occlusion Remapping" ทุกประการ ไม่ใช่แค่ตั้งชื่อให้คล้าย

> **หมายเหตุ:** ลาก node **Keyword** จาก Blackboard (ไม่ใช่ Branch node) — Keyword node ที่มาจาก
> Enum keyword จะมีช่อง input ชื่อตรงกับ entry ที่ตั้งไว้ (`HDRP` / `URP`) ให้อัตโนมัติ ลาก `MAPFORMAT`
> มาวางในกราฟ 3 ครั้งสำหรับ Metallic / AO / Smoothness (จะได้ node แยกกัน 3 ตัว ใช้ keyword เดียวกัน)
>
> **จุดพลาดบ่อย:** node Sample Texture 2D ของ `_NormalMap` ต้องตั้ง dropdown ภายใน node เป็น
> **Type = Normal** (ไม่ใช่ Default) ไม่งั้นสีปกติ (normal) จะเพี้ยน

**4. เฉพาะ material ใบ (Leaf):**

- **Graph Settings** (มุมขวาบนของหน้าต่าง Shader Graph) → เปิด **Alpha Clipping** และตั้ง **Double-Sided**
- `_BaseMap.A` ต่อเข้า **Alpha** ของ Fragment ด้วย (ไม่ใช่แค่ Base Color) เพื่อให้ใบโปร่งใสตาม texture ได้

**5. Detail Map** (ลายละเอียดซ้อนทับ เช่น รอยแตกเปลือกไม้ระยะใกล้ — ใช้ convention เดียวกันทั้ง
HDRP/URP ไม่ต้องมี dropdown แยก) เพิ่ม Property:

| Reference | Type | ใช้ทำ |
|---|---|---|
| `_DetailMap` | Texture2D | R = Detail Albedo (overlay, 0.5 = ไม่มีผล) / G = Detail Normal Y / B = Detail Smoothness (overlay) / A = Detail Normal X |
| `_DetailMask` | Texture2D | grayscale, default ขาวล้วน = ใส่ detail เต็มที่ทุกจุด |
| `_DetailTiling` | Vector2 | tiling แยกของ detail (ปกติถี่กว่า `_Tiling` หลายเท่า) |
| `_DetailAlbedoStrength` | Float | 0–2 |
| `_DetailNormalStrength` | Float | 0–2 |
| `_DetailSmoothnessStrength` | Float | 0–2 |

ต่อ node:
```
UV(0) ──► Tiling And Offset (Tile = _DetailTiling) ──► Sample Texture 2D (_DetailMap, Type=Default)
Sample Texture 2D (_DetailMask) → R → mask

DetailAlbedo   = (_DetailMap.r − 0.5) × 2 × _DetailAlbedoStrength × mask
                 → Add เข้ากับ Base Color ที่ต่อไว้ในข้อ 3 (ก่อนออก Fragment.Base Color)

DetailNormalTS = Normalize( float3(_DetailMap.a×2−1, _DetailMap.g×2−1, 1) )
                 → Normal Blend node (Base Normal จากข้อ 3, Detail Normal นี้, Strength=_DetailNormalStrength×mask)
                 → Out ─► Normal (Tangent Space)  (แทนที่ output เดิมจาก _NormalMap อย่างเดียว)

DetailSmoothness = (_DetailMap.b − 0.5) × 2 × _DetailSmoothnessStrength × mask
                 → Add เข้ากับ Smoothness ที่ต่อไว้ในข้อ 3 ก่อนออก Fragment.Smoothness
```

**6. Height Map** (นูน/ยุบผิวเปลือกแบบเบา ๆ ผ่านการขยับ vertex ตามแนว normal — ไม่ใช้ Pixel/Parallax
Occlusion Mapping เพราะกิ่ง/ใบมีจำนวน vertex และ instance เยอะมาก POM ต่อพิกเซลจะแพงเกินจำเป็นสำหรับพืช):

| Reference | Type | ใช้ทำ |
|---|---|---|
| `_HeightMap` | Texture2D | grayscale height (0 = ยุบ, 1 = นูน) |
| `_HeightScale` | Float | ระยะยุบ/นูนสูงสุด (เมตร) |

ต่อใน **Vertex stage** (ต่อจากผลลัพธ์ Add ของข้อลม ที่ทำไว้แล้วในหัวข้อ Custom Function ก่อนหน้า):
```
Sample Texture 2D LOD (_HeightMap, LOD = 0) → R → height   ⚠ ต้องใช้ "Sample Texture 2D LOD"
    ไม่ใช่ "Sample Texture 2D" ธรรมดา — vertex shader ไม่มี mip derivative ใช้ node ปกติไม่ได้

HeightOffset = (height − 0.5) × _HeightScale
Position (Object Space) → Normal (Object Space) × HeightOffset → Add ตัวใหม่:
    A = ผลลัพธ์ Add เดิม (ตำแหน่งหลังใส่ลมแล้ว), B = Normal(Object)×HeightOffset
    → Out ─► ต่อเข้า Vertex Position แทนที่ Add เดิม (เอา Add ใหม่นี้เป็นตัวสุดท้ายก่อนเข้า Master Stack)
```

**7. Emission** (จุดเรืองแสง เช่น ใบไม้เรืองแสงเวทมนตร์/ยันต์ ถ้าไม่ใช้ก็เว้น Emission Color ไว้ที่ดำ):

| Reference | Type | ใช้ทำ |
|---|---|---|
| `_EmissionMap` | Texture2D | RGB emission (default ดำ = ไม่เรือง) |
| `_EmissionColor` | Color (HDR) | สี |
| `_EmissionIntensity` | Float (nits) | ตัวคูณความแรงแยกจากสี — วิธีเดียวกับที่ HDRP Material ใช้จริง (สี + Intensity แยกช่อง) |

ต่อ: `Sample Texture 2D (_EmissionMap).RGB × _EmissionColor × _EmissionIntensity` → **Emission** ใน Fragment
(อย่าลืมเปิด **Emission** ใน Graph Settings ถ้า Shader Graph ซ่อน slot นี้ไว้โดย default)

**8. ค่าเริ่มต้นแนะนำทั้งหมด (Recommended Defaults)**

ตั้งค่า default บน node/property ในกราฟให้ตรงตารางนี้ตั้งแต่แรก จะได้ material ที่ดูสมเหตุสมผลทันที
โดยไม่ต้องลองผิดลองถูก (ปรับต่อได้เสมอจาก material ที่ export ออกมา):

| Property | ค่าเริ่มต้นแนะนำ | เหตุผล |
|---|---|---|
| `_BaseColor` | ขาวล้วน (1,1,1,1) | ให้ texture คุมสีเต็มที่ ไม่มีสีทับ |
| `_NormalScale` | 1 | ความแรง normal ตามที่ปั้นมาเป๊ะ |
| `_Tiling` | (1, 1) | ไม่ทำซ้ำ ใช้ตาม UV ที่ปั้นมา |
| `_MetallicScale` | 1 (เปลือกไม้/ใบ) หรือ 0.3–0.5 ถ้าอยากลดความมันวาวแบบโลหะ | พืชแทบไม่เป็นโลหะ ปกติ Mask Map ควรมี Metallic ต่ำอยู่แล้ว |
| `_SmoothnessMinScale` / `Max` | 0.0 / 0.6 | เปลือก/ใบไม่ควรมันวาวเกิน (ค่าเต็ม 1 จะดูเหมือนพลาสติก) |
| `_AmbientOcclusionMinScale` / `Max` | 0.0 / 1.0 | ใช้ค่า AO ตรงจาก texture เต็มช่วง ไม่บีบ |
| `_DetailAlbedoStrength` | 0.5–1 | ให้เห็น texture รายละเอียดแต่ไม่กลบสีหลัก |
| `_DetailNormalStrength` | 0.5–1 | นูนพอสังเกตได้ ไม่ล้นจน normal เพี้ยน |
| `_DetailSmoothnessStrength` | 0.3 | ผิวสัมผัส detail ปกติควรบางเบากว่า albedo/normal |
| `_DetailTiling` | (4, 4) ถึง (8, 8) | ถี่กว่า `_Tiling` หลักหลายเท่า ให้เห็นเป็น "รายละเอียดระยะใกล้" |
| `_HeightScale` | 0.01–0.02 (1–2 ซม.) | ผิวเปลือกไม้นูนเบา ๆ ค่าสูงกว่านี้จะดูเป็นก้อนเกินจริง |
| `_EmissionColor` | ดำ (0,0,0) | ปิด emission ไว้ก่อน จนกว่าจะต้องใช้ (ยันต์เรืองแสง ฯลฯ) |
| `_EmissionIntensity` | 0 | คู่กับข้างบน — เปิดใช้ค่อยปรับขึ้น |
| `MainAmplitude` (ลม) | 0.15–0.3 | ดูหัวข้อ Custom Function ด้านบน |
| `BranchAmplitude` (ลม) | 0.1–0.2 | ดูหัวข้อ Custom Function ด้านบน |
| `LeafAmplitude` (ลม) | 0.05–0.1 | ดูหัวข้อ Custom Function ด้านบน |

**9. ตั้ง Custom Editor GUI ให้ได้ dropdown ที่ซ่อนช่องอัตโนมัติ:**

ที่ **Graph Settings → Custom Editor GUI** ใส่ชื่อคลาส:
```
TreeTool.EditorTools.TreeWindShaderGUI
```
(มาจากไฟล์ `Editor/TreeWindShaderGUI.cs` ที่เพิ่มมาให้แล้ว) พอตั้งแล้ว material ที่ใช้ shader graph นี้
จะมี dropdown **"Texture Source"** ที่ด้านบนสุดของ Inspector — เลือก **HDRP (Mask Map)** จะซ่อนช่อง
Metallic/Occlusion Map ของ URP ไปเลย และกลับกัน ค่า map ของโหมดที่ไม่ได้เลือกจะไม่ถูกเอาไปคำนวณ (ทั้ง
ระดับ UI และระดับ shader compile เพราะเป็น Shader Feature)

> **ข้อแลกเปลี่ยนที่ควรรู้:** การตั้ง Custom Editor GUI เอง จะ**แทนที่ Inspector มาตรฐานของ HDRP ทั้งหมด**
> (เสีย foldout สวย ๆ อย่าง Surface Options/Advanced Options ของ HDRP ไป) `TreeWindShaderGUI` วาด field
> ที่จำเป็นให้ครบ (Base/Normal/Format dropdown/Emission/field อื่น ๆ ที่ expose ไว้ + Render Queue/GPU
> Instancing) แต่หน้าตาจะเรียบง่ายกว่า HDRP Lit ปกติ ถ้าไม่ต้องการ dropdown ซ่อนช่อง สามารถเว้นช่อง
> Custom Editor GUI ว่างไว้ได้ — Map Format ยังสลับได้ปกติ (Unity สร้าง dropdown enum ให้อัตโนมัติจาก
> Blackboard keyword) แค่ทั้งสองชุด field จะโชว์พร้อมกันเฉย ๆ (ค่าที่ไม่ได้เลือกก็ยังไม่ถูกใช้เหมือนเดิม)

หลังต่อครบทั้ง Vertex (ลม) และ Fragment (texture + format switch) แล้ว ลาก texture จริงใส่ที่ material
asset ใน Project window ตามปกติ — ทั้งสอง stage ทำงานแยกกันคนละส่วน ไม่กระทบการต่อ Vertex/ลมที่ทำไว้ก่อนหน้าเลย

### ข้อจำกัด

- Custom Function อ่านค่า global ที่ `Tree Wind Zone Driver` ตั้งไว้ — **ถ้าไม่มี driver ในฉาก ต้นไม้จะนิ่ง**
  (inspector ของ `ProceduralTree` จะเตือนพร้อมปุ่มเพิ่มให้อัตโนมัติ)
- ระหว่าง Edit Mode (ไม่ได้กด Play) การขยับจะอัพเดทเป็นช่วง ๆ ตามที่ editor repaint ไม่ liquid เท่า Play Mode — เข้า Play Mode เพื่อดูผลลื่นที่สุด
- ค่า Wind Response ที่ bake ไว้เป็นแค่ "น้ำหนัก" ไม่ใช่ระยะจริง (เมตร) — ระยะจริงคุมที่ Amplitude บน material

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
- ยังไม่มี Shader Graph asset สำเร็จรูปให้ลากใช้เลย ต้องต่อ Custom Function node เอง 1 ครั้งตามข้อ 8
  (เหตุผล: ไฟล์ `.shadergraph` เป็น JSON ที่เปราะบางมากถ้าสร้างโดยไม่ผ่าน Shader Graph editor)

**Tips**

- ต้นไกล ๆ จำนวนมาก: ลด `Count` ของ Twigs แล้วเพิ่ม `Count Per Branch` ของใบแทน — ได้พุ่มแน่นใน poly ที่ถูกกว่า
- อยากได้ป่าหลากหลาย: ใช้ settings เดียวกันแล้วเปลี่ยนแค่ `Seed` ต่อต้น
- ต้นสน/ต้นทรงเฉพาะ: เพิ่ม Branch Level ชั้นเดียว count เยอะ ๆ, Angle แคบ, Gravity บวก
- ถ้า scene มีต้นไม้เยอะแล้วเปิด Unity ช้าลง: export mesh เป็น asset จะไม่ต้อง regenerate ตอนโหลด

---

*สร้างโดย Tree Generator Tool — Yantra Project*
