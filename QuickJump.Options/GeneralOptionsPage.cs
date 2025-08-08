using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using QuickJump2022.Data;
using FontStyle = System.Drawing.FontStyle;

namespace QuickJump2022.Options;

[Guid("132D5DE0-0D5B-4E2B-884A-9C2595D27CC2")]
[ComVisible(true)]
public class GeneralOptionsPage : DialogPage
{
	private Color m_CodeBackgroundColor = Color.DimGray;

	private Color m_CodeDescriptionForegroundColor = Color.Silver;

	private Color m_CodeForegroundColor = Color.LightPink;

	private Color m_CodeSelectedBackgroundColor = Color.PaleVioletRed;

	private Color m_CodeSelectedDescriptionForegroundColor = Color.Black;

	private Color m_CodeSelectedForegroundColor = Color.WhiteSmoke;

	private Color m_FileBackgroundColor = Color.DimGray;

	private Color m_FileDescriptionForegroundColor = Color.Silver;

	private Color m_FileForegroundColor = Color.PaleGreen;

	private Color m_FileSelectedBackgroundColor = Color.CadetBlue;

	private Color m_FileSelectedDescriptionForegroundColor = Color.Black;

	private Color m_FileSelectedForegroundColor = Color.WhiteSmoke;

	private Font m_ItemFont = new Font("Consolas", 12f, FontStyle.Regular, GraphicsUnit.Pixel, 0);

	private Font m_SearchFont = new Font("Consolas", 14f, FontStyle.Regular, GraphicsUnit.Pixel, 0);

	private int m_OffsetTop = -200;

	private int m_OffsetLeft;

	private int m_Width = 700;

	private int m_MaxHeight = 600;

	private Color m_SeperatorColor = Color.DarkGray;

	private Color m_BorderColor = Color.FromArgb(0, 122, 204); // VSCode blue

 private Color m_StatusBackgroundColor = Color.FromArgb(90, 90, 90);

	private bool m_UseModernIcons;

	private bool m_ShowStatusBar = true;

	private bool m_ShowIcons = true;

	private bool m_UseWPFInterface = false;

	private Enums.SortType m_CSharpSortType = Enums.SortType.LineNumber;

	private Enums.SortType m_FileSortType = Enums.SortType.Alphabetical;

	private Enums.SortType m_MixedSortType = Enums.SortType.Alphabetical;

	[LocDisplayName("Code Background Color")]
	[Description("Background color for code")]
	[Category("Code")]
	public Color CodeBackgroundColor
	{
		get
		{
			return m_CodeBackgroundColor;
		}
		set
		{
			m_CodeBackgroundColor = value;
		}
	}

	[LocDisplayName("Code Foreground Color")]
	[Description("Foreground color for code")]
	[Category("Code")]
	public Color CodeForegroundColor
	{
		get
		{
			return m_CodeForegroundColor;
		}
		set
		{
			m_CodeForegroundColor = value;
		}
	}

	[LocDisplayName("Code Description Foreground Color")]
	[Description("Foreground color for the description of code")]
	[Category("Code")]
	public Color CodeDescriptionForegroundColor
	{
		get
		{
			return m_CodeDescriptionForegroundColor;
		}
		set
		{
			m_CodeDescriptionForegroundColor = value;
		}
	}

	[LocDisplayName("Selected Code Background Color")]
	[Description("BackGround color for the selected code")]
	[Category("Code")]
	public Color CodeSelectedBackgroundColor
	{
		get
		{
			return m_CodeSelectedBackgroundColor;
		}
		set
		{
			m_CodeSelectedBackgroundColor = value;
		}
	}

	[LocDisplayName("Selected Code Foreground Color")]
	[Description("Foreground color for the selected code")]
	[Category("Code")]
	public Color CodeSelectedForegroundColor
	{
		get
		{
			return m_CodeSelectedForegroundColor;
		}
		set
		{
			m_CodeSelectedForegroundColor = value;
		}
	}

	[LocDisplayName("Selected Code Description Foreground Color")]
	[Description("Foreground color for the description of the selected code")]
	[Category("Code")]
	public Color CodeSelectedDescriptionForegroundColor
	{
		get
		{
			return m_CodeSelectedDescriptionForegroundColor;
		}
		set
		{
			m_CodeSelectedDescriptionForegroundColor = value;
		}
	}

	[LocDisplayName("File Background Color")]
	[Description("Background color for files")]
	[Category("File")]
	public Color FileBackgroundColor
	{
		get
		{
			return m_FileBackgroundColor;
		}
		set
		{
			m_FileBackgroundColor = value;
		}
	}

	[LocDisplayName("File Foreground Color")]
	[Description("Foreground color for files")]
	[Category("File")]
	public Color FileForegroundColor
	{
		get
		{
			return m_FileForegroundColor;
		}
		set
		{
			m_FileForegroundColor = value;
		}
	}

	[LocDisplayName("File Description Foreground Color")]
	[Description("Foreground color for the description of files")]
	[Category("File")]
	public Color FileDescriptionForegroundColor
	{
		get
		{
			return m_FileDescriptionForegroundColor;
		}
		set
		{
			m_FileDescriptionForegroundColor = value;
		}
	}

	[LocDisplayName("Selected File Background Color")]
	[Description("Background color for the selected file")]
	[Category("File")]
	public Color FileSelectedBackgroundColor
	{
		get
		{
			return m_FileSelectedBackgroundColor;
		}
		set
		{
			m_FileSelectedBackgroundColor = value;
		}
	}

