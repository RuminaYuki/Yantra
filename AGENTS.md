# AGENTS.md

Guidance for AI coding agents working on this Unity project.

## Project Shape

- This is the `Yanta` Unity project. Open the project from `D:\Progame\Unity\Yanta`, not from `Assets` alone.
- Unity editor version: `6000.3.15f1`.
- Main gameplay/source files live under `Assets/Scripts`.
- Scenes and level content live under `Assets/Scenes` and `Assets/OutdoorsScene.unity`.
- Prefabs, character assets, materials, and imported packages under `Assets/Character`, `Assets/Prefabs`, `Assets/Materials`, `Assets/Model`, and `Assets/Plugins` are Unity-authored assets; edit them cautiously.
- Package dependencies are managed in `Packages/manifest.json` and include Unity Input System, Animation Rigging, ProBuilder, URP/HDRP 17.x, Timeline, UGUI, Visual Scripting, and `com.kogetsu.library`.

## Working Rules

- Do not delete or regenerate `.meta` files manually. Keep Unity asset GUIDs stable.
- Do not edit `Library`, `Temp`, `Logs`, or `UserSettings` unless the user explicitly asks.
- Avoid rewriting `.unity`, `.prefab`, `.asset`, `.inputactions`, and other serialized Unity files by hand unless the requested change specifically requires it and the file format is understood.
- Preserve existing user changes. The worktree may contain active edits from Unity, Plastic SCM, or another agent.
- Keep changes scoped. Prefer small gameplay/script fixes over broad refactors.
- Use ASCII for new code and docs unless the file already uses another encoding or the user requests Thai text.

## C# Style

- Follow the existing Unity MonoBehaviour style in `Assets/Scripts`.
- Use `[SerializeField] private` fields for inspector configuration.
- Use `Awake` for component references and `Start` for scene/runtime lookup when needed.
- Use `Update` for frame logic and `FixedUpdate` for physics/Rigidbody movement.
- Prefer `Rigidbody.MovePosition` for physics-driven movement.
- Guard serialized references and scene lookups before use; log clear `Debug.LogWarning` or `Debug.LogError` messages when setup is missing.
- Keep public methods intentional. Expose methods for UnityEvents, event bus handlers, or other scripts only when needed.
- Add comments only for non-obvious gameplay/state-machine behavior.

## Gameplay Architecture Notes

- `YantraStatsController` extends `Kogetsu.Library.Core.StatsController` and stores Yant count.
- Save data is held in `SaveSO` and written by `SaveFile`.
- Tani-related gameplay currently lives under `Assets/Scripts/Event/Tani` and `Assets/Scripts/Tani`.
- Event publishing uses `Kogetsu.Library.DesignPatternCore.EventBus`.
- `TaniStateMachine.cs` may reference state classes or enemy classes that are not present yet in the working tree. Before changing related code, search the full project for those definitions and check compile errors instead of assuming they should be removed.

## Validation

- After C# edits, prefer validating through Unity compilation in the Editor when available.
- If using command line checks, inspect generated `.csproj` files and run a C# build only if the Unity-generated project is current.
- For asset, prefab, or scene changes, open Unity and confirm there are no missing scripts, missing references, or import errors.
- When changing input actions, animations, scenes, prefabs, or ScriptableObjects, verify the behavior in Play Mode.

## Git And Source Control

- This project appears to use both Git metadata and Plastic SCM workspace files. Do not modify `.plastic` files unless explicitly requested.
- Do not run destructive commands such as `git reset --hard`, `git clean`, or checkout/revert operations unless the user explicitly asks.
- Unity may update `ProjectSettings`, `Packages/packages-lock.json`, generated solution/project files, and serialized assets during normal editor use. Review those diffs before including them in a final change.

## Useful Commands

```powershell
git status --short
rg --files Assets/Scripts
rg "class Tani|TaniRunningEvent|YantraStatsController" Assets/Scripts
Get-Content -Raw Packages\manifest.json
Get-Content -Raw ProjectSettings\ProjectVersion.txt
```

