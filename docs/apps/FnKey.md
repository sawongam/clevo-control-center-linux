# FnKey port notes

Status legend: DONE / STUB (logged no-op, reason given) / UNVERIFIED (needs real hardware).

## Layout
| file | origin |
|---|---|
| OsdForm.cs | OsdForm.cs (WinForms) - the OSD popup, code-only Avalonia Window |
| Form1.cs | Form1.cs - controller (no tray UI); event switch `Arg001_GetEvent` copied 1:1 |
| Features.cs, Global.cs, DCHU.cs, InsydeAcpiProt.cs, KBModule/RGBKB/WhiteKB/PerkeyKB/BaseKB.cs, Device/FAN.cs, Device/GPU.cs | original logic, DllImports redirected to `RippleCC.Platform.InsydeDCHU` (same command numbers, order) |
| EventSources.cs, App.axaml.cs, Program.cs | new: IEventSource + unix socket + CLI |
| Rfkill.cs, LCDBrightness.cs, Device/Brightness.cs, Device/battery.cs, Device/DeviceControl.cs, SMBIOS.cs, CallingVariations.cs | Linux adapters named after what they replace |
| tools/extract_resx.py | extracts Resources.resx images -> Assets/Resources/*.png (72 osd_*/bg PNGs) |
| tools/gen_features.py | one-shot helper used to lift methods verbatim from decompiled source |

## OSD geometry / timing (from OsdForm.cs)
- Form 429x90 initially, resized to the image size on each show; BackColor black; borderless; TopMost; not activated; location (50,60) screen px.
- ShowOsd(img): pic = image, opacity 0.8, timer1 = 1500 ms; timer1 tick -> timer2 (50 ms): opacity -= 0.2 until <= 0.3 -> content hidden, then window hidden on next tick.
  Sequence 0.8,0.6,0.4,0.2 -> hide (~150 ms fade).
- Bar OSDs (ShowBarOSD, opacity 0.9): pic_bar (osd_bar_full.png, 240x90) at (107,0), width = 240/max*value (<10 -> 8, >230 -> 240); value label in panel1 (352,29,67x33), Arial 15.75pt (21 px) white, centred.
  ShowVolume/ShowBrightness/ShowWhiteKbfor5/6 exist but the original Form1 only ever calls ShowOsd + ShowWhiteKbfor5/6 (Windows draws its own volume/brightness OSD).
- Gate: OSD shows only if ShowOSD setting on (AppSettings page1 off3 = 1, tray "OSD" item), `fnKeyStatus == Ready`, and (for caps/num/scr/Fn-lock) bSupportLockOsd.
- Deviations: opacity applied to the content (compositor alpha), not the OS window; position is honoured on X11 only (Wayland ignores toplevel positions; would need layer-shell); window is not click-through / `_NET_WM_WINDOW_TYPE_NOTIFICATION` not set.
- Hi-DPI: layout is in physical px (scaled by 1/Screens.Primary.Scaling), as the WinForms form was.

## Command protocol (socket `$XDG_RUNTIME_DIR/ripple-cc-fnkey.sock`, also CLI flags)
One text line per command:
- `event <n>` / `--event 0x59`: n = the ACPI notify value. event = n & 0xFF, data = n >> 8 (only 186 uses data). Equivalent to log message `Arg001^<n>` (decimal).
- `msg <text>` / `--msg`: raw original message, e.g. `Arg001^89`, `clevocc30^101^2` (set power mode 2 from CC30), `clevocc30^107`, `clevokeyboard^202^5`, `dchu^10e` (quit).
- `osd <name>` / `--osd`: `osd_caps_on`, `caps_on`, `volume:40`, `volumemute`, `brightness:70`, `kbwhite6:3`, `kbwhite5:2` (direct OSD, no action).
- `sleep` / `resume` (Windows WM_POWERBROADCAST 4 / 18 -> OnSleep/OnResume), `toggle-osd`, `quit`.
- CLI without a running agent: one-shot (no start-up DCHU init), exits ~2.2 s later. `--no-init` / FNKEY_NO_INIT=1 skips OnFirstRun/OnStart in the agent. `FNKEY_LOG=1` (or a path) logs the original "OutLog/InLog" diagnostics.

