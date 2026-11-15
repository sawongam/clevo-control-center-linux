#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-2.0-only
# Copyright (C) 2026 ccenter contributors
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
rm -rf "$dest/opt/ccenter"
install -d "$dest/opt/ccenter/bin"
cp -a --no-preserve=ownership dist/. "$dest/opt/ccenter/"
install -m 755 daemon/ccenterd "$dest/opt/ccenter/bin/ccenterd"

# Commands on PATH.
install -d "$dest/usr/bin"
install -m 755 cli/ccenterctl "$dest/usr/bin/ccenterctl"
ln -sf /opt/ccenter/ControlCenter30/ControlCenter30 "$dest/usr/bin/ccenter"

# Kernel module source for DKMS, and loading it at boot.
rm -rf "$dest/usr/src/ccenter_dchu-$module_version"
install -d "$dest/usr/src/ccenter_dchu-$module_version"
install -m 644 kernel/Makefile kernel/dkms.conf kernel/ccenter_dchu.c kernel/ccenter_dchu.h \
  "$dest/usr/src/ccenter_dchu-$module_version/"
install -D -m 644 packaging/ccenter_dchu.conf "$dest/usr/lib/modules-load.d/ccenter_dchu.conf"

# Service, menu entry, icon, licence.
install -D -m 644 packaging/ccenterd.service "$dest/usr/lib/systemd/system/ccenterd.service"
install -D -m 644 packaging/ccenter.desktop "$dest/usr/share/applications/ccenter.desktop"
install -D -m 644 packaging/ccenter.png "$dest/usr/share/icons/hicolor/256x256/apps/ccenter.png"
install -D -m 644 LICENSE "$dest/usr/share/licenses/ccenter/LICENSE"
install -d "$dest/etc/ccenter"
