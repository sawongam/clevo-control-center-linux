# Clevo Control Center for Linux

[![Latest release](https://img.shields.io/github/v/release/sawongam/clevo-control-center-linux?label=version)](https://github.com/sawongam/clevo-control-center-linux/releases/latest)
[![License: GPL-2.0](https://img.shields.io/badge/license-GPL--2.0-blue.svg)](LICENSE)

An unofficial Linux port of Control Center 3.0, the Windows utility that ships with Clevo laptops and their
rebrands. It reads fan speeds and temperatures, switches fan and performance modes, edits fan curves and sets the
keyboard backlight, through the same firmware interface the Windows software uses.

The package, the commands and the install paths use the project's short name, `ccenter`.

![Control Center launcher](docs/screenshots/controlcenter30.png)

## Status

This is early software, and only part of it has run on real hardware.

| Part | State |
|---|---|
| Kernel module `ccenter_dchu` | Tested on hardware |
| `ccenterctl`: fan and temperature readings, fan modes, performance modes | Tested on hardware |
| `ccenterctl curve`: custom fan curve write | Not tested on hardware |
| `ccenterd` service | Tested against the simulator only |
| Graphical apps | Tested against the simulator only |
| CPU and GPU overclocking | Interface only. Writes are locked and not connected to a back-end |

Every change on `master` that passes the checks is published as a new
[release](https://github.com/sawongam/clevo-control-center-linux/releases), so the latest release is always the
current code. [docs/roadmap.md](docs/roadmap.md) has the details and the list of open work.

## Supported hardware

One machine has been tested: the Clevo NH5x_NH7xHP (Intel i7-11800H, RTX 3060, two fans, BIOS 1.07.09).
Clevo boards are sold under many brand names; `cat /sys/class/dmi/id/board_name` shows which one you have.

Other Clevo models expose the same ACPI device (`CLV0001`, method `\_SB.DCHU._DSM`), and the original software
carries code paths for many of them, which the port keeps. None of those have been tested. A firmware call that is
harmless on one board can do something else on another, so try a different model only if you accept that risk, and
please [report what you find](https://github.com/sawongam/clevo-control-center-linux/issues/new?template=hardware-report.yml).

## What is included

Control Center opens as one window. Its tiles open the other tools, as on Windows.

| Tool | What it does |
|---|---|
| Control Center | Launcher and power modes (quiet, power saving, performance, entertainment) |
| Fan Speed Setting | Fan and temperature gauges, fan modes, custom fan curve, anti-dust |
| LED Keyboard | Keyboard backlight colour, brightness and sleep timer |
| Energy Save | Power plans and a scheduled energy-saving mode |
| Battery Utility | Battery refresh and health diagnosis |
| Flexikey | Key and mouse remapping, macros. Macros are saved but not applied yet |
| CPU Overclocking, GPU Overclocking | Monitoring works. Writes are locked |
| Fn key OSD | On-screen popups for Fn keys. Not connected to real key events yet |

| | |
|---|---|
| ![Fan Speed Setting](docs/screenshots/fanspeedsetting.png) | ![LED Keyboard](docs/screenshots/ledkeyboardsetting.png) |

There is also `ccenterctl`, a command line tool that needs neither the service nor a desktop.

## Install

Each [release](https://github.com/sawongam/clevo-control-center-linux/releases/latest) has three downloads. Pick
the one for your distribution. All of them install the same things: the kernel module (built by DKMS, so it
survives kernel updates), the `ccenterd` service, `ccenterctl`, and the apps under `/opt/ccenter` with one
"Control Center" entry in the application menu.

| Distribution | File |
|---|---|
| Debian, Ubuntu, Mint, Pop!_OS | `ccenter_<version>_amd64.deb` |
| Arch, Manjaro, EndeavourOS | `ccenter-<version>-1-x86_64.pkg.tar.zst` |
| Anything else with systemd | `ccenter-<version>-linux-x64.tar.gz` |

### Debian and Ubuntu

1. Install the headers for your kernel, so that DKMS can build the module:
   ```sh
   sudo apt install linux-headers-$(uname -r)
   ```
2. Download the `.deb` from the [latest release](https://github.com/sawongam/clevo-control-center-linux/releases/latest),
   or fetch it from a terminal:
   ```sh
   curl -s https://api.github.com/repos/sawongam/clevo-control-center-linux/releases/latest \
     | grep -o 'https://[^"]*_amd64\.deb' | xargs curl -LO
   ```
3. Install it. This builds the module and starts the service:
   ```sh
   sudo apt install ./ccenter_*_amd64.deb
   ```

### Arch Linux

1. Install the headers for your kernel (`linux-headers` for the stock kernel, `linux-lts-headers` for LTS):
   ```sh
   sudo pacman -S --needed linux-headers
   ```
2. Download the package from the [latest release](https://github.com/sawongam/clevo-control-center-linux/releases/latest),
   or fetch it from a terminal:
   ```sh
   curl -s https://api.github.com/repos/sawongam/clevo-control-center-linux/releases/latest \
     | grep -o 'https://[^"]*\.pkg\.tar\.zst' | xargs curl -LO
   ```
3. Install it, then start the service:
   ```sh
   sudo pacman -U ccenter-*.pkg.tar.zst
   sudo systemctl enable --now ccenterd
   ```

### Any other distribution

The tarball contains the built apps with the .NET runtime included. You need systemd, `dkms`, the headers for your
kernel, `python3`, and the Liberation and Noto fonts.

1. Download the tarball from the [latest release](https://github.com/sawongam/clevo-control-center-linux/releases/latest),
   or fetch it from a terminal:
   ```sh
   curl -s https://api.github.com/repos/sawongam/clevo-control-center-linux/releases/latest \
     | grep -o 'https://[^"]*linux-x64\.tar\.gz' | xargs curl -LO
   ```
2. Unpack it and run the installer:
   ```sh
   tar xf ccenter-*-linux-x64.tar.gz
   cd ccenter-*-linux-x64
   sudo scripts/install.sh
   ```

### Check that it works

```sh
systemctl status ccenterd     # should be "active (running)"
sudo ccenterctl status           # fan speeds and temperatures
```

If the service is not running, the module probably did not load. Reboot if a kernel update was pending, and check
`sudo dkms status` and `journalctl -u ccenterd -b`. To verify a download, compare it with the `SHA256SUMS` file
attached to the release.

### From source

Requirements: .NET SDK 10, `dkms`, kernel headers, `python3`, and the Liberation and Noto fonts.
On Arch: `sudo pacman -S --needed dotnet-sdk dkms linux-headers python ttf-liberation noto-fonts`.

```sh
git clone https://github.com/sawongam/clevo-control-center-linux.git
cd clevo-control-center-linux
scripts/build.sh            # as your normal user
sudo scripts/install.sh
```

If the installed kernel is newer than the running one, the module is built for the new kernel and the installer
asks you to reboot.

### Update

Install the newer release the same way. Your settings are kept.

### Uninstall

Remove the package with your package manager, or run `sudo scripts/uninstall.sh` if you used the install script.
Settings in `/var/lib/ccenter`, `/etc/ccenter` and `~/.config/ccenter` are kept.

## Usage

Open **Control Center** from the application menu, or run `ccenter`.

`ccenterctl` talks to the kernel module directly and needs root:

```sh
sudo ccenterctl status [--watch SEC]             # fan rpm, duty and temperatures
sudo ccenterctl table                            # fan curves stored in the firmware
sudo ccenterctl perf quiet|powersave|performance|entertainment
sudo ccenterctl fan auto|max|maxq|custom
sudo ccenterctl curve --cpu T2:D2,T3:D3          # write a custom fan curve, see --help
sudo ccenterctl kbd --color RRGGBB --brightness 0-100
```

`ccenterctl kbd` uses the keyboard LED from [tuxedo-drivers](https://github.com/tuxedocomputers/tuxedo-drivers)
(`/sys/class/leds/rgb:kbd_backlight`). Optional extras: `power-profiles-daemon` for the power plans in Energy Save,
and `nvidia-utils` for GPU readings.

## How it works

```mermaid
flowchart TD
    A[Graphical apps<br/>your user] -->|unix socket<br/>/run/ccenter/ccd.sock| B[ccenterd service<br/>root]
    C[ccenterctl<br/>root] -->|ioctl| D
    B -->|ioctl| D[/dev/ccenter_dchu/]
    D --> E[ccenter_dchu kernel module]
    E -->|_SB.DCHU._DSM| F[Laptop firmware]
```

On Windows the apps reach the firmware through a service and a driver that evaluates one ACPI method.
`ccenter_dchu` is a small kernel module that exposes that method as `/dev/ccenter_dchu`. The `ccenterd` service
owns the device, so the apps run without root. It also stores settings and applies the last fan mode,
performance mode and fan curve again at boot, because the firmware forgets them. The apps are the original
.NET programs ported from WPF to [Avalonia](https://avaloniaui.net/).

The decoded command set and the hardware test log are in [docs/protocol.md](docs/protocol.md).

## Safety

- This software sends commands to your laptop's firmware. Wrong values can cause unexpected fan, thermal or power
  behaviour. There is no warranty: see the [license](LICENSE).
- Fan writes are real. `fan max`, `fan custom` and curve writes change what the firmware does until the next
  reboot, and the service then applies your last settings again.
- Fan Speed Setting writes the fan defaults and a default curve to the firmware the first time it starts, as the
  Windows app does.
- CPU and GPU overclock writes are refused unless `/etc/ccenter/oc-unlocked` exists. They are not connected to a
  real back-end in any case. Leave that file alone unless you are developing that part.

## Documentation

- [docs/roadmap.md](docs/roadmap.md): what works, what does not, and the open work
- [docs/protocol.md](docs/protocol.md): the firmware protocol
- [docs/porting.md](docs/porting.md): how the Windows apps were ported, and the rules for changing them
- [docs/apps/](docs/apps/): notes for each app
- [CONTRIBUTING.md](CONTRIBUTING.md): development setup and how to send changes

Hardware reports from other Clevo models are the most useful contribution right now.

## License and disclaimer

The code in this repository is licensed under [GPL-2.0](LICENSE).

This project is not affiliated with, endorsed by or supported by Clevo or any laptop brand. The apps are ports of the
vendor's Control Center 3.0 and include its artwork, which remains the property of its owners, as do all
trademarks and product names. The firmware protocol was worked out by analysing the software and firmware of one
machine, for interoperability.
