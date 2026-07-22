# AI Code Change Log

Purpose: record script-related changes so future AI work can quickly understand what changed and why.

Rule for future edits:
- Every time a script is changed, add a new entry here.
- Use a clear title, date, touched files, and a short reason/effect summary.
- Keep entries concise and factual.

## 2026-06-26 - Revert Yant Button Test Caster

Touched files:
- `Scripts/Yant/YantCaster.cs`
- `Scripts/Yant/YantTast.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Removed the temporary `YantTast` test caster script.
- Restored `YantCaster.AnalyzeAndCast()` so it runs the normal drawing analysis flow through `TryAnalyzeAndCast()`.
- Removed the temporary UI test cast methods and test binding index from `YantCaster`.

Notes:
- UI buttons wired to `YantCaster.AnalyzeAndCast()` behave like the original setup again and require the drawn shape/matcher result.

## 2026-06-06 - TaniGhostMockup Event Components

Touched files:
- `Prefabs/TaniGhostMockup.prefab`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added `TaniEventListener` to `TaniGhostMockup` so it can subscribe to `TaniRunningEvent` like the scene `TaniGhost` example.
- Added `TaniMoveController3D` to `TaniGhostMockup` so `TaniGhostDriver` can find its movement component.
- Left `targetTransform` and `StartTransform` empty on the prefab because prefab assets cannot safely reference scene-only objects. Assign these references in the scene instance when needed.

Notes:
- No C# script behavior was changed in this entry.

## 2026-06-07 - Tani Enemy AI State Machines

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/TaniStateMachine.cs`
- `Scripts/Event/Tani/PatrolState.cs`
- `Scripts/Event/Tani/ChaseState.cs`
- `Scripts/Event/Tani/AttackState.cs`
- `Scripts/Event/Tani/SearchState.cs`
- `Scripts/Event/Tani/WaitingState.cs`
- `Scripts/Event/Tani/LeapState.cs`
- `Scripts/Event/Tani/PushedState.cs`
- `Scripts/Event/Tani/TaniGhost.cs`

Changes:
- Added a NavMeshAgent-based enemy AI controller for Tani with serialized detection, attack, wait, leap, patrol waypoint, and Yantra references.
- Added a single transition gate through `TaniStateMachine`.
- Added separate states for Patrol, Chase, Attack, Search, Waiting, Leap, and Pushed.
- Added Animator calls using the requested state names: `Tani-Patrol`, `Tani-Chase`, `Tani-Attack`, `Tani-Search`, `Tani-Waiting`, `Tani-Leap`, and `Tani-Pushed`.
- Added OnDrawGizmos range, patrol path, search marker, and active movement debug lines.
- Replaced the empty `TaniGhost.cs` placeholder with `TaniEnemy.cs`.

Notes:
- `TaniGhostDriver.cs` already had local changes in the working tree and was not modified by this entry.
- Animator Controller still needs matching states/clips assigned for every requested animation name.

## 2026-06-07 - Tani Running Prefab Stability

Touched files:
- `Prefabs/TaniGhostMockup.prefab`
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/PatrolState.cs`
- `Scripts/Event/Tani/States/ChaseState.cs`
- `Scripts/Event/Tani/States/SearchState.cs`

Changes:
- Prepared the Tani running prefab to use `TaniEnemy` and `NavMeshAgent`.
- Disabled Animator root motion in code so animation clips cannot lift the whole Tani object upward.
- Disabled Rigidbody gravity and froze X/Z rotation so Tani does not fall over from collisions with Player or other characters.
- Added safe NavMesh destination routing through `TrySetDestination` so states do not throw when the agent is not on a NavMesh yet.

Notes:
- If the scene object is named `TaniGhostRuning`, it should inherit these prefab/script fixes after reimporting the prefab.

## 2026-06-07 - Tani NavMesh Remaining Distance Guard

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/PatrolState.cs`
- `Scripts/Event/Tani/States/SearchState.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added `HasReachedDestination` to centralize safe `NavMeshAgent.remainingDistance` checks.
- Updated Patrol and Search states to stop reading `remainingDistance` directly when Tani is not on a NavMesh.

Notes:
- Fixes the Unity Console error: `GetRemainingDistance can only be called on an active agent that has been placed on a NavMesh`.

## 2026-06-07 - Chase Uses TaniMoveController3D

Touched files:
- `Prefabs/TaniGhostMockup.prefab`
- `Scripts/Tani/TaniMoveController3D.cs`
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/ChaseState.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added runtime target movement support to `TaniMoveController3D`.
- Added `StopMove` so states can cleanly stop Tani-specific movement.
- Updated `ChaseState` to use `TaniMoveController3D` for chasing the Player instead of direct NavMesh destination updates.
- Added `chaseMoveSpeed` to `TaniEnemy` for Inspector tuning.

Notes:
- The original `StartMove(float speed)` behavior remains for existing event use.
- Chase movement disables NavMesh steering while chasing to avoid Rigidbody and NavMeshAgent fighting each other.

## 2026-06-07 - Tani Vision Patrol Search Flow

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/PatrolState.cs`
- `Scripts/Event/Tani/States/ChaseState.cs`
- `Scripts/Event/Tani/States/SearchState.cs`
- `Scripts/Event/Tani/States/AttackState.cs`
- `Scripts/Event/Tani/States/LeapState.cs`
- `Scripts/Event/Tani/States/PushedState.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Re-enabled Rigidbody gravity while keeping X/Z rotation frozen so Tani can fall naturally without tipping over.
- Added line-of-sight checks with `CanSeePlayer` so Tani cannot chase or attack through walls.
- Updated Patrol, Chase, Search, and Attack transitions to use visible-player checks instead of range-only checks.
- Updated Leap and Pushed recovery transitions to return to Chase only when Tani can see the Player.
- Updated last known player position only when Tani can actually see the Player.
- Ignored physics collisions between Tani and the tagged Player to reduce character bumping while keeping ground collision active.

Notes:
- Patrol still walks through `patrolWaypoints` in array order, so assign pos1 through pos5 in the desired Inspector order.

## 2026-06-07 - Remove Tani Rotation Lock

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Prefabs/TaniGhostMockup.prefab`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Removed the runtime Rigidbody X/Z rotation freeze from `TaniEnemy`.
- Reset the Tani prefab Rigidbody constraints to `None`.

Notes:
- Tani can now rotate or fall over naturally from physics again.

## 2026-06-07 - Tani Movement Keeps Ground Height

Touched files:
- `Scripts/Tani/TaniMoveController3D.cs`
- `Scenes/Level 1/Villge/Villge.unity`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Updated `TaniMoveController3D` to move toward targets only on the X/Z plane.
- Kept the Rigidbody's current Y position during MovePosition so target/player height no longer pulls Tani upward or downward.
- Removed remaining scene-level Rigidbody constraint overrides on Tani prefab instances.

Notes:
- This addresses Tani sinking or floating while chasing a target with a different Y position.

## 2026-06-08 - Tani State Spec From Reference

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/TaniStateMachine.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Updated the Tani state flow to match the requested animation/state names: `Tani-Patrol`, `Tani-Chase`, `Tani-Attack`, `Tani-Search`, `Tani-Waiting`, `Tani-Leap`, and `Tani-Pushed`.
- Patrol remains the normal mode when Tani does not see the Player.
- Chase starts when the Player is inside the view cone and line of sight.
- Attack starts when the Player is inside attack range.
- Search moves to the last seen Player position, then scans left and right.
- Waiting starts when the Player collides with or enters Tani's trigger, then Leap fires after the configured wait duration.
- Added `EnterPushedState` entry points so a Yantra hit script can push Tani into the Pushed state.
- Reworked debug Gizmos to draw the vision cone, dashed center/debug lines, attack range, and per-state colors.

Notes:
- Player lookup still uses the required `Player` tag.
- Yantra/projectile logic should call `TaniEnemy.EnterPushedState(...)` when the hit is confirmed.

## 2026-06-08 - Search Rotation Handled By Animation

Touched files:
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Removed code-driven left/right turning from `SearchState`.
- Search now only moves to the last seen Player position, plays `Tani-Search`, waits for the configured search duration, then returns to Patrol unless the Player is seen again.

Notes:
- Left/right scanning should be authored in the Animator or animation clip for the mockup.

## 2026-06-08 - Always Show Tani Debug Gizmos

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Changed Tani debug drawing from selected-only Gizmos to always-visible Gizmos.

Notes:
- The Scene view must still have Gizmos enabled in Unity.

## 2026-06-08 - Prevent NPC Physics Floating

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Disabled Animator root motion at runtime for Tani so animation clips cannot move the root upward.
- Made normal Tani movement kinematic/agent-owned to avoid Rigidbody and NavMeshAgent fighting over the same transform.
- Added explicit physics mode only for Leap and Pushed, then restored agent-owned motion after each recovery duration.
- Skipped missing Animator states before calling `Animator.Play` to avoid repeated console errors while mockup animation states are incomplete.

Notes:
- This should stop NPCs from slowly floating upward when Play Mode starts.

## 2026-06-09 - Chase Faces Player

Touched files:
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Updated `ChaseState` to rotate Tani toward the Player before moving.

Notes:
- The debug vision cone uses Tani's forward direction, so it now follows the chase target direction.

## 2026-06-09 - Smooth Chase Movement

Touched files:
- `Scripts/Tani/TaniMoveController3D.cs`
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Enabled Rigidbody interpolation at runtime for Tani movement.
- Moved chase rotation into `TaniMoveController3D.FixedUpdate` using `Rigidbody.MoveRotation`.
- Started chase movement once on state entry and only refreshed the target during chase ticks.

Notes:
- This keeps chase position and rotation in the same physics step, reducing visible jitter.

## 2026-06-09 - Search Last Seen Delay

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added `searchVisionDelay` so Tani briefly ignores vision immediately after entering Search.
- Search still walks to the last seen Player position and plays `Tani-Search`.
- After the short delay, Search changes back to Chase immediately if Tani sees the Player again.

Notes:
- This reduces rapid Chase/Search flicker when Player briefly leaves the view cone.

## 2026-06-09 - Search Always Moves To Last Known Position

Touched files:
- `Scripts/Tani/TaniMoveController3D.cs`
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added point-target movement to `TaniMoveController3D`.
- Added `searchMoveSpeed` and Search movement fallback through `TaniMoveController3D`.
- Updated Search so it always tries to move to the last seen Player position instead of standing still when NavMesh destination setup fails.
- Search still switches to Chase immediately after the short vision delay if the Player is seen again.

Notes:
- This makes Tani continue investigating narrow corners or player jukes even when the NavMeshAgent cannot immediately route to the last seen point.

## 2026-06-09 - Search Predicts Player Juke Direction

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Stored the Player's last visible movement direction while Tani can see the Player.
- Changed Search to investigate a predicted point past the last seen position in that movement direction.
- Clamped the predicted Search point before walls using raycasts and sampled it back onto the NavMesh when possible.
- Updated Search debug target to show the predicted investigation point instead of only the raw last seen position.

Notes:
- This helps Tani turn toward the direction the Player juked instead of running face-first into the old wall-side position.

## 2026-06-09 - Prevent Search From Crossing Walls

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added body-height wall line checks for Search prediction and fallback movement.
- Search prediction now clamps before walls when the Player's last movement direction points through geometry.
- Search uses a complete NavMesh path check before accepting a predicted investigation point.
- Direct fallback movement is only used when the body line to the target is clear.

Notes:
- This prevents Tani from chasing a predicted point through a wall when the Player cuts across geometry.

## 2026-06-09 - Allow Tani To Contact Player

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Removed the runtime `Physics.IgnoreCollision` between Tani and the tagged Player.
- Added `playerContactRange` as a proximity fallback so Tani can enter Waiting even when Player movement does not send a collision callback to Tani.

Notes:
- Player still must be tagged `Player`.

## 2026-06-09 - Guard Tani Runtime Initialization

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added lazy initialization guards before `Update`, `FixedUpdate`, and `Start` use the state machine.
- Split player lookup and initial Patrol transition into safe helper methods.

Notes:
- Prevents `NullReferenceException` spam if Unity reloads scripts or enters Play Mode with runtime fields reset.

