#!/usr/bin/env bash
# =====================================================================
#  Fare Play - GitHub setup (one-week plan, due Fri Oct 2 2026, 1 PM)
#
#  Run by ONE teammate, from the root of the Unity project
#  (the folder that contains Assets/, Packages/ and ProjectSettings/):
#
#      bash Tools/setup_github.sh
#
#  On Windows, run it from "Git Bash" (installed with Git for Windows).
#
#  Safe to re-run: every step skips what's already done. Re-run it after your
#  teammate accepts the invite so their issues get assigned to them.
#
#  What it does:
#    1. Checks tools, the Unity project, and that Unity has made every .meta file
#    2. Creates the repo + first commit (with Git LFS), or commits the new kit files
#    3. Creates/pushes the GitHub repo and invites your teammate
#    4. Retires the old 12-week plan if it's there, then creates the one-week
#       milestones and issues, and assigns each lane's issues
#    5. Optional: a GitHub Project board with every open issue on it
#    6. Optional: a Discord webhook so GitHub activity posts to a channel
# =====================================================================
set -eo pipefail

say()  { printf '\n\033[1;36m==> %s\033[0m\n' "$*"; }
ok()   { printf '   \033[32m+\033[0m %s\n' "$*"; }
skip() { printf '   \033[90m= %s\033[0m\n' "$*"; }
warn() { printf '   \033[33m!\033[0m %s\n' "$*"; }
die()  { printf '\n\033[31mError:\033[0m %s\n\n' "$*" >&2; exit 1; }

# ask "Question" "default"  -> prints the answer (or the default)
ask() {
  local prompt="$1" default="$2" reply=""
  if [ -n "$default" ]; then prompt="$prompt [$default]"; fi
  read -r -p "   $prompt: " reply || true
  printf '%s' "${reply:-$default}"
}

# Refuse to commit Unity's cache folders.
guard_staged() {
  if git diff --cached --name-only | grep -qiE '^(library|temp|logs|obj|usersettings)/'; then
    git reset -q 2>/dev/null || git rm -r -q --cached . >/dev/null
    die "Unity's Library/Temp folders were about to be committed, so .gitignore isn't working.
   Make sure .gitignore is in the project root (next to Assets/)."
  fi
}

# ---------------------------------------------------------------------
say "1/6  Checking tools and project folder"
# ---------------------------------------------------------------------
command -v git >/dev/null 2>&1 || die "git not found. Install it from https://git-scm.com"
git lfs version >/dev/null 2>&1 || die "Git LFS not found. Install it from https://git-lfs.com"
command -v gh >/dev/null 2>&1  || die "GitHub CLI not found. Install it from https://cli.github.com, then run: gh auth login"
gh auth status >/dev/null 2>&1 || die "GitHub CLI isn't logged in. Run: gh auth login"
git config user.email >/dev/null 2>&1 || die "Git doesn't know who you are yet. Run:
   git config --global user.name  \"Your Name\"
   git config --global user.email \"you@example.com\""
ok "git, git-lfs and gh are ready"

if [ ! -d Assets ] || [ ! -d ProjectSettings ]; then
  die "Run this from your Unity project root (the folder with Assets/ and ProjectSettings/).
   Open the project in Unity once first so those folders exist."
fi
if [ ! -f .gitignore ] || [ ! -f .gitattributes ]; then
  die ".gitignore / .gitattributes are missing from this folder.
   Copy them in from the starter kit. They are hidden files: on a Mac press Cmd+Shift+. in Finder to see them."
fi
ok "Unity project found"

if [ -f ProjectSettings/EditorSettings.asset ] && ! grep -q 'm_SerializationMode: 2' ProjectSettings/EditorSettings.asset; then
  die "Unity's Asset Serialization isn't set to 'Force Text'.
   In Unity: Edit > Project Settings > Editor > Asset Serialization > Mode = Force Text. Save, close Unity, re-run."
fi

# Every file and folder Unity sees must have a .meta, or each of you will get different GUIDs.
MISSING_META=$(find Assets -mindepth 1 -not -name '*.meta' -not -name '.*' -not -path '*/.*' \
                 -not -name '*~' -not -path '*~/*' 2>/dev/null |
               while IFS= read -r f; do [ -e "$f.meta" ] || printf '%s\n' "$f"; done | head -n 5)
if [ -n "$MISSING_META" ]; then
  die "Unity hasn't created .meta files for these yet:
