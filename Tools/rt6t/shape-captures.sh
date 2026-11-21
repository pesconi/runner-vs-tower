#!/usr/bin/env bash
# RT-6t.2: side-by-side captures of the crowd shapes, Unity vs the three.js reference (docs/reference/crowd-proto.html).
# 1. Unity (Editor CLOSED on this project, GPU: no -nographics): Claybreak.Tests.UnitShapeCaptureTests renders the four
#    shapes (Bolinha, Pintinho, with and without eyes) in close-up and as a crowd of 100 on 9:16, and writes the crowd
#    camera poses.
# 2. Headless Chrome renders Tools/rt6t/proto-shapes.html (the prototype's shape code, lights and toon ramp; three.js
#    from the CDN, so it needs the network) from the same camera poses, mirrored (three.js z = -Unity z).
# 3. Tools/rt6t/compose_shapes.py puts them side by side in docs/art/rt6t/.
# Usage (Git Bash, repository root): Tools/rt6t/shape-captures.sh [out=/c/temp/rt6t2/shapes] [steps="unity proto sheets"]
set -euo pipefail
OUT=${1:-/c/temp/rt6t2/shapes}
STEPS=${2:-"unity proto sheets"}
UNITY=${UNITY:-"/c/Program Files/Unity/Hub/Editor/6000.6.7f1/Editor/Unity.exe"}
CHROME=${CHROME:-"/c/Program Files/Google/Chrome/Application/chrome.exe"}
ROOT=$(cd "$(dirname "$0")/../.." && pwd)
PROJECT=$(cd "$ROOT" && pwd -W 2>/dev/null || pwd)
mkdir -p "$OUT/unity" "$OUT/proto" "$OUT/logs"
WIN_OUT=$(cd "$OUT" && pwd -W 2>/dev/null || pwd)
PAGE="file:///$PROJECT/Tools/rt6t/proto-shapes.html"

# id -> prototype shape and eyes
proto_args() {
  case $1 in
    Bolinha) echo "shape=blob&eyes=1" ;;
    BolinhaNoEyes) echo "shape=blob&eyes=0" ;;
    Pintinho) echo "shape=chick&eyes=1" ;;
    PintinhoNoEyes) echo "shape=chick&eyes=0" ;;
  esac
}

shot() { # file, width, height, query
  "$CHROME" --headless=new --enable-unsafe-swiftshader --use-angle=swiftshader --allow-file-access-from-files \
    --hide-scrollbars --window-size="$2,$3" --virtual-time-budget=15000 --screenshot="$1" "$PAGE?w=$2&h=$3&$4" >/dev/null 2>&1
  [ -s "$1" ] || { echo "FAIL reference render $1"; exit 1; }
}

for step in $STEPS; do
  case $step in
    unity)
      "$UNITY" -batchmode -buildTarget Android -projectPath "$PROJECT" -logFile "$WIN_OUT/logs/unity-shapes.log" \
        -runTests -testPlatform PlayMode -testFilter Claybreak.Tests.UnitShapeCaptureTests \
        -testResults "$WIN_OUT/logs/unity-shapes.xml" -shapeCaptureDir "$WIN_OUT/unity" \
        && echo "PASS unity" || { echo "FAIL unity (see $OUT/logs/unity-shapes.log)"; exit 1; }
      grep -q 'result="Passed"' "$OUT/logs/unity-shapes.xml" || { echo "FAIL unity: capture test did not pass (ignored = no GPU)"; exit 1; }
      ;;
    proto)
      # Close-ups: same views as UnitShapeCaptureTests.Views (Unity x,y,z -> three.js x,y,-z), FOV 30, target (0,0.22,0).
      for id in Bolinha BolinhaNoEyes Pintinho PintinhoNoEyes; do
        a=$(proto_args $id)
        shot "$WIN_OUT/proto/proto-$id-frente.png" 360 360 "$a&cam=0.55,0.5,-1.15&look=0,0.22,0&fov=30"
        shot "$WIN_OUT/proto/proto-$id-lado.png" 360 360 "$a&cam=1.3,0.4,0&look=0,0.22,0&fov=30"
        shot "$WIN_OUT/proto/proto-$id-costas.png" 360 360 "$a&cam=-0.55,0.5,1.15&look=0,0.22,0&fov=30"
      done
      # Crowds: the Unity lab camera of each capture, mirrored; the reference crowd stands at rest on its slots.
      while read -r id cam forward fov fog; do
        IFS=, read -r x y z <<<"${cam#cam=}"
        IFS=, read -r fx fy fz <<<"${forward#forward=}"
        look=$(awk -v x="$x" -v y="$y" -v z="$z" -v fx="$fx" -v fy="$fy" -v fz="$fz" \
          'BEGIN{printf "%.4f,%.4f,%.4f", x+10*fx, y+10*fy, -(z+10*fz)}')
        nz=$(awk -v z="$z" 'BEGIN{printf "%.4f", -z}')
        shot "$WIN_OUT/proto/proto-crowd-$id.png" 540 960 \
          "$(proto_args $id)&n=100&cam=$x,$y,$nz&look=$look&fov=${fov#fov=}&fog=${fog#fog=}"
      done < <(tr -d '\r' < "$OUT/unity/unity-crowd.txt")
      echo "PASS proto"
      ;;
    sheets)
      python -X utf8 "$ROOT/Tools/rt6t/compose_shapes.py" "$OUT" "$ROOT/docs/art/rt6t" && echo "PASS sheets"
      ;;
    *) echo "unknown step $step"; exit 1 ;;
  esac
done
