using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using QuickJump2022.Data;
using FontStyle = System.Drawing.FontStyle;

namespace QuickJump2022.Options;

[Guid("132D5DE0-0D5B-4E2B-884A-9C2595D27CC2")]
[ComVisible(true)]
public class GeneralOptionsPage : DialogPage {
    [LocDisplayName("Code Background Color")]
    [Description("Background color for code")]
    [Category("Code")]
    public Color CodeBackgroundColor { get; set; } = Color.DimGray;

    [LocDisplayName("Code Foreground Color")]
    [Description("Foreground color for code")]
    [Category("Code")]
    public Color CodeForegroundColor { get; set; } = Color.LightPink;

    [LocDisplayName("Code Description Foreground Color")]
    [Description("Foreground color for the description of code")]
    [Category("Code")]
    public Color CodeDescriptionForegroundColor { get; set; } = Color.Silver;

    [LocDisplayName("Selected Code Background Color")]
    [Description("BackGround color for the selected code")]
    [Category("Code")]
    public Color CodeSelectedBackgroundColor { get; set; } = Color.PaleVioletRed;

    [LocDisplayName("Selected Code Foreground Color")]
    [Description("Foreground color for the selected code")]
    [Category("Code")]
    public Color CodeSelectedForegroundColor { get; set; } = Color.WhiteSmoke;

    [LocDisplayName("Selected Code Description Foreground Color")]
    [Description("Foreground color for the description of the selected code")]
    [Category("Code")]
    public Color CodeSelectedDescriptionForegroundColor { get; set; } = Color.Black;

    [LocDisplayName("File Background Color")]
    [Description("Background color for files")]
    [Category("File")]
    public Color FileBackgroundColor { get; set; } = Color.DimGray;

    [LocDisplayName("File Foreground Color")]
    [Description("Foreground color for files")]
    [Category("File")]
    public Color FileForegroundColor { get; set; } = Color.PaleGreen;

    [LocDisplayName("File Description Foreground Color")]
    [Description("Foreground color for the description of files")]
    [Category("File")]
    public Color FileDescriptionForegroundColor { get; set; } = Color.Silver;

    [LocDisplayName("Selected File Background Color")]
    [Description("Background color for the selected file")]
    [Category("File")]
    public Color FileSelectedBackgroundColor { get; set; } = Color.CadetBlue;

    [LocDisplayName("Selected File Foreground Color")]
    [Description("Foreground color for the selected file")]
    [Category("File")]
    public Color FileSelectedForegroundColor { get; set; } = Color.WhiteSmoke;

    [LocDisplayName("Selected File Description Foreground Color")]
    [Description("Foreground color for the description of the selected file")]
    [Category("File")]
    public Color FileSelectedDescriptionForegroundColor { get; set; } = Color.Black;

    [LocDisplayName("Separator Color")]
    [Description("Color for the lines separating the items")]
    [Category("Miscellaneous")]
    public Color ItemSeparatorColor { get; set; } = Color.DarkGray;

    [LocDisplayName("Border Color")]
    [Description("Color for the border around the search form")]
    [Category("Miscellaneous")]
    public Color BorderColor { get; set; } = Color.FromArgb(0, 122, 204);

    [LocDisplayName("Status Background Color")]
    [Description("Background color for the status panel at the bottom")]
    [Category("Miscellaneous")]
    public Color StatusBackgroundColor { get; set; } = Color.FromArgb(90, 90, 90);

    [LocDisplayName("Show Icons")]
    [Description("If icons for file types and code types should be shown.")]
    [Category("Miscellaneous")]
    public bool ShowIcons { get; set; } = true;

    [LocDisplayName("Show Status Bar")]
    [Description("Shows the QuickJump status bar")]
    [Category("Miscellaneous")]
    public bool ShowStatusBar { get; set; } = true;

    [LocDisplayName("File Sort Type")]
    [Description("Choose the way QuickJump sorts when searching files.")]
    [Category("Sort")]
    public Enums.SortType FileSortType { get; set; } = Enums.SortType.Alphabetical;

    [LocDisplayName("C# Sort Type")]
    [Description("Choose the way QuickJump sorts when searching in C# files.")]
    [Category("Sort")]
    public Enums.SortType CSharpSortType { get; set; } = Enums.SortType.LineNumber;

    [LocDisplayName("Mixed Sort Type")]
    [Description("Choose the way QuickJump sorts when searching mixed.")]
    [Category("Sort")]
    public Enums.SortType MixedSortType { get; set; } = Enums.SortType.Alphabetical;

    [LocDisplayName("Top")]
    [Description("Adjust the vertical placement of the list. Negative moves it upwards.")]
    [Category("Layout (Offset)")]
    public int OffsetTop { get; set; } = -200;

    [LocDisplayName("Left")]
    [Description("Adjust the horizontal placement of the list. Negative moves it left.")]
    [Category("Layout (Offset)")]
    public int OffsetLeft { get; set; }

    [LocDisplayName("Width")]
    [Description("Adjust the width of the QuickJump display.")]
    [Category("Layout")]
    public int Width { get; set; } = 700;

    [LocDisplayName("Height (Maximum)")]
    [Description("The maximum height of the QuickJump display.")]
    [Category("Layout")]
    public int MaxHeight { get; set; } = 600;

    [LocDisplayName("Search Text Font")]
    [Description("The font of the search text.")]
    [Category("Font")]
    public Font SearchFont { get; set; } = new Font("Consolas", 14f, FontStyle.Regular, GraphicsUnit.Pixel, 0);

    [LocDisplayName("Item Text Font")]
    [Description("The font of the items in the list. NOTE: At small font sizes the icons will be hidden!")]
    [Category("Font")]
    public Font ItemFont { get; set; } = new Font("Consolas", 12f, FontStyle.Regular, GraphicsUnit.Pixel, 0);
}