$(printf '%s\n' "$MISSING_META" | sed 's/^/      /')
   Open the project in Unity, wait until importing finishes, close Unity, then re-run."
fi
ok "Every asset has its .meta file"

UNITY_VERSION=""
if [ -f ProjectSettings/ProjectVersion.txt ]; then
  UNITY_VERSION=$(sed -n 's/^m_EditorVersion: *//p' ProjectSettings/ProjectVersion.txt | tr -d '\r')
fi
if [ -n "$UNITY_VERSION" ] && [ -f README.md ] && grep -q 'UNITY_VERSION_HERE' README.md; then
  sed -i.bak "s/UNITY_VERSION_HERE/$UNITY_VERSION/" README.md && rm -f README.md.bak
  ok "Wrote Unity version $UNITY_VERSION into README.md"
fi

# ---------------------------------------------------------------------
say "2/6  Local repository"
# ---------------------------------------------------------------------
if [ ! -d .git ]; then
  git init -q -b main 2>/dev/null || { git init -q && git checkout -q -b main; }
  ok "Created git repo on branch main"
else
  skip "Git repo already exists"
fi
git lfs install >/dev/null
ok "Git LFS enabled"

if ! git rev-parse --verify -q HEAD >/dev/null; then
  git add -A
  guard_staged
  git commit -q -m "Initial commit: Unity project, Course Library, script skeletons, repo setup"
  LFS_COUNT=$(git lfs ls-files | wc -l | tr -d ' ')
  ok "First commit created ($LFS_COUNT file(s) stored with Git LFS)"
elif [ -n "$(git status --porcelain)" ]; then
  warn "You have uncommitted changes:"
  git status --short | head -n 15 | sed 's/^/      /'
  if [ "$(ask "Commit ALL of these now as 'Switch to the one-week plan'? (y/n)" "y")" = "y" ]; then
    git add -A
    guard_staged
    git commit -q -m "Switch to the one-week plan: new README, lanes, script skeletons"
    ok "Committed"
  else
    warn "Left them uncommitted. Commit them yourself before your teammate pulls."
  fi
else
  skip "Nothing new to commit"
fi

# ---------------------------------------------------------------------
say "3/6  GitHub repository and teammate"
# ---------------------------------------------------------------------
if git remote get-url origin >/dev/null 2>&1; then
  REPO=$(gh repo view --json nameWithOwner -q .nameWithOwner)
  skip "Using existing GitHub repo: $REPO"
  if git push -q -u origin HEAD 2>/dev/null; then ok "Pushed $(git rev-parse --abbrev-ref HEAD)"
  else warn "Couldn't push the current branch (the histories may differ). Pull first, then push."; fi
else
  NAME=$(ask "Repository name" "fare-play")
  VIS=$(ask "Visibility: private or public" "private")
  case "$VIS" in private|public) ;; *) die "Visibility must be 'private' or 'public'." ;; esac
  gh repo create "$NAME" "--$VIS" --source=. --remote=origin --push \
    --description "Fare Play: a bus game about speed vs. passengers" >/dev/null
  REPO=$(gh repo view --json nameWithOwner -q .nameWithOwner)
  ok "Created and pushed https://github.com/$REPO"
fi
OWNER="${REPO%%/*}"
ME=$(gh api user --jq .login)

MATE=$(ask "Teammate's GitHub username (blank to skip)" "")
MATE="${MATE#@}"
if [ -n "$MATE" ]; then
  if gh api -X PUT "repos/$REPO/collaborators/$MATE" -f permission=push >/dev/null 2>&1; then
    ok "Invited @$MATE (or they already have access)."
  else
    warn "Couldn't invite @$MATE. Add them in the repo's Settings > Collaborators."
  fi
fi

echo "   Who takes which lane? (see CONTRIBUTING.md > Who owns what)"
LANE_A=$(ask "Lane A, Driving & World (bus, cameras, routes, lights)" "$ME");  LANE_A="${LANE_A#@}"
LANE_B=$(ask "Lane B, Rules & UI (timer, stops, score, HUD, build)" "$MATE"); LANE_B="${LANE_B#@}"