## How the Windows side obtains events (for the kernel/daemon side)
Arg001Event.cs: opens `\\?\ACPI#CLV0001#1#{86994c74-ad43-4812-b7e7-0c420b5c5fd7}` (AcpiBridge.sys, same ACPI device clevo_acpi binds on Linux), creates a manual-reset Win32 event, hands it to the driver with IOCTL_ACPIBIOS_WAIT_EVENT (0x322004, `ACPI_HSA_WAIT_EVENT{HANDLE}`), then loops: WaitForSingleObject(INFINITE); ResetEvent; IOCTL_ACPIBIOS_GET_NOTIFY_VALUE (0x322008) -> 8-byte value; writes event-log line `Arg001^<value>`. FnKey (Form1.outlog_EntryWritten) parses it: `event = value & 0xFF; data = value >> 8`.
Who runs that loop is chosen by AppSettings page1 off12 (`Check_Arg001_byDCHUServiceSend`): 1 = DCHUService sends the lines, 0 = FnKey itself starts `Arg001Event`. The WMI fallback is root\WMI CLEVO_EVENT.
Linux: the ACPI Notify(0x?? ) value for `\_SB.DCHU` / CLV0001 must be forwarded as `event <value>` to the socket (clevo_acpi currently only handles a few notify codes in-kernel). The notify value is the event code in the table below (decimal).

