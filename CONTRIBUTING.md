# How we work on Fare Play

Two people, one Unity project, zero lost work. The rules below exist mainly to stop the one Unity problem Git can't solve for you: **two people editing the same scene**.

## One-time setup (each of us)

1. Install Git, Git LFS, and the GitHub CLI (`gh`), then run:
   ```bash
   git lfs install
   gh auth login
   git config --global user.name  "Your Name"
   git config --global user.email "you@example.com"   # same email as your GitHub account
   ```
2. Install the Unity version listed in the README (it's also in `ProjectSettings/ProjectVersion.txt`). **Nobody upgrades Unity without agreeing on Discord first.**
3. Check once in Unity: **Edit → Project Settings → Editor**
   - Version Control → Mode: **Visible Meta Files**
   - Asset Serialization → Mode: **Force Text**

## The daily loop

```bash
git switch main
git pull                                  # always start from the latest main
git switch -c feature/12-bus-steering     # one branch per issue

# ...work in Unity, save often (Ctrl/Cmd+S, and File → Save Project)...

git add -A
git commit -m "Add bus steering (#12)"
git push -u origin feature/12-bus-steering
gh pr create --fill                       # or open the PR on github.com
```

Keeping a long-running branch up to date with main:

```bash
git fetch
git merge origin/main
```

### Branch names
`feature/<issue#>-short-name`, `fix/<issue#>-short-name`, `polish/<issue#>-short-name`

### Commits
Small and often. Present tense, and mention the issue: `Add capacity limit to boarding (#8)`.

### Pull requests
- Put `Closes #<issue>` in the description so the issue closes itself on merge.
- Add a screenshot or short GIF for anything visual.
- **The other person reviews:** they pull the branch, press Play, and approve. This is also how each of us stays familiar with the other's code.
- Use **Squash and merge**, then delete the branch.

`main` must always open and play without Console errors.

## Unity rules

1. **Always commit `.meta` files** together with their asset. A missing `.meta` = broken references on the other machine.
2. **Create, rename, and move assets inside Unity's Project window**, never in Finder/Explorer, so the `.meta` follows the file.
3. **Save the project before committing** (File → Save Project). Some prefab/settings changes only hit disk then.
4. Never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/` (the `.gitignore` handles this; don't fight it).
5. Keep big working files (layered PSDs, raw audio sessions, `.blend` scratch files) out of the repo if you can. They go through Git LFS, which has a storage and bandwidth quota on GitHub.

## Avoiding scene conflicts

Git can't meaningfully merge two people's edits to the same `.unity` file. So:

- **`Main.unity` belongs to Lane A** (the person building the world). Lane B only touches it to drop in the `GameSystems` prefab, and claims it first in Discord `#dev` with 🔒 `Main.unity`, then posts 🔓 when that PR is merged.
- **Lane B works in prefabs.** `GameSystems.prefab` holds GameManager, RaceTimer, ScoreManager, the HUD canvas, and the results screen. Editing a prefab doesn't touch the scene file, so both of you can work at the same time.
- **Test in your own sandbox scene:** `Sandbox_A.unity` / `Sandbox_B.unity`. Only the owner edits their sandbox.
- **If a scene conflict happens anyway, don't hand-merge the YAML.** Keep one version and redo the smaller change:
  ```bash
  # while merging main into your branch:
  git checkout --theirs Assets/_FarePlay/Scenes/Main.unity   # keep main's version
  # or --ours to keep your branch's version
  git add Assets/_FarePlay/Scenes/Main.unity
  ```

### Optional: Unity Smart Merge
Unity ships a tool that merges scenes/prefabs much better than plain Git. Add this to your global git config (`git config --global --edit`), fixing the path for your Unity version:

```ini
[merge]
    tool = unityyamlmerge
[mergetool "unityyamlmerge"]
    trustExitCode = false
    # Windows:
    cmd = 'C:\\Program Files\\Unity\\Hub\\Editor\\<VERSION>\\Editor\\Data\\Tools\\UnityYAMLMerge.exe' merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"
    # macOS (use this line instead):
    # cmd = '/Applications/Unity/Hub/Editor/<VERSION>/Unity.app/Contents/Tools/UnityYAMLMerge' merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"
```
Then on a conflict run `git mergetool`.

## Who owns what

| | Lane A: Driving & World | Lane B: Rules & UI |
|---|---|---|
| **Scripts** | `Bus/`, `Camera/`, `Traffic/`, `Route/Route.cs` | `Core/GameManager.cs`, `Game/`, `UI/`, `Route/StartLine`, `FinishLine`, `StopZone`, `BusStop` |
| **Assets** | `Main.unity`, `Bus.prefab`, `TrafficLight.prefab`, route layout | `GameSystems.prefab` (managers + HUD + results), `BusStop.prefab`, `GameTuning.asset` |
| **Places in the world** | Everything. Lane A drags Lane B's prefabs (bus stops, start/finish lines) into the routes. | — |

**The two lanes talk only through `Scripts/Core/GameEvents.cs`.** Example: `TrafficStopLine` (A) calls `GameEvents.RaiseRedLightViolation()` and `ScoreManager` (B) listens for it. Neither needs the other's code to compile, so you can both work from day one. If you need a new event, add it to `GameEvents.cs` in its own tiny PR and merge it quickly.

Subscribe to events in `OnEnable` and unsubscribe in `OnDisable`, or a restarted scene will call destroyed objects.

## Discord conventions

| Channel | Use |
|---|---|
| `#github-feed` | Automatic GitHub notifications (webhook). No chatting here. |
| `#dev` | Async stand-up (what I did / what's next / blocked on), 🔒/🔓 scene claims, quick questions |
| `#playtest` | Builds, GIFs, feedback. Every real piece of feedback becomes a GitHub issue. |

**Talk on Discord, decide on GitHub.** If a Discord conversation changes what we're building, write the decision into the issue so it isn't lost in chat.

## Definition of done

- Works in `Main.unity`, not just a sandbox
- Tunable numbers live in `GameTuning.asset` or the Inspector, not hard-coded
- No errors in the Console
- PR reviewed and played by the other person
- Issue closed (automatically, via `Closes #`)

## Troubleshooting

- **`InvalidOperationException: You are trying to read Input using the UnityEngine.Input class...`:** Unity 6 projects default to the new Input System. The Create with Code scripts use the old `Input.GetAxis`, so set **Edit → Project Settings → Player → Other Settings → Active Input Handling = Both** and let Unity restart.
- **Pink Course Library materials:** the package was made for a different render pipeline. Run **Window → Rendering → Render Pipeline Converter**, tick Material Upgrade, and convert.
- **Pink/missing textures after pulling:** run `git lfs pull`. LFS probably wasn't installed when you cloned.
- **Huge files got committed without LFS:** ask on Discord before fixing. The fix (`git lfs migrate import`) rewrites history and both of us need to re-clone afterwards.
- **Enforcing "no direct pushes to main":** GitHub branch rules on private repos need GitHub Pro, which is free for students via the GitHub Student Developer Pack. Otherwise, it's on the honor system.
