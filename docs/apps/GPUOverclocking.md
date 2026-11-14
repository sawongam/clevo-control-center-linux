# GPUOverclocking port notes

Single-page app (Page_system_monitor): GPU gauges (clock, VRAM clock, utilisation, temperature) and, in "performance"
situational mode, Core/Memory increment sliders + comboboxes with Default/Save buttons and the GPU-OC warning dialog.
Hidden-by-design Windows leftovers (CPU/FAN/RAM panels, tab radios, main menu) are ported but stay `IsVisible=False` as in the original.

## Pages / windows
| item | status |
|---|---|
| MainWindow (title bar, fullscreen/min/close, resize grip, skin colour, size persistence) | ported |
| Page_system_monitor (all visible gauges, sliders, combos, Save/Default, dialogs) | ported |
| UserControl_CoreClock, UserControl_DialogBox | ported |
| loading splash (spinner, Storyboard_load re-created in code) | ported (not shown in screenshot mode) |
| UserControl_FAN, UserControl_FanTable2, FAN.cs logic, Clevo_MSG, CleDef | NOT ported: never instantiated/called by this app (FAN reduced to the 4 version fields) |

## Hardware / OC call audit
OC write = anything that changes clocks/offsets/voltage/power/thermal/fan overrides, or persists such a target.
| call | where | class | handling |
|---|---|---|---|
| NVGPU.Set_CoreOC / Set_MEMOC | Page.Save_OC_GPU (gpu 1), ControlGpu.SetIncrement (via DebugLog w_arg255) | OC WRITE | records value (UI reads back), hardware step inside `HardwareGate.GuardOcWrite`, stub |
| NVGPU.Set_VFOffset / Set_ManualMode / Set_LinearMode / Lock_Frequency | declared in original, not called by GUI | OC WRITE | same, gated stubs |
| WriteAppSettings page 5 off 16/18/20/22 (saved OC offsets, replayed by the Windows service at boot) | Save_OC_GPU, BTN_Reset_GPUOC (off 16 len 8) | OC WRITE (persisted target) | `GuardOcWrite` in Page |
| "w_arg255^ControlGPU.exe^set_increment:i:core:mem" (EventLog -> DCHUService -> ControlGPU.exe) | Save/Reset | OC WRITE | DebugLog.WriteDbgLog2Inlog -> ControlGpu.Run -> NVGPU.Set_* (gated) |
| ControlGPU `set_lock_frequency` / `GC6` / `info` | helper only | OC WRITE / registry / AppSettings | lock gated; GC6 and info not ported (never requested) |
| SetAPPData(0,24,4) app version | MainWindow | not OC (bookkeeping) | direct |
| NVGPU reads: InitGPU_API, Check_NVStatus, Get_GPU_Temp/Util/Graphics_Clock/Memory_Clock/Processor_Clock, Base/Boost, ranges, Get_Memory_OC_max, name, TotalNumber, DeviceID, driver ver, Pstate, ... | Page | READ | nvidia-smi (cached 1 s, background thread) / constants |
| GetDCHU_Data_Integer 16/70/122, GetDCHU_Data_Buffer 17 (skin colours) | MainWindow | READ | RippleCC.Platform.InsydeDCHU |
| ReadAppSettings (1,1,1 mode) (1,2,1 lang) (5,2,4 default increments) (0,24,4) | Page/MainWindow/language | READ | InsydeDCHU |

No SetDCHU_Data call is made by this app at all.

## NVGPU adapter (NVGPU.cs)
* Reads: `nvidia-smi --query-gpu=name,temperature.gpu,utilization.gpu,utilization.memory,memory.used,memory.total,clocks.current.graphics,clocks.current.memory,power.draw,fan.speed`, plus once `clocks.max.*,driver_version,pci.device_id`. Never run when sysfs `power/runtime_status` says the dGPU is `suspended` (would wake it) -> UI shows OFF like Windows.
* Constants (nvidia-smi has no equivalent): base 1283 / boost 1703 MHz, max temp 87, core offset range -200..+200, memory offset range -500..+500 (UNVERIFIED guesses for the NVAPI limits), voltage reads return 0.
* `RIPPLE_GPU_FAKE="temp=75,util=60,clk=1650,mem=7001,off=0"` dev override for screenshots.
* Writes: value clamped+recorded, hardware step is a stub. A real implementation needs the proprietary driver with Coolbits (>=8) under X11 and
  `nvidia-settings -a [gpu:0]/GPUGraphicsClockOffsetAllPerformanceLevels=N` / `GPUMemoryTransferRateOffsetAllPerformanceLevels=N` (Wayland: none; NVML needs root).
  Even with `/etc/ripple-cc/oc-unlocked` present nothing is applied until this stub is replaced.

## Deviations
* Under the in-process mock (screenshot / no daemon) `SeededMock` answers cmd 16 with 0xB0 and AppSettings(1,1)=2 (performance mode); otherwise the original code shows "This machine does not support." and hides the OC controls. With the real daemon real values are used.
* `--page` dev option (combine with +): dialog, dialog10, dialog11, battery, mode=N, core=N, mem=N.
* Windows EventLog listener (service pushes "SetGPUOC", Fn-key events 102/202), SystemEvents.PowerModeChanged (resume), WM_COPYDATA: no-ops. Power-mode changes made in other apps are not reflected until restart.
* SystemParameters.PowerLineStatus -> /sys/class/power_supply Mains `online`.
* Single-instance check only when the process is the apphost.
* Resize grip is drawn by hand (WPF default window template grip).
* Images copied to `Image/` with the casing the XAML uses (Page_SystemMonitor, FanSpeedSetting, GPUOverclocking, main, dialog); unreferenced originals (Page_ExtraSetting etc.) not included.
* Slider LayoutTransform(skew) done with LayoutTransformControl; ComboBox/ScrollBar/Slider templates rebuilt (Styles.axaml).

## Not verifiable without hardware / Windows
Real NVAPI offset ranges, base/boost values, behaviour of Save on real hardware, pixel comparison with Windows.

## DCHU commands sent at start-up (verified with RIPPLE_CC_MOCK_LOG; all reads)
| func | when |
|---|---|
| 16 (0x10) get, 70 (0x46) get, 122 (0x7A) get | Init_WMI at first render (16 answered by SeededMock in mock mode) |
| 17 (0x11) get package | LoadSkinColor |
Plus AppSettings reads page 1 off 1/2, page 0 off 24 (+ write of the app version there when it differs), per timer/language refresh.
No other DCHU write exists; Save/Default only touch AppSettings page 5 (gated) and NVGPU (gated).

## Rendered states
01 main (idle, real nvidia-smi values), 02/03 sliders + gauges at different loads (RIPPLE_GPU_FAKE), 04 GPU-OC warning dialog,
05 dGPU asleep (OFF), 06 on battery in performance mode ("Please plug in the power."), 07 quiet mode, 08 entertainment mode, 09 "This machine does not support." dialog.

## Status
Complete (all visible UI ported, builds clean, screenshots checked). Unverified: hover/pressed visuals, real mouse/slider interaction, popup drop-down rendering (headless capture cannot open it), loading splash, fullscreen/resize behaviour, pixel comparison with Windows.
