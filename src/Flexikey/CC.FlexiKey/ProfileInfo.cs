using System.Collections.Generic;

namespace CC.FlexiKey;

public class ProfileInfo
{
	public List<KeyInfo> profileData = new List<KeyInfo>();

	public int ProfileIndex { get; set; }

	public string ProfileName { get; set; }

	public string ProfileIconPath { get; set; }

	public int CurrentDevice { get; set; }

	public bool IsKbEnable { get; set; }

	public bool IsMouseEnable { get; set; }

	public bool IsSelected { get; set; }
}
