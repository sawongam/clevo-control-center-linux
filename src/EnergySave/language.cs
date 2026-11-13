using System;
using System.IO;
using System.Text;
using RippleCC.Platform;
using Clevo;

namespace Clevo
{
	public static class CleDef
	{
		public enum UI_Language : byte
		{
			system, en_us, en_gb, es, de, it, ja, ko, zh_cn, zh_tw, fr_fr, fr_ca, pt_br, pt_pt, tr_tr, th_th, vi_vn, ru, ja_ep
		}
	}
}

namespace DesktopBridge_EnergySave
{
	public class language
	{
		public static readonly Lazy<language> LazyInstance = new Lazy<language>(() => new language());

		public static language Instance => LazyInstance.Value;

		/// <summary>Dev/screenshot override (--lang de): replaces the UI language byte stored in the BIOS app settings.</summary>
		public static int? OverrideLanguage;

		private language()
		{
		}

		public byte GetUILanguage()
		{
			if (OverrideLanguage.HasValue) return (byte)OverrideLanguage.Value;
			int page = 1;
			int offset = 2;
			int length = 1;
			try
			{
				byte[] array = DCHU.ReadAppSettings(page, offset, length);
				if (array[0] == 6)
				{
					return 18;
				}
				return array[0];
			}
			catch
			{
				MessageBox.Show("Error");
			}
			return 0;
		}

		public byte GetCustomId()
		{
			byte[] wMIPackage = DCHU.GetWMIPackage(17);
			return wMIPackage[2];
		}

		public static string Code(CleDef.UI_Language l) => l switch
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
		};

		public string TranslationLanguage(string Text)
		{
			StringBuilder stringBuilder = new StringBuilder(255);
			string def = "null";
			int bufferSize = 255;
			string filePath = Path.Combine(AppContext.BaseDirectory, "mtlanguage.ini");
			Ini.GetPrivateProfileString(Code((CleDef.UI_Language)GetUILanguage()), Text, def, stringBuilder, bufferSize, filePath);
			if (stringBuilder.ToString().Equals("null"))
			{
				Ini.GetPrivateProfileString("en-us", Text, def, stringBuilder, bufferSize, filePath);
			}
			return stringBuilder.ToString();
		}
	}
}
