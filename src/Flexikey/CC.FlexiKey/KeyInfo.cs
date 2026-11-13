using System.Collections.Generic;

namespace CC.FlexiKey;

public class KeyInfo
{
	public List<MacrokeyContent> MacroKey = new List<MacrokeyContent>();

	public List<MacrokeyContent> StartKey = new List<MacrokeyContent>();

	public List<MacrokeyContent> SendKey = new List<MacrokeyContent>();

	public KeyGlobal.DeviceType Device { get; set; }

	public int KeyCode { get; set; }

	public string KeyName { get; set; }

	public string CheckBoxName { get; set; }

	public string KeyDisplay { get; set; }

	public string Tip { get; set; }

	public KeyGlobal.KeyActionType ActionType { get; set; }

	public bool IsSelected { get; set; }

	public bool IsKeypressDelay { get; set; }

	public int MacroKeyGruopFlag { get; set; }

	public string AppPath { get; set; }

	public int AppEventId { get; set; }

	public string TextContent { get; set; }
}
