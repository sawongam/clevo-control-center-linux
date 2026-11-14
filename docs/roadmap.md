# Roadmap and current state

Target: Clevo NH5x_NH7xHP, i7-11800H, RTX 3060, Arch Linux. Goal: a Linux clone of Control
Center 3.0 with the same look and behaviour, installed as one program. Design notes are in
[protocol.md](protocol.md) and [porting.md](porting.md).

## Components
| path | what |
|---|---|
| `kernel/` | `ripple_dchu` kernel module (DKMS) providing `/dev/ripple_dchu` |
| `cli/ripplectl` | CLI: status, table, perf, fan, curve, kbd, raw |
| `daemon/ripple-ccd` | root daemon on `/run/ripple-cc/ccd.sock`; keeps app settings; re-applies perf/fan/curve at boot |
| `src/RippleCC.Platform/` | shared library: `InsydeDCHU` drop-in, `MockBackend`, `Ini`/`Reg`/`CcPaths` shims, `AppHost` (`--screenshot`) |
| `src/<App>/` | one project per app; notes in [apps/](apps/) |
| `scripts/` | `build.sh` (publishes to `dist/`), `stage.sh` (lays out the installed files), `install.sh`, `uninstall.sh` |
| `packaging/` | systemd unit, menu entry, icon; Arch, Debian and tarball packaging |

## Hardware-verified
- Kernel module and `ripplectl`: reading fans, temperatures and curves works. `fan max`, `fan auto` and `perf`
  writes work (fans went from about 2200 to 4990 rpm and back).
- Not tested: custom curve write (`ripplectl curve`).
- Nothing from the GUI apps or the daemon has touched real hardware yet.

## State of the apps
The suite is nine programs, as on Windows: the launcher, seven tools it opens, and the Fn key OSD agent. They are
installed together under `/opt/ripple-cc` with shared libraries and one menu entry.
All are tested against the simulator only. Each builds and renders its screens headlessly. Details and stubs
are in the per-app notes.

| app | state | main gaps |
|---|---|---|
| [ControlCenter30](apps/ControlCenter30.md) (launcher) | renders, 5 s real run ok | clicks untested; power modes skip Windows power plan / refresh rate / GPU clock; real tile set unconfirmed |
| [FanSpeedSetting](apps/FanSpeedSetting.md) | renders; mode clicks and curve save send the right commands (mock log) | dropdown look simplified; no resize grip; first run writes fan defaults and the default curve to firmware |
| [LedKeyboardSetting](apps/LedKeyboardSetting.md) | renders all variants; colour/brightness commands correct (mock log) | per-key and light-bar device I/O stubbed (not this laptop's keyboard); pointer paths untested |
| [EnergySave](apps/EnergySave.md) | renders | mouse dragging untested; power plans applied only if `power-profiles-daemon` is installed |
| [BatteryUtility](apps/BatteryUtility.md) | renders | real refresh untested (24 h+ cycle); AC-removed event not detected |
| [CPUOC](apps/CPUOC.md) | renders | OC writes gated and not wired (needs an Intel XTU equivalent); voltage, memory clock and ratios are placeholder values |
| [GPUOverclocking](apps/GPUOverclocking.md) | renders | OC writes gated and stubbed (needs Coolbits and nvidia-settings, X11 only); slider ranges and base/boost clocks are guesses; never run as a real window |
| [Flexikey](apps/Flexikey.md) | renders | macros are saved but not applied (no Linux driver yet, needs uinput); never run as a real window |
| [FnKey](apps/FnKey.md) (OSD popups) | 74 popups render; test events work | not fed by real Fn keys (the kernel module has no event path); tray icon not ported; first-run init is opt-in via `--first-run` / `FNKEY_FIRST_RUN=1`; not in any autostart |

## Open work
1. First release: the packages built by the release workflow (tarball, `.deb`, Arch package) have not been
   installed on a real machine yet.
2. Hardware tests, to be done with the install in place: daemon, FanSpeedSetting (first launch writes defaults),
   keyboard colours, power modes, custom fan curve.
3. Click-through testing of every app on a real display; mouse paths are largely untested.
4. Fn-key events: a kernel/ACPI notify path feeding FnKey.
5. Flexikey macros: apply them through uinput.
6. Real CPU and GPU overclock back-ends (XTU equivalent; nvidia-settings). Writes stay locked until tested.
7. Daemon: the `OC_LOCKED` table for DCHU functions is empty, so only app-settings ranges are protected.
8. Daemon: `appget`/`appset` accept any page number; they should be restricted to the pages the apps use.
9. Tray icon for FnKey, autostart integration.
10. Reports from other Clevo models.

## Known notes
- Windows fonts are mapped to free look-alikes (Liberation Sans, Noto Sans), so glyph shapes differ slightly.
- Run an app's own binary (`./EnergySave`), not `dotnet EnergySave.dll`: the single-instance check depends on it.
- `src/LedKeyboardSetting/tools/regen.sh` regenerates its `.axaml` and overwrites hand edits.
- Vendor artwork in the apps belongs to Clevo/Ripple; see the README disclaimer.
