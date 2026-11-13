#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-2.0-only
# Copyright (C) 2026 ripple-cc contributors
# Build every ported app into dist/<App>/ (run as your normal user, no sudo).
set -euo pipefail
cd "$(dirname "$0")/.."
APPS="ControlCenter30 FanSpeedSetting LedKeyboardSetting Flexikey CPUOC GPUOverclocking EnergySave BatteryUtility FnKey"
rm -rf dist; mkdir -p dist
fail=""
for a in $APPS; do
  if [ ! -f "src/$a/$a.csproj" ]; then echo "skip $a (not ported yet)"; continue; fi
  echo "== $a"
  if timeout 300 dotnet publish "src/$a/$a.csproj" -c Release -o "dist/$a" --nologo -v q --source "$HOME/.nuget/packages" 2>&1 | grep -E ' error ' | sort -u; then fail="$fail $a"; fi
  if [ -d "dist/$a/runtimes" ]; then find "dist/$a/runtimes" -mindepth 1 -maxdepth 1 ! -name linux-x64 -exec rm -rf {} +; fi
  [ -x "dist/$a/$a" ] || fail="$fail $a(no-exe)"
done
[ -z "$fail" ] && echo "all apps built into $(pwd)/dist" || { echo "FAILED:$fail"; exit 1; }