## 2026-06-09 - Search Scan And NavMesh Chase

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Simplified Search to investigate the reachable last known Player position instead of predicting through walls.
- Added code-driven body scanning in Search after Tani reaches the investigation point.
- Changed Chase to use complete NavMesh paths to the Player instead of straight-line fallback movement.

Notes:
- Tani now finds players in alleys by scanning its vision cone, then chases only when NavMesh can route to the Player.

## 2026-06-09 - Keep Chase While Player Is Visible

Touched files:
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Changed Chase so a temporary NavMesh path failure does not immediately bounce Tani back into Search.
- While Tani can still see the Player, it stays in Chase, faces the Player, and waits for a valid NavMesh route.

Notes:
- Tani still does not use straight-line chase movement through walls.

## 2026-06-09 - Chase Uses Reachable Point Near Player

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added `navMeshSampleRadius` for wider NavMesh sampling around the Player.
- Chase now falls back to the closest reachable NavMesh point near the Player when the Player's exact position is not routable.

Notes:
- This keeps Chase moving without using straight-line movement through walls.

## 2026-06-09 - Revert Tani Code To Always Show Gizmos Point

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/Tani/TaniMoveController3D.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Reverted the Tani runtime code to the behavior after `2026-06-08 - Always Show Tani Debug Gizmos`.
- Removed later Search prediction, Search scan, NavMesh-only Chase, lazy initialization, contact fallback, and physics ownership changes.
- Kept always-visible Tani debug Gizmos.

Notes:
- This intentionally restores the earlier Chase/Search behavior for comparison and iteration.

## 2026-06-09 - Prevent Floating After Revert

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Disabled Animator root motion at runtime.
- Made normal Patrol, Chase, Attack, Search, and Waiting movement use a kinematic Rigidbody.
- Enabled dynamic Rigidbody physics only for Leap and Pushed, then restored kinematic mode before returning to Chase.

Notes:
- This keeps the reverted state behavior while preventing Rigidbody, NavMeshAgent, and animation root motion from pushing Tani upward.

## 2026-06-09 - Chase Faces Player

Touched files:
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Updated Chase so Tani faces the Player before moving toward them.

Notes:
- The debug vision cone follows Tani's forward direction, so it now points at the Player during Chase.

## 2026-06-09 - Chase Keeps Pursuing After Seeing Player

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added `chaseLoseSightDuration` so Chase does not instantly stop when Player briefly leaves the view cone.
- Chase now keeps facing and moving toward the Player while the lost-sight timer is below the configured duration.

Notes:
- This makes Chase behave like a pursuit state after Tani has found the Player.

## 2026-06-09 - Chase Releases NavMesh Position Control

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Disabled NavMeshAgent position and rotation updates while Chase uses `TaniMoveController3D`.
- Restored NavMeshAgent updates when custom movement stops.

Notes:
- Prevents a stopped NavMeshAgent from holding Tani in place during Chase.

## 2026-06-09 - Chase Movement Runs In Physics Step

Touched files:
- `Scripts/Tani/TaniMoveController3D.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Restored Rigidbody interpolation for Tani movement.
- Updated `TaniMoveController3D` to rotate toward the moving Player target with `Rigidbody.MoveRotation` in `FixedUpdate`.
- Changed Chase to start target movement once on state entry instead of restarting movement every frame.

Notes:
- This keeps Chase movement and rotation in the physics step so Tani keeps following when the Player moves.

## 2026-06-09 - Clamp Lost Player Point At Detection Edge

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added `RecordPlayerExitRangePosition` to store a last known point on the detection-range edge when the Player runs out of range.
- Updated Chase to record that edge point when vision is lost because the Player left detection range.
- Changed Chase debug target to show the last known point after sight is lost instead of continuing to point at the live Player position.

Notes:
- This makes the Search target/debug point match where Tani should think the Player disappeared.

## 2026-06-09 - Chase Routes Around Obstacles

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added a view-cone check that ignores line-of-sight walls so Chase can notice when the Player is still in range but obstructed.
- Added NavMesh-based chase routing for obstructed targets with a complete path check.
- Updated Chase to switch from straight Rigidbody chasing to NavMesh routing when a wall blocks line of sight.
- Kept manual facing toward the Player during NavMesh chase so the debug cone stays aimed at the target.

Notes:
- Tani should now walk around reachable obstacles while chasing instead of stopping or moving straight through walls.

## 2026-06-09 - Split Tani State Sets And NavMesh-Only Chase

Touched files:
- `Scripts/Event/Tani/TaniStateMachine.cs`
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added `TaniStateSet` to separate normal Run states from Contact/Reaction states.
- Kept Patrol, Chase, Attack, and Search in the Run state set.
- Marked Waiting, Leap, and Pushed as the Contact state set.
- Exposed `CurrentStateSet` from `TaniStateMachine` and `TaniEnemy`.
- Changed Chase to stop straight-line `TaniMoveController3D` movement and use NavMesh routing only.
- Added NavMesh sampling near the Player before Chase sets a destination.

Notes:
- Chase should now route around reachable walls instead of running directly through them.

## 2026-06-09 - Align Chase Search With Quick Fix Prompt

Touched files:
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Updated Chase to request a NavMesh route immediately on state entry.
- Kept direct `TaniMoveController3D` fallback out of Chase so it cannot run straight through walls.
- Simplified Search to set the last-known-position destination on entry and wait for `HasReachedDestination` before timing out.

Notes:
- This matches the quick-fix flow while preserving NavMesh-only Chase movement.

## 2026-06-09 - Restore TaniGhostRuning NavMesh Agent

Touched files:
- `Scenes/Level 1/Villge/Villge.unity`
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Removed the scene override that stripped the prefab `NavMeshAgent` from the `TaniGhostRuning` instance.
- Added one-time Chase warnings for missing/disabled NavMeshAgent, agent not on NavMesh, missing sampled Player NavMesh point, and incomplete chase paths.

Notes:
- `TaniGhostRuning` could not move with NavMesh-only Chase while its `NavMeshAgent` was removed in the scene instance.

## 2026-06-09 - Snap Tani Agent To NavMesh On Start

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added `navMeshSpawnSampleRadius`.
- Added startup NavMesh sampling and `NavMeshAgent.Warp` so Tani can recover when the scene object starts slightly above, below, or beside the baked NavMesh.
- Added warnings when no nearby spawn NavMesh can be found or Warp fails.

Notes:
- This helps `TaniGhostRuning` move after NavMesh-only Chase if its scene position is not exactly on a baked NavMesh surface.

## 2026-06-09 - Remove Generated Tani AI Code For Restart

Touched files:
- `Scripts/Event/Tani/TaniEnemy.cs`
- `Scripts/Event/Tani/TaniStateMachine.cs`
- `Scripts/Event/Tani/States/TaniEnemyStates.cs`
- `Scripts/Tani/TaniMoveController3D.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Deleted the generated `TaniEnemy`, `TaniStateMachine`, and combined Tani state file so the AI can be rebuilt from scratch.
- Removed the generated `States` folder metadata.
- Restored `TaniMoveController3D.cs` back to the tracked project version.

Notes:
- `AI_Code_Change_Log.md` and `AGENTS.md` were intentionally kept.

## 2026-06-09 - Rebuild Tani Patrol State Start

Touched files:
- `Scripts/Event/Tani/Sate/TaniState.cs`
- `Scripts/Event/Tani/Sate/TaniStateMachine.cs`
- `Scripts/Event/Tani/Sate/Patrol.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added a small base state class and a first `TaniStateMachine` controller.
- Replaced the placeholder `Patrol.cs` script with a Patrol state that uses `NavMeshAgent` destinations.
- Patrol walks through the assigned waypoint array in order, then loops back to the first waypoint.

Notes:
- Assign waypoint transforms in the Inspector in the desired patrol order, such as point 1 through point 5.

## 2026-06-09 - Patrol Waypoint Gizmos

Touched files:
- `Scripts/Event/Tani/Sate/TaniStateMachine.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added always-visible patrol Gizmos for assigned waypoints.
- Each waypoint draws a wire sphere, and each point draws a line to the next valid waypoint.
- The last waypoint loops its line back to the first valid waypoint.

Notes:
- Scene view Gizmos must be enabled to see the patrol markers and path lines.

## 2026-06-09 - Add Tani Chase State

Touched files:
- `Scripts/Event/Tani/Sate/TaniState.cs`
- `Scripts/Event/Tani/Sate/TaniStateMachine.cs`
- `Scripts/Event/Tani/Sate/Patrol.cs`
- `Scripts/Event/Tani/Sate/Chase.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added a `Chase` state that ignores patrol waypoints and routes the NavMeshAgent toward the Player.
- Added configurable sight enable, sight range, patrol speed, chase speed, and chase animation state name.
- Patrol now switches to Chase when the Player is inside sight range.
- Chase returns to Patrol when sight is disabled, the Player is missing, or the Player leaves sight range.
- Moved Patrol waypoint debug drawing into the Patrol state and added Chase sight-range debug drawing.

Notes:
- Patrol debug is green and Chase debug is yellow. Attack red and Search orange are reserved for those future states.

## 2026-06-09 - State-Colored Sight Cone

Touched files:
- `Scripts/Event/Tani/Sate/TaniStateMachine.cs`
- `Scripts/Event/Tani/Sate/Patrol.cs`
- `Scripts/Event/Tani/Sate/Chase.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Changed sight detection from a full radius check to a forward-facing cone check.
- Added configurable `sightAngle` and sight cone segment count.
- Debug drawing now shows the active state's sight cone color: Patrol green, Chase yellow.
- Patrol still draws waypoint markers and path lines in green while it is the active/debug state.

Notes:
- The sight cone follows the Tani object's forward direction.

## 2026-06-09 - Add Tani Attack State

Touched files:
- `Scripts/Event/Tani/Sate/TaniStateMachine.cs`
- `Scripts/Event/Tani/Sate/Patrol.cs`
- `Scripts/Event/Tani/Sate/Chase.cs`
- `Scripts/Event/Tani/Sate/Attack.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added an `Attack` state that stops the NavMeshAgent, plays `Tani-Attack`, waits for the configured duration, then returns to Patrol.
- Added configurable attack range and attack duration.
- Patrol and Chase now enter Attack when the Player is inside the attack circle.
- Patrol and Chase debug drawing includes the attack range circle; Attack debug is red.

Notes:
- Expected animation state names are `Tani-Run`, `Tani-Patrol`, `Tani-Chase`, `Tani-Attack`, and `Tani-Search`.

## 2026-06-09 - Add Tani Search State

Touched files:
- `Scripts/Event/Tani/Sate/TaniStateMachine.cs`
- `Scripts/Event/Tani/Sate/Patrol.cs`
- `Scripts/Event/Tani/Sate/Chase.cs`
- `Scripts/Event/Tani/Sate/Search.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added a `Search` state that moves to the last seen Player position after Chase loses sight.
- Patrol and Chase record the Player's last seen position before entering or while staying in Chase.
- Search returns to Chase if it sees the Player while moving to the last seen point.
- After reaching the last seen point, Search stops, plays `Tani-Search`, expands the sight cone using configurable range and angle bonuses, then returns to Patrol when the search duration ends.

Notes:
- Search debug is orange and draws the last seen Player point.

## 2026-06-11 - Patrol Random Look Turns

Touched files:
- `Scripts/Event/Tani/Sate/TaniStateMachine.cs`
- `Scripts/Event/Tani/Sate/Patrol.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added optional random left, right, or back turns when Patrol reaches a waypoint.
- Added Inspector settings for waypoint turn enable, duration, left angle, right angle, and back angle.
- Patrol stops the NavMeshAgent while turning, then continues to the next waypoint.
- Patrol still checks Attack and Chase transitions while turning.

Notes:
- Use `Patrol Turn Duration` to control how long Tani pauses and rotates at each waypoint.

## 2026-06-11 - Patrol Look And Return

Touched files:
- `Scripts/Event/Tani/Sate/TaniStateMachine.cs`
- `Scripts/Event/Tani/Sate/Patrol.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Removed the random back-turn option from Patrol waypoint behavior.
- Patrol now randomly turns left or right, holds that look direction, then turns back to the original direction before moving.
- Added `Patrol Look Hold Duration` and increased the default turn duration.

Notes:
- Patrol still checks Attack and Chase transitions during the turn-out, hold, and turn-back steps.

## 2026-06-11 - Patrol Back Look Direction

