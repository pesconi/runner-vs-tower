#!/usr/bin/env bash
# RT-2 device gate (3x headroom) on the OPPO CPH2791: the measurement APK runs the device-gate level
# (300 units in flight, cannon at its projectile limit, by itself, no touch needed) and is sampled with
# the kit's tools: SurfaceFlinger frame pacing + PSS (stage7-measure.sh) and the kit's frame timing
# probe (CPU main/render thread and GPU per 5 s window), next to this game's [ClaybreakGate] lines
# (units in flight and projectiles in the air at that moment).
# Usage (Git Bash, from the repository root, phone connected with USB debugging):
#   Tools/rt2-gate.sh <label> [fps=60] [seconds=20] [apk=/c/temp/rt2/Claybreak-measure.apk] [world=1]
# (RT-6: world N runs that world's device-gate level: its heaviest tower, its defenses and its scenery.)
# RT-6t.1: world "toonlab:N[:msaa[:hdr]]" runs the toon lab instead: N instanced units of the crowd toon look;
# msaa 1|2|4 and hdr 0|1 override the pipeline asset (left out = project settings). Example:
#   Tools/rt2-gate.sh toon1000-m4-h1 60 20 /c/temp/rt4/Claybreak-measure.apk toonlab:1000:4:1
# The toon lab gate also needs frame p95 <= 16.7 ms at 60 fps; [DevFps] lines give the presented rate.
# RT-6t.4: world "run:N" measures the RUN of world N's device-gate level instead of its assault: the instanced crowd at
# its cap (D27: 300) running on the track with the world's scenery, looping back before the tower. Same gate as the toon lab
# (frame p95 <= 16.7 ms at 60 fps). Example:
#   Tools/rt2-gate.sh run-w3 60 20 /c/temp/rt4/Claybreak-measure.apk run:3
# Gate (SISTEMA.md 5.4 of the studio): CPU main p95 <= 11 ms, GPU p95 <= 11 ms, frame p95 <= 33.3 ms,
# < 5% of frames over 50 ms, PSS <= 450 MB, with inFlight=300 and projectiles at the level's limit.
set -euo pipefail
LABEL=${1:?label}
FPS=${2:-60}
SECONDS_TO_SAMPLE=${3:-20}
APK=${4:-/c/temp/rt2/Claybreak-measure.apk}
WORLD=${5:-1}
PKG=com.jungledegree.runnervstower
KIT_TOOLS="$(cd "$(dirname "$0")/../../studiokit/com.jungledegree.studiokit/Tools~" && pwd)"
OUT=/c/temp/rt2
mkdir -p "$OUT"

adb get-state >/dev/null
adb install -r "$APK" >/dev/null
adb shell am force-stop "$PKG"
adb logcat -c
ACTIVITY=$(adb shell cmd package resolve-activity --brief "$PKG" | tr -d '\r' | tail -n 1)
if [[ "$WORLD" == toonlab:* ]]; then
  IFS=: read -r _ UNITS MSAA HDR <<<"$WORLD"
  EXTRAS=(--ez toonlab true --ei units "${UNITS:-1000}")
  if [ -n "${MSAA:-}" ]; then EXTRAS+=(--ei msaa "$MSAA"); fi
  if [ -n "${HDR:-}" ]; then
    if [ "$HDR" = 1 ]; then EXTRAS+=(--ez hdr true); else EXTRAS+=(--ez hdr false); fi
  fi
  adb shell am start -n "$ACTIVITY" "${EXTRAS[@]}" --ei fps "$FPS" >/dev/null
  # Load, then the switch to the lab scene: the crowd is complete from its first frame.
  sleep "${RT2_SETTLE_SECONDS:-8}"
elif [[ "$WORLD" == run:* ]]; then
  adb shell am start -n "$ACTIVITY" --ez stress true --ez run true --ei world "${WORLD#run:}" --ei fps "$FPS" >/dev/null
  # Load, auto start (1 s), the crowd at its cap from the first frame of the run.
  sleep "${RT2_SETTLE_SECONDS:-8}"
else
  adb shell am start -n "$ACTIVITY" --ez stress true --ei world "$WORLD" --ei fps "$FPS" >/dev/null
  # Load, auto start (1 s), 300 units launched in 2 s, first wave in flight: sample after it settles.
  sleep "${RT2_SETTLE_SECONDS:-14}"
fi
"$KIT_TOOLS/stage7-measure.sh" "rt2-$LABEL" "$SECONDS_TO_SAMPLE" "$PKG" | tee "$OUT/gate-$LABEL.txt"
adb logcat -d -s Unity | tr -d '\r' \
  | grep -E '\[DevTiming\] window|\[ClaybreakGate\]|\[Claybreak\]|\[ToonLab\]|\[DevFps\]' \
  | tail -n 24 | tee -a "$OUT/gate-$LABEL.txt"
echo "Saved: $OUT/gate-$LABEL.txt"
