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
