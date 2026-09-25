# 🚌 Fare Play

An isometric bus game about **speed vs. service**. Study the routes from above, then drive: pick up passengers, stop properly at bus stops, deal with traffic lights, and reach the finish before the clock runs out.

> Original proposal: [`Docs/Fare_Play_Design_Document.pdf`](Docs/Fare_Play_Design_Document.pdf). This README describes the **current** design, which replaced it.

## How it plays

1. **Overview (30 s):** a top-down view of the route(s), with the bus locked in the depot. Press **Space** to skip.
2. **Chase view:** the camera swoops behind the bus and you can drive.
3. **Start line:** the **1:30 timer** starts when the bus crosses either route's start line. If it hits 0:00, the run fails.
4. **Bus stops:** pull **fully into the bay and stop**. The bay turns green and passengers board one at a time. Drive away early and the rest stay behind.
5. **Traffic lights:** the next light's state and countdown are always on screen. Waiting at a red? Stop at the bus stop just before it and board passengers with that time. Running a red is allowed, but it costs points.
6. **Obstacles:** every hit costs points.
7. **Finish line:** delivered passengers are counted, and each second left on the clock becomes bonus points.

### Controls
| Key | Action |
|---|---|
| ↑ / ↓ | Throttle / brake, then reverse |
| ← / → | Steer |
| Space | Skip the overview |

### Scoring (all values live in `GameTuning.asset`)
| | Points |
|---|---|
| Each passenger delivered to the finish | **+100** |
| Each red light run | **−50** |
| Each obstacle hit (1 s cooldown) | **−25** |
| Time bonus at the finish | **+10 × seconds left** |
| Run out of time | **Run failed, score 0** |

Example: 8 passengers, one red light, two hits, 12 s left → 800 − 50 − 50 + 120 = **820**

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
   git clone https://github.com/<owner>/fare-play.git
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

- **Ruchir Bhatia**
- **(teammate)**

We coordinate on Discord. GitHub activity posts to `#github-feed`.
