# Firmware protocol and reverse-engineering notes

Target: Clevo NH5x_NH7xHP, i7-11800H, RTX 3060, BIOS 1.07.09I
Source: Clevo "ControlCenter 3.0 Package v3.98" (InstallShield, extract with `unshield`). The original files and decompiled
sources are not part of this repository.

## Windows architecture
```
UWP/Desktop-bridge apps (.NET, decompilable)
   │  text commands via Windows Event Log ("PowerBIOSServer" in/out logs), e.g. s^w_arg..., s^r_arg...
   ▼
DCHUService.exe (.NET service, v1.0.0.36)
   │  InsydeDCHU.dll exports: SetDCHU_Data, GetDCHU_Data_Integer, GetDCHU_Data_Buffer,
   │                          ReadAppSettings, WriteAppSettings
   │  WMI fallback: root\WMI CLEVO_GET (InstanceName ACPI\PNP0C14\0_0), events CLEVO_EVENT
   ▼
AcpiBridge.sys  → ACPI device \\?\ACPI#CLV0001  (interface GUID 86994c74-ad43-4812-b7e7-0c420b5c5fd7)
                  IOCTL_ACPIBIOS_READ_METHOD / WRITE / WAIT_EVENT / GET_NOTIFY_VALUE
```
Other GUIDs seen in DCHUService: 93F224E4-FBDC-4BBF-ADD6-DB71BDC0AFAD, 0B6D7934-1EB5-453E-5825-D945072A45AA,
6076EF27-160E-4686-35E5-924A50B4DEAA

## Existing Linux drivers
- tuxedo-drivers via DKMS: `clevo_acpi` (binds CLV0001 — same device as AcpiBridge), `clevo_wmi`
  (WMI ABBC0F6D-8EA1-11D1-00A0-C90629100000), `tuxedo_io` (/dev/tuxedo_io), `tuxedo_keyboard`
- Keyboard: /sys/class/leds/rgb:kbd_backlight (multi_intensity RGB, brightness 0-255)
- TUXEDO Control Center (AUR bin): tried, its features did not work on this model.

## Front-end apps (all managed .NET → decompile with ilspycmd)
| bundle | exe |
|---|---|
| CC30 | ControlCenter30.exe |
| FanSpeedSetting | FanSpeedSetting.exe |
| Keyboard | LedKeyboardSetting.exe (+ perkey_api.dll, LineLEDAPI.dll native) |
| FnKey | FnKey.exe (+ many helpers) |
| CPU_OC | CPUOC2.0.exe, CC30_BG.exe |
| GPUOverclocking | GPUOverclocking.exe, ControlGPU.exe |
| EnergySave | DesktopBridge_EnergySave.exe |
| BatteryUtility | BatteryPackUtility.exe |
| Flexikey (x86) | CC.exe |

Service command names seen (strings): SetCPUPerformance/GetCPUPerformance, SetFanDuty, SetFanAutoDuty,
GetCPUFANDuty, GetVGA1FANDuty, GetFan12RPM, GetFan34RPM, Fan1Info..Fan4Info, SetKBLED, SetWhiteLedKB,
SetPowerLimit1/2CurrentValue, SetCoreRatio, SetCPUVR, SetH2RAM/GetH2RAMData (EC RAM), SetTP, SetCamera, ...

## Decoded protocol (verified against this machine's DSDT)
Entry: `\_SB.DCHU._DSM(UUID 93f224e4-fbdc-4bbf-add6-db71bdc0afad, rev 0, func, Package{arg})`
(DSDT ~line 67852). Dispatch: SCMD (sets), GCMD (gets), CC30 (packages 04/07/0C/0D/0E/11), CPKG, OCWR (OC).

`SetWMI(cmd, sub, data)` = `SetDCHU_Data(cmd, LE32(data | sub<<24))`.

