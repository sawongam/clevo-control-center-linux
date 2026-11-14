#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-2.0-only
# Copyright (C) 2026 ripple-cc contributors
#
# Install the Control Center system-wide from this directory.
#
#   scripts/build.sh            # as your normal user (not needed for a release tarball)
#   sudo scripts/install.sh
set -euo pipefail
cd "$(dirname "$0")/.."

[ "$(id -u)" = 0 ] || { echo "error: run with sudo" >&2; exit 1; }
command -v dkms >/dev/null || {
  echo "error: dkms is not installed. Install dkms and the headers for your kernel, then run this again." >&2
  echo "  Arch:           pacman -S --needed dkms linux-headers" >&2
  echo "  Debian/Ubuntu:  apt install dkms linux-headers-\$(uname -r)" >&2
  exit 1
}
module_version=$(sed -n 's/^PACKAGE_VERSION="\(.*\)"/\1/p' kernel/dkms.conf)

echo "== removing files from earlier installs"
systemctl stop ripple-ccd 2>/dev/null || true
dkms remove "ripple_dchu/$module_version" --all >/dev/null 2>&1 || true
# Locations used before the suite was installed as a single program.
rm -f /etc/systemd/system/ripple-ccd.service /etc/modules-load.d/ripple_dchu.conf \
  /usr/local/bin/ripplectl /usr/local/bin/ripple-* \
  /usr/share/applications/ripple-cc-*.desktop /usr/share/icons/hicolor/256x256/apps/ripple-cc-*.png

echo "== copying files"
scripts/stage.sh /

echo "== kernel module (DKMS rebuilds it on kernel updates)"
dkms add "ripple_dchu/$module_version"
# Build for every installed kernel that has headers: the running kernel can be older than the
# installed one when a kernel update is waiting for a reboot.
built=()
for build_dir in /lib/modules/*/build; do
  kernel=$(basename "$(dirname "$build_dir")")
  if dkms install "ripple_dchu/$module_version" -k "$kernel" --force; then
    built+=("$kernel")
  fi
done
[ ${#built[@]} -gt 0 ] || { echo "error: the module did not build for any installed kernel" >&2; exit 1; }

need_reboot=false
if printf '%s\n' "${built[@]}" | grep -qx "$(uname -r)"; then
  rmmod ripple_dchu 2>/dev/null || true
  modprobe ripple_dchu
else
  need_reboot=true
fi

echo "== service"
systemctl daemon-reload
if ! systemctl enable --now ripple-ccd; then
  echo "warning: ripple-ccd did not start (see: journalctl -u ripple-ccd)." >&2
fi
gtk-update-icon-cache -q /usr/share/icons/hicolor 2>/dev/null || true
update-desktop-database -q /usr/share/applications 2>/dev/null || true

echo
echo "Installed. Open \"Control Center\" from the application menu, or run: ripple-cc"
if $need_reboot; then
  echo "Reboot first: the module was built for ${built[*]}, but the running kernel is $(uname -r)."
fi