# ---------------------------------------------------------------------
say "4/6  Labels, milestones and issues"
# ---------------------------------------------------------------------
label() { gh label create "$1" --color "$2" --description "$3" --repo "$REPO" --force >/dev/null; }
label "must"               "b60205" "Required for submission"
label "should"             "fbca04" "Planned; cut it if we fall behind"
label "stretch"            "c5def5" "Only if we're ahead of schedule"
label "setup"              "ededed" "Project setup"
label "bug"                "d73a4a" "Something is broken"
label "lane:driving-world" "1d76db" "Lane A: bus, cameras, routes, lights, obstacles"
label "lane:rules-ui"      "d93f0b" "Lane B: game flow, timer, stops, score, HUD, build"
label "lane:both"          "5319e7" "Both of us"
ok "Labels ready"

# --- Retire the old 12-week plan, if this repo has it ---
OLD_TITLES="M1 - Driving & Circuit
M2 - Passengers & Capacity
M3 - Timer, Scoring, Obstacles & Signals
M4 - Routes, UI, Sound & VFX
M5 - Balance, Polish & Final Build
Backlog - Stretch Goals"
OPEN_MS=$(gh api "repos/$REPO/milestones?state=open&per_page=100" --jq '.[] | "\(.number)\t\(.title)"')
OLD_OPEN=$(printf '%s\n' "$OPEN_MS" | while IFS=$'\t' read -r num title; do
             if [ -n "$title" ] && printf '%s\n' "$OLD_TITLES" | grep -Fxq -- "$title"; then
               printf '%s\t%s\n' "$num" "$title"
             fi
           done)
RETIRED=""
if [ -n "$OLD_OPEN" ]; then
  warn "Found the old 12-week plan ($(printf '%s\n' "$OLD_OPEN" | wc -l | tr -d ' ') open milestones)."
  if [ "$(ask "Retire it? Its open issues get closed as 'not planned' and its milestones closed. (y/n)" "y")" = "y" ]; then
    while IFS=$'\t' read -r num title; do
      for n in $(gh issue list --repo "$REPO" --milestone "$title" --state open --limit 200 \
                   --json number --jq '.[].number' </dev/null); do
        gh issue close "$n" --repo "$REPO" --reason "not planned" \
          --comment "Replaced by the one-week plan." >/dev/null </dev/null
        RETIRED="$RETIRED $n"
      done
      gh api -X PATCH "repos/$REPO/milestones/$num" -f state=closed >/dev/null </dev/null
      ok "Retired: $title"
    done <<< "$OLD_OPEN"
  fi
fi

# --- One-week milestones ---
M0="0 - Setup (Fri 9/25)"
M1="1 - First Playable (Sun 9/27)"
M2="2 - Feature Complete (Tue 9/29)"
M3="3 - Build Online + Playtest (Wed 9/30)"
M4="4 - Final Build & Submit (Fri 10/2, 1 PM)"
MS="Stretch - Only If Ahead"

EXISTING_MS=$(gh api "repos/$REPO/milestones?state=all&per_page=100" --jq '.[].title')
milestone() {
  if printf '%s\n' "$EXISTING_MS" | grep -Fxq -- "$1"; then skip "Milestone exists: $1"; return; fi
  gh api "repos/$REPO/milestones" -f title="$1" -f due_on="$2T12:00:00Z" -f description="$3" >/dev/null
  ok "Milestone: $1"
}
milestone "$M0" "2026-09-25" "Repo, Unity project and Course Library ready; both of us can open it and press Play."
milestone "$M1" "2026-09-27" "A complete run of Route 1: overview, drive, start line, timer, bus stops, score, finish."
milestone "$M2" "2026-09-29" "Traffic lights, signal HUD, Route 2, results screen, restart. Every Must and Should is in."
milestone "$M3" "2026-09-30" "A WebGL build at a public URL, playtested, balanced. Decide tonight on the stretch goal."
milestone "$M4" "2026-10-02" "Feature freeze Thu 6 PM. Final build published Thu night. Submit Fri morning, before 1 PM."
milestone "$MS" "2026-10-01" "Only start these if Milestone 3 is done by Wed night."

