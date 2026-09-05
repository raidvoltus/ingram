#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
UNITY="${UNITY_PATH:-}"
if [[ -z "$UNITY" ]]; then
  for c in \
    "/opt/unity/Editor/Unity" \
    "/opt/Unity/Editor/Unity" \
    "$HOME/Unity/Hub/Editor/2022.3.32f1/Editor/Unity" \
    "$(command -v unity-editor || true)"; do
    [[ -x "$c" ]] && UNITY="$c" && break
  done
fi
if [[ -z "${UNITY}" || ! -x "${UNITY}" ]]; then
  echo "ERROR: Unity Editor not found. Set UNITY_PATH."
  exit 127
fi
mkdir -p "$ROOT/Builds" "$ROOT/Logs"
"$UNITY" -batchmode -nographics -quit \
  -projectPath "$ROOT" \
  -executeMethod Genevore.Editor.AndroidReleaseBuild.BuildApk \
  -logFile "$ROOT/Logs/unity-build.log"
ls -la "$ROOT/Builds/" || true
