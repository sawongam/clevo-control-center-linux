using System;
using System.Diagnostics;

namespace GPUOverclocking;

internal class DebugLog
{
	// Windows: EventLog "OutLog" (consumed by nobody here). Linux: no event log; discarded.
	public static void WriteDbgLog2Outlog(string log)
	{
		if (Environment.GetEnvironmentVariable("GPUOC_DEBUG") == "1")
			Console.Error.WriteLine("DebugGPU:" + log);
	}

	// Windows: EventLog "PowerBiosServerLog", read by DCHUService which then executes the command.
	// The only command this app sends is  w_arg255^<path>\ControlGPU.exe^set_increment:<gpu>:<core>:<mem>
	// which the service ran through ControlGPU.exe. The service does not exist on Linux, so the same
	// request is handled in-process by ControlGpu (which funnels into the gated NVGPU writes).
	public static void WriteDbgLog2Inlog(string log)
	{
		if (Environment.GetEnvironmentVariable("GPUOC_DEBUG") == "1")
			Console.Error.WriteLine("InLog:" + log);
		try
		{
			string[] array = log.Split('^');
			if (array.Length >= 3 && array[0] == "w_arg255" && array[1].EndsWith("ControlGPU.exe"))
				ControlGpu.Run(array[2]);
		}
		catch
		{
		}
	}
}