EXISTING_ISSUES=$(gh issue list --repo "$REPO" --state all --limit 500 --json title --jq '.[].title')
# issue MILESTONE LABELS TITLE WHAT [done-when items...]
issue() {
  local ms="$1" labels="$2" title="$3" what="$4" body item
  shift 4
  if printf '%s\n' "$EXISTING_ISSUES" | grep -Fxq -- "$title"; then skip "Issue exists: $title"; return; fi
  body="## What"$'\n'"$what"$'\n\n'"## Done when"
  for item in "$@"; do body="$body"$'\n'"- [ ] $item"; done
  case "$labels" in *setup*) ;; *) body="$body"$'\n'"- [ ] Works in Main.unity with no Console errors" ;; esac
  gh issue create --repo "$REPO" --milestone "$ms" --label "$labels" --title "$title" --body "$body" >/dev/null
  ok "$title"
}

# ---------------- 0 - Setup (Fri 9/25) ----------------
issue "$M0" "setup,must,lane:driving-world" "Set up the Unity project and import the Course Library" \
  "One Unity project for both of us, built on the Create with Code Unit 1 assets (vehicles, obstacles, road)." \
  "Unity 6 project created with the Universal 3D template" \
  "Prototype 1 starter package imported (Create with Code Lesson 1.1). Course Library is under Assets/. The .unitypackage file itself is NOT committed." \
  "No pink materials. If there are any: Window > Rendering > Render Pipeline Converter > Material Upgrade" \
  "Player Settings > Active Input Handling = Both (the Unit 1 code uses Input.GetAxis)" \
  "Scenes created: Assets/_FarePlay/Scenes/Main.unity, Sandbox_A.unity, Sandbox_B.unity" \
  "GameTuning asset created: Create > Fare Play > Game Tuning, saved in Assets/_FarePlay/Settings/" \
  "Tag Obstacle created (Tags & Layers)" \
  "Committed and pushed. Yellow 'field is never used' warnings from the skeleton scripts are expected until the TODOs are filled in."
issue "$M0" "setup,must,lane:both" "Teammate: clone, open and press Play" \
  "Make sure the second machine works before Saturday's work starts." \
  "Invite accepted" \
  "git lfs install, then git clone" \
  "Project opens in the same Unity version with no errors, and Play works"

# ---------------- 1 - First Playable (Sun 9/27) ----------------
issue "$M1" "must,lane:driving-world" "Bus driving: throttle, brake/reverse, steering" \
  "Build Scripts/Bus/BusController.cs. Start from your Unit 1 car PlayerController, but drive the Rigidbody so obstacles block the bus and speed can be measured." \
  "Bus.prefab made from the biggest Course Library vehicle (scale it up so it reads as a bus)" \
  "Up accelerates, Down brakes then reverses, Left/Right steer only while moving" \
  "CurrentSpeed is accurate (bus stops depend on it)" \
  "Input is locked during the overview (InputEnabled)" \
  "Rigidbody rotation X/Z frozen so the bus can't tip over"
issue "$M1" "must,lane:driving-world" "Chase camera behind the bus" \
  "The CameraDirector chase pose: behind and above the bus, looking a bit ahead. This is the Unit 1 FollowPlayer idea, smoothed out." \
  "Smooth follow with no jitter at full speed" \
  "You can see far enough ahead to spot bus stops and lights in time"
issue "$M1" "must,lane:driving-world" "Overview camera for 30 s, then switch to the chase view" \
  "The game opens on a top-down view of the whole map. After 30 s (GameTuning.overviewSeconds) the camera swoops down behind the bus." \
  "OverviewPose object placed so both routes are fully in frame" \
  "Switch is a smooth blend of about 1.5 s, not a cut" \
  "Bus can't move until the switch"
issue "$M1" "must,lane:driving-world" "Route 1 blockout in Main.unity" \
  "Build Route 1 from Course Library road pieces (duplicate and rotate for corners) with obstacles as walls and props." \
  "Depot where the bus spawns, then a StartLine across the route entrance" \
  "Two bus stop bays (aligned with the world axes) and a spot for one traffic light, with a stop placed just before the light" \
  "Shared FinishLine" \
  "Walls/props tagged Obstacle, so the bus can't leave the route" \
  "Driving it cleanly with no stops takes about 60 s, which leaves room in 1:30 for stops and a red light"
issue "$M1" "must,lane:driving-world" "Collision penalty when the bus hits something" \
  "Scripts/Bus/BusCollisionPenalty.cs: hitting anything tagged Obstacle slows the bus and raises GameEvents.CollisionPenalty." \
  "One penalty per hit (1 s cooldown), so scraping a wall doesn't count 60 times" \
  "Only counts while the timer is running"
