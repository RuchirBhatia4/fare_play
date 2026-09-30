# Fare Play — project context

Handoff from the planning sessions (Sep 25–28, 2026). Shared by both teammates: keep this file up to date as work lands (update it in the same PR as the work).

## The project
- **Fare Play**: a Unity 6 (URP) bus-driving game. Class team project, **Assignment 2**.
- **Deadline: Friday Oct 2, 2026, 1 PM** (submit on Brightspace in the morning). Web build must be hosted publicly (Unity Play or GitHub Pages; Assignment 1 required that, assume the same).
- **Team:** Ruchir Bhatia (GitHub `RuchirBhatia4`) = Lane A, Driving & World. Yuyang Bai = Lane B, Rules & UI.
- **Repo:** https://github.com/RuchirBhatia4/fare_play (private). Ruchir's local path: `~/fare_play` on macOS (zsh). `gh` CLI is logged in. Git LFS is on.
- **Chat:** Discord server; `#github-feed` receives GitHub webhook events.
- Assets: Create with Code Unit 1 "Course Library" (Prototype 1 starter package), already imported under `Assets/Course Library/`. It includes a real bus (`ST_Veh_Bus_Blue_Z`), `road_straight_mesh`, `Prop_Barrier01/02`, crate, barrel, spool, boulder, car/ute/van.

## Game design (current, replaces the original proposal PDF in Docs/)
1. **Overview:** top-down view of the route(s) for 30 s, bus locked. Space skips.
2. **Chase camera** swoops in behind the bus; the player drives with arrow keys.
3. **Route timer** (per route, `Route.timeLimitSeconds`; Route 1 = 1:50 because it is ~665 m) starts when the bus crosses either route's start line. At 0:00 the run fails (score 0).
4. **Bus stops:** passengers board one at a time (design: 0.75 s each; `GameTuning.asset` currently has 1.5 s, confirm with Yuyang) only when the WHOLE bus is inside the bay AND stopped (< 0.5 m/s). Driving away early leaves the rest.
5. **Traffic lights:** green 8 s / yellow 2 s / red 8 s. Crossing the stop line on red = penalty (once per light). Running a red is allowed. The HUD always shows the NEXT light's state + countdown so players can board passengers during a red (a bus stop sits just before each light).
6. **Collisions** with anything tagged `Obstacle` = penalty (1 s cooldown).
7. **Finish:** delivered passengers count; time bonus = seconds left x 10.
8. **Scoring:** +100 per passenger delivered, -50 per red light, -25 per hit, +10 x seconds left. Total never below 0. Failed run = 0. All values live in `GameTuning.asset`.
9. **Stretch:** fuel bar drained by throttle + fuel station (reuse StopZone), passenger capacity, sounds.
10. Two routes (Route 2 is a "should"); one polished route beats two rough ones.

## Architecture
- Namespace `FarePlay`. Scripts in `Assets/_FarePlay/Scripts/{Core,Bus,Camera,Route,Traffic,Game,UI}`.
- **`Core/GameEvents.cs` is the contract between lanes**: static events `StateChanged, RunStarted, PassengerBoarded, RedLightViolation, CollisionPenalty, RunEnded`. Subscribe in `OnEnable`, unsubscribe in `OnDisable`.
- `GameState`: Overview -> Ready -> Driving -> Won / Failed. `GameManager.Instance` owns it.
- `GameTuning` (ScriptableObject, `Assets/_FarePlay/Settings/GameTuning.asset`) holds every balancing number.
- `Route` (on each route root): name, time limit (Route 1: 110 s), `lightsInOrder`, `NextLight`, `MarkLightPassed`.
- Prefabs in `Assets/_FarePlay/Prefabs/`: `Bus.prefab` (root has Rigidbody mass 3000 + BoxCollider + BusController; model is a child), `GameSystems.prefab` (GameManager + RaceTimer + ScoreManager + Canvas with HUD + EventSystem), `BusStop.prefab` (StopZone + BusStop + bay marking + waiting passengers).
- Scenes in `Assets/_FarePlay/Scenes/`: `Main.unity` (Ruchir only; the only scene in Build Settings), `Sandbox_A` (Ruchir), `Sandbox_B` (Yuyang; has a test route, start/finish lines, Bus, GameSystems and a BusStop).
- Unity 6 APIs: `rb.linearVelocity` (not `velocity`). Active Input Handling = Both; scripts use `Input.GetAxis/GetKeyDown`.
- `StopZone` checks "whole bus inside" with axis-aligned bounds: keep bays aligned to world X/Z and ~1–2 m larger than the bus.

