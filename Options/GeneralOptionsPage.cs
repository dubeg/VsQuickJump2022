using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using QuickJump2022.Models;

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

    [LocDisplayName("Show Icons")]
    [Description("If icons for file types and code types should be shown.")]
    [Category("Miscellaneous")]
    public bool ShowIcons { get; set; } = true;

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
}