issue "$M1" "must,lane:rules-ui" "Game flow: Overview -> Ready -> Driving -> Won / Failed" \
  "Scripts/Core/GameManager.cs. It owns the state and raises GameEvents.StateChanged. Build GameSystems.prefab (GameManager + RaceTimer + ScoreManager) and test it in Sandbox_B." \
  "Overview counts down 30 s, then Ready" \
  "StartRun / FinishRun / FailRun work and ignore calls in the wrong state" \
  "Restart reloads the scene cleanly (no duplicate event listeners)"
issue "$M1" "must,lane:rules-ui" "Start lines, finish line and the 1:30 route timer" \
  "StartLine, FinishLine and RaceTimer. The timer starts when the bus crosses either route's start line and fails the run at 0:00." \
  "Only the first start line crossed counts" \
  "Time limit comes from the Route (default 90 s), so each route can have its own" \
  "Timer stops on Won/Failed"
issue "$M1" "must,lane:rules-ui" "Bus stops: stop properly in the bay to board passengers" \
  "StopZone + BusStop. Passengers board only when the whole bus is inside the bay AND stopped. Drive away early and the rest stay behind." \
  "Bay tint: white = empty, yellow = partly in or still moving, green = boarding" \
  "One passenger boards every 0.75 s (GameTuning) and their capsule disappears" \
  "Raises GameEvents.PassengerBoarded for each one" \
  "BusStop.prefab with 2-5 capsule passengers, ready for Lane A to place"
issue "$M1" "must,lane:rules-ui" "Scoring: passengers, penalties and time bonus" \
  "ScoreManager listens to the events and builds a RunResult. Values come from GameTuning: +100 per passenger delivered, -50 per red light, -25 per hit, +10 per second left at the finish." \
  "LiveScore updates during the run" \
  "BuildResult fills in every field, with a time bonus only on a win" \
  "Failed run = total 0; the total never goes below 0"
issue "$M1" "must,lane:rules-ui" "HUD v1: timer, score, passengers" \
  "Screen-space canvas inside GameSystems.prefab, so UI work never touches Main.unity." \
  "Timer mm:ss, turns red under 15 s" \
  "Live score and passengers on board" \
  "Readable at 1920x1080 and in a small browser window"

# ---------------- 2 - Feature Complete (Tue 9/29) ----------------
issue "$M2" "must,lane:driving-world" "Traffic lights: cycle, stop line and red-light violations" \
  "TrafficLight + TrafficStopLine. The light cycles green 8 s / yellow 2 s / red 8 s. Crossing the stop line on red raises GameEvents.RedLightViolation, once per light. Running the red is allowed; it just costs points." \
  "TrafficLight.prefab with three lamps; the lit lamp is obvious from the chase camera" \
  "Violation only on red (yellow is fine), once per light" \
  "Route.lightsInOrder filled in, and MarkLightPassed called so the HUD moves on"
issue "$M2" "must,lane:driving-world" "Bus stop right before each traffic light" \
  "The signature strategy: when a light is red, the player can pull into the stop just before it and board passengers instead of idling." \
  "Every route has at least one stop within about 20 m before a light" \
  "Tested: waiting through a red at that stop boards several passengers"
issue "$M2" "should,lane:driving-world" "Route 2: an alternative route with a different tradeoff" \
  "A second route from the depot to the same finish. Make it a real choice: for example, shorter with more obstacles and fewer passengers." \
  "Own StartLine, stops, light and obstacles" \
  "Both routes are fully visible in the overview" \
  "Time limit set per route and tested"
issue "$M2" "must,lane:rules-ui" "HUD: always-visible signal indicator for the next light" \
  "A lamp icon plus text like 'RED 6s' for the next light on the active route, on screen the whole run. This is what lets players time their stops around reds." \
  "Tint and countdown are correct, and it updates after each light is passed" \
  "'No signals ahead' after the last light"
issue "$M2" "must,lane:rules-ui" "Results screen: win/fail, score breakdown, restart" \
  "The end-of-run panel from ResultsScreen.cs, listening to GameEvents.RunEnded." \
  "Title: 'Route complete!' or 'Out of time'" \
  "Breakdown lines: passengers, red lights, collisions, time bonus, total" \
  "Restart button works"
issue "$M2" "should,lane:rules-ui" "Overview screen: instructions, countdown, Space to skip" \
  "Graders will replay this. Nobody wants to wait 30 s every time." \
  "Shows 'Driving starts in 23s (Space to skip)' and the controls" \
  "Space skips straight to the chase camera"

