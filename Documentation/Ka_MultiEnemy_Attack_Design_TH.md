# Ka Multi-Enemy Attack Design

เอกสารนี้สรุปความต้องการสำหรับ AI/ผู้พัฒนาคนถัดไป ก่อนลงมือแก้ระบบ State Machine ของศัตรู Ka

## เป้าหมาย

ต้องการให้ผี Ka หลายตัวต่อสู้กับ Player ได้โดยไม่โจมตีซ้อนกัน ผีที่ได้รับสิทธิ์เท่านั้นจึงเดินเข้าไปโจมตี ส่วนผีตัวอื่นต้องเดินวนรอบ Player เพื่อรอจังหวะ เมื่อเจ้าของสิทธิ์โจมตีและถอยเสร็จ จึงคืนสิทธิ์ให้ผีตัวอื่น

ความต้องการด้านพฤติกรรม:

- ผีหลายตัวสามารถ Chase และล้อม Player พร้อมกันได้
- ในแต่ละช่วงมีผีโจมตี Player ได้เพียงหนึ่งตัว
- ผีที่ยังไม่ได้สิทธิ์ต้องเดินวน/จัดตำแหน่งรอบ Player ไม่ยืนนิ่ง
- ผีที่ได้สิทธิ์ต้องเดินจากตำแหน่งที่วนอยู่เข้า Attack Range ก่อนโจมตี ห้ามโจมตีจากระยะไกล
- หลังโจมตี ผีต้องถอยออกมาก่อนคืนสิทธิ์
- ระบบต้องไม่อาศัย `Player.IsHurt` เป็นตัวล็อกหลัก เพราะ Hurt animation อาจสั้นและไม่ครอบคลุมช่วงเดินเข้าตีหรือถอย
- เมื่อเจ้าของสิทธิ์ตาย ถูก Disable ถูก Stun จนยกเลิกการโจมตี ผู้เล่นหนีไกล หรือเส้นทางโจมตีถูกยกเลิก ต้องคืนสิทธิ์ด้วย

## แนวคิดหลัก: Attack Slot / Attack Token

ให้มีตัวกลางบน Player เช่น `PlayerAttackCoordinator` ซึ่งมี Attack Slot เริ่มต้นหนึ่งช่อง และเก็บ reference ของผีที่เป็นเจ้าของสิทธิ์:

```text
CurrentAttacker = Enemy instance หรือ null
```

ไม่ควรเก็บเพียง Boolean เพราะต้องรู้ว่าใครเป็นเจ้าของ และต้องป้องกันผีตัวอื่นเรียก Release แทนเจ้าของ

API เชิงแนวคิด:

```csharp
bool TryClaim(GameObject enemy);
void Release(GameObject enemy);
bool IsOwner(GameObject enemy);
```

กติกา:

- Slot ว่าง: `TryClaim` บันทึกผู้ขอเป็น `CurrentAttacker` และคืน `true`
- เจ้าของเดิมขอซ้ำ: คืน `true`
- มีผีตัวอื่นถือสิทธิ์: คืน `false`
- `Release` เคลียร์ Slot ได้เฉพาะเมื่อผู้เรียกคือเจ้าของ
- หากเจ้าของถูกทำลายหรือ inactive ต้องทำให้ Slot กลับมาว่าง
- การตรวจและจองต้องเกิดใน operation เดียว เพื่อไม่ให้ผีสองตัวอ่านว่าว่างพร้อมกัน

ชื่อสถานะที่เกี่ยวข้อง:

```text
บน Player: CurrentAttacker / IsAttackReserved
บนผี: HasAttackToken
คูลดาวน์รายตัวของผี: CanAttackPlayer หรือ AttackCooldownReady
```

แหล่งข้อมูลจริงควรเป็น `CurrentAttacker` บน Coordinator ส่วน `HasAttackToken` เป็นเพียงผลจาก `IsOwner(enemy)` ไม่ควรมี Boolean ซ้ำสองฝั่งที่อาจไม่ตรงกัน

## ตำแหน่งของระบบ

### Player

ติด `PlayerAttackCoordinator` บน Player เพราะ Attack Slot เป็นทรัพยากรของเป้าหมายที่กำลังถูกรุม ผีทุกตัวต้องขอสิทธิ์จากเป้าหมายเดียวกัน

### Enemy Ka

