using System;
namespace CC.other_Class;
[Flags]
public enum BatteryChargeStatus : byte
{
	High = 1,
	Low = 2,
	Critical = 4,
	Charging = 8,
	NoSystemBattery = 0x80,
	Unknown = byte.MaxValue
}
