#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-2.0-only
# Copyright (C) 2026 ccenter contributors
#
# Build the Control Center into dist/. Run as your normal user, not with sudo.
#
#   scripts/build.sh [--self-contained] [--offline]
#
#   --self-contained   bundle the .NET runtime, so the target machine needs no dotnet install
#   --offline          restore NuGet packages from ~/.nuget/packages only
#
# Each app is published to dist/<App>/. Files that are byte-identical in more than one app
# (Avalonia, Skia, the runtime) are then moved to dist/lib/ and replaced by relative symlinks,
# so the suite installs as one program with one copy of its libraries.
set -euo pipefail
cd "$(dirname "$0")/.."

APPS=(ControlCenter30 FanSpeedSetting LedKeyboardSetting Flexikey CPUOC GPUOverclocking EnergySave BatteryUtility FnKey)
RID=linux-x64
publish_args=(-c Release --nologo -v quiet -p:DebugType=none)
self_contained=false

for arg in "$@"; do
  case "$arg" in
    --self-contained) self_contained=true ;;
    --offline) publish_args+=(--source "$HOME/.nuget/packages") ;;
    -h|--help) sed -n '5,14p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) echo "unknown option: $arg" >&2; exit 2 ;;
  esac
done
if $self_contained; then
  publish_args+=(-r "$RID" --self-contained true)
fi

rm -rf dist
mkdir -p dist/lib
for app in "${APPS[@]}"; do
  echo "== $app"
  dotnet publish "src/$app/$app.csproj" "${publish_args[@]}" -o "dist/$app"
  [ -x "dist/$app/$app" ] || { echo "error: dist/$app/$app was not produced" >&2; exit 1; }
  # A framework-dependent publish carries native libraries for every platform; keep only ours.
  if [ -d "dist/$app/runtimes" ]; then
    find "dist/$app/runtimes" -mindepth 1 -maxdepth 1 ! -name "$RID" -exec rm -rf {} +
  fi
done

echo "== sharing common files in dist/lib"
cd dist
# A file is shared when the same relative path exists in at least two apps and every copy is identical.
find "${APPS[@]}" -type f -printf '%P\n' | sort | uniq -d | while IFS= read -r rel; do
  copies=()
  for app in "${APPS[@]}"; do
    [ -f "$app/$rel" ] && copies+=("$app/$rel")
  done
  [ "$(sha256sum "${copies[@]}" | cut -d' ' -f1 | sort -u | wc -l)" = 1 ] || continue
  mkdir -p "lib/$(dirname "$rel")"
  mv "${copies[0]}" "lib/$rel"
  for copy in "${copies[@]}"; do
    ln -srf "lib/$rel" "$copy"
  done
done
cd ..

echo "built dist/ ($(du -sh dist | cut -f1))"
