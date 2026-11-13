using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace CC.FlexiKey;

public class XmlRW
{
	public static void Save2Xml(List<ProfileInfo> _profileInfo, string filename)
	{
		XmlDocument xmlDocument = new XmlDocument();
		XmlDeclaration newChild = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", "yes");
		xmlDocument.AppendChild(newChild);
		XmlElement xmlElement = xmlDocument.CreateElement("root");
		xmlDocument.AppendChild(xmlElement);
		foreach (ProfileInfo item in _profileInfo)
		{
			Console.WriteLine("Save2Xml Profile_" + item.ProfileIndex);
			XmlElement xmlElement2 = xmlDocument.CreateElement("Profile_" + item.ProfileIndex);
			xmlElement2.SetAttribute("Name", item.ProfileName);
			xmlElement2.SetAttribute("IsSelected", Convert.ToInt32(item.IsSelected).ToString());
			if (item.IsSelected)
			{
				xmlElement.SetAttribute("Active", item.ProfileIndex.ToString());
			}
			xmlElement2.SetAttribute("CurrentDevice", item.CurrentDevice.ToString());
			xmlElement2.SetAttribute("IsKbEnable", Convert.ToInt32(item.IsKbEnable).ToString());
			xmlElement2.SetAttribute("IsMouseEnable", Convert.ToInt32(item.IsMouseEnable).ToString());
			xmlElement2.SetAttribute("Icon", item.ProfileIconPath);
			xmlElement.AppendChild(xmlElement2);
			int num = 0;
			int num2 = 0;
			foreach (KeyInfo profileDatum in item.profileData)
			{
				num++;
				if (profileDatum.ActionType == KeyGlobal.KeyActionType.none)
				{
					continue;
				}
				XmlElement xmlElement3 = xmlDocument.CreateElement("Set_" + num);
				xmlElement3.SetAttribute("Device", Convert.ToString((int)profileDatum.Device));
				xmlElement3.SetAttribute("KeyCode", profileDatum.KeyCode.ToString());
				xmlElement3.SetAttribute("UiName", profileDatum.KeyName);
				xmlElement3.SetAttribute("KeyDisplay", profileDatum.KeyDisplay);
				xmlElement3.SetAttribute("Action", Convert.ToString((int)profileDatum.ActionType));
				xmlElement3.SetAttribute("IsKeypressDelay", Convert.ToInt32(profileDatum.IsKeypressDelay).ToString());
				xmlElement3.SetAttribute("Tip", profileDatum.Tip);
				xmlElement3.SetAttribute("CheckBoxName", profileDatum.CheckBoxName);
				xmlElement2.AppendChild(xmlElement3);
				switch (profileDatum.ActionType)
				{
				case KeyGlobal.KeyActionType.keypress:
					num2 = 0;
					foreach (MacrokeyContent item2 in profileDatum.MacroKey)
					{
						XmlElement xmlElement7 = xmlDocument.CreateElement("Index_" + num2);
						xmlElement7.SetAttribute("KeyCode", item2.KeyCode.ToString());
						xmlElement7.SetAttribute("KeysGroup", item2.KeysGroup.ToString());
						xmlElement7.SetAttribute("KeyMakeBreak", item2.KeyMakeBreak.ToString());
						xmlElement7.SetAttribute("KeyDelay", item2.KeyDelay.ToString());
						xmlElement7.SetAttribute("KeyDisplay", item2.KeyDisplay);
						num2++;
						xmlElement3.AppendChild(xmlElement7);
					}
					break;
				case KeyGlobal.KeyActionType.launch:
				{
					XmlElement xmlElement8 = xmlDocument.CreateElement("Index_" + 0);
					xmlElement8.SetAttribute("AppPath", profileDatum.AppPath);
					xmlElement8.SetAttribute("AppEventId", profileDatum.AppEventId.ToString());
					xmlElement3.AppendChild(xmlElement8);
					break;
				}
				case KeyGlobal.KeyActionType.text:
				{
					num2 = 0;
					if (profileDatum.StartKey.Count == 0)
					{
						MacrokeyContent macrokeyContent = new MacrokeyContent();
						macrokeyContent.KeyCode = 0;
						macrokeyContent.KeyDelay = 0;
						macrokeyContent.KeyDisplay = "";
						macrokeyContent.KeyMakeBreak = KeyGlobal.MakeBreak.KeyMake;
						macrokeyContent.KeysGroup = 0;
						profileDatum.StartKey.Add(macrokeyContent);
						MacrokeyContent macrokeyContent2 = new MacrokeyContent();
						macrokeyContent2.KeyCode = 0;
						macrokeyContent2.KeyDelay = 0;
						macrokeyContent2.KeyDisplay = "";
						macrokeyContent2.KeyMakeBreak = KeyGlobal.MakeBreak.KeyBreak;
						macrokeyContent2.KeysGroup = 0;
						profileDatum.StartKey.Add(macrokeyContent2);
					}
					foreach (MacrokeyContent item3 in profileDatum.StartKey)
					{
						XmlElement xmlElement4 = xmlDocument.CreateElement("StartKey");
						xmlElement4.SetAttribute("KeyCode", item3.KeyCode.ToString());
						xmlElement4.SetAttribute("KeyMakeBreak", item3.KeyMakeBreak.ToString());
						xmlElement4.SetAttribute("KeyDelay", item3.KeyDelay.ToString());
						xmlElement4.SetAttribute("KeyDisplay", item3.KeyDisplay);
						xmlElement3.AppendChild(xmlElement4);
						num2++;
					}
					XmlElement xmlElement5 = xmlDocument.CreateElement("Text");
					xmlElement5.SetAttribute("TextContent", profileDatum.TextContent);
					xmlElement3.AppendChild(xmlElement5);
					num2 = 0;
					if (profileDatum.SendKey.Count == 0)
					{
						MacrokeyContent macrokeyContent3 = new MacrokeyContent();
						macrokeyContent3.KeyCode = 0;
						macrokeyContent3.KeyDelay = 0;
						macrokeyContent3.KeyDisplay = "";
						macrokeyContent3.KeyMakeBreak = KeyGlobal.MakeBreak.KeyMake;
						macrokeyContent3.KeysGroup = 0;
						profileDatum.SendKey.Add(macrokeyContent3);
						MacrokeyContent macrokeyContent4 = new MacrokeyContent();
						macrokeyContent4.KeyCode = 0;
						macrokeyContent4.KeyDelay = 0;
						macrokeyContent4.KeyDisplay = "";
						macrokeyContent4.KeyMakeBreak = KeyGlobal.MakeBreak.KeyBreak;
						macrokeyContent4.KeysGroup = 0;
						profileDatum.SendKey.Add(macrokeyContent4);
					}
					foreach (MacrokeyContent item4 in profileDatum.SendKey)
					{
						XmlElement xmlElement6 = xmlDocument.CreateElement("SendKey");
						xmlElement6.SetAttribute("KeyCode", item4.KeyCode.ToString());
						xmlElement6.SetAttribute("KeyMakeBreak", item4.KeyMakeBreak.ToString());
						xmlElement6.SetAttribute("KeyDelay", item4.KeyDelay.ToString());
						xmlElement6.SetAttribute("KeyDisplay", item4.KeyDisplay);
						xmlElement3.AppendChild(xmlElement6);
						num2++;
					}
					break;
				}
				}
			}
		}
		xmlDocument.Save(filename);
	}

