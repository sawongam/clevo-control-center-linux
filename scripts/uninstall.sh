#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-2.0-only
# Copyright (C) 2026 ripple-cc contributors
#
# Remove what scripts/install.sh installed. Settings are kept.
#
#   sudo scripts/uninstall.sh
set -uo pipefail

[ "$(id -u)" = 0 ] || { echo "error: run with sudo" >&2; exit 1; }

systemctl disable --now ripple-ccd 2>/dev/null
rmmod ripple_dchu 2>/dev/null
for src in /usr/src/ripple_dchu-*; do
  [ -d "$src" ] || continue
  dkms remove "ripple_dchu/${src##*-}" --all 2>/dev/null
  rm -rf "$src"
done

rm -rf /opt/ripple-cc /usr/share/licenses/ripple-cc
rm -f /usr/bin/ripple-cc /usr/bin/ripplectl \
  /usr/lib/systemd/system/ripple-ccd.service /usr/lib/modules-load.d/ripple_dchu.conf \
  /usr/share/applications/ripple-cc.desktop /usr/share/icons/hicolor/256x256/apps/ripple-cc.png
# Locations used by early versions.
rm -f /etc/systemd/system/ripple-ccd.service /etc/modules-load.d/ripple_dchu.conf \
  /usr/local/bin/ripplectl /usr/local/bin/ripple-* \
  /usr/share/applications/ripple-cc-*.desktop /usr/share/icons/hicolor/256x256/apps/ripple-cc-*.png
systemctl daemon-reload

echo "Removed. Settings are kept in /var/lib/ripple-cc, /etc/ripple-cc and ~/.config/ripple-cc."
