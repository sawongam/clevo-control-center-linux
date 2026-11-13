using System.Collections.Generic;
namespace DesktopBridge_EnergySave;

/// <summary>Minimal System.Data.DataTable stand-in for the chart's AC/DC columns.</summary>
public class PowerRow
{
	public double? AC, DC;
	public object this[string col]
	{
		set { double v = System.Convert.ToDouble(value); if (col == "AC") AC = v; else DC = v; }
	}
}
public class PowerTable
{
	public List<PowerRow> Rows = new List<PowerRow>();
	public PowerRow NewRow() => new PowerRow();
	public void Clear() => Rows.Clear();
}