## Event table (Form1.Arg001_GetEvent). Hex = value passed to `--event`.
"Linux" column: whether the desktop already does it natively and what the port does.
| dec | hex | meaning / action | DCHU / state | OSD | Linux |
|---|---|---|---|---|---|
| 32 | 0x20 | KB backlight down (white/RGB) | KB.SetBrightnessDown (type1: DCHU 0x27 level; type2/6/22: DCHU 0x67 0xE0.. status + brightness 47/95/143/191; AppSettings p2 off35/80), notify clevofnkey^201 | osd_backlight_low | tuxedo_keyboard/UPower handle kbd backlight keys; port only drives DCHU+OSD |
| 33 | 0x21 | KB backlight up | KB.SetBrightnessUp | osd_backlight_high | same |
| 63 | 0x3F | KB backlight toggle on/off | KB.Status toggle + SetStatus | osd_backlight_on/off | same |
| 129/130 | 0x81/0x82 | KB brightness down/up (typeRule 3,2,6,22 only) | as 32/33 | low/high | same |
| 138 | 0x8A | KB brightness step: type22 = 5 levels cycle 0..4; type1 = read level (DCHU get 61), 6-white: ShowWhiteKbfor6 else off/low/high | SetBrightnessLevel | osd_backlight_0..4 bar / off/low/high | same |
| 139/140/141 | 0x8B-0x8D | RGB left/mid/right zone toggle | KB.*Status toggle + SetStatus (DCHU 0x67 0xE0) | - | |
| 158/159 | 0x9E/0x9F | KB backlight on/off toggle (types 3,2,6,22) | Status toggle | osd_backlight_on/off | |
| 57 | 0x39 | cycle RGB colour (KB.Change_ToggleRGBColor) | 15/16-colour table, AppSettings p2 off127, DCHU 0x67 0xF0/F1/F2 | - | |
| 89 | 0x59 | performance-mode key (cycles NextMode) | SetPowerMode (DCHU 0x79 sub 0x19), AppSettings p1 off1 and p1 off16, fan p4 off5, EnergyStar 0x4F; ignores re-trigger for 1 s | osd_quiet2 / powerSaving2 / peformance2 (Gaming on MEDION) / entetainment2 (balanced if power-mode UI 1/2) | Linux has no firmware-agnostic key; port does the DCHU write + OSD |
| 246/247 | 0xF6/0xF7 | camera off/on | AppSettings p1 off6 (WriteCCDValue; firmware already switched it) | osd_camera_off/on | desktop does not toggle; OSD only |
| 252/253 | 0xFC/0xFD | touchpad toggle (I2C touchpad) | state in AppSettings p1 off10 (+ registry shim); Windows sends Win+Ctrl+F24 | osd_touchpad_on/off | libinput/DE handle it natively (STUB for the action); port shows OSD |
| 93 | 0x5D | touchpad state refresh | - | osd_touchpad_on/off | |
| 149 | 0x95 | open Control Center 3.0 (or notify clevofnkey^204) | - | - | AppHost.LaunchSibling("ControlCenter30") |
| 131 | 0x83 | open LED keyboard app (types 1,2,3,6,22) | notify clevofnkey^203 | - | LaunchSibling("LedKeyboardSetting") |
| 133 | 0x85 | airplane mode key | DCHU 0x79 sub 26 (LED) 1/0; radio on/off | (no OSD here) | rfkill state read; write STUB (DE handles key) |
| 123 | 0x7B | Flexikey on/off | AppSettings p1 off8/9 | osd_combo_on/off | filter driver STUB |
| 124 | 0x7C | rotate screen 180 | - | - | STUB |
| 132 | 0x84 | open Windows recovery | - | - | STUB |
| 192 | 0xC0 | lid closed: suspend/shutdown per power plan | - | - | logind handles lid; STUB |
| 84 | 0x54 | NVSR panel workaround / OLED screen refresh | DCHU 0x5E data 1 if OLED | - | no-op unless OLED |
| 215 | 0xD7 | OLED screen off / FnF2 touchpad hook | - | - | STUB |
| 166/167 | 0xA6/0xA7 | docking on/off | - | osd_docking_on/off | |
| 184/185 | 0xB8/0xB9 | GPS off/on | - | osd_gps_off/on | |
| 199/200 | 0xC7/0xC8 | NumLock off/on (only if no USB keyboard) | - | osd_num_off/on | DE shows own OSD; OSD only |
| 201/202 | 0xC9/0xCA | CapsLock off/on | - | osd_caps_off/on | |
| 203/204 | 0xCB/0xCC | ScrollLock off/on | - | osd_scr_off/on | |
| 222/223 | 0xDE/0xDF | silent mode on/off | - | osd_silent_on/off | |
| 234/235 | 0xEA/0xEB | 3G off/on | - | osd_3g_off/on | |
| 301-304 | 0x12D-0x130 | flexi-access on/off, game-center on/off | - | osd_flexiaccess_on/off, osd_gc_on/off | (>0xFF: use `msg Arg001^301`; code arrives as value 301 only if firmware sends it) |
| 64 | 0x40 | anti-dust fan running | - | osd_fan_speed_AntiDustRuning | |
| 106 | 0x6A | fan MaxQ | AppSettings p4 off5 = 5 | osd_fan_speed_MaxQ | |
| 107 | 0x6B | fan custom | p4 off5 = 6 | osd_fan_speed_Custom | |
| 112 | 0x70 | fan auto (silent when power mode 0) | p4 off5 = 0 | osd_fan_speed_silent/auto | |
| 143 | 0x8F | fan max | p4 off5 = 1 | osd_fan_speed_max | |
| 58 | 0x3A | turbo fan | - | osd_fan_speed_Turbo | |
| 72/73 | 0x48/0x49 | Fn lock disable/enable | - | osd_Fn_lock_disable/enable | |
| 186 | 0xBA | battery alarm start: bit7 of data = set brightness (data & 0x7F), loop beep, low-battery control | Write_Brightness (STUB), EC RAM 0x551 read (DCHU 4 sub 1) | - | brightness write STUB |
| 187 | 0xBB | battery alarm stop | - | - | |
| 59 / 60 | 0x3B/0x3C | battery low on battery / restore | DCHU 0x79 sub 44 data 2 / 3 | - | |
| 48-56,78,79 | 0x30-0x38,0x4E,0x4F | brightness preset keys (WMI 17 package bytes 210-220, default 20..100 / 10) | SetBrightnessFromWMI17 -> LCDBrightness.SetBrightness | - | brightness write STUB (backlight read-only) |
| 96/97 | 0x60/0x61 | GPU turbo off / on | NVIDIA lock clock via ControlGPU.exe (not ported) | - | STUB |
| 65-71 | 0x41-0x47 | AMD (ClevoID 0x50F0/0x50F4 only) PROC_HOT / power-limit state machine with 10 ms/5 s timer | ryzen.dll | - | n/a on Intel; state machine kept, AMD calls are logged stubs |
AppSettings messages besides Arg001: `clevocc30^101^<mode>` set AC power mode, `^107` re-enable key, `^10f^<n>` AMD manual mode UI, `^114` refresh-rate flag; `clevokeyboard^202^5|11`, `clevobattery^207^0|1`, `clevoenergy^208`, `dchu^10e` quit, `dchu^110/111` re-check who sends Arg001.

