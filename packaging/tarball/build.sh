#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-2.0-only
# Copyright (C) 2026 ripple-cc contributors
#
# Build ripple-cc-<version>-linux-x64.tar.gz: the prebuilt dist/ plus what scripts/install.sh
# needs. Build dist/ with `scripts/build.sh --self-contained` first.
#
#   packaging/tarball/build.sh [OUTPUT_DIR]
set -euo pipefail
cd "$(dirname "$0")/../.."

out=$(realpath "${1:-.}")
name="ripple-cc-$(cat VERSION)-linux-x64"
[ -d dist ] || { echo "error: dist/ is missing, run scripts/build.sh first" >&2; exit 1; }

tar -czf "$out/$name.tar.gz" --owner=0 --group=0 --transform "s|^|$name/|" \
  dist kernel daemon cli \
  packaging/ripple-ccd.service packaging/ripple-cc.desktop packaging/ripple-cc.png packaging/ripple_dchu.conf \
  scripts/stage.sh scripts/install.sh scripts/uninstall.sh \
  LICENSE README.md VERSION
echo "$out/$name.tar.gz"
