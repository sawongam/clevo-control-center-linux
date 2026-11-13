using System;

namespace ControlCenter30;

public class ReceiveEventLog
{
	public delegate void Arg001Handler(int EventID);

	public delegate void FnKeyHandler(string EventID);

	public delegate void FanHandler(string EventID);

	public event Arg001Handler Arg001Event;

	public event FnKeyHandler FnKeyEvent;

	public event FanHandler FanEvent;

	// Original: subscribed to the "OutLog" Windows event log. Port: in-process DebugLog bus.
	public void InitialEventLog()
	{
		DebugLog.OutLogWritten += outlog_EntryWritten;
	}

	private void outlog_EntryWritten(string message)
	{
		string[] array = message.Split(new char[1] { '^' });
		if (array.Length != 1)
		{
			if (array[0] == "Arg001")
			{
				string value = array[1];
				Arg001Event?.Invoke(Convert.ToInt32(value));
			}
			else if (array[0] == "clevofnkey")
			{
				string eventID = array[1];
				FnKeyEvent?.Invoke(eventID);
			}
			else if (array[0] == "clevocc30")
			{
				string text = array[1];
				CC30Event(text, array[2]);
			}
			else if (array[0] == "clevofan")
			{
				string eventID2 = array[1];
				FanEvent?.Invoke(eventID2);
			}
		}
	}

	private void CC30Event(string Event, string data)
	{
	}
}
