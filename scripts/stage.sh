#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-2.0-only
# Copyright (C) 2026 ripple-cc contributors
#
# Copy every installed file into a directory tree. Used by install.sh (with DESTDIR=/) and by
# the distribution packages. It only copies files: registering the DKMS module and enabling the
# service is left to the caller.
#
#   scripts/stage.sh DESTDIR
set -euo pipefail
cd "$(dirname "$0")/.."

dest=${1:?usage: scripts/stage.sh DESTDIR}
dest=${dest%/}
[ -x dist/ControlCenter30/ControlCenter30 ] || { echo "error: dist/ is missing, run scripts/build.sh first" >&2; exit 1; }
module_version=$(sed -n 's/^PACKAGE_VERSION="\(.*\)"/\1/p' kernel/dkms.conf)

# Apps and daemon. cp -a keeps the symlinks into lib/.
rm -rf "$dest/opt/ripple-cc"
install -d "$dest/opt/ripple-cc/bin"
cp -a --no-preserve=ownership dist/. "$dest/opt/ripple-cc/"
install -m 755 daemon/ripple-ccd "$dest/opt/ripple-cc/bin/ripple-ccd"

# Commands on PATH.
install -d "$dest/usr/bin"
install -m 755 cli/ripplectl "$dest/usr/bin/ripplectl"
ln -sf /opt/ripple-cc/ControlCenter30/ControlCenter30 "$dest/usr/bin/ripple-cc"

# Kernel module source for DKMS, and loading it at boot.
rm -rf "$dest/usr/src/ripple_dchu-$module_version"
install -d "$dest/usr/src/ripple_dchu-$module_version"
install -m 644 kernel/Makefile kernel/dkms.conf kernel/ripple_dchu.c kernel/ripple_dchu.h \
  "$dest/usr/src/ripple_dchu-$module_version/"
install -D -m 644 packaging/ripple_dchu.conf "$dest/usr/lib/modules-load.d/ripple_dchu.conf"

# Service, menu entry, icon, licence.
install -D -m 644 packaging/ripple-ccd.service "$dest/usr/lib/systemd/system/ripple-ccd.service"
install -D -m 644 packaging/ripple-cc.desktop "$dest/usr/share/applications/ripple-cc.desktop"
install -D -m 644 packaging/ripple-cc.png "$dest/usr/share/icons/hicolor/256x256/apps/ripple-cc.png"
install -D -m 644 LICENSE "$dest/usr/share/licenses/ripple-cc/LICENSE"
install -d "$dest/etc/ripple-cc"
