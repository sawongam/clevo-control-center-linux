# ControlCenter30 port notes

Port of `ControlCenter30.exe`, the launcher and power-mode window. Window1, Window_MOUSE and the dialogs are ported.

## Layout
- Window1.axaml(.cs): main launcher (UniformGrid of app tiles + "Power Modes" sub-page + AMD manual mode + PgUp/PgDn tile). 
- Window_MOUSE.axaml(.cs): alternate UI used when BIOS feature page 7 byte17 high nibble (PowerModeUI_ID) == 2.
- UserControl_DialogBox / DialogBox2: modal overlays.
- Page_1App..Page_8Apps (in the original decompiled sources): NOT referenced anywhere in the app (dead code; Window1 holds button_1..8 itself). Not ported.
- App.axaml(.cs): startup logic of original App.cs (feature detection) + ToolTip theme.
- XAML was converted mechanically from the ILSpy XAML, then hand-fixed. Only resource
  styles actually used were converted. ComboBoxStyle1 (AMD only) was rewritten by hand (simplified popup).
- Images: lower-case paths (Img.Load lower-cases, ' ' -> '-').
- ContentStringFormat (used by WPF templates as a label carrier) -> `Tag`.

## Tile / model decisions (what is shown on this machine)
Decided at runtime exactly like App.cs: DCHU 16 (apps bitmask), 96 (DTT, AMD manual), 122 (bit23 CPU OC), package 17 (custom id/skin RGB),
package 13 byte15&0xF = kb type (LED tile needs !=0,4), AppSettings page 7 (BIOS feature list; byte17 = power mode bits + UI id).
Tile -> sibling: PowerModes (internal sub-page), LED Keyboard->LedKeyboardSetting, Flexikey->Flexikey, GPU->GPUOverclocking,
CPU->CPUOC, Fan->FanSpeedSetting, EnergySave->EnergySave, Battery->BatteryUtility, FlexiAccess->none (never installed on Linux).
SMBIOS -> /sys/class/dmi/id: System.ManufacturerName=sys_vendor ("Dream Machines"), Family=product_family ("Not Applicable"),
so default title.png (no AORUS/MEDION/Gigabyte). Skin colour = package 17 bytes 3..5 (else XAML default #FFFF00D2 from BTN_close.Background).
GetProductdll.dll GetProductID_PCI was declared but never called -> nothing to map.
UNVERIFIED: real values of the above feature reads on the laptop (not read; no hardware access). In mock mode MockProfile.cs injects a
plausible set (not measured). Env overrides RIPPLE_CC30_* documented there; `--page` names: fnkey, dialog, dialog2, dialog3, apps1..apps8, pgupdn, mouse, amd, oldmodes.

## Power mode buttons
Original CC30 only writes event "clevocc30^101^N" to the OutLog; FnKey.exe does the DCHU work. Port: ServiceBridge.cs performs FnKey's
Features.SetPowerMode directly: AppSettings(1,1)=N; N==2: DTT/turbo bits (0x80/0x40, DTT driver never present) -> SetWMI(121,25,..)
= DCHU 0x79 arg (0x19<<24 | N); else SetWMI(121,25,N); energy star SetWMI(79, 2/0/1) when WMI122 bit30 clear; AppSettings(4,5)=0; then "clevofnkey^202".
NOT replicated: GPU clock lock/OC (Load_GPU_forFn_3), Windows power plans/ASPM, XTU, panel refresh rate, MaxQ fan check.
Power-mode changes by FnKey are picked up by polling AppSettings(1,1) every 1 s (replaces event log 202).

## Deviations
- Single instance via lock file; SetCC30_APP_Launch loop terminates (original could loop forever).
- ChangeFocus (FindWindow/SetForegroundWindow + 2 s sleep) is a no-op.
- Registry Dynamic Tuning / WMI driver scan -> constant false; CPU name from /proc/cpuinfo.
- AMD controls only for AMD CPU + WMI96 bit13 (not on this Intel machine).
- "OSD Only" message then exit when no tiles.
- Tooltip placement rectangles not replicated (ToolTip.ShowDelay=1000 kept).
- Window_MOUSE resize grip not implemented.

## Verification status
Rendered with `--screenshot` and checked against the XAML: main (7 tiles), apps3, pgupdn,
fnkey (new 4-mode UI), oldmodes (circular UI), amd (RIPPLE_CC30_P7_17=0 --page amd), mouse, dialog, dialog2. 5 s run: no exceptions;
startup DCHU reads: 0x46, 0x0D, 0x11, 0x7A, 0x10, 0x60 (+ AppSettings).
Click paths (mode buttons -> ServiceBridge writes 0x79 sub 0x19, tile launch -> LaunchSibling) are coded but were NOT exercised
(no UI automation); tooltips, hover/pressed states, real-hardware feature bits unverified. Negative-margin quirk: Avalonia clamps
desired size for negative bottom margins, so tile label margin was changed to Margin="0,200,0,0" (same position). Other
negative-margin spots inside RadioButton templates looked correct in screenshots.
ComboBox popup art (dropdown arrow) simplified. With 7 apps + PgUp/PgDn tile (pgupdn page) the extra tile overflows; original has
no special-case for that combination either.
