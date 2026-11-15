#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-2.0-only
# Copyright (C) 2026 ccenter contributors
#
# Remove what scripts/install.sh installed. Settings are kept.
#
#   sudo scripts/uninstall.sh
set -uo pipefail

[ "$(id -u)" = 0 ] || { echo "error: run with sudo" >&2; exit 1; }

systemctl disable --now ccenterd 2>/dev/null
rmmod ccenter_dchu 2>/dev/null
for src in /usr/src/ccenter_dchu-*; do
  [ -d "$src" ] || continue
  dkms remove "ccenter_dchu/${src##*-}" --all 2>/dev/null
  rm -rf "$src"
done

rm -rf /opt/ccenter /usr/share/licenses/ccenter
rm -f /usr/bin/ccenter /usr/bin/ccenterctl \
  /usr/lib/systemd/system/ccenterd.service /usr/lib/modules-load.d/ccenter_dchu.conf \
  /usr/share/applications/ccenter.desktop /usr/share/icons/hicolor/256x256/apps/ccenter.png
# Locations used by early versions.
rm -f /etc/systemd/system/ccenterd.service /etc/modules-load.d/ccenter_dchu.conf \
  /usr/local/bin/ccenterctl /usr/local/bin/ccenter-* \
  /usr/share/applications/ccenter-*.desktop /usr/share/icons/hicolor/256x256/apps/ccenter-*.png
systemctl daemon-reload

echo "Removed. Settings are kept in /var/lib/ccenter, /etc/ccenter and ~/.config/ccenter."
