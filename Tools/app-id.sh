#!/usr/bin/env bash
# Prints the game's Android application ID, read from its single source,
# Assets/Claybreak/Editor/ProjectConfiguration.cs (ApplicationId), so the device tools never carry their own copy.
# Usage (Git Bash): PKG=$(Tools/app-id.sh)
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
SOURCE="$HERE/../Assets/Claybreak/Editor/ProjectConfiguration.cs"
ID=$(sed -n 's/.*const string ApplicationId = "\([a-z0-9_.]*\)";.*/\1/p' "$SOURCE" | head -n 1)
if [ -z "$ID" ]; then
  echo "app-id.sh: no ApplicationId in $SOURCE" >&2
  exit 1
fi
echo "$ID"
