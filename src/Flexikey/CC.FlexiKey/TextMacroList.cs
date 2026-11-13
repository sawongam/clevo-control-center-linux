using System.Collections.Generic;

namespace CC.FlexiKey;

public class TextMacroList
{
	public List<MacrokeyContent> StartMacroKey = new List<MacrokeyContent>();

	public List<MacrokeyContent> SendMacroKey = new List<MacrokeyContent>();

	public string TextContent { get; set; }
}
