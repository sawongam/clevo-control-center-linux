# FanSpeedSetting port notes

Port of `FanSpeedSetting.exe`: fan and temperature gauges, fan modes, custom curve editor and anti-dust.
The main page, modes, curve editor and anti-dust are checked against the simulator.

## Layout
- `FanSpeedSetting.csproj`, `Program.cs`, `App.axaml(.cs)` (App.cs also has `MockOverlay`, see below)
- `MainWindow.axaml(.cs)`: title bar, window sizing, skin colour, test hooks (`--page`, `--act`)
- `Page_system_monitor.axaml(.cs)`: the only page (fan gauges, temp gauges, mode radios, custom curve, anti-dust)
- `UControl/UserControl_FAN`, `UserControl_FanTable2` (curve editor), `UserControl_DialogBox`; `loading.axaml` splash
- Non-UI logic copied from original with minimal edits: FAN.cs, InsydeDriver.cs (externs redirected to RippleCC.Platform.InsydeDCHU), RWReg.cs, RWIni.cs, language.cs, Support_BIT.cs, Global.cs
- `Platform.cs`: `Img` loader, `Registry` shim (-> Reg), `MutableThickness`
- Images copied to `image/` (lower-case, all XAML refs lower-cased). mtlanguage.ini/oem.ini copied to output.

## Conversion method
XAML was machine-converted from the ILSpy output, then trimmed by hand: only the styles in use were kept.
Template triggers were converted to `ControlTheme` + `^:pointerover /template/ X#name` styles; base values touched by triggers moved into styles.

## Deviations / stubs
- Windows event-log notifications (fan hotkey events 106/107/112/143, turbo fan) have no Linux source: handlers exist in MainWindow (`ExecEvent`, `FnKeyEvent`, `CC30Event`) but nothing calls them.
- Single-instance check skipped when process name is `dotnet`.
- Loading splash: timer-driven opacity ramp instead of Storyboard.
- UserControl_FAN: rotation angle applied directly (WPF bound RotateTransform.Angle to FontSize / StrokeDashOffset).
- Slider LayoutTransform skew -> to verify (RenderTransform).
- ComboBox `Text=` (non-editable) replaced with SelectedIndex = FanOffset.
- Hidden vs Collapsed both -> IsVisible=false.
- `MockOverlay` (App.axaml.cs): in mock mode answers WMI 16 = 0xF1 (fan setting supported), WMI 96 = 0 (0x80 with RIPPLE_CC_MOCK_ANTIDUST=1), seeds app page1 off1 = 3.
- Window defaults to maximised when registry value absent (as WPF); screenshot mode stays windowed.
- Resize grip not yet implemented.

## Hardware commands (from FAN.cs, unchanged): fan mode 0x79/01, offset 0x79/0E, defaults 0x79/22, anti-dust 0x79/29 and 28, time 0x76/01, curve pkg 0x0E, reads 0x0C, 0x0D, 0x11; app settings page 4 (fan state), page 1 (mode/lang), page 0 off 12 (version).
First run (registry First_run==0) writes defaults + curve + mode 0 + offset 0, as the original.

## Verification (mock backend)
Rendered with `--screenshot` and checked against the XAML: auto, max, custom (curve editor), antidust (RIPPLE_CC_MOCK_ANTIDUST=1).
Test hooks: `--page auto|max|maxq|silent|custom|custom-gpu1|custom-gpu2|antidust`, `--act "click:RB_FAN_max;drag:CPU:2:210:150;save-curve;reset-curve"`; use with RIPPLE_CC_MOCK_LOG=file and RIPPLE_CC_CONFIG=<fresh dir>.
MOCK_LOG verified: mode clicks send 0x79 sub01 data 1/5/0/6; dragging CPU T2 to (210,150) then Save sends pkg 0x0E with T2=65 D2=0x80 T3=80 D3=0xDB (curve math as FAN.cs); first run sends 0x79/0x22, 0x0E, mode 0, offset 0, anti-dust scheduler 0x79/0x28 (as original: LoadMultiLang resets CB_AM items which fires the scheduler handler).
Headless screen is 3:2 so the window is 1440x960 with the 16:9 content letter-boxed (WPF would do the same on that screen).

## Known gaps
- Windows resize grip (CanResizeWithGrip) not drawn; window edge resize unverified.
- ComboBox (offset, AM/hour) uses a simplified template (skewed box + arrow + popup list), not pixel-compared; ScrollViewer/ScrollBar templates of the drop-down not ported.
- Turbo-fan UI states, loading splash, and dialog box not screenshot-checked (dialog shows only when WMI16 lacks fan support).
- Real hardware untested; first run on real daemon would send the factory-default writes listed above.
