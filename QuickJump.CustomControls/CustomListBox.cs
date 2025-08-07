using System.Windows.Forms;

namespace QuickJump2022.CustomControls;

public class CustomListBox : ListBox
{
	private bool mShowScroll;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams cp = base.CreateParams;
			if (!mShowScroll)
			{
				cp.Style &= -2097153;
			}
			return cp;
		}
	}

	public bool ShowScrollbar
	{
		get
		{
			return mShowScroll;
		}
		set
		{
			if (value != mShowScroll)
			{
				mShowScroll = value;
				if (base.IsHandleCreated)
				{
					RecreateHandle();
				}
			}
		}
	}
}
