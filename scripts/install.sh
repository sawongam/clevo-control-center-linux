#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-2.0-only
# Copyright (C) 2026 ripple-cc contributors
# Install the Ripple Control Center Linux port system-wide.
#   1. as your user:  scripts/build.sh
#   2. then:          sudo scripts/install.sh
set -euo pipefail
cd "$(dirname "$0")/.."
[ "$(id -u)" = 0 ] || { echo "run with sudo"; exit 1; }
[ -d dist ] || { echo "run scripts/build.sh first (as your normal user)"; exit 1; }
VER=1.0
PREFIX=/opt/ripple-cc

echo "== kernel module (DKMS: rebuilds itself on kernel updates)"
if ! command -v dkms >/dev/null; then
  echo "dkms is not installed. Run:  sudo pacman -S --needed dkms linux-headers   then re-run this script."; exit 1
fi
rm -rf /usr/src/ripple_dchu-$VER; mkdir -p /usr/src/ripple_dchu-$VER
cp kernel/Makefile kernel/ripple_dchu.c kernel/ripple_dchu.h kernel/dkms.conf /usr/src/ripple_dchu-$VER/
dkms remove ripple_dchu/$VER --all >/dev/null 2>&1 || true
dkms add ripple_dchu/$VER
# Build for every installed kernel that has headers (the running kernel may be older than
# the installed one if a kernel update is waiting for a reboot).
built=""
for b in /lib/modules/*/build; do
  k=$(basename "$(dirname "$b")")
  if dkms install ripple_dchu/$VER -k "$k" --force; then built="$built $k"; fi
done
[ -n "$built" ] || { echo "module build failed for every kernel"; exit 1; }
echo ripple_dchu > /etc/modules-load.d/ripple_dchu.conf
NEED_REBOOT=0
case " $built " in
  *" $(uname -r) "*) rmmod ripple_dchu 2>/dev/null || true; modprobe ripple_dchu ;;
  *) NEED_REBOOT=1
     echo "NOTE: module built for$built, but you are running $(uname -r)."
     echo "NOTE: a REBOOT IS REQUIRED before the daemon can reach the hardware (the module only loads on the new kernel)."
     lsmod | grep -q '^ripple_dchu' || insmod kernel/ripple_dchu.ko 2>/dev/null || true ;;
esac

echo "== apps and daemon → $PREFIX"
systemctl stop ripple-ccd 2>/dev/null || true
rm -rf "$PREFIX"; mkdir -p "$PREFIX/bin" /etc/ripple-cc
cp -r dist/* "$PREFIX/"
install -m 755 daemon/ripple-ccd "$PREFIX/bin/ripple-ccd"
install -m 755 cli/ripplectl "$PREFIX/bin/ripplectl"
ln -sf "$PREFIX/bin/ripplectl" /usr/local/bin/ripplectl
for a in ControlCenter30 FanSpeedSetting LedKeyboardSetting Flexikey CPUOC GPUOverclocking EnergySave BatteryUtility FnKey; do
  [ -x "$PREFIX/$a/$a" ] && ln -sf "$PREFIX/$a/$a" "/usr/local/bin/ripple-$(echo $a | tr A-Z a-z)"
done

echo "== service"
install -m 644 packaging/ripple-ccd.service /etc/systemd/system/ripple-ccd.service
systemctl daemon-reload
systemctl enable --now ripple-ccd || echo "WARNING: ripple-ccd failed to start (see: journalctl -u ripple-ccd); it will work after the module loads or after a reboot."
sleep 1; systemctl --no-pager --lines=5 status ripple-ccd || true

echo "== menu entries"
for f in packaging/desktop/*.desktop; do [ -f "$f" ] && install -m 644 "$f" /usr/share/applications/; done
for f in packaging/icons/*.png; do [ -f "$f" ] && install -D -m 644 "$f" "/usr/share/icons/hicolor/256x256/apps/$(basename "$f")"; done
gtk-update-icon-cache -q /usr/share/icons/hicolor 2>/dev/null || true

echo
if [ "$NEED_REBOOT" = 1 ]; then echo "REBOOT REQUIRED: the module was built for a newer kernel than the running one ($(uname -r)); reboot so it loads."; fi
echo "Installed. Start it from the app menu (Control Center) or run: ripple-controlcenter30"
echo "Overclock writes are locked. To unlock later:  sudo touch /etc/ripple-cc/oc-unlocked"