State Machine ของผีเป็นผู้ขอ ตรวจ และคืนสิทธิ์ โดยเรียก Coordinator บน Player

### ScriptableObject State Machine (SMSO)

SMSO ควรเป็น configuration และตัวเชื่อมไปยัง runtime component เท่านั้น ตัวอย่าง asset/action/condition ที่อาจต้องมี:

```text
TryClaimAttackSlotActionSO
HasAttackSlotConditionSO
ReleaseAttackSlotActionSO
```

ห้ามเก็บ `CurrentAttacker` หรือ runtime mutable state ไว้ใน ScriptableObject asset เพราะ asset เดียวถูกแชร์โดยผีหลาย instance

### GameManager

ยังไม่ต้องใช้ GameManager สำหรับเวอร์ชันแรก ระบบนี้สัมพันธ์กับ Player โดยตรง ค่อยพิจารณาระบบระดับ Encounter หากภายหลังมีหลายเป้าหมาย จำนวน Attack Slot มากกว่าหนึ่ง หรือระบบจัด priority ที่ซับซ้อน

## State Machine Flow ที่ต้องการ

Main State Machine ปัจจุบันมีแนวทาง:

```text
Idle -> Patrol -> Chasing -> Attack (Sub-State Machine)
```

เมื่อ Chasing เข้าถึง Combat/Attack-State Range ให้ลองจอง Attack Slot:

```text
Chasing
  -> TryClaimAttackSlot
     -> สำเร็จ: เข้า Attack Sub-State
     -> ไม่สำเร็จ: WalkAround และลองใหม่ภายหลัง
```

Flow เชิงพฤติกรรมที่ต้องได้:

```text
Chasing
  -> Claim สำเร็จ
  -> ApproachAttack (เดินเข้าหาจนถึง Attack Range)
  -> Strike (โจมตี)
  -> Retreat (ถอยออกจาก Player)
  -> Release Attack Slot
  -> WalkAround/Chasing
```

ผีที่จองไม่สำเร็จ:

```text
Chasing
  -> Claim ไม่สำเร็จ
  -> WalkAround
  -> หน่วงเวลาสุ่มสั้น ๆ
  -> TryClaim อีกครั้ง
```

ไม่ควรลองจองทุกเฟรม เพราะผีที่มีลำดับ Update ได้เปรียบอาจได้สิทธิ์ซ้ำบ่อย แนะนำ retry interval แบบสุ่ม เช่น 0.5-1.5 วินาที หรือใช้ระบบคิวจริงในอนาคต

## Attack Sub-State ที่เสนอ

หากคง `Attack` เป็น Sub-State Machine ให้แยกหน้าที่ชัดเจน:

```text
Enter
  -> ApproachAttack
  -> Strike
  -> Retreat
  -> ReleaseToken
  -> ExitToChasing
```

รายละเอียด:

- `ApproachAttack`: เดินเข้าหา Player จนถึงระยะโจมตีจริง เช่น 1.5 เมตร
- `Strike`: หยุดเดิน หันหา Player และโจมตี
- `Strike -> Retreat`: ใช้ `AnimationFinished` ไม่ใช้ condition ที่คืน `true` ตลอด
- `Retreat`: ถอยออกจนถึงระยะปลอดภัยหรือ timeout
- `ReleaseToken`: คืนสิทธิ์หลัง Retreat เสร็จ เพื่อไม่ให้ผีตัวถัดไปเข้ามาซ้อนระหว่างเจ้าของเดิมยังอยู่ประชิด Player
- `ExitToChasing`: ออกจาก Sub-State กลับ Main `Chasing`

หาก `WalkAround` อยู่ภายนอก Attack Sub-State ให้ Main/Chasing flow เป็นผู้ควบคุมมัน ผีต้องออกจาก WalkAround กลับไป Approach/Chasing ก่อนเสมอ เพื่อเดินเข้าระยะจริงก่อน Strike

## ระยะที่ควรแยกกัน

อย่าใช้ระยะเดียวควบคุมทุกขั้น:

```text
CombatRange / AttackStateRange: ระยะที่เริ่มเข้าระบบล้อมและขอสิทธิ์ เช่น 4-5 เมตร
WalkAroundRadius: ระยะเดินวน เช่น 3-4 เมตร
AttackRange: ระยะที่เริ่ม Strike จริง เช่น 1.5 เมตร
LoseCombatRange: ระยะที่ยกเลิกการต่อสู้และคืนสิทธิ์
```

