# Documentation

- [roadmap.md](roadmap.md): the state of every component and the open work.
- [protocol.md](protocol.md): how the Windows suite talks to the firmware, the decoded command set and the hardware test log.
- [porting.md](porting.md): the WPF to Avalonia mapping and the rules for porting or changing an app.

## Per-app notes
Each file lists what was ported, where the port differs from the original, what is stubbed, and which firmware
commands the app sends.

- [ControlCenter30](apps/ControlCenter30.md): launcher and power modes
- [FanSpeedSetting](apps/FanSpeedSetting.md)
- [LedKeyboardSetting](apps/LedKeyboardSetting.md)
- [EnergySave](apps/EnergySave.md)
- [BatteryUtility](apps/BatteryUtility.md)
- [CPUOC](apps/CPUOC.md)
- [GPUOverclocking](apps/GPUOverclocking.md)
- [Flexikey](apps/Flexikey.md)
- [FnKey](apps/FnKey.md): on-screen popups for Fn keys

[screenshots/](screenshots/) holds one screenshot per app, rendered against the simulator.