Touched files:
- `Scripts/Event/Tani/Sate/TaniStateMachine.cs`
- `Scripts/Event/Tani/Sate/Patrol.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Changed Patrol waypoint looking from left/right side glances to a back-facing look.
- Patrol now randomly chooses whether to rotate toward the back by turning left or turning right.
- Replaced separate left/right turn angle settings with `Patrol Look Back Angle`.

Notes:
- Keep `Patrol Look Back Angle` at 180 for a full behind-the-back look.

## 2026-06-11 - Ka Patrol Ping Pong Waypoints

Touched files:
- `Scripts/Event/Ka/Sate/KaPatrol.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Changed Ka Patrol waypoint movement from looping first-to-last back to first into ping-pong traversal.
- Ka now patrols forward through the waypoint array, reverses at the final waypoint, then walks back toward the first waypoint.
- Updated Ka Patrol Gizmos to draw connected waypoint lines forward and backward without wrapping the final waypoint line back to the first.

Notes:
- With five waypoints, Ka patrols `1 -> 2 -> 3 -> 4 -> 5 -> 4 -> 3 -> 2 -> 1` and repeats.

## 2026-06-11 - Ka Per-Instance Debug Gizmos

Touched files:
- `Scripts/Event/Ka/Sate/KaStateMachine.cs`
- `Scripts/Event/Ka/Sate/KaPatrol.cs`
- `Scripts/Event/Ka/Sate/KaChase.cs`
- `Scripts/Event/Ka/Sate/KaSearch.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added per-instance debug coloring for Ka state Gizmos so multiple Ka ghosts can be distinguished in Scene view.
- Added a small automatic vertical debug offset so overlapping waypoint paths from three Ka ghosts do not draw directly on top of each other.
- Updated Ka Patrol, Chase, Search, sight cone, and attack area debug drawing to use the per-instance color and offset.

Notes:
- Runtime patrol/chase/search logic remains instance-local; this change only separates Scene view debug display.

## 2026-06-12 - Ka Event Ghost Activator

Touched files:
- `Scripts/Event/Ka/Sate/KaEvent.cs`
- `Scripts/Event/Ka/KaEventPublisher.cs`
- `Scripts/Event/Ka/KaEvant.cs`
- `Scripts/Event/Ka/Sate/KaStateMachine.cs`
- `Prefabs/KaGhostMockup.prefab`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Replaced the placeholder `KaEvent` MonoBehaviour with an EventBus payload carrying `Event1`, `Event2`, or `Event3`.
- Added `KaEventPublisher` for trigger objects such as Event1, Event2, and Event3 to publish the selected Ka event when the Player enters.
- Updated `KaEvant` into an EventBus listener/manager that activates the first one, two, or three assigned Ka ghosts.
- Event1 activates one ghost at 1.2x movement speed, Event2 activates two ghosts at 1.5x movement speed, and Event3 activates three ghosts at normal movement speed.
- Added movement speed multiplier support to `KaStateMachine`.
- Removed `KaEvant` from `KaGhostMockup` because the listener should live on a scene manager object that references the three ghost instances.

Notes:
- Add `KaEventPublisher` to the Event1/Event2/Event3 trigger GameObjects and set their `Event Level` in the Inspector.
- Add `KaEvant` to a scene manager object and assign the three Ka ghost GameObjects in the intended activation order.

## 2026-06-12 - Rename Ka Run Event Payload

Touched files:
- `Scripts/Event/Ka/Sate/KaEvent.cs`
- `Scripts/Event/Ka/KaEventPublisher.cs`
- `Scripts/Event/Ka/KaEvant.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Renamed the Ka EventBus payload from `KaEvent` to `KaEventRun` so it reads like the existing `TaniRunningEvent` pattern.
- Added `KaEventRunName` and stored the selected event name in the payload as `EventName`.
- Updated `KaEventPublisher` to publish `KaEventRun` with `Event1`, `Event2`, or `Event3`.
- Updated `KaEvant` to subscribe to `KaEventRun` and branch from the event name.

Notes:
- `KaEventPublisher` still belongs on the Event1/Event2/Event3 trigger GameObjects.

## 2026-06-12 - Wire Ka Event Run Scene Flow

Touched files:
- `Scripts/Event/Ka/KaEvant.cs`
- `Scenes/Level/Cave/Cave.unity`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Updated `KaEvant` to auto-find inactive and active `KaStateMachine` instances when no ghost list is assigned.
- Made `KaEvant` subscribe safely even if the EventBus becomes available after `OnEnable`.
- Restored Event3 speed multiplier to normal speed.
- Wired Cave scene Event1, Event2, and Event3 trigger objects to publish `KaEventRun`.
- Added a scene `KaEventRunManager` object with `KaEvant` attached.

Notes:
- Event trigger colliders must remain triggers for `OnTriggerEnter` to publish events.

## 2026-06-12 - Shared Ghost Art Controller

Touched files:
- `Scripts/Event/GhostArtController.cs`
- `Scripts/Event/Ka/Sate/KaStateMachine.cs`
- `Scripts/Event/Tani/Sate/TaniStateMachine.cs`
- `Scripts/Event/Tani/Sate/Patrol.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added a shared `GhostArtController` for Tani and Ka animation/sound cues.
- Added configurable art events for Patrol, PatrolLookBack, Chase, Attack, AttackJumpscare, and Search.
- Updated Ka and Tani state machines to route state animation calls through `GhostArtController` when present, with existing Animator state names as fallback.
- Added a Tani PatrolLookBack cue call when Tani starts the waypoint back-look behavior.
- Added placeholder attack jumpscare entry points for future HP/death integration.

Notes:
- Artists can add `GhostArtController` and an `AudioSource` to each ghost, then assign animation state names and sound clips per cue without editing AI logic.

## 2026-07-16 - Procedural Tree Generator Tool (LOD + Wind Data)

Touched files:
- `Scripts/Tool/TreeGenerator/MinMaxRangeAttribute.cs` (new)
- `Scripts/Tool/TreeGenerator/TreeRanges.cs` (new)
- `Scripts/Tool/TreeGenerator/ProceduralTreeSettings.cs` (new)
- `Scripts/Tool/TreeGenerator/TreeSkeleton.cs` (new)
- `Scripts/Tool/TreeGenerator/TreeMeshBuilder.cs` (new)
- `Scripts/Tool/TreeGenerator/ProceduralTree.cs` (new)
- `Scripts/Tool/TreeGenerator/Editor/MinMaxRangeDrawer.cs` (new)
- `Scripts/Tool/TreeGenerator/Editor/ProceduralTreeEditor.cs` (new)
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added a procedural tree generator tool (namespace `TreeTool`) for HDRP, inspired by Unity's Tree Creator.
- `ProceduralTree` component ([ExecuteAlways]) rebuilds the tree instantly in edit mode whenever an inspector value changes.
- Three independent seeds: master (trunk/structure), branch seed and leaf seed - each reshuffles only its own part.
- Min-max range sliders ([MinMaxRange] + FloatRange/IntRange) for height, radius, branch count/angle/length, leaf count/size, etc.
- Configurable branch levels list (trunk -> main branches -> twigs -> ...), leaf shapes (Quad/Cross/TripleCross).
- Generates one mesh per LOD level (radial resolution, max branch level, leaf density per LOD) and auto-configures a LODGroup with cross-fade.
- Wind data baked into vertex colors for a future HDRP wind shader (R = main bend, G = branch sway, B = leaf flutter mask, A = phase).
- Editor: seed dice buttons, per-LOD vertex/triangle/leaf stats, "Export Meshes To Assets" (freezes meshes into `Assets/GeneratedTrees/<name>/` for prefabs), and a `GameObject > 3D Object > Procedural Tree (Tool)` menu item.

Notes:
- Generated meshes are HideFlags.DontSave and regenerate on scene load, keeping scene files small; export meshes before prefabbing a final tree.
- Placeholder HDRP/Lit materials are used until bark/leaf materials are assigned.

## 2026-07-16 - Tree Tool: Custom Prefab Geometry Mode

Touched files:
- `Scripts/Tool/TreeGenerator/ProceduralTreeSettings.cs`
- `Scripts/Tool/TreeGenerator/TreeSkeleton.cs`
- `Scripts/Tool/TreeGenerator/TreeMeshBuilder.cs`
- `Scripts/Tool/TreeGenerator/Editor/ProceduralTreeEditor.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added `GeometrySourceSettings` with per-part source (Procedural / Prefabs) for trunk, branches and leaves.
- Each part accepts a list of mesh prefabs (FBX); one variant is picked per trunk/branch/leaf using the existing seed streams (`variantRoll` on Branch/Leaf), so variant selection is deterministic and reshuffles with the same seed buttons.
- Trunk/branch prefabs (modeled along +Y, pivot at base) are bent along the generated branch splines and scaled so the mesh base matches the spline radius (taper/root flare still apply).
- Leaf prefabs (pivot at attach point, +Z outward, +Y normal) are placed at leaf positions and scaled by Leaf Size.
- Custom geometry is baked into the same bark/leaf submeshes, so LOD levels, leaf density thinning, wind vertex colors and materials keep working unchanged.

Notes:
- Custom meshes ignore the per-LOD radial resolution (their vertex count is fixed); LOD branch-level culling and leaf density still reduce cost.
- Adding a prefab variantRoll consumes one extra random value per branch/leaf, so trees generated before this change get a slightly different (still deterministic) layout for the same seed.

## 2026-07-17 - Material Converter Tool (URP <-> HDRP Textures & Materials)

Touched files:
- `Scripts/Tool/MaterialConverter/Editor/TextureNameParser.cs` (new)
- `Scripts/Tool/MaterialConverter/Editor/TexturePixelReader.cs` (new)
- `Scripts/Tool/MaterialConverter/Editor/TexturePacker.cs` (new)
- `Scripts/Tool/MaterialConverter/Editor/MaterialConverterCore.cs` (new)
- `Scripts/Tool/MaterialConverter/Editor/MaterialConverterMenu.cs` (new)
- `Scripts/Tool/MaterialConverter/MaterialConverter_Manual.md` (new)
- `Scripts/Tool/MaterialConverter/MaterialConverter_Manual.pdf` (new)
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added the "Material Converter" editor tool (namespace `MaterialConverterTool`): right-click selected textures/materials in the Project window under `Assets > Material Converter`.
- 8 commands in 4 categories: Create Texture (pack loose maps into an HDRP Mask Map or URP MetallicSmoothness), Convert Texture (repack HDRP<->URP packed maps), Convert Material (HDRP/Lit <-> URP/Lit with full property/texture transfer, creates `<name>_URP.mat` / `<name>_HDRP.mat` copies), Create Material (textures -> ready material per object).
- Smart grouping: texture names are split into object name + map suffix (albedo/normal/metallic/roughness/ao/height/emission/mask etc., resolution/OGL/DX noise tokens ignored), so selecting e.g. 12 maps of 3 objects creates exactly 3 materials named after the objects.
- Supports PNG and EXR sources; roughness is auto-inverted to smoothness; non-readable/compressed textures are read via temporary import-setting switch and restored; packed outputs are linear (sRGB off) PNGs; re-running updates existing outputs instead of duplicating.

Notes:
- URP-material commands require the Universal RP package (tool shows a warning otherwise); texture commands work without it.
- Metallic workflow only; advanced surface options (transparency, detail maps, coat) are not transferred.

## 2026-07-17 - Tree Tool v1.1 + Material Converter HDMaterial Fix

