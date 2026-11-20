#!/usr/bin/env bash
# RT-2 device gate (3x headroom) on the OPPO CPH2791: the measurement APK runs the device-gate level
# (300 units in flight, cannon at its projectile limit, by itself, no touch needed) and is sampled with
# the kit's tools: SurfaceFlinger frame pacing + PSS (stage7-measure.sh) and the kit's frame timing
# probe (CPU main/render thread and GPU per 5 s window), next to this game's [ClaybreakGate] lines
# (units in flight and projectiles in the air at that moment).
# Usage (Git Bash, from the repository root, phone connected with USB debugging):
#   Tools/rt2-gate.sh <label> [fps=60] [seconds=20] [apk=/c/temp/rt2/Claybreak-measure.apk]
# Gate (SISTEMA.md 5.4 of the studio): CPU main p95 <= 11 ms, GPU p95 <= 11 ms, frame p95 <= 33.3 ms,
# < 5% of frames over 50 ms, PSS <= 450 MB, with inFlight=300 and projectiles at the level's limit.
set -euo pipefail
LABEL=${1:?label}
FPS=${2:-60}
SECONDS_TO_SAMPLE=${3:-20}
APK=${4:-/c/temp/rt2/Claybreak-measure.apk}
PKG=com.jungledegree.runnervstower
KIT_TOOLS="$(cd "$(dirname "$0")/../../studiokit/com.jungledegree.studiokit/Tools~" && pwd)"
OUT=/c/temp/rt2
mkdir -p "$OUT"

adb get-state >/dev/null
adb install -r "$APK" >/dev/null
adb shell am force-stop "$PKG"
adb logcat -c
ACTIVITY=$(adb shell cmd package resolve-activity --brief "$PKG" | tr -d '\r' | tail -n 1)
adb shell am start -n "$ACTIVITY" --ez stress true --ei fps "$FPS" >/dev/null
# Load, auto start (1 s), 300 units launched in 2 s, first wave in flight: sample after it settles.
sleep "${RT2_SETTLE_SECONDS:-14}"
"$KIT_TOOLS/stage7-measure.sh" "rt2-$LABEL" "$SECONDS_TO_SAMPLE" "$PKG" | tee "$OUT/gate-$LABEL.txt"
adb logcat -d -s Unity | tr -d '\r' | grep -E '\[DevTiming\] window|\[ClaybreakGate\]|\[Claybreak\]' \
  | tail -n 16 | tee -a "$OUT/gate-$LABEL.txt"
echo "Saved: $OUT/gate-$LABEL.txt"
