#!/usr/bin/env bash
# RT-8b (D21 = A) device proof of the minimal Firebase Analytics setup on the OPPO CPH2791.
# Usage (Git Bash, repository root, phone connected and unlocked, USB debugging on):
#   Tools/rt8b-firebase-check.sh [apk=/c/temp/rt8b/Claybreak.apk] [seconds=45]
# What it proves, from the device log of a FRESH install (app data cleared):
#   1. nothing is logged or uploaded by Firebase before the game enables collection (the "[Metrics] Firebase
#      collection enabled" line comes from the game only after the UMP consent flow ends);
#   2. the game's events reach Firebase with the approved names (game_session, level_start, level_end,
#      rewarded_ad), seen in Firebase's verbose log ("Logging event") and in DebugView (debug mode is on for
#      this run only and switched off at the end);
#   3. the final manifest has no Firebase advertising-ID collection and every minimal flag set to false.
# To also produce level events, play one level by hand while it records (the script waits <seconds>).
set -uo pipefail
APK=${1:-/c/temp/rt8b/Claybreak.apk}
SECONDS_TO_RECORD=${2:-45}
PKG=$(bash "$(dirname "$0")/app-id.sh")   # single source: ProjectConfiguration.ApplicationId
OUT=/c/temp/rt8b
AAPT2=${AAPT2:-"/c/Program Files/Unity/Hub/Editor/6000.6.7f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/build-tools/36.0.0/aapt2.exe"}
LOG="$OUT/firebase-check-logcat.txt"
mkdir -p "$OUT"
FAILED=0
check() { if eval "$2"; then echo "PASS $1"; else echo "FAIL $1"; FAILED=1; fi; }

# 0. The configuration (RT-9b.1): google-services.json outside the repository must have an Android app for this
# package, and the APK must carry it (google_app_id); a file of another package builds an APK without Firebase.
CONFIG=${CLAYBREAK_GOOGLE_SERVICES_JSON:-"$(cd "$(dirname "$0")/../.." && pwd)/_privado/runner-vs-tower/google-services.json"}
PKG_LINE="\"package_name\": \"$PKG\""
if [ -f "$CONFIG" ] && tr -s ' ' < "$CONFIG" | grep -qF "$PKG_LINE"; then HAS_APP=1; else HAS_APP=0; fi
"$AAPT2" dump resources "$APK" > "$OUT/firebase-check-resources.txt" 2>/dev/null
check "google-services.json exists and has an Android app for $PKG ($CONFIG)" "[ $HAS_APP -eq 1 ]"
check "the APK carries the Firebase configuration (google_app_id)" "grep -qF 'string/google_app_id' \"\$OUT/firebase-check-resources.txt\""
if [ $FAILED -ne 0 ]; then
  echo "Stop: download the google-services.json of $PKG from the Firebase console, rebuild the APK and run again."
  exit 1
fi

# 3. Manifest (no device needed).
"$AAPT2" dump xmltree --file AndroidManifest.xml "$APK" > "$OUT/firebase-check-manifest.txt"
for flag in firebase_analytics_collection_enabled google_analytics_adid_collection_enabled \
  google_analytics_ssaid_collection_enabled google_analytics_default_allow_ad_personalization_signals \
  google_analytics_default_allow_analytics_storage google_analytics_default_allow_ad_storage \
  google_analytics_default_allow_ad_user_data google_analytics_default_allow_ad_personalization \
  google_analytics_automatic_screen_reporting_enabled; do
  check "manifest $flag=false" "grep -A1 '\"$flag\"' '$OUT/firebase-check-manifest.txt' | grep -q '=false'"
done
check "manifest has no collection_deactivated" "! grep -q collection_deactivated '$OUT/firebase-check-manifest.txt'"

# 1 and 2. Device.
adb start-server >/dev/null 2>&1
adb get-state >/dev/null || { echo "FAIL no device (connect the OPPO, unlock it, accept USB debugging)"; exit 1; }
adb install -r "$APK" >/dev/null
adb shell pm clear "$PKG" >/dev/null                      # fresh install state: no stored consent, first_open again
adb shell setprop log.tag.FA VERBOSE
adb shell setprop log.tag.FA-SVC VERBOSE
adb shell setprop debug.firebase.analytics.app "$PKG"    # DebugView for this run
restore() { adb shell setprop debug.firebase.analytics.app .none. >/dev/null 2>&1; adb shell setprop log.tag.FA INFO >/dev/null 2>&1; adb shell setprop log.tag.FA-SVC INFO >/dev/null 2>&1; }
trap restore EXIT
adb logcat -c
ACTIVITY=$(adb shell cmd package resolve-activity --brief "$PKG" | tr -d '\r' | tail -n 1)
adb shell am start -n "$ACTIVITY" >/dev/null
echo "Recording $SECONDS_TO_RECORD s (play a level now to see level_start/level_end)..."
sleep "$SECONDS_TO_RECORD"
adb logcat -d -v time -s Unity FA FA-SVC > "$LOG"
adb shell am force-stop "$PKG"

ENABLED_LINE=$(grep -n "Firebase collection enabled" "$LOG" | head -1 | cut -d: -f1)
check "the game enabled collection after the consent flow" "[ -n '$ENABLED_LINE' ]"
if [ -n "$ENABLED_LINE" ]; then
  BEFORE=$(head -n "$ENABLED_LINE" "$LOG" | grep -E "Logging event|Uploading data|Upload successful" | wc -l)
  check "no Firebase event or upload before the game enabled collection ($BEFORE)" "[ $BEFORE -eq 0 ]"
fi
check "game_session reached Firebase" "grep -q 'Logging event.*name=game_session' '$LOG'"
check "no event named session_start from the game" "! grep -q 'origin=app,name=session_start' '$LOG'"
NAMES=$(grep -o "Logging event: origin=app,name=[a-z_]*" "$LOG" | sed 's/.*name=//' | sort -u | tr '\n' ' ')
echo "Game events seen by Firebase: ${NAMES:-none}"
for name in $NAMES; do
  check "event name $name is approved" "echo ' game_session level_start level_end rewarded_ad ' | grep -q ' $name '"
done
echo "Log: $LOG"
exit $FAILED
