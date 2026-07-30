# Yanta Ghost Art Setup / คู่มือใส่งาน Art ผี

## English

### What This Is

This project currently has two ghost types:

- Tani
- Ka

The AI/state logic is separated from animation and sound. Artists should use `GhostArtController` to assign animation state names and audio clips without editing AI code.

### Main Script For Artists

Add this component to each ghost GameObject:

- `GhostArtController`

Recommended components on the same ghost:

- `Animator`
- `AudioSource`
- `GhostArtController`
- The ghost state machine, such as `TaniStateMachine` or `KaStateMachine`

If `GhostArtController` is missing, the AI will still try to play the old fallback Animator state names.

### GhostArtController Cues

In `GhostArtController > Cues`, add entries for the events you need:

- `Patrol`: normal walking/patrolling.
- `PatrolLookBack`: Tani-only extra cue when Tani looks behind during patrol.
- `Chase`: running/chasing the Player.
- `Attack`: normal attack.
- `AttackJumpscare`: reserved for a later HP/death flow when an attack finishes the Player.
- `Search`: searching after losing sight of the Player.

Each cue can set:

- `Art Event`: which state/event this cue belongs to.
- `Animation State Name`: Animator state to play.
- `Sound`: optional audio clip.
- `Loop Sound`: use this for looping patrol/chase/search sounds.
- `Volume`: per-cue volume.

### Animator Notes

The `Animation State Name` must match the state name inside the Animator Controller.

Suggested names:

- Tani: `Tani-Patrol`, `Tani-Chase`, `Tani-Attack`, `Tani-Search`
- Ka: `Ka-Patrol`, `Ka-Chase`, `Ka-Attack`, `Ka-Search`

You may use different names, but then update the cue's `Animation State Name`.

### Audio Notes

Add an `AudioSource` to the ghost before assigning sound clips.

Use looping sounds for continuous states:

- Patrol ambience/steps
- Chase run/voice
- Search loop

Use one-shot sounds for short moments:

- Attack
- PatrolLookBack
- AttackJumpscare

### Ka Event Test Setup

In `Assets/Scenes/Level/Cave/Cave.unity`, Ka has test trigger objects:

- `Event1`: activates 1 Ka ghost and applies 1.2x movement speed.
- `Event2`: activates 2 Ka ghosts and applies 1.5x movement speed.
- `Event3`: activates 3 Ka ghosts at normal speed.

These objects use:

- `KaEventPublisher`
- Trigger collider

The scene also has:

- `KaEventRunManager` with `KaEvant`

Do not remove `KaEventRunManager`; it listens for Ka event triggers and controls the active Ka ghosts.

### Before Sending Art Work

Checklist:

- The ghost has `GhostArtController`.
- The ghost has an `AudioSource` if sounds are used.
- Every cue has the correct `Art Event`.
- Animator state names match exactly.
- Looping sounds are only used for continuous states.
- Event trigger colliders are still set to `Is Trigger`.
- Play Mode has no missing script or missing reference warnings.

## Thai

### ไฟล์นี้คืออะไร

ตอนนี้โปรเจกต์มีผีหลัก 2 แบบ:

- Tani
- Ka

โค้ด AI/state ถูกแยกออกจาก animation และ sound แล้ว ฝั่ง artist ให้ใช้ `GhostArtController` เพื่อใส่ชื่อ animation และเสียง โดยไม่ต้องแก้โค้ด AI

### Script หลักสำหรับ Artist

ให้ใส่ component นี้บน GameObject ของผีแต่ละตัว:

- `GhostArtController`

component ที่ควรอยู่บนผีตัวเดียวกัน:

- `Animator`
- `AudioSource`
- `GhostArtController`
- state machine ของผี เช่น `TaniStateMachine` หรือ `KaStateMachine`

ถ้ายังไม่ได้ใส่ `GhostArtController` ระบบ AI จะยัง fallback ไปเล่น Animator state name เดิมอยู่

### Cue ใน GhostArtController

ใน `GhostArtController > Cues` ให้เพิ่มรายการตาม event ที่ต้องใช้:

- `Patrol`: เดิน patrol ปกติ
- `PatrolLookBack`: ใช้กับ Tani ตอนเดิน patrol แล้วมองข้างหลัง
- `Chase`: วิ่งไล่ Player
- `Attack`: โจมตีปกติ
- `AttackJumpscare`: เตรียมไว้สำหรับระบบ HP/death ในอนาคต ถ้าโจมตีแล้ว HP Player หมด
- `Search`: ค้นหา Player หลังจากมองไม่เห็น

แต่ละ cue ตั้งค่าได้:

- `Art Event`: event/state ของ cue นี้
- `Animation State Name`: ชื่อ state ใน Animator ที่ต้องการเล่น
- `Sound`: ใส่เสียง ถ้ามี
- `Loop Sound`: ใช้กับเสียงที่ต้องวน loop
- `Volume`: ความดังเฉพาะ cue นั้น

### หมายเหตุเรื่อง Animator

ชื่อ `Animation State Name` ต้องตรงกับชื่อ state ใน Animator Controller ทุกตัวอักษร

ชื่อที่แนะนำ:

- Tani: `Tani-Patrol`, `Tani-Chase`, `Tani-Attack`, `Tani-Search`
- Ka: `Ka-Patrol`, `Ka-Chase`, `Ka-Attack`, `Ka-Search`

ถ้าใช้ชื่ออื่นได้ แต่ต้องแก้ใน cue ของ `GhostArtController` ให้ตรง

### หมายเหตุเรื่องเสียง

ถ้าจะใช้เสียง ให้ใส่ `AudioSource` บนผีด้วย

เสียงที่ควร loop:

- เสียงเดิน patrol
- เสียงวิ่งไล่ chase
- เสียงค้นหา search

เสียงที่ควรเล่นครั้งเดียว:

- Attack
- PatrolLookBack
- AttackJumpscare

### Setup สำหรับทดสอบ Ka Event

ใน scene `Assets/Scenes/Level/Cave/Cave.unity` มี trigger test:

- `Event1`: เปิดผี Ka 1 ตัว และเพิ่มความเร็วเป็น 1.2x
- `Event2`: เปิดผี Ka 2 ตัว และเพิ่มความเร็วเป็น 1.5x
- `Event3`: เปิดผี Ka 3 ตัว ความเร็วปกติ

GameObject เหล่านี้ใช้:

- `KaEventPublisher`
- Collider ที่เปิด `Is Trigger`

ใน scene มี:

- `KaEventRunManager` ที่ติด `KaEvant`

อย่าลบ `KaEventRunManager` เพราะมันเป็นตัวฟัง event แล้วสั่งเปิดผี Ka

### Checklist ก่อนส่งงาน Art

- ผีมี `GhostArtController`
- ถ้าใช้เสียง ผีมี `AudioSource`
- Cue ทุกตัวเลือก `Art Event` ถูกต้อง
- ชื่อ Animator state ตรงทุกตัวอักษร
- เสียง loop ใช้เฉพาะ state ที่ต่อเนื่อง
- Collider ของ Event trigger ยังเปิด `Is Trigger`
- เข้า Play Mode แล้วไม่มี missing script หรือ missing reference warning
