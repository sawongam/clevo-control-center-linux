#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-2.0-only
# Copyright (C) 2026 ripple-cc contributors
# Remove the Ripple Control Center Linux port (run: sudo scripts/uninstall.sh).
set -uo pipefail
[ "$(id -u)" = 0 ] || { echo "run with sudo"; exit 1; }
systemctl disable --now ripple-ccd 2>/dev/null
rm -f /etc/systemd/system/ripple-ccd.service /etc/modules-load.d/ripple_dchu.conf
systemctl daemon-reload
rmmod ripple_dchu 2>/dev/null
dkms remove ripple_dchu/1.0 --all 2>/dev/null
rm -rf /usr/src/ripple_dchu-1.0 /opt/ripple-cc
rm -f /usr/local/bin/ripplectl /usr/local/bin/ripple-* /usr/share/applications/ripple-cc-*.desktop /usr/share/icons/hicolor/256x256/apps/ripple-cc-*.png
echo "removed (settings kept in /var/lib/ripple-cc, /etc/ripple-cc and ~/.config/ripple-cc)"