| feature | call | source |
|---|---|---|
| performance mode | 0x79, sub 0x19, data 0=quiet 1=powersave 2=performance 3=entertainment; perf only: \|0x40 turbo fan, \|0x80 DTT | FnKey Features.SetPowerMode; DSDT SCMD 0x79/0x19 → EC FCMD 0xD8, SMI SSMP 0xC0, NPCF (GPU TGP) notify |
| fan mode | 0x79, sub 0x01, app sends 0=auto 1=max 3=silent 5=maxQ 6=custom. THIS BIOS maps 0→EC 0x02, 1→0x10, 2→0x08, 5→0x01, 6→0x04; 3/4 are no-ops (silent unsupported). Readback = PK0D byte 14 (EC 0xD7 query) in the EC bit encoding. | FanSpeedSetting FAN.SetFanMode; DSDT SCMD ~63648 |
| fan offset | 0x79, sub 0x0E, round(255*pct/100) | FAN.SetFanOffset |
| fan defaults | 0x79, sub 0x22 (34), 1 | FAN.WMI13_LoadDefault |
| anti-dust | 0x79 sub 0x29 run; sub 0x28 schedule | FAN.cs |
| fan status | pkg 0x0C → rpm BE words @2/4/6 (rpm=2156250/raw), duty @16/19/22, temp @18/21/24 | FAN.Read_FanSpeed, DSDT PK0C |
| fan table | pkg 0x0D → count @12, mode @14, curves @0x10/0x18/0x20 (T,D)x4, caps @43 bit1=no custom | FAN.Read_WMI13, PK0D |
| write curve | pkg 0x0E Package{Buffer256}: [2+4i]=T2,D2,T3,D3 (D in 0-255); R12/R23/R34 BE words @14+6i; R=round(ΔD%/ΔT*2.55*16); then fan mode 6 | FAN.Write_WMI14/Load_Rxx, PK0E |
| RGB kbd | 0x67, 0xF0000000\|rgb zones, 0xF4000000+brightness | RGBKB.cs (tuxedo_keyboard already does this) |
| white kbd | 0x27 | WhiteKB.cs |
| energy star | 0x4F (79) mode | Features.SetEnergyStarMode |

ReadAppSettings/WriteAppSettings(page, off, len) = Windows-side persisted settings (page 1 off 1 = power mode,
page 4 off 5 = fan mode, off 8 = turbo fan). Not hardware state.

## Hardware verification log

### Test 1 (2026-11-08, read-only): passed
status: fan mode EC 0x02 (=auto), 2 fans, cpu 2218 rpm / gpu 956 rpm, both 35% duty, cpu 48°C (= coretemp pkg 48°C), gpu 44°C.
table: cpu 40→35 60→55 80→86 100→100; gpu 48→35 60→51 80→86 99→100; custom supported (byte43=0x01).
PK0D extras: byte12 FANQ=2 fans, byte15 KBTP=0x06 (keyboard type). CPU temp needs no CalCPUTemp correction (45W TDP → identity).

### Test 2 (2026-11-12, first writes): accepted
perf quiet, fan max, fan auto all returned 121 (=0x79, fw echoes the cmd; not an error code).
After `fan max`: readback mode=max (EC 0x10), duty 100% on both fans, but rpm still 2191/917 —
read immediately, before the fans had spun up (confirmed in test 3).
Perf mode left at quiet after the test.

### Test 3 (2026-11-12): fan writes confirmed
`fan max`: cpu 2190→4990 rpm, gpu 900→3150 rpm within ~2 s, duty 100%. `fan auto`: back to ~2200/~1000, duty 35%.
PK0D byte 14 is not purely fan mode: reads 0x80 in auto while perf=quiet, 0x08 after `perf performance`,
0x02 at boot (Nov 8). ccenterctl labels updated; powersave/entertainment values still unmapped.

## Overclock lock
CPU and GPU overclock writes go through `HardwareGate.GuardOcWrite` in the apps and are refused by the daemon unless
`/etc/ccenter/oc-unlocked` exists. The daemon's `OC_LOCKED` table for DCHU functions is still empty: no overclock DCHU
function has been identified yet, so only the app-settings ranges that hold overclock values are protected.