	public static bool LoadXml(string filename, out List<ProfileInfo> _profileInfo)
	{
		_profileInfo = new List<ProfileInfo>();
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			XmlNodeList xmlNodeList = xmlDocument.SelectNodes("root");
				int count = xmlNodeList.Count;
			for (int i = 0; i < count; i++)
			{
				int num = 0;
				foreach (XmlNode childNode in xmlNodeList.Item(i).ChildNodes)
				{
					ProfileInfo profileInfo = new ProfileInfo();
					profileInfo.ProfileName = childNode.Attributes["Name"].Value;
					profileInfo.IsSelected = Convert.ToBoolean(Convert.ToInt32(childNode.Attributes["IsSelected"].Value));
					profileInfo.CurrentDevice = Convert.ToInt32(childNode.Attributes["CurrentDevice"].Value);
					profileInfo.IsKbEnable = Convert.ToBoolean(Convert.ToInt32(childNode.Attributes["IsKbEnable"].Value));
					profileInfo.IsMouseEnable = Convert.ToBoolean(Convert.ToInt32(childNode.Attributes["IsMouseEnable"].Value));
					profileInfo.ProfileIconPath = childNode.Attributes["Icon"].Value;
					profileInfo.ProfileIndex = num;
					num++;
					Console.WriteLine("LoadXml prfile index=" + profileInfo.ProfileIndex);
					foreach (XmlNode childNode2 in childNode.ChildNodes)
					{
						if (!childNode2.Name.Contains("Set_"))
						{
							continue;
						}
						try
						{
							KeyInfo keyInfo = new KeyInfo();
							keyInfo.Device = (KeyGlobal.DeviceType)Convert.ToInt32(childNode2.Attributes["Device"].Value);
							keyInfo.ActionType = (KeyGlobal.KeyActionType)Convert.ToInt32(childNode2.Attributes["Action"].Value);
							keyInfo.KeyCode = Convert.ToInt32(childNode2.Attributes["KeyCode"].Value);
							keyInfo.KeyDisplay = childNode2.Attributes["KeyDisplay"].Value;
							keyInfo.KeyName = childNode2.Attributes["UiName"].Value;
							keyInfo.Tip = childNode2.Attributes["Tip"].Value;
							keyInfo.IsKeypressDelay = Convert.ToBoolean(Convert.ToInt32(childNode2.Attributes["IsKeypressDelay"].Value));
							keyInfo.CheckBoxName = childNode2.Attributes["CheckBoxName"].Value;
							foreach (XmlNode childNode3 in childNode2.ChildNodes)
							{
								switch (keyInfo.ActionType)
								{
								case KeyGlobal.KeyActionType.keypress:
									keyInfo.MacroKey = AddMacroKeyData(childNode3, keyInfo.MacroKey, KeyGlobal.KeyActionType.keypress);
									break;
								case KeyGlobal.KeyActionType.launch:
									keyInfo.AppPath = childNode3.Attributes["AppPath"].Value;
									keyInfo.AppEventId = Convert.ToInt32(childNode3.Attributes["AppEventId"].Value);
									break;
								case KeyGlobal.KeyActionType.text:
									if (childNode3.Name.Contains("StartKey"))
									{
										keyInfo.StartKey = AddMacroKeyData(childNode3, keyInfo.StartKey, KeyGlobal.KeyActionType.text);
									}
									if (childNode3.Name.Contains("Text"))
									{
										keyInfo.TextContent = childNode3.Attributes["TextContent"].Value;
									}
									if (childNode3.Name.Contains("SendKey"))
									{
										keyInfo.SendKey = AddMacroKeyData(childNode3, keyInfo.SendKey, KeyGlobal.KeyActionType.text);
									}
									break;
								}
							}
							profileInfo.profileData.Add(keyInfo);
						}
						catch
						{
						}
					}
					_profileInfo.Add(profileInfo);
				}
			}
		}
		catch
		{
			return false;
		}
		return true;
	}

	public static ProfileInfo LoadActiveProfile(string filename)
	{
		ProfileInfo profileInfo = new ProfileInfo();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(filename);
		XmlNodeList xmlNodeList = xmlDocument.SelectNodes("root");
		int count = xmlNodeList.Count;
		for (int i = 0; i < count; i++)
		{
			foreach (XmlNode childNode in xmlNodeList.Item(i).ChildNodes)
			{
				bool flag = Convert.ToBoolean(Convert.ToInt32(childNode.Attributes["IsSelected"].Value));
				if (!flag)
				{
					continue;
				}
				profileInfo.ProfileName = childNode.Attributes["Name"].Value;
				profileInfo.IsSelected = flag;
				profileInfo.ProfileIconPath = childNode.Attributes["Icon"].Value;
				profileInfo.ProfileIndex = i;
				Console.WriteLine("LoadActiveProfile prfile index=" + profileInfo.ProfileIndex);
				profileInfo.CurrentDevice = Convert.ToInt32(childNode.Attributes["CurrentDevice"].Value);
				profileInfo.IsKbEnable = Convert.ToBoolean(Convert.ToInt32(childNode.Attributes["IsKbEnable"].Value));
				profileInfo.IsMouseEnable = Convert.ToBoolean(Convert.ToInt32(childNode.Attributes["IsMouseEnable"].Value));
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					KeyInfo keyInfo = new KeyInfo();
					keyInfo.ActionType = (KeyGlobal.KeyActionType)Convert.ToInt32(childNode2.Attributes["Action"].Value);
					keyInfo.IsKeypressDelay = Convert.ToBoolean(Convert.ToInt32(childNode2.Attributes["IsKeypressDelay"].Value));
					keyInfo.KeyCode = Convert.ToInt32(childNode2.Attributes["KeyCode"].Value);
					keyInfo.KeyDisplay = childNode2.Attributes["KeyDisplay"].Value;
					keyInfo.Tip = childNode2.Attributes["Tip"].Value;
					foreach (XmlNode childNode3 in childNode2.ChildNodes)
					{
						switch (keyInfo.ActionType)
						{
						case KeyGlobal.KeyActionType.keypress:
							keyInfo.MacroKey = AddMacroKeyData(childNode3, keyInfo.MacroKey, KeyGlobal.KeyActionType.keypress);
							break;
						case KeyGlobal.KeyActionType.launch:
							keyInfo.AppPath = childNode3.Attributes["AppPath"].Value;
							keyInfo.AppEventId = Convert.ToInt32(childNode3.Attributes["AppEventId"].Value);
							break;
						case KeyGlobal.KeyActionType.text:
						{
							List<MacrokeyContent> list = new List<MacrokeyContent>();
							if (childNode3.Name.Contains("Start"))
							{
								list.Clear();
								list = (keyInfo.StartKey = AddMacroKeyData(childNode3, list, KeyGlobal.KeyActionType.text));
							}
							if (childNode3.Name.Contains("Text"))
							{
								keyInfo.TextContent = childNode3.Attributes["TextContent"].Value;
							}
							if (childNode3.Name.Contains("SendKey"))
							{
								list.Clear();
								list = AddMacroKeyData(childNode3, list, KeyGlobal.KeyActionType.text);
								keyInfo.SendKey = list;
							}
							break;
						}
						}
					}
					profileInfo.profileData.Add(keyInfo);
				}
				return profileInfo;
			}
		}
		return profileInfo;
	}

	private static List<MacrokeyContent> AddMacroKeyData(XmlNode data, List<MacrokeyContent> MacroKey, KeyGlobal.KeyActionType action)
	{
		MacrokeyContent macrokeyContent = new MacrokeyContent();
		macrokeyContent.KeyCode = Convert.ToInt32(data.Attributes["KeyCode"].Value);
		if (data.Attributes["KeyMakeBreak"].Value == "KeyMake")
		{
			macrokeyContent.KeyMakeBreak = KeyGlobal.MakeBreak.KeyMake;
		}
		else if (data.Attributes["KeyMakeBreak"].Value == "KeyBreak")
		{
			macrokeyContent.KeyMakeBreak = KeyGlobal.MakeBreak.KeyBreak;
		}
		macrokeyContent.KeyDelay = Convert.ToInt32(data.Attributes["KeyDelay"].Value);
		macrokeyContent.KeyDisplay = data.Attributes["KeyDisplay"].Value;
		if (action == KeyGlobal.KeyActionType.keypress)
		{
			macrokeyContent.KeysGroup = Convert.ToInt32(data.Attributes["KeysGroup"].Value);
		}
		MacroKey.Add(macrokeyContent);
		return MacroKey;
	}

	public static string ReadLanguage(string filename, string strKey)
	{
		string result = "";
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			foreach (XmlNode childNode in xmlDocument.SelectNodes("root").Item(0).ChildNodes)
			{
				result = childNode.Attributes[strKey].Value;
			}
			return result;
		}
		catch (NullReferenceException)
		{
			return "Read Error.";
		}
	}
}