# ---------------- 3 - Build Online + Playtest (Wed 9/30) ----------------
issue "$M3" "must,lane:rules-ui" "First WebGL build published at a public URL" \
  "Publish early so build problems show up Wednesday, not Thursday night. Use Unity Play or GitHub Pages (the same hosts as Assignment 1)." \
  "File > Build Profiles: Web platform, Main.unity is scene 0" \
  "For GitHub Pages: Player Settings > Publishing Settings > Compression Format = Disabled (or turn on Decompression Fallback). Host the build in a separate PUBLIC repo, not this one." \
  "Played start to finish in a browser from the public URL"
issue "$M3" "must,lane:both" "Playtest and balance pass" \
  "At least 3 full runs per route by someone who isn't us. Tune only in GameTuning.asset and the Inspector." \
  "Time limit feels tight but fair: finishing with some stops is possible" \
  "Stopping at a red to board passengers is actually worth it" \
  "Neither route is strictly better than the other" \
  "Final numbers written into this issue"
issue "$M3" "must,lane:both" "Bug bash" \
  "Both of us play every route trying to break it. File each bug as an issue." \
  "No known crash or blocker left" \
  "Decide tonight: stretch goals Thursday, or bug fixes only?"

# ---------------- 4 - Final Build & Submit (Fri 10/2, 1 PM) ----------------
issue "$M4" "must,lane:rules-ui" "Feature freeze Thu 6 PM + final build published" \
  "After 6 PM Thursday: bug fixes only. Publish the final build the same night." \
  "Final WebGL build at the public URL, played start to finish" \
  "README updated with the final controls, scoring and play link" \
  "Release tagged v1.0 on GitHub"
issue "$M4" "must,lane:both" "Submit Assignment 2 on Brightspace (before Fri 1 PM)" \
  "Check Assignment 2's own deliverable list. Assignment 1 wanted a ZIP with a .txt of public URLs." \
  "Every deliverable Assignment 2 asks for, checked off against its PDF" \
  "Design doc updated to the new design, if it's required (the original proposal changed)" \
  "Submitted Friday morning, not at 12:55"

# ---------------- Stretch ----------------
issue "$MS" "stretch,lane:driving-world" "Fuel: the throttle drains a fuel bar" \
  "Holding the throttle burns fuel (GameTuning.fuelPerSecondAtFullThrottle). With an empty tank the bus can only coast (BusController.HasFuel = false)." \
  "HUD fuel bar (the fuelFill Image is already on the HUD)" \
  "Tank size means the long route needs one refuel"
issue "$MS" "stretch,lane:rules-ui" "Fuel station: refuel while stopped" \
  "Reuse StopZone: while the bus is stopped inside, refill at GameTuning.refuelPerSecond. Refueling costs time, which is the tradeoff." \
  "FuelStation.prefab placed on at least one route"
issue "$MS" "stretch,lane:rules-ui" "Passenger capacity limit" \
  "GameTuning.busCapacity: boarding stops when the bus is full. Makes choosing which stops to take a real decision." \
  "HUD shows passengers / capacity"
issue "$MS" "stretch,lane:both" "Sounds: engine, boarding, crash, red-light buzzer" \
  "Quick audio feedback that makes the game feel finished." \
  "Engine pitch follows speed" \
  "A ding per passenger, a thud on a hit, a buzzer on a red-light violation"

UNASSIGNABLE=""
# --- Assign each lane's open issues (re-runnable: only adds missing assignees) ---
assign_label() {
  local lbl="$1" user="$2" n
  [ -n "$user" ] || return 0
  for n in $(gh issue list --repo "$REPO" --label "$lbl" --state open --limit 200 --json number,assignees \
               --jq ".[] | select([.assignees[].login] | index(\"$user\") | not) | .number" </dev/null); do
    if ! gh issue edit "$n" --repo "$REPO" --add-assignee "$user" >/dev/null 2>&1 </dev/null; then
      case " $UNASSIGNABLE " in *" $user "*) ;; *)
        warn "Couldn't assign @$user yet (invite not accepted?). Re-run this script after they accept."
        UNASSIGNABLE="$UNASSIGNABLE $user" ;;
      esac
      return 0
    fi
  done
  ok "Assigned '$lbl' issues to @$user"
}
assign_label "lane:driving-world" "$LANE_A"
assign_label "lane:rules-ui"      "$LANE_B"
assign_label "lane:both"          "$LANE_A"
assign_label "lane:both"          "$LANE_B"

