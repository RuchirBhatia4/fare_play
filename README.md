# 🚌 Fare Play

**Racing + Brakes as Boost:** a time-trial racing game where braking charges your boost and every passenger you pick up makes your bus heavier.

**▶ Play it in your browser: https://ruchirbhatia4.github.io/fare-play-web/**

> Original proposal: [`Docs/Fare_Play_Design_Document.pdf`](Docs/Fare_Play_Design_Document.pdf). This README describes the **current** design, which replaced it.

## How it plays

1. **Overview (30 s):** a top-down view of the whole route. Press **Space** to skip.
2. **Start:** drive through the green START gate and the **1:30 timer** starts (shown to the thousandth of a second).
3. **Bus stops:** pull fully into the bay and stop. Each stop is graded **PERFECT / GOOD / SLOPPY** by how centred and straight you are, and charges your **boost**. PERFECT stops in a row build a combo for bigger boosts.
4. **Weight:** every passenger who boards makes the bus heavier: slower to accelerate, longer to stop, wider to turn, and it rolls on further.
5. **Regenerative braking:** braking hard also charges boost, more when the bus is heavy.
6. **Boost (Shift):** faster top speed and acceleration, but it burns fuel 3x faster. Boost through a red light and you **beat the light** (no penalty).
7. **Hazards:** traffic lights, moving cars, obstacles, passengers who give up if you're late, and a fuel tank that refills only while parked in a bay. Run dry and stop outside a bay: game over.
8. **Finish:** drive through the red FINISH gate. Your best runs (with names) are saved in your browser.

### Controls
| Key | Action |
|---|---|
| ↑ / ↓ | Throttle / brake, then reverse |
| ← / → | Steer |
| Shift | Boost (while the boost bar has charge) |
| C | Switch between chase view and top view (handy for lining up with bays) |
| Space | Skip the overview |
| R | Start over (any time during a run) |
| P | Play again (results screen) |

### Scoring (all values live in `GameTuning.asset`)
| | Points |
|---|---|
| Each passenger delivered to the finish | **+100** |
| Each red light run (not boosting) | **−50** |
| Each obstacle or car hit (1 s cooldown) | **−50** |
| Time bonus at the finish | **+10 × exact seconds left** (9.876 s = +98.76) |
| Run out of time or fuel | **Run failed, score 0** |

Example: 9 passengers, no reds, two hits, 9.876 s left → 900 − 100 + 98.76 = **898.76**

## Scope for the one-week build

| Priority | Features |
|---|---|
| **Must** | Overview → chase camera, bus driving, Route 1, start line + 1:30 timer + fail, bus stops with a proper stop, score per passenger, traffic lights + penalty + on-screen signal, collision penalty, time bonus, results screen, WebGL build |
| **Should** | Route 2, Space to skip the overview, restart button, instructions on the overview |
| **Stretch** | Fuel bar drained by throttle + fuel station, passenger capacity, sounds, particles |
| **Not this week** | AI traffic, leaderboards, weather, extra maps |

## Timeline (due **Fri Oct 2, 1 PM**)

| Day | Lane A: Driving & World | Lane B: Rules & UI | Checkpoint |
|---|---|---|---|
| **Fri 9/25** | Unity project, Course Library, repo setup | Clone and open the project | Both can press Play |
| **Sat 9/26** | Bus driving + chase camera | Game flow, start/finish lines, timer, HUD | Crossing the start line starts the clock |
| **Sun 9/27** | Route 1, overview→chase switch, collision penalty | Bus stops + boarding, scoring + time bonus | **First playable** |
| **Mon 9/28** | Traffic lights + red-light detection | HUD signal indicator, passengers, live score | Lights cost points and show on screen |
| **Tue 9/29** | Route 2, bus stop before each light | Results screen, restart, overview instructions | **Feature complete** |
| **Wed 9/30** | Balance pass | First WebGL build online | **Build online.** Decide on the stretch goal |
| **Thu 10/1** | Fuel (stretch) or bug fixes | Fuel station (stretch) or bug fixes, final build | **Feature freeze 6 PM** |
| **Fri 10/2** | — | — | Submit before 1 PM (aim for the morning) |

Progress is tracked in GitHub **Milestones** and on the **Fare Play** project board.

## Getting started

1. Install **Git**, **[Git LFS](https://git-lfs.com)**, and **Unity Hub**.
2. Install Unity **`6000.6.0f1`** through Unity Hub. Use this exact version.
3. Clone:
   ```bash
   git lfs install
   git clone https://github.com/RuchirBhatia4/fare_play.git
   ```
4. Unity Hub → **Add → Add project from disk** → pick the folder. The first open takes a few minutes.
5. Open `Assets/_FarePlay/Scenes/Main.unity` and press **Play**.

Read **[CONTRIBUTING.md](CONTRIBUTING.md)** before your first commit.

## Project layout

```
Assets/
├── Course Library/          Create with Code Unit 1 assets (vehicles, obstacles, road). Don't edit.
└── _FarePlay/
    ├── Scenes/              Main.unity (Lane A owns), Sandbox_A.unity, Sandbox_B.unity
    ├── Scripts/
    │   ├── Core/            GameEvents (the contract between lanes), GameManager, GameTuning, GameState, RunResult
    │   ├── Bus/             BusController, BusCollisionPenalty           (Lane A)
    │   ├── Camera/          CameraDirector                               (Lane A)
    │   ├── Route/           Route, StartLine, FinishLine, StopZone, BusStop
    │   ├── Traffic/         TrafficLight, TrafficStopLine                (Lane A)
    │   ├── Game/            RaceTimer, ScoreManager                      (Lane B)
    │   └── UI/              HUD, ResultsScreen                           (Lane B)
    ├── Prefabs/             Bus, TrafficLight, BusStop, GameSystems (managers + HUD + results)
    ├── Settings/            GameTuning.asset
    └── Materials/
Docs/                        design document
Tools/                       setup_github.sh
```

## Team

| Name | GitHub | Contributions |
|---|---|---|
| **Ruchir Bhatia** | [RuchirBhatia4](https://github.com/RuchirBhatia4) | Bus driving and cameras, game flow and route timer, Route 1 level design with traffic and hazards, the Brakes as Boost twist, WebGL build and hosting |
| **Yuyang Bai** | [Ne1sonBa1](https://github.com/Ne1sonBa1) | Bus stops and passenger boarding, scoring, HUD (score, passengers, next-light signal), results screen, gameplay video, Discord marketing post, final document and submission |

We coordinate on Discord. GitHub activity posts to `#github-feed`.