### Script status (as of Sep 29)
- **Implemented:** BusController, CameraDirector, GameManager, RaceTimer, StartLine/FinishLine, HUD (timer, centre message, score, passengers, route name, signal indicator; all slots optional), Route, GameEvents, GameState, RunResult, GameTuning, StopZone + BusStop (#10, Yuyang), BusCollisionPenalty (#7), ScoreManager (#11), ResultsScreen (#17).
- **Still skeletons with TODOs:** TrafficLight, TrafficStopLine (#13).

## GitHub issues (verify with `gh issue list --state all`)
| # | Issue | Owner | Status (Sep 29) |
|---|---|---|---|
| 1 | Set up the Unity project + Course Library | Ruchir | ✅ closed |
| 2 | Teammate: clone, open, press Play | Yuyang | ✅ closed |
| 3 | Bus driving | A | ✅ done |
| 4 | Chase camera | A | ✅ done |
| 5 | Overview camera 30 s -> chase switch (needs an OverviewPose object in Main) | A | Mon |
| 6 | Route 1 blockout in Main.unity | A | in PR: ~665 m staircase, walls, 3 stops, 11 obstacles |
| 7 | Collision penalty (BusCollisionPenalty) | A | ✅ done (PR #34) |
| 8 | Game flow | B (done by Ruchir) | ✅ done |
| 9 | Start lines, finish line, 1:30 timer | B (done by Ruchir) | ✅ done |
| 10 | Bus stops (StopZone + BusStop.prefab) | B | ✅ done (PR #30) |
| 11 | Scoring (ScoreManager.BuildResult, LiveScore) | B | ✅ done (PR #31) |
| 12 | HUD v1: timer, score, passengers | B | ✅ done (PR #33) |
| 13 | Traffic lights: cycle, stop line, red-light violations | A | Tue |
| 14 | Bus stop right before each light | A | Tue |
| 15 | Route 2 | A | Wed (should) |
| 16 | HUD signal indicator | B | ✅ done (PR #35) |
| 17 | Results screen + Restart button | B | ✅ done (PR #36) |
| 18 | Overview instructions + Space skip | B (done by Ruchir) | ✅ done |
| 19 | First WebGL build at a public URL | B | Wed |
| 20 | Playtest + balance | both | Wed-Thu |
| 21 | Bug bash | both | Thu |
| 22 | Feature freeze Thu 6 PM + final build | B | Thu |
| 23 | Submit on Brightspace | both | Fri morning |
| 24 | Fuel bar (stretch) | A | Thu if ahead |
| 25 | Fuel station (stretch) | B | Thu if ahead |
| 26 | Passenger capacity (stretch) | B | if ahead |
| 27 | Sounds (stretch) | both | if ahead |

## Schedule
- **Mon 9/28 (First playable):** A: #6 -> #7 ✅ -> #5. B: #10 ✅ -> #11 ✅ -> #12 ✅. Evening: full Route 1 run in Main with passengers + score.
- **Tue 9/29:** A: #13, #14 (merge TrafficLight.prefab by midday). B: #16 ✅, #17 ✅. All musts on Route 1.
- **Wed 9/30:** A: #15 or polish. B: #19 web build online, #20 playtest. Decide on stretch.
- **Thu 10/1:** #20 balance, #21 bugs, stretch only if clean; #22 freeze 6 PM, final build.
- **Fri 10/2:** #23 submit before 1 PM.
- Cut order if behind: fuel/sounds -> Route 2 -> polish. Never cut Wednesday's web build.

## Working rules
- Branch per issue: `feature/<n>-short-name`. PR with `Closes #n`, squash merge, delete branch. `git switch main && git pull` before opening Unity.
- **Main.unity: only Ruchir edits it.** Yuyang works in Sandbox_B and prefabs.
- Never commit `Library/ Temp/ Logs/ UserSettings/`. Binaries go through LFS (`.gitattributes` handles it).
- **New C# files need a .meta from Unity.** After creating a script, open/focus Unity so it generates the .meta, then commit both. Never hand-edit `.unity`/`.prefab` YAML.
- Unity Editor work (scenes, prefabs, Inspector) is done by hand: write/modify scripts, then give exact, numbered Unity steps (menu paths, Inspector values, which object to select).
- Keep tunables in GameTuning or serialized fields, not hard-coded.
- Keep this CONTEXT.md current: update the status table and script status in the PR that changes them.

## How to help (both teammates)
- Clear numbered steps with exact values and menu paths; issue numbers included.
- When giving Terminal commands to paste, **don't put `# comments` on the same line** (zsh passes them as arguments).
- Test checklists ("do this -> you should see") after each feature.

## First thing to do in a new session
1. `git switch main && git pull`, then `gh issue list --state all --limit 40`, `gh pr list --state merged`, `git log --oneline --stat -15`.
2. Work out what the other teammate pushed and which issues are really done; update the status table above.
3. Show the plan with done items checked, then continue with that person's next task in their lane.