## คูลดาวน์

คูลดาวน์โจมตีควรเป็น runtime state รายตัวของผี ไม่ใช่สถานะส่วนกลางบน Player:

```text
AttackCooldownReady = true/false
```

Attack Slot และ cooldown ทำหน้าที่ต่างกัน:

- Attack Slot: ป้องกันผีหลายตัวโจมตีพร้อมกัน
- Cooldown: ป้องกันผีตัวเดิมโจมตีถี่เกินไป

เงื่อนไขเข้าตีในภาพรวม:

```text
AttackCooldownReady
AND
TryClaimAttackSlot สำเร็จ
AND
เดินถึง AttackRange
```

ระบบ timer ปัจจุบันตั้ง Flag เป็น `false` ตอนเริ่มนับ และเป็น `true` เมื่อหมดเวลา ดังนั้นชื่ออย่าง `CanAttackPlayer` หรือ `AttackCooldownReady` จะตรงกับ semantics ปัจจุบันมากกว่า `IsAttackOnCooldown`

## Cancellation และการป้องกัน Slot ค้าง

ทุกเส้นทางที่ทำให้การโจมตีไม่จบตามปกติต้อง Release Token:

- Enemy ตายหรือ `OnDisable`
- Enemy ถูก Stun และยกเลิก attack sequence
- Player ตาย
- Player หรือ Enemy พ้น LoseCombatRange
- NavMesh หาเส้นทางไม่ได้หรือ Approach timeout
- Attack animation ถูก interrupt
- เปลี่ยนฉากหรือ reset encounter

ควรทำ cleanup ที่ runtime component ของ Enemy/Coordinator เพิ่มเติม ไม่ควรพึ่งเฉพาะ State Exit เส้นทางเดียว

## ข้อควรตรวจในโปรเจกต์ก่อน Implement

- ตรวจระบบ `StateFlags` และ `StateFlagsAccess/StateFlagReader` ว่าสามารถอ่าน Flag จาก Player ภายนอก StateMachine GameObject ได้อย่างไร
- ตรวจ Anchor ของ Player ที่ State Machine ใช้อยู่แล้ว
- ตรวจ lifecycle ของ Sub-State Machine และ `SubStateMachineExitedCondition`
- ตรวจว่า paired attack system เคลื่อน attacker เข้า snap point อยู่แล้วหรือไม่ เพื่อไม่ให้ `ApproachAttack` และ snap movement ตีกัน
- ตรวจ Action/Condition ที่มีอยู่ก่อนสร้างใหม่ โดยเฉพาะ navigation, animation finished, stop movement และ exit state
- รักษา ScriptableObject ให้เป็น stateless configuration ต่อ enemy instance

## Acceptance Criteria

ถือว่าระบบสำเร็จเมื่อ:

1. มี Ka อย่างน้อย 3 ตัวล้อม Player ได้ แต่มีเพียง 1 ตัวเข้าสู่ Strike ในแต่ละครั้ง
2. ตัวที่ไม่ได้ Token เดินวน ไม่ยืนนิ่งและไม่โจมตี
3. เจ้าของ Token เดินเข้า Attack Range ก่อนโจมตี
4. Token ยังไม่ถูกปล่อยระหว่าง Strike/Retreat
5. เมื่อ Retreat จบ ผีตัวอื่นสามารถรับ Token และโจมตีต่อได้
6. เมื่อเจ้าของ Token ตาย ถูก Disable หรือถูก interrupt Slot ไม่ค้าง
7. ผีตัวเดิมไม่ผูกขาด Token ซ้ำตลอด และมี retry timing ที่ดูเป็นธรรมชาติ
8. ไม่มี runtime state ของศัตรูถูกเก็บใน SMSO asset ที่แชร์ร่วมกัน

## ขอบเขตงาน ณ ตอนเขียนเอกสาร

เอกสารนี้เป็นเพียง design handoff จากการสนทนา ยังไม่ได้อนุญาตหรือดำเนินการแก้ C# Script, Scene, Prefab หรือ ScriptableObject asset ใด ๆ ผู้พัฒนาคนถัดไปควรตรวจโค้ดปัจจุบันและเสนอแผน implementation ก่อนลงมือแก้
