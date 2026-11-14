#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-2.0-only
# Copyright (C) 2026 ripple-cc contributors
#
# Build ripple-cc_<version>_amd64.deb from dist/. Build dist/ with
# `scripts/build.sh --self-contained` first: Debian does not package the .NET runtime.
#
#   packaging/deb/build.sh [OUTPUT_DIR]
set -euo pipefail
cd "$(dirname "$0")/../.."

out=$(realpath "${1:-.}")
version=$(cat VERSION)
module_version=$(sed -n 's/^PACKAGE_VERSION="\(.*\)"/\1/p' kernel/dkms.conf)
root=$(mktemp -d)
trap 'rm -rf "$root"' EXIT

scripts/stage.sh "$root"
# Debian keeps the licence under /usr/share/doc.
rm -rf "$root/usr/share/licenses"
install -D -m 644 LICENSE "$root/usr/share/doc/ripple-cc/copyright"

install -d "$root/DEBIAN"
for script in postinst prerm; do
  sed "s/@MODULE_VERSION@/$module_version/g" "packaging/deb/$script" > "$root/DEBIAN/$script"
  chmod 755 "$root/DEBIAN/$script"
done
sed -e "s/@VERSION@/$version/" \
    -e "s/@INSTALLED_SIZE@/$(du -sk --exclude=DEBIAN "$root" | cut -f1)/" \
    packaging/deb/control > "$root/DEBIAN/control"

dpkg-deb --build --root-owner-group "$root" "$out/ripple-cc_${version}_amd64.deb"
