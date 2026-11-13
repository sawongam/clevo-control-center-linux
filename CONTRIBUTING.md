# Contributing

Contributions are welcome, particularly hardware test reports and fixes. Please read the safety notes in the
[README](README.md#safety) first: changes touch laptop firmware.

## Layout
`kernel/` (kernel module), `daemon/` (Python root daemon), `cli/` (`ripplectl`), `src/` (Avalonia apps and the shared
`RippleCC.Platform` library), `packaging/`, `scripts/`, `docs/`. Start with [docs/README.md](docs/README.md) and
[docs/porting.md](docs/porting.md).

## Dev setup
- .NET SDK 10, Python 3, and for the kernel module `dkms` and the headers for your kernel (Arch: `dotnet-sdk dkms linux-headers python`).
- Solution: `src/RippleCC.slnx`. Per-app build: `dotnet build src/<App>/<App>.csproj`.
- No root is needed to develop the apps or the daemon.

## Building
```sh
scripts/build.sh     # publishes all apps to dist/<App>/
```
If an online NuGet restore hangs or you are offline, restore from the local cache first:
`dotnet restore src/<App>/<App>.csproj --source ~/.nuget/packages`, then `dotnet build --no-restore`.
`scripts/build.sh` already uses the local package cache as its source.

## Running apps against the simulator
`RippleCC.Platform` picks a backend at start (`Backend.cs`):
- if `RIPPLE_CC_MOCK=1`, or the daemon socket does not exist, the in-process `MockBackend` simulator is used;
- otherwise the app talks to the daemon at `/run/ripple-cc/ccd.sock`, overridable with `RIPPLE_CC_SOCKET`.

```sh
RIPPLE_CC_MOCK=1 dist/FanSpeedSetting/FanSpeedSetting
RIPPLE_CC_MOCK_LOG=/tmp/calls.log RIPPLE_CC_MOCK=1 dist/FanSpeedSetting/FanSpeedSetting   # log each hardware call
```
Run the apphost binary, not `dotnet App.dll` (single-instance check). With the simulator the overclock
guard allows writes, since nothing real is touched.

### Headless screenshots
```sh
dist/<App>/<App> --screenshot out.png [--after-ms 1500] [--page NAME]
```
This renders the main window with Avalonia's headless platform (no display needed) and writes a PNG.
`--page` selects a page or state in apps that implement it; see the app's notes in `docs/apps/`.
Use screenshots to compare against the original XAML when porting.

## Testing the daemon
`ripple-ccd` can run without hardware or root:
```sh
daemon/ripple-ccd --mock --socket /tmp/ccd.sock --state-dir /tmp/ccd-state --no-restore
RIPPLE_CC_SOCKET=/tmp/ccd.sock dist/FanSpeedSetting/FanSpeedSetting
```
`--oc-unlock-file PATH` changes the file whose existence unlocks overclock writes (for tests only). The wire
protocol (JSON lines, one request per connection) is documented at the top of `daemon/ripple-ccd`.

## Commits
Use Conventional Commits: `type(scope): summary`.
- Types: `feat`, `fix`, `docs`, `chore`, `refactor`, `build`, `ci`, `test`.
- Scopes: `kernel`, `daemon`, `cli`, `ui` (shared platform library), or an app name (`fanspeed`, `flexikey`, ...).
- Examples: `fix(kernel): handle buffer replies shorter than 256 bytes`, `docs: update roadmap`.
- Keep commits focused; say in the message what you verified and on which hardware.

## Code style
- Match the surrounding code. C#: 4-space indent, the original namespaces and class names are kept in ported files so they stay diffable against the originals.
- Ported UI is a faithful clone: do not restyle or simplify; see [docs/porting.md](docs/porting.md).
- Never change command numbers, byte layouts or call order of hardware calls without evidence from the firmware or a hardware test; document findings in [docs/protocol.md](docs/protocol.md).
- Python (daemon, CLI): standard library only, PEP 8, type of change explained in comments where the firmware behaviour is non-obvious.
- Kernel: Linux kernel coding style, GPL-2.0.
- Route every CPU/GPU overclock write through `HardwareGate.GuardOcWrite`.
- Update the relevant `docs/apps/<App>.md` and [docs/roadmap.md](docs/roadmap.md) with your change.
- Do not add files from the original Windows software or decompiled sources to the repository; `reference/` is gitignored.

## Reporting hardware results
Include the model, BIOS version, kernel, and output of `sudo ripplectl status` and `sudo ripplectl table`.
