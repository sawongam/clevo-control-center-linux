# CPUOC (CPU / DRAM Overclocking, CPUOC2.0.exe) - port notes

Status is at the [end](#status). Project/assembly `CPUOC`, namespaces kept (`CPUOC30.View`, `CPUOC30.ViewModel`, `CPUOC30.Model`, `CPUOC.Model`).

## Structure
- `Views/index.axaml(.cs)` main window (converted from index.xaml by script, then post-processed; ratio handlers generated in `index.Ratio.cs`).
- `Views/Window1.axaml(.cs)` error/warning window (platform / BIOS / XTU problems).
- `Styles/Theme.axaml` hand-ported WPF styles: slider (SliderStyle_INC), combobox (dropdown), scrollviewer/scrollbars, checkbox, radiobutton, buttons, TextBox (chromeless).
- `Model/DCHU.cs` original DCHU wrapper, externs redirected to `RippleCC.Platform.InsydeDCHU`. `Model/OverClockModel.cs`, `Model/DCHUModel.cs` ported; `ViewModel/IndexViewModel.cs` unchanged.
- `Platform/Xtu.cs` XtuStub (Intel XTU SDK stand-in), LinuxSensors (read-only Linux readings), SeedMock (simulator seed for screenshots).
- Assets in `Assets/` (the Windows `assets/` folder; XAML says `../Assets/`). `mtlanguage.ini` copied from the installed package.
- Screenshots: `dotnet bin/Debug/net10.0/CPUOC.dll --screenshot out.png --page NAME`; pages: (none)=quiet power mode, `perf` (ratio override panel), `power`, `vr`, `voltage`, `dialog`, `quiet|powersave|entertainment`, `unplugged`, `error`, `error_model`, `error_bios`.

## Architecture of the Windows app (what the "helper" does)
UI never talked to the hardware for OC. Flow: UI writes settings into Insyde "app settings" page 6 (offsets 32..84) then writes the string `cpuoc_bg_106` into the Windows Event Log
("PowerBiosServerLog"); CC30_BG.exe (CPUOC_Background, not ported) watches that log:
104 start per-second telemetry (BG then wrote CPU MHz/V/util/temp/RAM clock into page 6 offsets 9..23 from XTU monitors, perf counters and EC pkg 0x0C byte 18),
105 stop, 106 apply buffer to XTU, 107 reset, 108 apply (FnKey), 10a BIOS defaults, 10b ratio offsets, 10c default ratios/ICC/voltage -> all XTU `Tune()` writes (MSR/MCHBAR via XTU3SERVICE).
The `PipeClient/Client.cs` + MainWindow.xaml(.cs) in the decompiled output (named pipe `\\.\pipe\CPUOCBackgroundPipe`) belong to an unused stub window (not the startup window; startup is View/index.xaml) and are NOT ported.
Window `Window1` is shown instead of index on failures.

Linux replacement: the EventLog is an in-process dispatcher (`DebugLog` -> `CpuOcBackground`); 104/105 are no-ops (UI reads sensors directly); 106..10c are OC writes -> `HardwareGate.GuardOcWrite`, and even when unlocked they only log (no XTU/MSR on Linux).
FnKey power-mode change events ("clevofnkey^202") and AC events (SystemEvents.PowerModeChanged) are replaced by polling once a second (page 1 offset 1, /sys/class/power_supply Mains).

## Stubs / deviations
- Intel XTU SDK (tuning controls, monitors, service checks, platform compatibility): stubbed for i7-11800H (8C/16T, 45 W): base ratio 23, bclk 100, PL1 45 W (list 10..330), PL2 107 W, PL time list 1..128 s, ICCmax auto, per-core ratios 46/46/45/45/43/43/42/42 (range 8..48), no E-cores (so ratio list shows only P cores). XTU service is always "present and running".
- Per-second readings: CPU MHz = mean scaling_cur_freq; usage = /proc/stat delta; temperature = EC pkg 0x0C byte 18 via DCHU (identity as in the original for 45 W), fallback hwmon coretemp; **voltage = STUB** (0.80..0.92 V random, needs MSR); **memory clock = STUB 3200 MHz** (needs MCHBAR/dmidecode). CPU name from /proc/cpuinfo. Memory size from /proc/meminfo (unused by UI).
- `Memory_Speed_Max` is never assigned in the original either (stays 0) -> memory bar shows "full" above 810 MHz (faithful).
- Capability checks (DCHU func 16 bit 0x40, func 122 bit 23) are read from the firmware; in the simulator they are seeded as supported. `RIPPLE_CC_CPUOC_FORCE=1` bypasses both checks on real hardware (addition, off by default).
- Slider.Focus()/ComboBox.Focus() calls in the original value-changed handlers are omitted. ComboBox.Text is replaced by helpers (SelectedItem text).
- Windows event-log writes, GC.Collect calls, WMI memory query, PowerShell CIM query, performance counters: replaced as above.
- Embedded font "BankGothic Md BT": NOT referenced anywhere in the original XAML/C# of this app and no .ttf ships with it; nothing to bundle. Text uses Microsoft Sans Serif (mapped by AppHost) and default (Segoe UI -> Noto Sans).
- Window sizing replicates WindowFitSize: window = 1920*screenW/2560 (e.g. 1440x810 on a 1920x1080 screen), viewbox scaled; full-screen button toggles maximized.
- WPF ResizeMode CanResizeWithGrip (grip) not reproduced (CanResize true, no grip).

## Hardware command audit
Classification: R = read, H = harmless setting, OC = overclock write (every OC write goes through `HardwareGate.GuardOcWrite`; in `DCHU.WriteAppSettings` for page 6 offsets 32..84, in `XtuStub.Tune`, in `CpuOcBackground`).

| func / page | sub / offset | data layout | class | where called |
|---|---|---|---|---|
| GetDCHU_Data_Integer 16 | - | byte0 bit 0x40 = CPU OC supported | R | DCHUModel.IsSupport_CPUOC_WMI16 (startup) |
| GetDCHU_Data_Integer 122 | - | bit 23 (0x800000) | R | DCHUModel.IsSupport_CPUOC_Wmi122Bit23 (startup) |
| GetDCHU_Data_Integer 96 | - | bit 0x40 = BIOS controls undervolting | R | DCHUModel.IsControlCenterControlCPU_UnderVolting (Initial) |
| GetDCHU_Data_Buffer 17 | - | 256 B, byte 2 = custom id | R | DCHUModel.GetCustomId (Initial, SetGraphicsVoltage) |
| GetDCHU_Data_Buffer 18 | - | 256 B: PL1 @8 u32, PL2 @12, mem volt/XMP @16..175, CPU volt offset @176, GT offset @180, ratios @208.., PL time @219, VR @221 | R | OverClockModel.Read_package_WMI18 (Initial) |
| GetDCHU_Data_Buffer 12 | - | pkg 0x0C, byte 18 = CPU temp | R | DCHUModel.GetCPUTemperature (1 Hz; original in BG) |
| ReadAppSettings p1 | off 1 (1 B) | power mode 0 quiet/1 powersave/2 performance/3 entertainment | R | DCHUModel.GetPowerMode (startup + poll) |
| ReadAppSettings p1 | off 2 (1 B) | UI language id | R | DCHUModel.GetUILanguage |
| ReadAppSettings p6 | off 31 (1 B) | fullscreen pref | R | IndexViewModel.IsFullScreen (startup) |
| ReadAppSettings p6 | off 32 | IsSaved flag | R | OverClockModel.Sorting_Current_Info |
| ReadAppSettings p6 | off 33,37,41,49,53..60,61,65,69,73,74,75..84 | saved PL1 i32, PL2 i32, PL time f32, VR i32, core ratios (1 B each, ids 29..4507), voltage offset i32, override f32, GT i32, mode, VR auto | R | OverClockModel.Sorting_Current_Info (only if IsSaved==1) |
| ReadAppSettings p6 | off 9,13,21,22,23 | BG telemetry mirror | R (unused: replaced by LinuxSensors) | DCHU.GetCPU_Freq etc. |
| WriteAppSettings p0 | off 28..31 | app version bytes | H | OverClockModel.SetAppVersion (ctor) |
| WriteAppSettings p6 | off 31 (4 B) | fullscreen pref | H | DCHU.SetFullScreen (not called by UI) |
| WriteAppSettings p6 | off 32 (1 B) | IsSaved flag (1 on SAVE, 0 on DEFAULT) | OC | DCHUModel.SetIsSaved |
| WriteAppSettings p6 | off 33 / 37 / 41 | PL1 i32 / PL2 i32 / PL time f32 | OC | SAVE (OK in dialog) and DEFAULT, via IndexViewModel.SetXTUInfoToInsydeBuffer |
| WriteAppSettings p6 | off 49 / 74 / 61 / 65 / 69 | VR limit i32 / VR auto / core voltage offset i32 / override f32 / GT voltage offset i32 | OC | same |
| WriteAppSettings p6 | off 53..60, 75..84 | per-core ratio bytes (ids 29,30,31,32,42,43,96,97,107,108,4500..4507) | OC | same |
| WriteAppSettings p6 | off 73 | voltage mode | OC | DCHU.SetCPUVoltageMode/SetVoltageMode (not called) |
| In-log `cpuoc_bg_104` / `105` | - | start/stop BG telemetry | H (no-op) | ctor completion / window Closing |
| In-log `cpuoc_bg_106` | - | apply Insyde buffer to XTU (PL1/PL2/time/VR/voltage/ratios) | OC | after SetXTUInfoToInsydeBuffer (SAVE, DEFAULT) |
| In-log `Close_Loading` | - | closes the CPUOC_Loading splash | H (no-op on Linux) | startup / error |
| BG-only 107/108/10a/10b/10c | - | reset / FnKey apply / BIOS defaults / ratio offsets / default ratios+ICC+voltage | OC | emitted by other CC apps, handled by CpuOcBackground (gated, not applied) |
| XTU Tune(id, value) | - | any XTU control | OC | XtuStub.Tune (BG only; never reaches hardware) |
| Named pipe `CPUOCBackgroundPipe` | "Close_Loading" | splash close | H | only in unused MainWindow stub: not ported |

## Status
- Complete: main window (all four left panels: ratio override, power limit, VR limit, voltage), save dialog, Window1 error windows, power-mode gating (quiet/powersave/entertainment/unplugged messages), live gauges, translations from mtlanguage.ini. Pages rendered with `--screenshot` (main_quiet, main_perf, main_power, main_vr, main_voltage, main_dialog, main_powersave, main_entertainment, main_unplugged, err_xtu, err_model, err_bios) were checked against the XAML.
- Not verified: real-hardware values (needs the daemon), mouse interaction with sliders/combos (only rendered, not driven), Window1 transparent corners, dropdown popup rendering (not screenshot-able headless), OC writes on real hardware (locked by HardwareGate; XTU apply never performed).
- Screenshot window size follows the headless screen (1440x960 here), see WindowFitSize.
- Ratio panel scroll list shows only P cores 1..8 (stub, no E-cores); other core rows are removed like the original does.

## Font check
Original XAML uses FontFamily="Microsoft Sans Serif" on 132 elements and WPF default (Segoe UI) on the rest (e.g. Utilization/Voltage text boxes). Port maps these to Liberation Sans / Noto Sans in `AppHost.Fonts()` like every app; the narrower-looking text is the default-font elements. No change.