Touched files:
- `Scripts/Tool/MaterialConverter/Editor/MaterialConverterCore.cs`
- `Scripts/Tool/TreeGenerator/ProceduralTreeSettings.cs`
- `Scripts/Tool/TreeGenerator/TreeSkeleton.cs`
- `Scripts/Tool/TreeGenerator/TreeMeshBuilder.cs`
- `Scripts/Tool/TreeGenerator/ProceduralTree.cs`
- `Scripts/Tool/TreeGenerator/Editor/ProceduralTreeEditor.cs`
- `Scripts/Tool/TreeGenerator/TreeGenerator_Manual.md` / `.pdf` (new)
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Fixed CS0234: `HDMaterial` moved to `UnityEngine.Rendering.HighDefinition` in HDRP 17 (was referenced as `UnityEditor...`).
- Tree tool ranges widened (defaults unchanged): trunk height to 100m / radius 10m, branch count to 100 / length ratio to 3x / radius ratio to 2x (can exceed parent, clamp removed) / segments 32, leaves to 300 per branch / size 10m, radial segments 48.
- Leaves: `surfaceOffset` is now a random range (-2..+5 m), added `rotationOffset` (Euler XYZ) applied before the random tumble.
- Leaf cards re-pivoted: the bottom-left texture corner (UV 0,0 = stem) is glued to the branch, and cards are spun -45 deg so the texture diagonal points away from the branch (author leaf textures stem bottom-left, tip top-right).
- LODGroup two-way sync: transition heights edited directly on the LODGroup are adopted back into tool settings via `AdoptLODGroupOverrides()` (called from the inspector and at rebuild start).
- Inspector now hides prefab lists in Procedural mode and procedural-only fields (leaf shape, radial/UV settings) in Prefabs mode.
- Interactive performance: slider drags rebuild only LOD0 without tangents (meshes are reused via Mesh.Clear, prefab extraction cached, OnValidate suppressed while the editor drives); a full rebuild (all LODs + tangents + LODGroup) runs 0.35s after the last change.
- Manual updated to v1.1 and exported as `TreeGenerator_Manual.pdf` in the tool folder.

Notes:
- The extra random draws for leaf offset mean the same seed produces a slightly different (still deterministic) leaf layout than v1.0.
- Old `surfaceOffset` float values reset to the new range default when scenes deserialize.

## 2026-07-19 - Tree Tool v1.2: Smooth Branch Joints, Per-Part Sizing & Radial Segments

