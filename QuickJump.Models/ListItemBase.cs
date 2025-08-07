using System;
using System.Drawing;

namespace QuickJump2022.Models;

public class ListItemBase : IComparable
{
	public int Weight;

	public Icon IconImage;

	public string Name;

	public string Description;

	public int Line = 1;

	public virtual void Go()
	{
	}

	public int CompareTo(object obj)
	{
		ListItemBase item = (ListItemBase)obj;
		if (item.Weight > Weight)
		{
			return -1;
		}
		if (item.Weight >= Weight)
		{
			return 0;
		}
		return 1;
	}

	public override string ToString()
	{
		return Name;
	}
}
