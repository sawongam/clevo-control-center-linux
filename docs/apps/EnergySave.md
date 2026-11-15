# EnergySave port notes

Avalonia port of `DesktopBridge_EnergySave` (WPF). Assembly `EnergySave`, namespace `DesktopBridge_EnergySave` kept.

## Layout / structure
- `MainWindow.axaml` generated from the decompiled XAML (Apps_main canvas 1900x1060 inside Viewbox), `MainWindow.axaml.cs` is the code-behind ported method by method, `Knobs.cs` the two schedule-dial handlers.
- `Controls.cs`: `ProgressCircle` (pie + ring path), `KnobStart/KnobEnd` (replace the templated WPF Sliders), `PowerChart` (hand-drawn WinForms Chart), `ResizeGrip`, `MessageBox` (Windows-style dialog).
- `DCHU.cs` -> `CCenter.Platform.InsydeDCHU`, `IniFile.cs` -> `Ini`, `language.cs` reads `mtlanguage.ini` next to the exe.
- `PowerSchemeHelper.cs`: powrprof adapter + `PowerStatusAdapter` (SystemInformation.PowerStatus).

## Window sizing (from WindowFitSize)
Window declared 1920x1080 but MaxWidth/MaxHeight = (0.75*screen)+20 / +18 (rate = 2560/screenW), Viewbox fixed at 0.75*screen. So on a 1920x1080 screen the window is 1460x828 with a 10/9 px black margin. Fullscreen button maximises and sets the Viewbox to the declared 1920x1080 (WPF `Width` DP), exactly as the original. Drag region test uses declared width (1920) like the original. Resize grip drawn bottom-right (ResizeMode CanResizeWithGrip).

## Deviations / stubs
- Windows power plans: three stock plans mapped onto power-profiles-daemon (`powerprofilesctl get/set` if installed); otherwise the active plan is stored only in Reg (`PowerPlan`) - "stored only, not applied". The app itself only reads the active plan name for the summary label.
- Display-off / sleep / brightness / processor-state AC/DC values: not used by this app, no adapter written.
- `SystemEvents.PowerModeChanged`: emulated by polling AC state in the 1 s battery timer.
- Battery %, AC state: /sys/class/power_supply (dev overrides `CCENTER_ES_BATTERY`, `CCENTER_ES_AC`).
- `GetProductID_PCI()` (GetProductdll.dll): stub returns 0 (unknown model, affects only the WattLimit clamp for L140CU).
- Model check `(GetWMI(16) & 0x100)` is skipped under the mock backend (mock returns 0).
- `SetWMIForTotalCurrent` (cmd 65) needs the firmware result as return value; the shim's `SetDCHU_Data` returns 1/0, so `SetDCHU_DataEx` output bytes are used.
- EventLog IPC ("clevofnkey^102" language-change notification, debug logs): no-op.
- Single-instance check kept (not in screenshot mode).
- Chart: SplineArea x2 (AC #F000FFDE, DC #F0FFB400), dotted cyan grid, #1C1C1C background, labels 8pt Microsoft Sans Serif at device size (not scaled by the Viewbox, like the WinForms host). Plot rectangle is fixed to y 23..353 (aligned with the consumption arrow marker); exact WinForms auto-layout margins are approximated.
- Knob drag: ring hit area (r 97.5..225) emulated geometrically; hit-testing not verified with real pointer input.
- WPF scrollbar of the profile ListBox is Avalonia's default look.
- `--page`: powerplan (default), energysave, unlocked, manual, overlap, rename, minute, fullscreen, msg-save, msg-overlap, msg-last; suffix `-data` fills the chart with synthetic data. `--lang CODE` (e.g. de) overrides UI language.

## Hardware commands (via InsydeDCHU)
| func | sub | data | when |
|---|---|---|---|
| GetWMI 70, 16 | | | startup (model check) |
| GetWMIPackage 17 | | bytes 208/209 charge/discharge limits | startup |
| ReadAppSettings(0,0,2), (1,2,1) | | product id / UI language | startup, every translation |
| SetWMI 65 (ADC read) | | AC: 0x058B01B1 then 0x058D01B1; DC: 0x058B01B1-style words per code | every chart tick (1 s / 1 min) |
| SetWMI 118 | | 0x05000000 / 0x05000001 enable, time/days/schedule/limits words | start/stop/save |
| SetWMI 118 sub 7 | | FlexiCharge | BatteryRefreshCommand |
| SetWMI 121 sub 28 / 29 | | ACOFF / refresh | BatteryRefreshCommand |

## Status
- Complete: all pages/states ported and rendered with `--screenshot`: power plan (+data), energy save locked/unlocked/manual/overlap/rename, minute mode, fullscreen toggle state, message dialogs (msg-save/overlap/last), --lang.
- Run the `EnergySave` binary, not `dotnet EnergySave.dll`: the single-instance check counts processes named like the host and would exit.
- Not verified: real pointer dragging of the schedule knobs, battery-limit handle dragging, list/rename/add/delete clicks (logic ported unchanged, only screenshot-verified in static states); real maximise on a desktop; behaviour on real hardware (cmd 65 values, 118/121 writes).
- Screenshot window is 1460x978 because the headless screen is 1920x1280.
