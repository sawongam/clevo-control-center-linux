using System;
using System.IO;
using System.Text;
using Clevo;
using RippleCC.Platform;

namespace ControlCenter30;

public class language
{
	public static readonly Lazy<language> LazyInstance = new Lazy<language>(() => new language());

	public static language Instance => LazyInstance.Value;

	// kernel32 GetPrivateProfileString -> RippleCC.Platform.Ini
	public static int GetPrivateProfileString(string section, string key, string def, StringBuilder retStr, int bufferSize, string filePath)
		=> Ini.GetPrivateProfileString(section, key, def, retStr, bufferSize, filePath);

	private language()
	{
	}

	public byte GetUILanguage()
	{
		int page = 1;
		int offset = 2;
		int length = 1;
		try
		{
			byte[] array = DCHU.ReadAppSettings(page, offset, length);
			if (array[0] == 6 && GetCustomId() == 5)
			{
				return 18;
			}
			return array[0];
		}
		catch
		{
			Msg.Show("Error");
		}
		return 0;
	}

	public byte GetCustomId()
	{
		byte[] wMIPackage = DCHU.GetWMIPackage(17);
		return wMIPackage[2];
	}

	public string TranslationLanguage(string Text)
	{
		StringBuilder stringBuilder = new StringBuilder(255);
		string def = "null";
		int bufferSize = 255;
		if (Text.Contains("INFO"))
		{
			bufferSize = 500;
		}
		string filePath = Path.Combine(AppContext.BaseDirectory, "mtlanguage.ini");
		GetPrivateProfileString((CleDef.UI_Language)GetUILanguage() switch
		{
			CleDef.UI_Language.system => "en-us", 
			CleDef.UI_Language.en_us => "en-us", 
			CleDef.UI_Language.en_gb => "en-us", 
			CleDef.UI_Language.es => "es", 
			CleDef.UI_Language.de => "de", 
			CleDef.UI_Language.it => "it", 
			CleDef.UI_Language.ja => "ja", 
			CleDef.UI_Language.ko => "ko", 
			CleDef.UI_Language.zh_cn => "zh-cn", 
			CleDef.UI_Language.zh_tw => "zh-tw", 
			CleDef.UI_Language.fr_fr => "fr-fr", 
			CleDef.UI_Language.fr_ca => "fr-ca", 
			CleDef.UI_Language.pt_br => "pt-br", 
			CleDef.UI_Language.pt_pt => "pt-pt", 
			CleDef.UI_Language.tr_tr => "tr-tr", 
			CleDef.UI_Language.th_th => "th-th", 
			CleDef.UI_Language.vi_vn => "vi-vn", 
			CleDef.UI_Language.ru => "ru", 
			CleDef.UI_Language.ja_ep => "ja-ep", 
			_ => "en-us", 
		}, Text, def, stringBuilder, bufferSize, filePath);
		if (stringBuilder.ToString().Equals("null"))
		{
			GetPrivateProfileString("en-us", Text, def, stringBuilder, bufferSize, filePath);
		}
		return stringBuilder.ToString();
	}
}
