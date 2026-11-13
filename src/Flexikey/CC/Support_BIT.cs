namespace CC;

public class Support_BIT
{
	public bool OC_CPU;

	public bool OC_MEM;

	public bool OC_GPU;

	public bool _3DHP;

	public bool ESS;

	public bool Switch_MSHybrid_DGPU;

	public void Init_WMI()
	{
		int num = Global.RW_REG.WMI_122();
		if ((num & 4) == 4)
		{
			_3DHP = true;
		}
		else
		{
			_3DHP = false;
		}
		if (((num >> 19) & 1) == 1)
		{
			_3DHP = true;
		}
		if (((num >> 20) & 1) == 1)
		{
			Switch_MSHybrid_DGPU = true;
		}
		if ((num & 0x400000) == 4194304)
		{
			ESS = true;
		}
		if (((num >> 23) & 1) == 1)
		{
			OC_CPU = true;
		}
		if (((num >> 24) & 1) == 1)
		{
			OC_MEM = true;
		}
		if (((num >> 26) & 1) == 1)
		{
			OC_GPU = true;
		}
	}
}
