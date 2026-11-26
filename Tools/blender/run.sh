#!/usr/bin/env bash
# RT-6v3.4a: re-writes the simple-mesh data of Tools/blender/out with the pinned Blender (5.3), then checks it is
# deterministic (a second run gives the same bytes). Usage (Git Bash, repository root): Tools/blender/run.sh [--preview <png>]
# After it, rebuild the scene (Tools/rt4-check.sh <out> "scene") so the mesh assets follow the data.
set -euo pipefail
BLENDER=${BLENDER:-"/c/Program Files/Blender Foundation/Blender 5.3/blender.exe"}
HERE=$(cd "$(dirname "$0")" && pwd)
"$BLENDER" -b --factory-startup -P "$HERE/bibopzi.py" 2>&1 | grep -E '^\[bibopzi\]|Error|Traceback' || true
grep -q '"format"' "$HERE/out/bibopzi_crowd.json"
SUM=$(cat "$HERE"/out/*.json | md5sum)
"$BLENDER" -b --factory-startup -P "$HERE/bibopzi.py" > /dev/null 2>&1
if [ "$SUM" != "$(cat "$HERE"/out/*.json | md5sum)" ]; then
  echo "FAIL: two runs wrote different data"
  exit 1
fi
echo "PASS: data written and deterministic"
if [ "${1:-}" = "--preview" ]; then
  "$BLENDER" -b --factory-startup -P "$HERE/preview.py" -- "$2" "$HERE"/out/bibopzi_detail.json "$HERE"/out/bibopzi_crowd.json \
    "$HERE"/out/bibopzi_reserve.json > /dev/null 2>&1
  echo "preview: $2"
fi