Touched files:
- `Scripts/Tool/TreeGenerator/ProceduralTreeSettings.cs`
- `Scripts/Tool/TreeGenerator/TreeSkeleton.cs`
- `Scripts/Tool/TreeGenerator/TreeMeshBuilder.cs`
- `Scripts/Tool/TreeGenerator/ProceduralTree.cs`
- `Scripts/Tool/TreeGenerator/Editor/ProceduralTreeEditor.cs`
- `Scripts/Tool/TreeGenerator/TreeGenerator_Manual.md` / `.pdf`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Branch joints: new per-level `jointSmoothing` (branch starts along the parent's direction and curves smoothly into its own over the first fraction of its length, via RotateTowards per step in Grow) and `jointFlare` (base thickness boost fading over the joint). Child branches no longer visibly "float" at attach points.
- Per-part sizing: new per-level `thicknessScale` multiplies only that level's radius; deeper levels inherit it naturally because Radius Ratio measures against the parent's real radius (hierarchical propagation, not whole-tree scaling).
- Per-part radial segments: `radialSegments` moved from MeshSettings to TrunkSettings (3-64) and each BranchLevelSettings (3-48); `radialDecayPerLevel` removed. Leaves have no radial setting (cards/prefabs). Defaults preserve the old look (8 / 6 / 4).
- Editor: trunk radial segments hidden in trunk-Prefabs mode; mesh section reduced to barkUVTiling + generateTangents.
- Syntax modernized to the newest C# Unity 6 compiles (C# 9): target-typed `new()`, switch expressions, compound quaternion assignment. Unity-null-safe comparisons kept explicit (`!= null`) to avoid IDE Unity-object warnings. No behavior/performance change intended.
- Manual updated to v1.2 (md + PDF in the tool folder).

Notes:
- Unity 6000.x compiles C# 9, not C# 10 - C#10-only syntax (file-scoped namespaces, etc.) would fail to compile and was deliberately not used.
- Old serialized values for the removed MeshSettings radial fields are dropped; new per-part defaults apply.

## 2026-07-19 - Tree Tool v1.3: Working Wind System Driven By Unity's Real WindZone

Touched files:
- `Scripts/Tool/TreeGenerator/ProceduralTreeSettings.cs`
- `Scripts/Tool/TreeGenerator/TreeMeshBuilder.cs`
- `Scripts/Tool/TreeGenerator/TreeWindZoneDriver.cs` (new)
- `Scripts/Tool/TreeGenerator/TreeWind.hlsl` (new)
- `Scripts/Tool/TreeGenerator/Editor/ProceduralTreeEditor.cs`
- `Scripts/Tool/TreeGenerator/TreeGenerator_Manual.md` / `.pdf`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added `TreeWindZoneDriver` ([ExecuteAlways] MonoBehaviour): finds the first enabled `WindZone` in the scene (or a manually assigned one), computes strength/pulse the same way Unity's WindZone does, and pushes direction/strength/turbulence/time as global shader properties (`_TreeWindDirection`, `_TreeWindStrength`, `_TreeWindTurbulence`, `_TreeWindTime`) every frame. This is the real Unity WindZone component, not a custom manual-slider system - Unity's actual SpeedTree wind pipeline is a closed proprietary vertex format arbitrary meshes can't plug into, so this reads the same WindZone data instead and applies our own vertex math.
- Added `TreeWind.hlsl`: a Shader Graph Custom Function (`TreeWindDisplacement_float`) implementing 3-tier wind (main bend, local branch sway, leaf flutter) plus per-vertex turbulence jitter, driven by the global properties above and the baked vertex-color weights.
- Per-part wind authoring at tree-creation time (exactly what was asked - "set each part how much it sways"): new `TrunkSettings.windResponse` (default 0.25, rigid), new `BranchLevelSettings.windResponse` per branch level (main branches 1.0, twigs 1.8 by default), new `LeafSettings.windFlutterResponse` (default 1.5). These multiply directly into the R/G (bark) and R/G/B (leaf) vertex-color wind weights in `TreeMeshBuilder`, so response propagates hierarchically the same way size/thickness does (v1.2) rather than being a single global knob.
- Leaves now inherit their host branch's response for the local-sway (G) channel via `leaf.branchLevel` lookup; leaf flutter (B) is independent and comes from `Leaves.windFlutterResponse`.
- Editor: `ProceduralTree` inspector shows a HelpBox + "Add Wind Zone To Scene" button under the Wind section when no `TreeWindZoneDriver` exists, one-click-creating a configured `WindZone` + driver.
- Manual rewritten: section 8 changed from "future wind data" to a full working-system guide (enable steps, per-part response table, exact Custom Function node wiring for Shader Graph, limitations). No ready-made `.shadergraph` asset is shipped (hand-authoring raw ShaderGraph JSON is fragile/risky), so wiring one Custom Function node is still a manual one-time step.

Notes:
- Existing trees keep their old vertex-color values until rebuilt (Rebuild Now / any setting change) - the new response multipliers only apply on the next bake.
- Wind only visibly moves once a wind-aware Shader Graph material (per section 8) is assigned; the vertex weights alone do nothing without it.

## 2026-07-19 - Tree Tool v1.3.1: Fix Shader Graph Wind Wiring Docs

Touched files:
- `Scripts/Tool/TreeGenerator/TreeGenerator_Manual.md` / `.pdf`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Fixed the Custom Function node instructions in section 8: the Name field must be `TreeWindDisplacement` (no `_float` suffix) - Shader Graph appends `_float`/`_half` itself based on graph precision, so including it caused an `undeclared identifier ..._float_float` compile error users were hitting.
- Corrected the node wiring: the final Add node must combine an Object-Space Position node with the wind offset transformed World->Object via a Transform (Direction) node, not the raw Vertex Color output (an easy mix-up since both are small similarly-shaped nodes in the graph, but Vertex Color holds wind weights, not position).
- No C# code changed; `TreeWindZoneDriver.cs` was reconciled between the worktree and the live project (both now match the live project's formatting - braces on `foreach`, `gameObject.activeInHierarchy` check - no behavior change).

## 2026-07-19 - Tree Tool v1.3.2: PBR Fragment Stage + HDRP/URP Texture Format Switch

Touched files:
- `Scripts/Tool/TreeGenerator/Editor/TreeWindShaderGUI.cs` (new)
- `Scripts/Tool/TreeGenerator/TreeGenerator_Manual.md` / `.pdf`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added `TreeWindShaderGUI` (ShaderGUI subclass, standalone - does not inherit any internal HDRP editor class, so it has no dependency on HDRP-version-specific internal APIs and is safe to compile across HDRP updates). Assigned via a Shader Graph's Graph Settings -> Custom Editor GUI as `TreeTool.EditorTools.TreeWindShaderGUI`.
- The GUI adds a "Format" dropdown (HDRP Mask Map vs URP Metallic/Occlusion maps) backed by an Enum Keyword (`_MapFormat`, Shader Feature) the artist adds on the Shader Graph Blackboard. Selecting a format hides the other format's texture slots in the Inspector AND compiles that branch out of the material's shader variant (Shader Feature, not a runtime branch) - the unselected format's map is genuinely not sampled, not just hidden.
- Material stays a full HDRP Lit target throughout; the dropdown only controls which texture packing convention (HDRP Mask Map R/G/B/A vs URP separate Metallic Gloss Map + Occlusion Map) the artist is feeding in - matches the packing conventions the existing Material Converter Tool already produces.
- Manual section 8 rewritten with a full Fragment-stage wiring guide (Base Color, Normal Map, Tiling, and the 3 Keyword nodes for Metallic/AO/Smoothness) on top of the existing Vertex (wind) wiring, plus the exact property Reference names the ShaderGUI script expects.
- Documented trade-off: assigning a fully custom Editor GUI replaces HDRP's own Shader Graph inspector chrome (Surface/Advanced Options foldouts) - acceptable since this GUI draws the needed fields plus a generic fallback for any other exposed properties, and the manual notes the Custom Editor GUI field can be left blank if the artist prefers HDRP's default inspector (format switching still works via the auto-generated keyword dropdown, just without the field-hiding).
- Deliberately did NOT subclass HDRP's internal Shader-Graph-specific GUI class (e.g. HDLitGUI) to preserve its built-in surface/keyword validation - that class's exact name/API is version-fragile in a way that already bit this project once (the HDMaterial namespace move), and a second broken compile was not worth the risk for a cosmetic feature.

Notes:
- User had independently reorganized the TreeGenerator folder into Editor/, Manual/, Material/, and Scripts/ subfolders in their live Unity project (via the Editor, with matching .meta files) since the previous sync - verified all code/doc content matched the worktree before this update and placed new/updated files into the corresponding existing subfolders rather than creating a new flat layout.
- User already has an in-progress Shader Graph asset (`Material/Tree-HDRP-Lit-PBR.shadergraph`) built from the v1.3.1 Vertex/wind instructions - it still needs the new Fragment-stage (texture + format dropdown) wiring from this update applied by hand in the Shader Graph editor.

## 2026-07-19 - Tree Tool v1.3.3: Detail Map, Height Map, Emission

Touched files:
- `Scripts/Tool/TreeGenerator/Editor/TreeWindShaderGUI.cs`
- `Scripts/Tool/TreeGenerator/TreeGenerator_Manual.md` / `.pdf`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Extended `TreeWindShaderGUI` with three new always-visible sections (not gated by the HDRP/URP dropdown, since these three are packed the same way regardless of pipeline): Detail (`_DetailMap`, `_DetailMask`, `_DetailTiling`, `_DetailAlbedoStrength`, `_DetailNormalStrength`, `_DetailSmoothnessStrength`), Height (`_HeightMap`, `_HeightScale`), Emission (`_EmissionMap`, `_EmissionColor`).
- Manual section 8 gained matching wiring instructions:
  - Detail Map: custom-defined packing (R=albedo overlay, G=normal Y, B=smoothness overlay, A=normal X, matching alpha/green/red/blue is our own convention, not a literal reuse of HDRP's internal LitData.hlsl decode - see note below) blended via a Normal Blend node and additive overlay math into the Fragment stage's Base Color/Normal/Smoothness from v1.3.2.
  - Height Map: implemented as vertex displacement along the object-space normal (added as a further Add node chained after the existing wind-offset Add), not Parallax/Pixel Occlusion Mapping - explicitly called out as a deliberate choice, since POM's per-pixel ray marching is disproportionately expensive for foliage with its very high vertex/instance counts. Requires "Sample Texture 2D LOD" (not the regular Sample Texture 2D node) since vertex shaders have no mip derivatives.
  - Emission: straightforward Emission Map x Emission Color (HDR) into the Fragment Emission slot.

Notes:
- Did not attempt to byte-match HDRP's internal built-in Detail Map channel packing (undocumented/version-fragile); defined our own explicit convention instead, documented in both the manual and the GUI script's XML doc comment.

## 2026-07-19 - Tree Tool v1.3.4: Per-Map Adjustment Parameters + Recommended Defaults

Touched files:
- `Scripts/Tool/TreeGenerator/Editor/TreeWindShaderGUI.cs`
- `Scripts/Tool/TreeGenerator/TreeGenerator_Manual.md` / `.pdf`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Added intensity/remap controls matching real HDRP Material behavior: `_MetallicScale` (straight multiplier), `_SmoothnessRemapMin`/`_SmoothnessRemapMax` and `_AORemapMin`/`_AORemapMax` (min-max remap, named and behaving like HDRP's own "Smoothness Remapping" / "Ambient Occlusion Remapping" fields), `_EmissionIntensity` (separate nits multiplier from `_EmissionColor`, mirroring how HDRP splits emission color from intensity).
- `TreeWindShaderGUI` gained a `DrawMinMaxIfPresent` helper that renders an `EditorGUILayout.MinMaxSlider` with editable min/max number fields for the two remap pairs, always visible (format-independent - the remap applies after the HDRP/URP Keyword branch already resolved a single Metallic/AO/Smoothness value, so one remap works regardless of which texture format is selected).
- Manual section 8 wiring diagram updated to insert the new multiply/Lerp nodes between the existing Keyword-node outputs and the Fragment Metallic/Ambient Occlusion/Smoothness slots.
- Added a new consolidated "Recommended Defaults" table (all PBR + wind properties in one place) with concrete starting values and short rationale for each, so a from-scratch material looks reasonable immediately instead of needing trial and error - e.g. Smoothness Remap 0.0-0.6 (bark/leaves shouldn't look plastic-glossy at 1.0), Height Scale 1-2cm (subtle bark relief, not exaggerated bumps), Emission off by default (0,0,0 color, 0 intensity).

## 2026-07-20 - Tree Tool v1.3.5: ShaderGUI Property Names Matched To Actual Graph

Touched files:
- `Scripts/Tool/TreeGenerator/Editor/TreeWindShaderGUI.cs`
- `Scripts/Tool/TreeGenerator/TreeGenerator_Manual.md` / `.pdf`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Audited the user's actual `Material/Tree-HDRP-Lit-PBR.shadergraph` by parsing its JSON directly (it's a sequence of concatenated JSON objects, not one array/document - wrote a one-off Node.js parser + brace-matching splitter, then traced every SurfaceDescription/VertexDescription block backward through the edge graph to its property sources) rather than relying on screenshots.
- Findings: the graph's wiring is correct (Custom Function -> TreeWind.hlsl GUID matches exactly, Vertex Position chain including height displacement is correct, HDRP/URP Keyword branches route to the right textures) but several properties were named differently than the manual originally proposed: `_AmbientOcclusionMap` (not `_OcclusionMap`), `_SmoothnessMinScale`/`_SmoothnessMaxScale` (not `_SmoothnessRemapMin/Max`), `_AmbientOcclusionMinScale`/`_AmbientOcclusionMaxScale` (not `_AORemapMin/Max`), and the Map Format keyword's Reference is `MAPFORMAT` with no leading underscore (not `_MapFormat`), which also means the compiled Shader Feature keywords are `MAPFORMAT_HDRP`/`MAPFORMAT_URP` with no leading underscore.
- User chose to keep the graph's existing names and have the script adapt, rather than rename the graph - `TreeWindShaderGUI.cs` constants and the manual's property tables were updated to match the graph exactly.
- Also flagged (not part of this fix, left for the user to correct in the Shader Graph editor themselves since it requires wiring, not scripting): `_BaseColor` default is black (multiplies BaseMap to black), `_NormalScale` default is 0 (disables normal mapping), `_DetailTiling`/`_DetailOffset` and `_EmissionColor` are declared but never wired into the graph.

Notes:
- Parsing a `.shadergraph` file directly (vs. reading screenshots) turned out to be a reliable, low-effort way to verify Shader Graph wiring - useful precedent if this comes up again for other shader graphs in the project.

## 2026-07-20 - Tree Tool v1.3.6: Fix Obsolete API Blocking TreeWindShaderGUI

Touched files:
- `Scripts/Tool/TreeGenerator/Editor/TreeWindShaderGUI.cs`
- `Scripts/AI_Code_Change_Log.md`

Changes:
- Fixed `CS0618: 'MaterialProperty.PropFlags' is obsolete` by switching to `UnityEngine.Rendering.ShaderPropertyFlags.HideInInspector` in `DrawRemaining()`. In this Unity version the old nested enum is deprecated in favor of the new type; the user's Inspector was showing Shader Graph's built-in default material GUI instead of `TreeWindShaderGUI` (no section headers, no Format Popup/HelpBox, all properties flat in Blackboard declaration order), suggesting either this warning escalated to a build-blocking issue for the Editor assembly or the Custom Editor GUI field was never set - the API fix rules out the former as a cause.

## 2026-07-20 - Tree Tool v1.3.6 correction: Revert ShaderPropertyFlags Change (Broke Compile)

Touched files:
- `Scripts/Tool/TreeGenerator/Editor/TreeWindShaderGUI.cs`

Changes:
- The v1.3.6 fix (switching `p.flags` comparison to `UnityEngine.Rendering.ShaderPropertyFlags.HideInInspector`) was wrong and caused `CS0019: Operator '&' cannot be applied to operands of type 'MaterialProperty.PropFlags' and 'ShaderPropertyFlags'` - on this Unity/Editor version, `MaterialProperty.flags` still actually returns `MaterialProperty.PropFlags`, even though the CS0618 obsolete warning suggests the newer type name for future compatibility. Reverted to `MaterialProperty.PropFlags.HideInInspector`, wrapped in `#pragma warning disable/restore CS0618` so the (harmless, non-blocking) obsolete warning doesn't reappear.
- Root cause of the user's original "TreeWindShaderGUI isn't being used, Inspector shows Shader Graph's stock property list" symptom was this CS0618 warning being misdiagnosed as a possible compile blocker; the real compile error only appeared after the incorrect v1.3.6 fix. The Library/Bee-cache "Script Updater" crash reported separately by the user was an unrelated, coincidental Unity Editor infrastructure issue (Mono.Cecil crash resolving an empty type name), not caused by this file.

## 2026-07-20 - Tree Tool v1.3.7: Restore Surface Options Section In Custom Inspector

Touched files:
- `Scripts/Tool/TreeGenerator/Editor/TreeWindShaderGUI.cs`

Changes:
- User set the Shader Graph's Custom Editor GUI field (was previously empty, causing the stock Shader Graph inspector to render instead of ours - explains the earlier "URP fields still show with HDRP selected" report). Once set, a new problem appeared: the whole "Surface Options" block (Surface Type, Alpha Clipping, Double-Sided, Material Type, Receive Decals, Receive SSR, etc.) that HDRP/Lit's stock inspector shows had vanished, because those properties are marked `[HideInInspector]` on HDRP Shader Graph shaders (HDRP's own custom GUI draws them through a dedicated UI block system, not generic property enumeration) - our `DrawRemaining()` fallback skips anything flagged `HideInInspector`, so none of them were ever drawn.
- Added `DrawSurfaceOptionsSection()`: explicitly looks up and draws HDRP's well-known Surface Options property set by exact name (`_SurfaceType`, `_BlendMode`, `_CullMode`/`_TransparentCullMode`, `_AlphaCutoffEnable`/`_AlphaCutoff`, `_DoubleSidedEnable`/`_DoubleSidedNormalMode`/`_DoubleSidedGIMode`, `_MaterialID`, `_SupportDecals`, `_ReceivesSSR`/`_ReceivesSSRTransparent`, `_EnableGeometricSpecularAA`, plus several Transparent-only fields shown conditionally). These are the same property names HDRP's own hand-written Lit shader and every HDRP Shader Graph target use internally (referenced throughout HDRP's public scripting surface, e.g. `HDMaterial`/`HDShaderUtils`), so they're stable across HDRP versions even though hidden from generic enumeration - calling `MaterialEditor.ShaderProperty()` explicitly on a `[HideInInspector]` property still draws its attached Toggle/Enum control correctly, it just isn't auto-enumerated.
- Section is placed first (matches stock HDRP/Lit layout - Surface Options at the top) and only renders at all if `_SurfaceType` exists on the material, so this degrades safely (no-op) for any future shader that doesn't use this GUI's expected HDRP surface-option convention.
- Known limitation (documented in code comment, not fixed): HDRP's own built-in inspector calls `HDShaderUtils.ResetMaterialKeywords()` after Surface Option changes to keep render queue/blend state/keyword combinations fully in sync; this custom GUI does not call that (deliberately avoided referencing another HDRP-internal API after already hitting two compile-error round trips this session) - values set through these controls are stored correctly, but if a change doesn't visually take effect immediately, reselecting the material or the shader once should force a resync.

## 2026-07-20 - Tree Tool v1.3.8: Proper Labels On Surface Options Fields

Touched files:
- `Scripts/Tool/TreeGenerator/Editor/TreeWindShaderGUI.cs`

Changes:
- The Surface Options section added in v1.3.7 rendered with useless generic labels ("Float", "Boolean", or the raw reference name for a couple of fields) because these HDRP-generated properties have a blank/placeholder display name on the shader itself - HDRP's stock inspector supplies the real label text in its own C# code rather than relying on the shader-declared name.
- Added `DrawLabeled()` (alongside the existing `DrawIfPresent()`) taking an explicit label string, and wired every Surface Options field to the same label text HDRP/Lit's stock inspector uses ("Surface Type", "Rendering Pass", "Blending Mode", "Alpha Clipping", "Threshold", "Double-Sided", "Material Type", "Receive Decals", "Receive SSR", etc.) so the section now visually matches the reference HDRP/Lit inspector layout.

## 2026-07-20 - Tree Tool v1.3.9: Collapsible Sections + Real Surface Type Dropdown

Touched files:
- `Scripts/Tool/TreeGenerator/Editor/TreeWindShaderGUI.cs`

Changes:
- Every section (Surface Options, Base, Normal, Texture Source, Detail, Height, Emission, Other) now uses `EditorGUILayout.BeginFoldoutHeaderGroup`/`EndFoldoutHeaderGroup` instead of a plain bold label, matching HDRP/Lit's stock collapsible-section look exactly. Fold state is stored in instance fields on the `TreeWindShaderGUI` object (Surface Options/Base/Normal/Texture Source expanded by default, Detail/Height/Emission collapsed by default, mirroring how HDRP/Lit starts).
- `_SurfaceType` now renders as a real "Opaque"/"Transparent" `Popup` instead of a raw float field - safe to hand-roll since that 0/1 mapping is used identically everywhere in HDRP (already relied on it elsewhere in this same file).
- `_RenderQueueType` ("Rendering Pass") deliberately left as a plain number field rather than faking a dropdown: HDRP maps its dropdown labels to actual render-queue integers through internal `HDRenderQueue` logic that isn't safe to guess at, and a wrong mapping could silently misroute the material's render pass. Documented as a known, intentional gap rather than guessed at.
- Fixed a duplicate/mismatched `EditorGUI.BeginChangeCheck()`/`EndChangeCheck()` pairing introduced while adding the Surface Type popup (an orphaned `EndChangeCheck()` with no matching `Begin` would have thrown at runtime) - each control now owns its own balanced Begin/End pair.

## 2026-07-20 - Tree Tool v1.4.0: Use HDRP's Own SurfaceOptionUIBlock (Real Rendering Pass / Blend Mode / Sorting Priority)

Touched files:
- `Scripts/Tool/TreeGenerator/Editor/TreeWindShaderGUI.cs`

Changes:
- User correctly flagged that Rendering Pass, Blending Mode, and Sorting Priority still weren't rendering as proper Enum dropdowns / a slider, and that Rendering Pass vs Blending Mode should show conditionally based on Surface Type (Opaque vs Transparent) like stock HDRP. The hand-rolled v1.3.7-v1.3.9 approach for these specific fields required knowing HDRP's internal `HDRenderQueue` integer mappings, which isn't public/stable enough to guess correctly - offered the user a choice between continuing to hand-roll individual fields (imperfect but zero new API risk) or switching to HDRP's own reusable `SurfaceOptionUIBlock` (perfect parity, some risk of needing one more compile fix). User chose the latter.
- Replaced the entire hand-rolled `DrawSurfaceOptionsSection()` (all the `_SurfaceType`/`_RenderQueueType`/`_BlendMode`/etc. constants and manual Popup/Toggle code) with `UnityEditor.Rendering.HighDefinition.MaterialUIBlockList` + `SurfaceOptionUIBlock(MaterialUIBlock.ExpandableBit.Base)` - this is the exact same block HDRP's own built-in `HDLitGUI`/Shader-Graph-generated inspectors compose internally, so Surface Type/Rendering Pass/Blending Mode/Sorting Priority/Alpha Clipping/Double-Sided/Material Type etc. are now byte-identical to stock HDRP/Lit (correct enum dropdowns, correct Opaque/Transparent conditional fields, correct Sorting Priority slider) with zero guessed internal value mappings.
- Only instantiates the block if `_SurfaceType` exists on the material (keeps the same safe-degrade behavior as before for any future non-HDRP-surface-option shader reusing this GUI class).
- Simplified constants/HandledNames accordingly (only `_SurfaceType` is still referenced directly, just to gate whether the block is added).

Notes:
- This is a bigger architectural swing than earlier patches (depends on an HDRP-Editor-internal-ish but officially-reusable class) - flagged to the user as carrying a small risk of needing one more compile-error round trip if the exact API differs in their HDRP version.

## 2026-07-20 - Tree Tool v1.4.1: Fix Wind Direction Consistency, Leaf-Detach Bug, Duplicate Amplitude Knobs

Touched files:
- `Scripts/Tool/TreeGenerator/TreeWind.hlsl`
- `Scripts/Tool/TreeGenerator/TreeWindZoneDriver.cs`

User-reported symptoms and root causes found:
1. "Leaves/branches spin/sway in their own random direction, not together" - the per-part random `phase` (baked into vertex color alpha) was used with very large multipliers (14.5, 25.7, 44.1, 71.9) inside the sin() arguments for branch/leaf sway. A phase range of 0-1 multiplied by ~44-72 spans 7+ full oscillation cycles, so neighboring leaves/branches ended up essentially uncorrelated in timing even though they all share the same wind *direction* vector - reads as chaotic spinning rather than a coherent breeze. Reduced all multipliers to 1.5-4.0 (small natural stagger, not decorrelation) and removed phase entirely from the main whole-tree lean term (a real gust hits a tree-sized object at roughly the same time, so that component now moves the whole tree in lockstep by design).
2. "Leaves swing until they detach from the branch" - `BuildLeaf()` bakes one uniform vertex color for all 4 corners of a leaf card, so the old leaf-flutter offset applied identically to the stem (attach point) and the tip - the stem visibly drifted away from the branch surface it should stay pinned to. Added a `UV` input to the Custom Function (leaf cards are authored with the stem at UV (0,0), tip near UV (1,1)) and scaled the leaf-flutter term by `saturate(length(UV))` - zero at the stem, full at the tip, so the stem now tracks the branch exactly (via the pre-existing main-lean/branch-sway terms, which already use the same weights as the branch itself) while only the tip flutters, like a candle flame with a fixed base.
3. "Too many places to set leaf/branch speed, and adjusting them doesn't seem to slow anything down" - `MainAmplitude`/`BranchAmplitude`/`LeafAmplitude`/`MainSpeed`/`BranchSpeed`/`LeafSpeed` were exposed as per-material Shader Graph Blackboard properties; since bark and leaf are separate material instances (even from the same graph), each had its own independent copy - 12 knobs total across 2 materials, easy to edit only one and see no change on the other. Moved all six to `TreeWindZoneDriver` as global shader floats (`_TreeMainAmplitude` etc., same pattern as the existing `_TreeWindDirection`/`_TreeWindStrength`) - one shared value read by every tree material in the scene, single source of truth. Removed them from the Custom Function's input parameter list entirely (now reads the globals directly, like `_TreeWindStrength` already did).
4. Lowered default amplitude/speed values (0.12/0.08/0.05 m, 0.5/1.2/3 speed) to a calmer starting point than the old per-material defaults, since the reported "everything moves too fast no matter what I change" was partly just the old defaults being on the aggressive side.

BREAKING CHANGE for the existing `Tree-HDRP-Lit-PBR.shadergraph`: the Custom Function's signature changed (6 float inputs removed, 1 new `UV` input added) - the graph needs manual rewiring: delete the 6 Amplitude/Speed Property nodes and their wires into the Custom Function node, remove those 6 input ports from the Custom Function node itself (Node Settings), add a `UV` input port, and wire `UV(0)` node's output into it. Tuning now happens on the `Tree Wind Zone Driver` component in the scene instead of per-material.

## 2026-07-20 - Tree Tool v1.4.2: Fix Custom Function Parameter Order Mismatch

Touched files:
- `Scripts/Tool/TreeGenerator/TreeWind.hlsl`

Changes:
- The v1.4.1 UV-input addition documented the HLSL signature as `(PositionWS, UV, VertexColor)`, but the user had already wired the Custom Function node's Inputs list as `PositionWS, VertexColor, UV`. Shader Graph Custom Function nodes bind arguments positionally (by list order), not by name, so the mismatched order caused `float2`/`float4` argument swap compile errors (`cannot implicitly convert from 'float2' to 'float4'` + an implicit-truncation warning at the same call site).
- Reordered the HLSL function signature to `(PositionWS, VertexColor, UV)` to match what the user already had wired, rather than asking them to reorder the node's input list in the Shader Graph UI - fully equivalent, avoids more manual graph editing.

## 2026-07-20 - Tree Tool v1.4.3: Fix Trunk Twisting From Unclamped Wind Vertex Colors

Touched files:
- `Scripts/Tool/TreeGenerator/ProceduralTreeSettings.cs`
- `Scripts/Tool/TreeGenerator/TreeMeshBuilder.cs`

User-reported symptom: trunk visibly kinks/zigzags oddly in the wind, asked whether bark needed a separate shader from leaves.

Root cause: `Wind Response` on Trunk/Branch Level settings had a `[Range(0f, 3f)]` slider, and the value gets multiplied into the R/G vertex-color channels that bake the wind weight (`TreeMeshBuilder.BuildBranchTube`/`BuildCustomBranchMesh`/`BuildLeaf`/`BuildCustomLeafMesh`). Those channels were never clamped to [0,1] in C#, but Unity's mesh vertex color buffer is 8-bit-per-channel and silently clamps to [0,1] on the GPU regardless - so whenever a user (as here) dragged Wind Response toward the top of its 0-3 range, the C#-computed weight and the GPU-read weight diverged: the smooth height-based falloff curve got clipped flat wherever it crossed 1.0, producing a visible kink/discontinuity in the bend right at that height. Not a bark-vs-leaf shader issue at all - same shader is correct and intended for all tree parts.

Changes:
- `TreeMeshBuilder.cs`: wrapped all four wind vertex-color channel computations (bark tube, custom bark mesh, procedural leaf, custom leaf mesh) in `Mathf.Clamp01` so the C#-computed value always matches what the mesh format actually stores - eliminates the kink for any Wind Response value, even at the extreme end of the slider.
- `ProceduralTreeSettings.cs`: tightened the sliders themselves so it's harder to land on a confusing extreme value by accident - Trunk `windResponse` range 0-3 -> 0-1 (trunk should stay low, it's thick wood), Branch Level `windResponse` and Leaves `windFlutterResponse` range 0-3 -> 0-2 (existing defaults of 1/1.8/1.5 still fit). Existing serialized values above the new max are NOT auto-clamped by Unity (Range only constrains the slider UI) - told the user to manually pull Trunk back down since their scene already has it saved at 3.

## 2026-07-20 - Tree Tool v1.4.4: Wood Should Barely Move - Retuned Wind Response Defaults

Touched files:
- `Scripts/Tool/TreeGenerator/ProceduralTreeSettings.cs`

User-reported symptom: screenshot showed the canopy as large overlapping translucent "shard"/blade shapes during wind, described as "stretching too much," and asked explicitly that this motion should live almost entirely in the leaves, not the trunk (loosely including branches in that description).

Root cause / reasoning: leaves inherit their "follow the branch" motion (mainOffset + branchOffset in TreeWind.hlsl) from their host branch level's `Wind Response`, applied uniformly across each leaf's whole card (correct, no per-card shear from this term). With the old defaults (Main Branches 1.0, Twigs 1.8, Trunk 0.25 - and the user's own tree still had Trunk manually dragged to the old max of 3 from an earlier experiment), many leaves attached at different heights/branches each swing by a large, different amount, so a dense canopy of independently-swinging large offsets reads visually as a chaotic field of scattered shard shapes, even though no single leaf card is internally broken.

Changes:
- Lowered default `windResponse`: Trunk 0.25 -> 0.05, Main Branches 1.0 -> 0.15, Twigs 1.8 -> 0.4 (both the field default and the `DefaultMainBranches()`/`DefaultTwigs()` factory presets). Wood (trunk + branches) now barely moves at all by default; `Leaves.windFlutterResponse` (unchanged, 1.5) is where nearly all the visible wind motion should come from going forward, matching real tree physics (only leaves flutter dramatically; branches and trunk sway subtly).
- Existing serialized trees are NOT retroactively updated (only the field default changes) - told the user to manually drag their tree's Trunk / Main Branches / Twigs Wind Response sliders down to match, since Unity doesn't reapply new C# defaults to already-saved component values.

## 2026-07-20 - Tree Tool v1.4.5: Fix "Bounds Could Not Be Determined" Terrain Tree Error

Touched files:
- `Scripts/Tool/TreeGenerator/Editor/ProceduralTreeEditor.cs`

User-reported symptom: after using "Export & Add To Terrain As Paintable Tree" (added in v1.4.x), the prototype showed up in Terrain's Paint Trees list but with a warning "The tree Procedural Tree couldn't be instanced because bounds could not be determined" and painting/Mass Place Trees did nothing.

Root cause: `ExportAndAddToTerrain` was saving the live scene GameObject (still carrying the `[ExecuteAlways] ProceduralTree` component) directly as the prototype prefab via `PrefabUtility.SaveAsPrefabAsset`. Terrain internally instantiates a throwaway copy of a tree prototype prefab to measure its renderer bounds before allowing painting; if that throwaway instance still has our script attached, its `OnEnable` runs during the probe and could interact with the LODGroup/mesh state in ways Terrain's bounds check doesn't expect.

Changes:
- `ExportAndAddToTerrain` now makes a temporary in-scene clone of the tree, removes the `ProceduralTree` component from the clone specifically, saves that script-free clone as the prefab asset, then destroys the temporary clone - the live scene tree (with its editable ProceduralTree component intact) is untouched, while the Terrain-registered prefab is a purely static LODGroup + MeshFilter/MeshRenderer hierarchy with no custom script that could interfere with Terrain's internal prefab probing.

## 2026-07-20 - Tree Tool v1.4.6: Length-Preserving Wind (No More Bark Stretching) + Auto-Prune Broken Terrain Prototypes

Touched files:
- `Scripts/Tool/TreeGenerator/TreeWind.hlsl`
- `Scripts/Tool/TreeGenerator/Editor/ProceduralTreeEditor.cs`

User-reported symptoms: (1) trunk/branches (all the brown bark) still visibly stretch like taffy under strong wind, worse at high amplitude; (2) "couldn't be instanced because bounds could not be determined" still logged during Export & Add To Terrain even though painting now works.

Root causes and fixes:
1. Bark stretching had two stacked causes in TreeWind.hlsl:
   a. MACRO stretch: the displacement was a raw sideways translation scaled by a height/branch-position gradient - the top of a limb translates while its base stays, which is a geometric shear that genuinely elongates the tube. Fixed with the GPU Gems 3 length-preservation trick: after summing all offsets, every vertex is constrained back to its ORIGINAL distance from the object pivot (`pivot = mul(UNITY_MATRIX_M, float4(0,0,0,1)).xyz`, per-instance so every Terrain-painted copy bends around its own base) - shear becomes an arc, wood bends instead of stretching regardless of wind strength.
   b. MICRO tearing: the turbulence term hashed the WORLD POSITION per vertex (white noise) - neighboring vertices of the same trunk ring received uncorrelated random offsets (up to +-6cm with the user's settings), visually shredding the bark surface. Replaced with a smooth per-PART wave derived only from the baked per-part phase (constant across a branch/leaf), with a per-part random frequency so parts still don't move in unison - the surface now stays rigid.
   - Custom Function signature unchanged - no Shader Graph rewiring needed, materials (Bark.mat/Leaf.mat both on Tree-HDRP-Lit-PBR) pick it up on recompile.
2. Bounds warning: stale prototypes from pre-v1.4.5 exports (prefab still carrying the ProceduralTree script - visible as blank slots in the Terrain's Trees list) remained registered on the TerrainData; Terrain re-probes ALL prototypes' bounds on every asset refresh (both stack traces confirm: fired from AssetDatabase.SaveAssets inside ExportMeshesTo, and from set_treePrototypes - i.e. per-refresh re-validation, not from the new prefab). Added `PruneBrokenPrototypes()`: runs at the start of Export & Add To Terrain (before any SaveAssets), removes prototypes whose prefab is null or still has a ProceduralTree component, and REMAPS painted TreeInstances' prototypeIndex so already-painted trees of surviving prototypes keep their type (instances of removed prototypes are dropped - they could never render anyway).

Notes:
- Length preservation constrains vertices to spheres around the tree base - branch sway relative to the trunk is an approximation on that shell, visually indistinguishable at these amplitudes and strictly better than shearing.

## 2026-07-20 - Tree Tool v1.4.7: Local Arm-Bend (Fixes Base Warping), Leaf Direction, Fine-Near-Zero Sliders

Touched files:
- `Scripts/Tool/TreeGenerator/TreeWind.hlsl`
- `Scripts/Tool/TreeGenerator/TreeWindZoneDriver.cs`
- `Scripts/Tool/TreeGenerator/ProceduralTreeSettings.cs`
- `Scripts/Tool/TreeGenerator/FineRangeAttribute.cs` (new)
- `Scripts/Tool/TreeGenerator/Editor/FineRangeDrawer.cs` (new)

User-reported symptoms: (1) the trunk base looked warped/broken (screenshot showed a jagged split shape right where the trunk meets the ground); (2) leaves visibly don't sway in the same direction as the branches/trunk, "each leaf blows on its own"; (3) Wind Response / Amplitude / Speed sliders are all linear over a wide range but every value anyone actually wants sits in the first few percent near 0, making precise adjustment there nearly impossible.

Root causes and fixes:
1. Base warping: v1.4.6's anti-stretch fix preserved every vertex's distance from a single SHARED pivot (the whole tree's base, via `UNITY_MATRIX_M`). That's correct for a simple pole but wrong for a branching mesh - a branch attached 3m up the trunk got constrained to the SAME distance from the trunk's base point rather than its own attachment point, which visibly warps the mesh, worst right at the base where the largest branches join the trunk. Replaced with a purely LOCAL model: each vertex bends around its own short virtual "arm" (a `lerp(0.05, 0.5, flex)`-length imaginary rod hanging from a point directly above it, where `flex = saturate(R+G+B)` from the baked wind weights), renormalized to the same length after the wind offset is applied. No shared reference point exists anywhere in the math, so nothing can be warped relative to anything else - rigid wood (low flex) gets a short arm and barely bends, floppy tips get a longer arm and arc naturally. Still fully prevents the original stretch/shear look.
2. Leaf direction: `leafOffset`'s dominant term was `crossDir` (perpendicular to wind) + world-up, with no component along the actual wind direction at all - branches/trunk lean along `dir`, but leaves fluttered on an axis perpendicular to it, so they visually disagreed with the rest of the tree. Swapped so `dir` is now the dominant term (matching the trunk/branch pattern exactly) with `crossDir` as a smaller secondary twist, so every leaf now visibly agrees with the tree's overall lean direction while still keeping individual flutter character.
3. Fine control near zero: added `[FineRange(min, max, power)]` (new attribute + `PropertyDrawer`, same pattern as the existing `[MinMaxRange]`) - a power-curved slider (`value = min + (max-min) * t^power`) that dedicates most of the drag distance to the low end of the range instead of spreading it linearly. Applied to Trunk/Branch Level Wind Response, Leaf Wind Flutter Response, and all six Amplitude/Speed fields on `TreeWindZoneDriver`. Also lowered the driver's default Amplitude/Speed values to roughly match what the user had already hand-tuned to (~0.15/0.08/0.05 amplitude, ~0.2/0.15/0.4 speed).

Notes:
- Existing serialized values are unaffected by the new attribute/lowered defaults (Unity doesn't retroactively rewrite saved fields) - the finer slider applies immediately, but already-set values stay where they were.

## 2026-07-20 - Tree Tool v1.4.8: Leaf Stem Was Still Getting Turbulence Even At Zero Branch/Trunk Response

Touched files:
- `Scripts/Tool/TreeGenerator/TreeWind.hlsl`

User-reported symptom: with Trunk/Branch Wind Response turned down to near 0 specifically to test "only leaves should move, pivot stays fixed at the branch," leaves still visibly flew off their attach point instead of staying pinned.

Root cause: `leafOffset` was already correctly gated to zero at the stem via `stemWeight = saturate(length(UV))`, but `turbulenceOffset` was gated only by `saturate(VertexColor.r + VertexColor.g + VertexColor.b)` - a leaf's stem vertex has R=0 and G=0 (correctly following its near-zero-response branch) but B > 0 (its own Leaf Wind Flutter Response, independent of branch response), so the stem still picked up the turbulence term regardless of position on the card. That was the actual cause of the stem drifting - not the flutter term at all, which was already working as designed.

Changes:
- Split the turbulence gate: bark (R+G) keeps its original ungated weight (no stem concept), leaf (B) is now multiplied by the same `stemWeight` used for `leafOffset`, so a leaf's stem vertex gets zero turbulence just like it already gets zero flutter. The arm-bend `flex` value (used to size the anti-stretch virtual arm) was switched to reuse this same stem-aware weight for consistency, though it was not strictly a visible bug on its own since the arm math naturally collapses to ~0 offset when windOffset is already ~0.

## 2026-07-22 - Tree Tool v1.4.9: Optional Root System (Buttress / Pneumatophore), Own Seed, Full LOD Support

Touched files:
- `Scripts/Tool/TreeGenerator/ProceduralTreeSettings.cs`
- `Scripts/Tool/TreeGenerator/TreeSkeleton.cs`
- `Scripts/Tool/TreeGenerator/TreeMeshBuilder.cs`
- `Scripts/Tool/TreeGenerator/ProceduralTree.cs`
- `Scripts/Tool/TreeGenerator/Editor/ProceduralTreeEditor.cs`

User request: add a root system with an on/off toggle, its own random seed, and LOD support, selectable between two types via a dropdown - large buttress-style surface roots (fig/ceiba/banyan) and mangrove-style "breathing root" spikes (pneumatophores).

Design:
- Roots reuse the exact same spline-growth (`Grow`) and tube-meshing (`BuildBranchTube`) pipeline as branches, so they inherit LOD (`radialResolution`), wind-vertex-color baking, and radial-segment control for free instead of needing a parallel mesh path. They're stored in a new `TreeSkeleton.Roots` list (separate from `Branches`) purely so leaves never try to spawn on them and so `TreeMeshBuilder` can build them as their own pass driven by `RootSettings` instead of a `BranchLevelSettings` entry.
- New `ProceduralTreeSettings.roots` (`RootSettings`, class) holds: `enabled` toggle, `type` (`RootType.Buttress` / `RootType.Pneumatophore` dropdown), shared `radialSegments`/`segments`/`windResponse` (defaults to 0.02, near-zero on purpose - roots are grounded wood and shouldn't sway), plus separate field groups for each type (`buttressCount/Length/StartHeight/Flare/Taper/Droop/Crookedness` and `pneumatophoreCount/Height/Radius/Spread/Lean`). New `rootSeed` field on `ProceduralTreeSettings`, independent of `seed`/`branchSeed`/`leafSeed`, so reshuffling roots never moves the trunk/branches/leaves.
- `TreeSkeletonGenerator.GenerateRoots` (new, called right after the trunk is grown, before branch levels): Buttress mode samples a point partway up the trunk (`buttressStartHeight`), then grows `buttressCount` ridges outward and downward (`buttressDroop` tips them into the ground) with `curveFrom`/`curveBlend` set so each ridge starts along the trunk's own direction and curves into its own, hiding the trunk joint the same way branch `jointSmoothing` does. Pneumatophore mode scatters `pneumatophoreCount` short spikes randomly (not evenly spaced, unlike branches) in an annulus around the trunk base and grows each one nearly straight up with a small random lean.
- Roots use `level = 0` deliberately: `BuildBranchTube` already forces the local branch-sway (vertex color G channel) to 0 for level-0 parts, which is exactly the right wind behavior for roots - only the same negligible whole-tree main-bend (R) a trunk gets, no local whip.
- `TreeMeshBuilder.Build` adds one more pass after the existing bark loop: if `s.roots.enabled`, builds every `TreeSkeleton.Roots` entry through `BuildBranchTube` with `s.roots.radialSegments` (scaled by the current LOD's `radialResolution`, same as bark) and `s.roots.windResponse`. `ProceduralTree.BranchCount` now includes `skeleton.Roots.Count` so the inspector's part-count stat stays accurate.
- `ProceduralTreeEditor` adds a `rootSeed` row (Shuffle button) next to the existing seed rows, a `DrawRootsSection()` foldout (same collapsible-group pattern as Geometry/Leaves/Mesh) placed between Branch Levels and Leaves, and hides whichever type's fields don't apply (`ButtressOnlyFields`/`PneumatophoreOnlyFields`) based on the selected `type`, exactly like the existing prefab-source field hiding.

Notes:
- Roots only use the procedural tube pipeline - they don't currently support the custom-prefab geometry mode that trunk/branch/leaf parts have.
- Off by default (`enabled = false`), so existing trees are unaffected until a user explicitly turns roots on.

## 2026-07-23 - Tree Tool v2.0: Wind Mode Switch (Fake/True), Manual Entry Toggle, Fine Roots, Per-Tree Export Folders, Editable Source Prefab, Full Manual Rewrite

Touched files:
- `Scripts/Tool/TreeGenerator/ToolRangeAttribute.cs` (new)
- `Scripts/Tool/TreeGenerator/Editor/ToolRangeDrawer.cs` (new)
- `Scripts/Tool/TreeGenerator/Editor/ManualEntry.cs` (new)
- `Scripts/Tool/TreeGenerator/Editor/MinMaxRangeDrawer.cs`
- `Scripts/Tool/TreeGenerator/Editor/FineRangeDrawer.cs`
- `Scripts/Tool/TreeGenerator/ProceduralTreeSettings.cs`
- `Scripts/Tool/TreeGenerator/TreeSkeleton.cs`
- `Scripts/Tool/TreeGenerator/TreeMeshBuilder.cs`
- `Scripts/Tool/TreeGenerator/ProceduralTree.cs`
- `Scripts/Tool/TreeGenerator/Editor/ProceduralTreeEditor.cs`
- `Scripts/Tool/TreeGenerator/Manual/TreeGenerator_Manual.md` / `.pdf`

Context: the user built their own simplified "Fake Wind" Shader Graph system (`Shader/FakeWind/` -
a self-contained per-object sway effect, no WindZone needed) alongside the existing hand-wired
wind system from earlier versions, now relocated to `Shader/TrueWind/`. This entry wires the tool
to understand and switch between both, plus a large batch of authoring/export UX requests bundled
into the same session.

**1. Wind Mode switch (Fake / True)**
- New `WindMode` enum (`Fake` default, `True`) + `ProceduralTreeSettings.windMode`.
- `ProceduralTree.GetDefaultLeaf(WindMode)`: Fake mode loads `Assets/Texture/Tree/Leaf.mat` (the
  user's own Fake Wind material) as the default when the Leaf Material field is empty; True mode
  loads-or-creates `Assets/Texture/Tree/Leaf_TrueWind.mat` from `Shader/TrueWind/Leaf-HDRP-Lit-PBR.shadergraph`
  the first time it's needed (base texture reused from the Fake material for a consistent look),
  cached after that. Trunk default is unchanged (still the plain HDRP/Lit placeholder) per explicit
  instruction - only the leaf default depends on Wind Mode.
- Editor: new "Wind Mode" section at the top of the inspector with the mode dropdown + an inline
  explanation of what's active. Per-part Wind Response fields (Trunk/Roots/Leaves - directly, via
  `DrawGroup`'s hidden-field list) and the whole Wind Settings block + "Add Wind Zone" hint are
  hidden entirely in Fake mode and shown only in True mode. (Branch Levels' per-item Wind Response
  is drawn through Unity's own array/list drawer, not `DrawGroup`, so it isn't hidden per-mode -
  left visible with its existing "[True Wind only]" tooltip instead of writing a full custom
  `BranchLevelSettings` PropertyDrawer just for this.)

**2. Parameter rework: wider ranges, Manual Number Entry, simpler labels, categorization**
- New `[ToolRange(min,max)]` attribute + `ToolRangeDrawer`, a drop-in replacement for Unity's
  built-in `[Range]` used throughout `ProceduralTreeSettings.cs` now. New `ManualEntry.cs` helper
  (`property.serializedObject.FindProperty("settings.manualNumberEntry")` - always resolves to the
  root `ProceduralTree` instance regardless of how deeply nested the field is) shared by
  `ToolRangeDrawer`, `MinMaxRangeDrawer`, and `FineRangeDrawer`. New `ProceduralTreeSettings.manualNumberEntry`
  bool + inspector checkbox: when on, every slider/min-max-range/fine-range field in the whole tool
  switches at once to a plain typed number field with no clamp (any value, even outside the
  declared range) - one checkbox for everything, not per-field.
- Numeric bounds widened across the board (examples: Trunk Height 100->300m, Radius 10->30m,
  Branch Count 100->300, Leaf Count Per Branch 300->800, Leaf Size 10->25m; full new bounds listed
  in the manual's parameter tables). Crookedness-type fields (Trunk/Branch/Buttress/Pneumatophore)
  widened from 0-1 to 0-2 specifically per request for more available waviness.
- `[InspectorName(...)]` applied to jargon-y field/enum labels (e.g. `radialSegments` -> "Sides",
  `azimuthRandomness` -> "Spin Randomness", `bendExponent` -> "Sway Curve", LOD `screenHeight` ->
  "Visible Until (Screen Size)", `RootType`/`WindMode` enum values get descriptive labels) - this
  only changes the displayed label, not the serialized field name, so existing tuned values on
  live trees are unaffected.
- Editor reorganized into bold category headers (Wind Mode, Random Seeds, Geometry Source,
  Structure, Foliage, Rendering, Level Of Detail, Materials) instead of a flat list of foldouts.

**3. Root system upgrade**
- New `FineRootSettings` (off by default): thin fibrous roots spawned along each generated root
  (Buttress ridge or Pneumatophore spike), same recursive spawn-along-a-parent-spline idea
  `BranchLevelSettings` uses off the trunk. New `TreeSkeleton.Branch.radialOverride` field lets fine
  roots carry their own (thinner) side count independent of the parent root's, read by
  `TreeMeshBuilder`'s root-building loop.
- `RootSettings.buttressStartHeight` changed from a single float to a `FloatRange`, sampled per-root
  inside the generation loop instead of once for the whole set - ridges now break away from the
  trunk at slightly different heights instead of all lining up identically, and (being a normal
  serialized field going through the same live-rebuild pipeline as everything else) updates
  immediately when dragged, same as every other parameter.
- Crookedness ranges widened for both root types (see item 2).

**4. Export overhaul: per-tree folders, material/texture duplication, editable Source prefab**
- Both export buttons now go through a shared `DoCoreExport`: resolves/creates
  `Assets/GeneratedTrees/<tree.name>/` with `Texture/`, `Material/`, `TreePrefab/` subfolders;
  duplicates whatever Bark/Leaf materials are currently active (`Object.Instantiate` + `AssetDatabase.CreateAsset`,
  named `<name>_Bark.mat` / `<name>_Leaf.mat`) so editing one tree's material copy never affects
  another tree or the shared source material; duplicates every texture the shader exposes
  (`shader.GetPropertyType(i) == ShaderPropertyType.Texture`, walked via `Shader.GetPropertyCount`)
  into `Texture/` renamed `<name>_<suffix>_<mapName>.<ext>` and repoints the duplicated material at
  the copies; freezes LOD meshes into `TreePrefab/`; saves the live tree via
  `PrefabUtility.SaveAsPrefabAssetAndConnect` as `<name>_Source.prefab` - this both creates an
  editable prefab (keeps the `ProceduralTree` component, unlike a frozen static export) and converts
  the scene GameObject into a connected Prefab Instance of it, so editing either one and exporting
  again keeps both in sync automatically.
- New `ProceduralTree.exportedFolderName` (hidden serialized field) + `ResolveTreeFolder`: if the
  tree's GameObject was renamed since the last export, the existing `Assets/GeneratedTrees/` folder
  is renamed (`AssetDatabase.MoveAsset`) to match instead of creating an orphaned duplicate.
- New `BackupIfExists`: before a re-export overwrites `TreePrefab/`/`Material/`/`Texture/`, their
  current contents are moved wholesale into a timestamped `Backup/<yyyy-MM-dd_HH-mm-ss>/` subfolder
  first (folder-level `AssetDatabase.MoveAsset`, not per-file), so nothing from a previous export is
  ever silently lost.
- `ExportAndAddToTerrain` still additionally saves a SEPARATE script-stripped static clone for
  Terrain's prototype system (unchanged reasoning from v1.4.5/v1.4.6 - Terrain's bounds-probing
  can't tolerate the live `[ExecuteAlways] ProceduralTree` component) - this is now clearly a second,
  Terrain-only artifact alongside the always-editable `_Source.prefab` from the shared core export.

**5. Manual rewritten as an end-user step-by-step guide (v2.0)**
- Removed every "what's new in vX.X" version-history callout and the entire Shader Graph
  node-wiring walkthrough (Custom Function setup, Fragment stage PBR wiring) - no longer relevant
  now that both wind systems are fully self-contained and switched with one dropdown.
- Rewritten around a numbered 6-step "create a tree to finished result" quick start, a dedicated
  side-by-side Fake Wind vs True Wind section, a full parameter reference table (default value +
  min-max range for every field, matching item 2's widened bounds), and a walkthrough of the new
  per-tree export folder layout and Terrain painting flow.
- Regenerated via the established scratchpad-HTML-mirror -> headless Edge `--print-to-pdf` pipeline;
  synced `.md`/`.pdf` to both the tool folder and `Manual/`.

Notes:
- Fake Wind's own material-level knobs (Wind/Branch Move/Branch Stiffness/Leaf Frequency/Leaf Speed,
  already present on `Leaf.mat`) are intentionally left as plain material properties, not exposed as
  new per-tree fields in `ProceduralTreeSettings` - they're a self-contained material effect, edited
  directly on the material like any other HDRP material property, not per-branch baked data the way
  True Wind's response values are.

## 2026-07-23 - Tree Tool v2.0.1: Fix Root Settings Reshuffling Branch/Leaf Positions

Touched files:
- `Scripts/Tool/TreeGenerator/TreeSkeleton.cs`

User-reported symptom: adjusting root settings (or just toggling Roots on/off) also moved branches
and leaves that had nothing to do with roots - "เวลาผมแก้ระยยรากช่วยดูให้หน่อยมันไปเปลี่ยนตำแหน่งใบ
ตำแหน่งกิ่งอะไรแบบนี้ด้วยตอน เปิดรากใบก็เปลี่ยนเหมือนกัน".

Root cause: `TreeSkeletonGenerator.Generate()` uses one shared `int nextId` counter for every
`Branch.id` it hands out (trunk, then roots, then branch levels). Branch generation seeds its RNG
stream from `CreateRand(..., parent.id)` and leaf generation from `CreateRand(..., b.id)` - so a
branch/leaf's shape depends on its parent branch's *id*, not just on Branch Seed/Leaf Seed. Root
generation ran right after the trunk and consumed `nextId` for every root (and every fine root) it
created *before* the branch-levels loop started, so enabling roots or changing how many get
generated shifted the id every later branch received, which reseeded its RNG stream and moved it -
even though nothing about that branch's own settings changed. This is exactly the kind of coupling
the file's own doc comment says shouldn't exist ("changing leafSeed never moves a branch, and
changing branchSeed never bends the trunk" - roots quietly broke that guarantee for the seed-less
case of "no seed changed at all, just root settings").

Fix: roots now get their own local `int rootId` counter, entirely separate from the trunk/branch
`nextId` sequence (`GenerateRoots(..., ref int nextId)` -> `GenerateRoots(...)` with a local
`int rootId = 0;` inside; `GenerateFineRoots` takes `ref int rootId` instead of `ref int nextId`).
Nothing outside the root system ever looks up a root by id (roots don't grow branches or leaves, and
`TreeMeshBuilder` iterates `sk.Roots` directly rather than by id), so this is a pure decoupling with
no other behavior change - root ids can now overlap with branch ids without any collision risk.
Root/branch/leaf shapes no longer shift when only root settings change.

## 2026-07-23 - Tree Tool v2.0.2: Consolidated Wind Response Panel, Moved Up Top

Touched files:
- `Scripts/Tool/TreeGenerator/Editor/ProceduralTreeEditor.cs`
- `Scripts/Tool/TreeGenerator/Editor/BranchLevelSettingsDrawer.cs` (new)
- `Manual/TreeGenerator_Manual.md` / `.pdf`

User feedback (with an inspector screenshot): the "no WindZone in the scene, click to create one"
warning sat too far down the inspector (after Rendering); Fake Wind mode should show neither that
button nor any per-part wind settings at all; and every part's Wind Response field (Trunk, each
Branch Level, Roots, Leaves) should be pulled out of their own sections and consolidated into one
shared category, specifically so a Branch Level added later (or a fine-root/sub-level) is still
reachable there instead of only living inline wherever it happens to be nested.

Changes:
- New `ProceduralTreeEditor.DrawWindResponseSection()`, called immediately after `DrawWindModeSection()`
  (right at the top of the inspector, directly under the Wind Mode dropdown) - only renders in True
  Wind mode. Draws `trunk.windResponse`, then loops `branchLevels` live (`arraySize`-driven, so a
  level added or removed later is picked up automatically next repaint) drawing each level's
  `windResponse` labeled with that level's own `name`, then `roots.windResponse` and
  `leaves.windFlutterResponse`, then the existing `WindSettings` block and `DrawWindZoneHint()`
  (the "Add Wind Zone To Scene" warning/button) - all in this one place now instead of scattered
  through Structure/Foliage/Rendering and stranded at the bottom of the inspector.
- `DrawGroup` calls for Trunk/Roots/Leaves now unconditionally hide `windResponse` /
  `windFlutterResponse` (previously only hidden in Fake Wind mode) since those fields are shown
  exclusively in the new consolidated section regardless of mode - removed the now-unused `fakeWind`
  parameter from `DrawRootsSection`/`DrawLeavesSection`.
- New `[CustomPropertyDrawer(typeof(BranchLevelSettings))]` (`BranchLevelSettingsDrawer.cs`) so each
  Branch Levels list element can skip `windResponse` too - Unity's default array/list rendering
  doesn't support hiding a named child field the way the hand-rolled `DrawGroup` foldout does, so
  this was the one spot that couldn't just reuse the existing hidden-fields mechanism. Preserves the
  existing look exactly: each element's foldout is still labeled with that level's own `name` value
  (matching Unity's default behavior for this field) and every other field renders unchanged, height
  calculation included (`GetPropertyHeight` sums each visible child's own height).
- `DrawWindModeSection`'s help text updated to describe the new layout (Wind Response panel sits
  directly below the mode dropdown; nothing wind-related shows at all in Fake Wind).
- Manual's Wind section (3) rewritten to describe the single consolidated panel instead of fields
  "appearing/disappearing" scattered across other sections.

Notes:
- Fake Wind mode now shows zero wind-related UI anywhere in the inspector, exactly matching the "no
  WindZone button, no per-part settings" request - confirmed by `DrawWindResponseSection` returning
  immediately (`if (!IsTrueWind()) return;`) before drawing anything, including the WindZone hint.
