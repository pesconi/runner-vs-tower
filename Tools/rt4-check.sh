#!/usr/bin/env bash
# Full batchmode validation of the game (RT-4, RT-5), Unity Editor CLOSED on this project:
# scene build, game validator (catalog, worlds, save fixtures, scene, code rules), kit isolation, EditMode
# and PlayMode tests, the economy and level table (docs/rt6-economia.md; docs/rt5-economia.md is the RT-5 record), art captures (needs a GPU, so no
# -nographics there), the two Android APKs (game and measurement) and the Play bundle (AAB, RT-8).
# Usage (Git Bash, repository root): Tools/rt4-check.sh [out=/c/temp/rt4] [steps="scene validate isolation editmode playmode economy capture apk aab"]
# Prints one PASS/FAIL line per step; logs in <out>/logs. Exit 1 if any step failed.
set -uo pipefail
OUT=${1:-/c/temp/rt4}
STEPS=${2:-"scene validate isolation editmode playmode economy capture apk aab"}
UNITY=${UNITY:-"/c/Program Files/Unity/Hub/Editor/6000.6.7f1/Editor/Unity.exe"}
PROJECT=$(cd "$(dirname "$0")/.." && pwd -W 2>/dev/null || pwd)
WIN_OUT=$(cd "$OUT" 2>/dev/null && pwd -W 2>/dev/null || echo "$OUT")
mkdir -p "$OUT/logs" "$OUT/art"
FAILED=0

run() { # name, extra args...
  local name=$1; shift
  "$UNITY" -batchmode -buildTarget Android -projectPath "$PROJECT" -logFile "$WIN_OUT/logs/$name.log" "$@"
  local code=$?
  if [ $code -eq 0 ]; then echo "PASS $name"; else echo "FAIL $name (exit $code, see $OUT/logs/$name.log)"; FAILED=1; fi
}

tests() { # platform
  local name=$(echo "$1" | tr '[:upper:]' '[:lower:]')
  run "$name" -nographics -runTests -testPlatform "$1" -testResults "$WIN_OUT/logs/$name.xml"
  grep -oE 'test-run [^>]*total="[0-9]+" passed="[0-9]+" failed="[0-9]+"' "$OUT/logs/$name.xml" | sed 's/.*total/  total/'
}

for step in $STEPS; do
  case $step in
    scene) run scene -nographics -executeMethod Claybreak.Editor.ClaybreakSceneBuilder.BuildAndQuit ;;
    validate) run validate -nographics -executeMethod Claybreak.Editor.ClaybreakValidation.RunAndQuit ;;
    isolation) run isolation -nographics -executeMethod JungleDegree.StudioKit.Editor.EngineIsolationValidation.RunAndQuit ;;
    editmode) tests EditMode ;;
    playmode) tests PlayMode ;;
    economy) run economy -nographics -executeMethod Claybreak.Editor.EconomyReport.WriteAndQuit -claybreakReport "$PROJECT/docs/rt6-economia.md" ;;
    capture) run capture -executeMethod Claybreak.Editor.ArtDirectionCapture.CaptureAndQuit -claybreakCaptureDir "$WIN_OUT/art" ;;
    apk)
      run apk-game -nographics -executeMethod Claybreak.Editor.ClaybreakBuild.BuildAndroidAndQuit -claybreakApk "$WIN_OUT/Claybreak.apk"
      run apk-measure -nographics -executeMethod Claybreak.Editor.ClaybreakBuild.BuildAndroidAndQuit -claybreakMeasure -claybreakApk "$WIN_OUT/Claybreak-measure.apk"
      ;;
    aab) # RT-8: the Play bundle (release player; debug key unless CLAYBREAK_KEYSTORE is set, see ClaybreakBuild)
      run aab -nographics -executeMethod Claybreak.Editor.ClaybreakBuild.BuildAndroidAndQuit -claybreakAab "$WIN_OUT/Claybreak.aab"
      ;;
    *) echo "unknown step $step"; FAILED=1 ;;
  esac
done
exit $FAILED
