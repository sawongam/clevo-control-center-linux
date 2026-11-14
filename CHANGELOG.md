# Changelog

Notable changes to this project. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and versions follow [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Added
- `ripple_dchu` kernel module exposing the Clevo DCHU firmware interface as `/dev/ripple_dchu`.
- `ripplectl` command line tool: fan status, fan table, performance modes, fan modes, custom fan curve, keyboard backlight.
- `ripple-ccd` service that lets the apps reach the hardware without root and restores fan and performance settings at boot.
- Ports of the Control Center 3.0 apps: launcher, Fan Speed Setting, LED Keyboard, Energy Save, Battery Utility,
  Flexikey, CPU Overclocking, GPU Overclocking and the Fn key OSD.
- Installation as a single program with one menu entry, and packaging for Debian/Ubuntu, Arch Linux and a generic tarball.
