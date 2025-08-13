using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using QuickJump2022.Models;

namespace QuickJump2022.Options;

[Guid("132D5DE0-0D5B-4E2B-884A-9C2595D27CC2")]
[ComVisible(true)]
public class GeneralOptionsPage : DialogPage {
    [LocDisplayName("Use symbol colors")]
    [Description("Use the same colors found in the text editor.")]
    [Category("Colors")]
    public bool UseSymbolColors { get; set; } = true;

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
