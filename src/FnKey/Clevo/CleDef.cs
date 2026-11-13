namespace Clevo;

// Subset of Clevo.CleDef actually used by FnKey (enum values identical to the original)
public class CleDef
{
	public enum ModeType
	{
		quiet,
		pwrsaving,
		performance,
		entertainment
	}

	public enum UI_Language : byte
	{
		system,
		en_us,
		en_gb,
		es,
		de,
		it,
		ja,
		ko,
		zh_cn,
		zh_tw,
		fr_fr,
		fr_ca,
		pt_br,
		pt_pt,
		tr_tr,
		th_th,
		vi_vn,
		ru,
		ja_ep
	}
}
