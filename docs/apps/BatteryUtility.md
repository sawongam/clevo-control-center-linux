# BatteryUtility port notes

Original: BatteryPackUtility.exe (Control Center 3.0). It is a "Battery Refresh" (calibration + health diagnosis) tool,
single page. No charge-threshold UI exists in it; the only battery-related DCHU writes are the refresh commands below.

## Status
Complete: main page (all states), dialog box (all messages), loading splash, window logic, language (mtlanguage.ini, `--lang xx`).
Build: `dotnet restore --source ~/.nuget/packages` once (offline), then `dotnet build --no-restore -p:BuildProjectReferences=false`.
Screenshots: `src/BatteryUtility/screenshots/*.png` (main, healthy, deteriorated, failure, discharge1..adjusting, finish, dialog6-9, unsupported, nobattery, lang_de, loading).
`--page` values: healthy deteriorated replace failure discharge1 charge1 discharge2 charge2 adjusting finish dialog6 dialog7 dialog8 dialog9 loading.
Env: RIPPLE_BAT_NONE=1 (simulate no battery), RIPPLE_BAT_UNSUPPORTED=1 (feature bit missing -> "This machine does not support."). Use RIPPLE_CC_CONFIG=<tmp dir> when screenshotting (state is persisted in registry.json: OngoingStatus, elapsed time, last refresh date).

## DCHU commands (all via RippleCC.Platform.InsydeDCHU; unchanged from the original)
| func | sub | data | when |
|---|---|---|---|
| 0x79 (121) | 0x1C (28) | 1 / 0 | SetACOFF: Start click (1), every stage change (0 during discharge stages, 1 during charge/stop), AC-out handling |
| 0x79 | 0x1D (29) | 1 / 0 | SetBatteryRefresh: 1 while a refresh runs, 0 on Stop |
| 0x76 (118) | 0x07 | 0 / 1 | SetBatteryFlexiDischange (FlexiCharger discharge): 0 during refresh stages, 1 on Stop |
| 0x76 | 0x05 | 0 | EnableEnergySave(false) on Start click (int 0x05000000) |
Reads: GetWMI(70), GetWMI(16) (bit 0x200 = BatteryUtility supported), GetWMI(122), pkg 17 (skin colour bytes 3..5), pkg 7 (battery pack data; thresholds, status), AppData(0,36,4) read + write of version stamp when different, AppData(1,2,1) language.
Nothing is sent at startup except those reads and the version stamp (app settings, not hardware). If OngoingStatus in registry.json is not Stop, startup resumes the refresh and re-sends the stage commands (as the original).

## Replacements
- GetSystemPowerStatus -> /sys/class/power_supply (battery.cs). No battery node: AC online, 100% (plausible values).
- powercfg "BatteryUtility" power plan -> `systemd-inhibit --what=idle:sleep sleep infinity` child while refreshing (no system settings modified). Not started in screenshot mode.
- VBScript ScriptControl Eval -> small integer expression evaluator (+ * parentheses), same semantics.
- Package 7: real DCHU answer used; if design capacity is 0 (simulator / unserved) the package is synthesised from sysfs (full/design charge, cycle_count, status) + Battery.ini thresholds/criteria (BatteryPackage.cs). Manufacturer date = now-1y and stop-charging threshold = 5 are guesses (not in sysfs).
- Events 85/90/91 (firmware via Windows EventLog/WM_COPYDATA) and PowerModeChanged: no source on Linux; a 2 s poll calls Update_Battery_Full_Status() while a refresh runs (reads pkg 7 bit 0x10/0x20) and UpdatePowerStatus() on AC/percent change when idle. ACOutEvent (event 90) is NOT emulated (UpdatePowerStatus' own AC-out handling remains).
- OutLog messages ("clevobattery^207^x" to the tray) -> Debug output only. Clevo_MSG window messages: no-op.
- C:\LOG\ battery logs -> ~/.config/ripple-cc/LOG/. WMI_7 registry blob stored as hex string.
- Init_WMI: under the mock backend (func 16 returns 0) the BatteryUtility support bit is forced on so the main page shows; real backend unchanged.
- Read_battery_ini: exception on malformed package falls back to default criteria instead of crashing.
- Window: starts maximised as in the original (window_FullSrceen getter is constant 1); Viewbox scales the 1920x1080 design. Resize grip / Windows' AllowsTransparency not reproduced (WindowDecorations=None, CanResize).

## Unverified / known gaps
- Splash window (loading) is ported (8 animated circles) but the headless screenshot captures the main window, so loading.png shows the main page; splash look unverified.
- Hover/pressed states, drag, minimise, real-hardware command effects not verified.
- Real firmware pkg 7 contents unknown; fallback path used in the simulator.