# ---------------------------------------------------------------------
say "5/6  Project board (optional)"
# ---------------------------------------------------------------------
setup_board() {
  if ! gh auth status 2>&1 | grep -q "'project'"; then
    warn "The GitHub CLI needs the 'project' permission. Opening the login flow..."
    gh auth refresh -s project || return 1
  fi
  local info num url
  info=$(gh project list --owner "$OWNER" --format json \
          --jq '.projects[] | select(.title=="Fare Play") | "\(.number) \(.url)"' | head -n1)
  if [ -z "$info" ]; then
    info=$(gh project create --owner "$OWNER" --title "Fare Play" --format json --jq '"\(.number) \(.url)"') || return 1
    ok "Created project board"
  else
    skip "Project board already exists"
  fi
  num="${info%% *}"; url="${info#* }"
  [ -n "$num" ] || return 1
  gh project link "$num" --owner "$OWNER" --repo "${REPO#*/}" >/dev/null 2>&1 || true

  # Take the retired old-plan issues off the board.
  if [ -n "$RETIRED" ]; then
    gh project item-list "$num" --owner "$OWNER" --limit 500 --format json \
       --jq '.items[] | select(.content.number != null) | "\(.id)\t\(.content.number)"' 2>/dev/null |
    while IFS=$'\t' read -r item_id inum; do
      case " $RETIRED " in
        *" $inum "*) gh project item-archive "$num" --owner "$OWNER" --id "$item_id" >/dev/null 2>&1 </dev/null || true ;;
      esac
    done
    ok "Archived the old plan's cards"
  fi

  gh issue list --repo "$REPO" --state open --limit 500 --json url --jq '.[].url' |
    while read -r issue_url; do
      gh project item-add "$num" --owner "$OWNER" --url "$issue_url" >/dev/null 2>&1 </dev/null || true
    done
  ok "All open issues are on the board: $url"
  BOARD_URL="$url"
}
BOARD_URL=""
if [ "$(ask "Create/update the 'Fare Play' project board? (y/n)" "y")" = "y" ]; then
  setup_board || warn "Board setup didn't finish. You can create one on GitHub under your profile > Projects."
fi

# ---------------------------------------------------------------------
say "6/6  Discord feed (optional)"
# ---------------------------------------------------------------------
echo "   In Discord: channel settings (gear) > Integrations > Webhooks > New Webhook > Copy Webhook URL"
HOOK=$(ask "Paste the Discord webhook URL (blank to skip)" "")
if [ -n "$HOOK" ]; then
  case "$HOOK" in
    https://discord.com/api/webhooks/*|https://discordapp.com/api/webhooks/*|https://*.discord.com/api/webhooks/*)
      HOOK="${HOOK%/}"
      case "$HOOK" in */github) ;; *) HOOK="$HOOK/github" ;; esac
      if gh api "repos/$REPO/hooks" --jq '.[].config.url' 2>/dev/null | grep -Fxq -- "$HOOK"; then
        skip "That Discord webhook is already connected"
      elif gh api "repos/$REPO/hooks" -f name=web -F active=true \
            -f "config[url]=$HOOK" -f "config[content_type]=json" \
            -f "events[]=push" -f "events[]=pull_request" -f "events[]=pull_request_review" \
            -f "events[]=issues" -f "events[]=issue_comment" -f "events[]=release" >/dev/null 2>&1; then
        ok "Discord will now get pushes, PRs, reviews, issues, comments and releases"
      else
        warn "Couldn't add the webhook. Add it manually: repo Settings > Webhooks (URL + /github, content type application/json)."
      fi
      ;;
    *) warn "That doesn't look like a Discord webhook URL, so I skipped it." ;;
  esac
fi

# ---------------------------------------------------------------------
say "All set!"
# ---------------------------------------------------------------------
echo "   Repo:        https://github.com/$REPO"
echo "   Milestones:  https://github.com/$REPO/milestones"
[ -n "$BOARD_URL" ] && echo "   Board:       $BOARD_URL"
echo
echo "   Teammate: accept the invite, then run"
echo "      git lfs install && git clone https://github.com/$REPO.git"
echo "   (Already cloned? Just: git pull)"
echo