## Start-up (Form1_Load/OnStart; skipped by --no-init)
Reads ClevoID, writes AppSettings p0 off0..1 (ClevoID swapped), compares version bytes p0 off8..11 -> OnFirstRun (zeroes AppSettings pages 0-15, DCHU 0x79 sub34=1 fan defaults, SetCCD(1) = DCHU 0x22, default power mode, ShowOSD=1 ...). OnStart reads features (DCHU 4 sub8, 0x52, 0x7A, 0x60, 0x3C, 0x10), loads fan info, KB.Init, sets current power mode. **This writes to hardware on first run exactly like Windows**; use `--no-init` for an event-only agent.

## Stubs / not ported
Tray icon + menu (OSD toggle, language, power mode, GPU switch, battery saver, open CC30): not ported (logic exists: ToggleOsdSetting, Mode_click). Windows power plans/ASPM, powercfg, SetSuspendState, keybd_event, SendInput, WMI, registry radio state, ryzen.dll, XTU, ControlGPU.exe helper (must use HardwareGate if ever implemented), perkey_api.dll (this laptop has KBTP=6 RGB so unused), LineLEDAPI, kb.dll Flexikey filter, key hooks (HookForm: first input after wake sends DCHU 0x5E=1 - not ported, OLED only), sound beeper (nosound.wav loop).
ClevoID: GetProductdll!GetProductID_PCI has no Linux equivalent; env FNKEY_CLEVO_ID (hex), default 0 -> Intel/generic branches.

## Verification status
- Every OsdImage enum value (`osd_*.png`, 67) was rendered, plus the bar variants (`bar_volume_37/100/0`, `bar_brightness_60`, `bar_kbwhite6_3`, `bar_kbwhite5_2`, `bar_volumemute`), rendered with `--screenshot out.png --page <name>`. Window sizes equal the source image sizes (429x90, 263x90 for 3G/GPS, 420x82 docking, ...). Shown at opacity 0.8/0.9 over a simulated grey desktop (screenshot mode only), so they look dimmer than the raw PNGs. Checked visually against OsdForm layout: bar fill width, value label position, image placement OK. Volume max/min text ("100"/"0"/MAXIMUM) is part of the PNGs.
- Not visually verifiable here: real window behaviour on X11/Wayland (topmost, no-focus, position (50,60), fade timing) - only headless rendering was run.
- Socket + CLI verified against a running `--no-init` agent with the mock backend (events 0xC9, 0x59, 0x6A, 0x20, 0xF6, `--osd`): handled, DCHU calls logged (e.g. 0x20 -> DCHU 0x67). With `--no-init` the SupportPowerMode flag is unread, so event 0x59 is a no-op until a normal start (faithful to the original gating).
- Mock quirk: with all-zero AppSettings, KB brightness-down computes brightness -1 exactly as the original would on uninitialised settings; real runs initialise it through OnFirstRun/KB.Init.
- Screenshot mode does not read the ShowOSD setting (forced on); in `--no-init` mode ShowOSD stays default true. `toggle-osd` flips the persisted setting.

## First run is opt-in (port change)
`OnFirstRun` + `KB.FirstRun` (zero AppSettings pages 0-15, DCHU 0x79 sub34 fan defaults) now run only with `--first-run` or `FNKEY_FIRST_RUN=1` (`Form1.AllowFirstRun`); otherwise a log line is written and start-up continues with OnStart. Without it, uninitialised AppSettings may leave ShowOSD=0. FnKey is not started automatically and has no menu entry.
