#!/usr/bin/env bash
# RT-6t.1 toon lab gate matrix on the OPPO CPH2791: N units (default 1000) with MSAA 1x/4x and HDR off/on, plus the
# record-only run (default 2000 units, MSAA 4x, HDR on), each through Tools/rt2-gate.sh (toonlab mode), then a
# screenshot of the lab and the app closed. Phone connected with USB debugging and unlocked.
# Usage (Git Bash, repository root):
#   Tools/rt6t-toonlab-gate.sh [apk=/c/temp/rt6t1/Claybreak-measure.apk] [units=1000] [record=2000] [prefix=toon]
# Results: /c/temp/rt2/gate-<prefix><units>-m<msaa>-h<hdr>.txt; screenshot docs/art/rt6t/toonlab-<units>.png.
set -euo pipefail
APK=${1:-/c/temp/rt6t1/Claybreak-measure.apk}
UNITS=${2:-1000}
RECORD=${3:-2000}
PREFIX=${4:-toon}
PKG=$(bash "$(dirname "$0")/app-id.sh")   # single source: ProjectConfiguration.ApplicationId
HERE="$(cd "$(dirname "$0")" && pwd)"
SHOTS="$HERE/../docs/art/rt6t"
mkdir -p "$SHOTS"

adb start-server >/dev/null
if adb shell dumpsys window | tr -d '\r' | grep -q 'isKeyguardShowing=true'; then
  echo "The phone is locked: unlock it and run again." >&2
  exit 2
fi

for msaa in 1 4; do
  for hdr in 0 1; do
    "$HERE/rt2-gate.sh" "$PREFIX$UNITS-m$msaa-h$hdr" 60 20 "$APK" "toonlab:$UNITS:$msaa:$hdr"
  done
done
"$HERE/rt2-gate.sh" "$PREFIX$RECORD-m4-h1" 60 20 "$APK" "toonlab:$RECORD:4:1"

# Screenshot of the lab with N units (MSAA 4x, HDR on), for the record.
ACTIVITY=$(adb shell cmd package resolve-activity --brief "$PKG" | tr -d '\r' | tail -n 1)
adb shell am force-stop "$PKG"
adb shell am start -n "$ACTIVITY" --ez toonlab true --ei units "$UNITS" --ei msaa 4 --ez hdr true --ei fps 60 >/dev/null
sleep 8
adb exec-out screencap -p > "$SHOTS/toonlab-$UNITS.png"
adb shell am force-stop "$PKG"
echo "Screenshot: docs/art/rt6t/toonlab-$UNITS.png"