	[LocDisplayName("Selected File Foreground Color")]
	[Description("Foreground color for the selected file")]
	[Category("File")]
	public Color FileSelectedForegroundColor
	{
		get
		{
			return m_FileSelectedForegroundColor;
		}
		set
		{
			m_FileSelectedForegroundColor = value;
		}
	}

	[LocDisplayName("Selected File Description Foreground Color")]
	[Description("Foreground color for the description of the selected file")]
	[Category("File")]
	public Color FileSelectedDescriptionForegroundColor
	{
		get
		{
			return m_FileSelectedDescriptionForegroundColor;
		}
		set
		{
			m_FileSelectedDescriptionForegroundColor = value;
		}
	}

	[LocDisplayName("Seperator Color")]
	[Description("Color for the lines seperating the items")]
	[Category("Miscellaneous")]
	public Color ItemSeperatorColor
	{
		get
		{
			return m_SeperatorColor;
		}
		set
		{
			m_SeperatorColor = value;
		}
	}

	[LocDisplayName("Border Color")]
	[Description("Color for the border around the search form")]
	[Category("Miscellaneous")]
	public Color BorderColor
	{
		get
		{
			return m_BorderColor;
		}
		set
		{
			m_BorderColor = value;
		}
	}

 [LocDisplayName("Status Background Color")]
 [Description("Background color for the status panel at the bottom")] 
 [Category("Miscellaneous")]
 public Color StatusBackgroundColor
 {
     get
     {
         return m_StatusBackgroundColor;
     }
     set
     {
         m_StatusBackgroundColor = value;
     }
 }

	[LocDisplayName("Use Modern Icons")]
	[Description("Use modern icons for the CSharp code parts. This requires a restart of Visual Studio to take effect.")]
	[Category("Miscellaneous")]
	public bool UseModernIcons
	{
		get
		{
			return m_UseModernIcons;
		}
		set
		{
			m_UseModernIcons = value;
		}
	}

	[LocDisplayName("Show Icons")]
	[Description("If icons for file types and code types should be shown.")]
	[Category("Miscellaneous")]
	public bool ShowIcons
	{
		get
		{
			return m_ShowIcons;
		}
		set
		{
			m_ShowIcons = value;
		}
	}

	[LocDisplayName("Show Status Bar")]
	[Description("Shows the QuickJump status bar")]
	[Category("Miscellaneous")]
	public bool ShowStatusBar
	{
		get
		{
			return m_ShowStatusBar;
		}
		set
		{
			m_ShowStatusBar = value;
		}
	}

	[LocDisplayName("Use WPF Interface")]
	[Description("Use WPF interface instead of WinForms to support font ligatures (requires restart)")]
	[Category("Miscellaneous")]
	public bool UseWPFInterface
	{
		get
		{
			return m_UseWPFInterface;
		}
		set
		{
			m_UseWPFInterface = value;
		}
	}

	[LocDisplayName("File Sort Type")]
	[Description("Choose the way QuickJump sorts when searching files.")]
	[Category("Sort")]
	public Enums.SortType FileSortType
	{
		get
		{
			return m_FileSortType;
		}
		set
		{
			m_FileSortType = value;
		}
	}

	[LocDisplayName("C# Sort Type")]
	[Description("Choose the way QuickJump sorts when searching in C# files.")]
	[Category("Sort")]
	public Enums.SortType CSharpSortType
	{
		get
		{
			return m_CSharpSortType;
		}
		set
		{
			m_CSharpSortType = value;
		}
	}

	[LocDisplayName("Mixed Sort Type")]
	[Description("Choose the way QuickJump sorts when searching mixed.")]
	[Category("Sort")]
	public Enums.SortType MixedSortType
	{
		get
		{
			return m_MixedSortType;
		}
		set
		{
			m_MixedSortType = value;
		}
	}

	[LocDisplayName("Top")]
	[Description("Adjust the vertical placement of the list. Negative moves it upwards.")]
	[Category("Layout (Offset)")]
	public int OffsetTop
	{
		get
		{
			return m_OffsetTop;
		}
		set
		{
			m_OffsetTop = value;
		}
	}

	[LocDisplayName("Left")]
	[Description("Adjust the horizontal placement of the list. Negative moves it left.")]
	[Category("Layout (Offset)")]
	public int OffsetLeft
	{
		get
		{
			return m_OffsetLeft;
		}
		set
		{
			m_OffsetLeft = value;
		}
	}

	[LocDisplayName("Width")]
	[Description("Adjust the width of the QuickJump display.")]
	[Category("Layout")]
	public int Width
	{
		get
		{
			return m_Width;
		}
		set
		{
			m_Width = value;
		}
	}

	[LocDisplayName("Height (Maximum)")]
	[Description("The maximum height of the QuickJump display.")]
	[Category("Layout")]
	public int MaxHeight
	{
		get
		{
			return m_MaxHeight;
		}
		set
		{
			m_MaxHeight = value;
		}
	}

	[LocDisplayName("Search Text Font")]
	[Description("The font of the search text.")]
	[Category("Font")]
	public Font SearchFont
	{
		get
		{
			return m_SearchFont;
		}
		set
		{
			m_SearchFont = value;
		}
	}

	[LocDisplayName("Item Text Font")]
	[Description("The font of the items in the list. NOTE: At small font sizes the icons will be hidden!")]
	[Category("Font")]
	public Font ItemFont
	{
		get
		{
			return m_ItemFont;
		}
		set
		{
			m_ItemFont = value;
		}
	}
}
